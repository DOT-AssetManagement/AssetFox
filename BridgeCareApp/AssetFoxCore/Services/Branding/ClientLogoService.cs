using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace AssetFoxCore.Services.Branding
{
    /// <summary>
    /// Stores client branding in the deployed Client folder that is a sibling of
    /// the deployed API folder. Logo filenames remain deployment configuration
    /// and are read from Client/config.json.
    /// </summary>
    public sealed class ClientLogoService : IClientLogoService
    {
        private const long MaximumLogoSize = 5 * 1024 * 1024;
        private const string ClientDirectoryName = "Client";
        private const string ConfigFileName = "config.json";
        private const string LogosDirectoryName = "logos";

        private static readonly IReadOnlyDictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".svg"] = "image/svg+xml",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".gif"] = "image/gif",
                [".webp"] = "image/webp"
            };

        private readonly string _clientDirectory;

        public ClientLogoService(string apiDirectory)
        {
            if (string.IsNullOrWhiteSpace(apiDirectory))
                throw new ArgumentException("The API directory is required.", nameof(apiDirectory));

            var apiDirectoryInfo = new DirectoryInfo(Path.GetFullPath(apiDirectory));
            if (apiDirectoryInfo.Parent == null)
                throw new InvalidOperationException($"The API directory '{apiDirectoryInfo.FullName}' has no parent directory.");

            _clientDirectory = Path.Combine(apiDirectoryInfo.Parent.FullName, ClientDirectoryName);
        }

        public async Task<string> SaveAsync(
            ClientLogoKind logoKind,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new InvalidDataException("A non-empty logo file is required.");
            if (file.Length > MaximumLogoSize)
                throw new InvalidDataException("Logo files cannot be larger than 5 MB.");

            var configuredLogo = await GetConfiguredLogoAsync(logoKind, cancellationToken);
            var bytes = await ReadUploadAsync(file, cancellationToken);
            ValidateFileContents(configuredLogo.Extension, bytes);

            var logosDirectory = Path.Combine(_clientDirectory, LogosDirectoryName);
            Directory.CreateDirectory(logosDirectory);

            var targetPath = GetContainedPath(logosDirectory, configuredLogo.FileName);
            var temporaryPath = Path.Combine(
                logosDirectory,
                $".{configuredLogo.FileName}.{Guid.NewGuid():N}.tmp");

            try
            {
                await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken);
                File.Move(temporaryPath, targetPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }

            return configuredLogo.FileName;
        }

        public async Task<string> ReadAsDataUrlAsync(
            ClientLogoKind logoKind,
            CancellationToken cancellationToken = default)
        {
            var configuredLogo = await GetConfiguredLogoAsync(logoKind, cancellationToken);
            var logoPath = GetContainedPath(
                Path.Combine(_clientDirectory, LogosDirectoryName),
                configuredLogo.FileName);

            if (!File.Exists(logoPath))
                return string.Empty;

            var bytes = await File.ReadAllBytesAsync(logoPath, cancellationToken);
            return $"data:{configuredLogo.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }

        private async Task<ConfiguredLogo> GetConfiguredLogoAsync(
            ClientLogoKind logoKind,
            CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_clientDirectory))
                throw new DirectoryNotFoundException(
                    $"Expected the Client deployment folder at '{_clientDirectory}'. " +
                    "The Client and API folders must be siblings.");

            var configPath = Path.Combine(_clientDirectory, ConfigFileName);
            if (!File.Exists(configPath))
                throw new FileNotFoundException(
                    $"The Client branding configuration was not found at '{configPath}'.",
                    configPath);

            try
            {
                await using var configStream = File.OpenRead(configPath);
                using var document = await JsonDocument.ParseAsync(
                    configStream,
                    new JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip
                    },
                    cancellationToken);

                var logos = document.RootElement.GetProperty("logos");
                var propertyName = logoKind == ClientLogoKind.Agency
                    ? "agencyFilename"
                    : "implementationFilename";
                var fileName = logos.GetProperty(propertyName).GetString();

                return ValidateConfiguredFileName(propertyName, fileName);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    $"The Client configuration at '{configPath}' is not valid JSON.",
                    exception);
            }
            catch (KeyNotFoundException exception)
            {
                throw new InvalidDataException(
                    $"The Client configuration at '{configPath}' is missing the required logos settings.",
                    exception);
            }
        }

        private static ConfiguredLogo ValidateConfiguredFileName(string propertyName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidDataException($"The logos.{propertyName} setting must contain a filename.");

            if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) ||
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidDataException(
                    $"The logos.{propertyName} setting must be a filename without a directory path.");
            }

            var extension = Path.GetExtension(fileName);
            if (!ContentTypes.TryGetValue(extension, out var contentType))
            {
                throw new InvalidDataException(
                    $"The configured logo extension '{extension}' is not supported. " +
                    "Supported extensions are .svg, .png, .jpg, .jpeg, .gif, and .webp.");
            }

            return new ConfiguredLogo(fileName, extension, contentType);
        }

        private static async Task<byte[]> ReadUploadAsync(IFormFile file, CancellationToken cancellationToken)
        {
            await using var input = file.OpenReadStream();
            using var output = new MemoryStream((int)file.Length);
            await input.CopyToAsync(output, cancellationToken);

            if (output.Length > MaximumLogoSize)
                throw new InvalidDataException("Logo files cannot be larger than 5 MB.");

            return output.ToArray();
        }

        private static void ValidateFileContents(string extension, byte[] bytes)
        {
            var isValid = extension.ToLowerInvariant() switch
            {
                ".svg" => IsSafeSvg(bytes),
                ".png" => StartsWith(bytes, 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a),
                ".jpg" or ".jpeg" => StartsWith(bytes, 0xff, 0xd8, 0xff),
                ".gif" => StartsWith(bytes, 0x47, 0x49, 0x46, 0x38, 0x37, 0x61) ||
                          StartsWith(bytes, 0x47, 0x49, 0x46, 0x38, 0x39, 0x61),
                ".webp" => bytes.Length >= 12 &&
                           StartsWith(bytes, 0x52, 0x49, 0x46, 0x46) &&
                           bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50,
                _ => false
            };

            if (!isValid)
            {
                throw new InvalidDataException(
                    $"The uploaded file contents do not match the configured '{extension}' logo format.");
            }
        }

        private static bool StartsWith(byte[] bytes, params byte[] signature)
        {
            if (bytes.Length < signature.Length)
                return false;

            for (var index = 0; index < signature.Length; index++)
            {
                if (bytes[index] != signature[index])
                    return false;
            }

            return true;
        }

        private static bool IsSafeSvg(byte[] bytes)
        {
            try
            {
                using var stream = new MemoryStream(bytes, false);
                using var reader = XmlReader.Create(
                    stream,
                    new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Prohibit,
                        XmlResolver = null
                    });

                var foundRoot = false;
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (!foundRoot)
                    {
                        if (!string.Equals(reader.LocalName, "svg", StringComparison.OrdinalIgnoreCase))
                            return false;
                        foundRoot = true;
                    }

                    if (IsForbiddenSvgElement(reader.LocalName))
                        return false;

                    if (!reader.HasAttributes)
                        continue;

                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                            return false;

                        if ((string.Equals(reader.LocalName, "href", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(reader.Name, "xlink:href", StringComparison.OrdinalIgnoreCase)) &&
                            IsUnsafeSvgReference(reader.Value))
                        {
                            return false;
                        }
                    }

                    reader.MoveToElement();
                }

                return foundRoot;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private static bool IsForbiddenSvgElement(string localName)
        {
            return string.Equals(localName, "script", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(localName, "foreignObject", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(localName, "iframe", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(localName, "object", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(localName, "embed", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUnsafeSvgReference(string value)
        {
            var normalized = value.TrimStart();
            return normalized.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith("data:text/html", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetContainedPath(string directory, string fileName)
        {
            var fullDirectory = Path.GetFullPath(directory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(fullDirectory, fileName));

            if (!fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The configured logo filename resolves outside the logos directory.");

            return fullPath;
        }

        private sealed class ConfiguredLogo
        {
            public ConfiguredLogo(string fileName, string extension, string contentType)
            {
                FileName = fileName;
                Extension = extension;
                ContentType = contentType;
            }

            public string FileName { get; }
            public string Extension { get; }
            public string ContentType { get; }
        }
    }
}
