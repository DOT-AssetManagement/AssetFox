using System.Text;
using AssetFoxCore.Services.Branding;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AssetFoxCoreTests.Tests.AdminSettings
{
    public sealed class ClientLogoServiceTests : IDisposable
    {
        private static readonly byte[] PngBytes =
        {
            0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x01, 0x02, 0x03
        };

        private readonly string _deploymentRoot;
        private readonly string _apiDirectory;
        private readonly string _clientDirectory;

        public ClientLogoServiceTests()
        {
            _deploymentRoot = Path.Combine(
                Path.GetTempPath(),
                $"AssetFox-ClientLogoServiceTests-{Guid.NewGuid():N}");
            _apiDirectory = Path.Combine(_deploymentRoot, "API");
            _clientDirectory = Path.Combine(_deploymentRoot, "Client");

            Directory.CreateDirectory(_apiDirectory);
            Directory.CreateDirectory(_clientDirectory);
            WriteConfig();
        }

        [Fact]
        public async Task SaveAsync_WritesBothLogosToConfiguredNamesInSiblingClientFolder()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var service = new ClientLogoService(_apiDirectory);
            var svgBytes = Encoding.UTF8.GetBytes(
                "<svg xmlns=\"http://www.w3.org/2000/svg\"><path d=\"M0 0\" /></svg>");

            var agencyName = await service.SaveAsync(
                ClientLogoKind.Agency,
                CreateFormFile(PngBytes, "uploaded-agency.png", "image/png"),
                cancellationToken);
            var implementationName = await service.SaveAsync(
                ClientLogoKind.Implementation,
                CreateFormFile(svgBytes, "uploaded-implementation.svg", "image/svg+xml"),
                cancellationToken);

            Assert.Equal("agency.png", agencyName);
            Assert.Equal("implementation.svg", implementationName);
            Assert.Equal(
                PngBytes,
                await File.ReadAllBytesAsync(
                    Path.Combine(_clientDirectory, "logos", agencyName),
                    cancellationToken));
            Assert.Equal(
                svgBytes,
                await File.ReadAllBytesAsync(
                    Path.Combine(_clientDirectory, "logos", implementationName),
                    cancellationToken));
        }

        [Fact]
        public async Task SaveAsync_ReplacesAnExistingConfiguredLogo()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var logosDirectory = Path.Combine(_clientDirectory, "logos");
            Directory.CreateDirectory(logosDirectory);
            var logoPath = Path.Combine(logosDirectory, "agency.png");
            await File.WriteAllBytesAsync(logoPath, new byte[] { 0x01, 0x02 }, cancellationToken);
            var service = new ClientLogoService(_apiDirectory);

            await service.SaveAsync(
                ClientLogoKind.Agency,
                CreateFormFile(PngBytes, "different-name.png", "image/png"),
                cancellationToken);

            Assert.Equal(PngBytes, await File.ReadAllBytesAsync(logoPath, cancellationToken));
        }

        [Fact]
        public async Task SaveAsync_RejectsContentsThatDoNotMatchConfiguredExtension()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var service = new ClientLogoService(_apiDirectory);
            var invalidPng = Encoding.UTF8.GetBytes("This is not a PNG file.");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                service.SaveAsync(
                    ClientLogoKind.Agency,
                    CreateFormFile(invalidPng, "fake.png", "image/png"),
                    cancellationToken));

            Assert.Contains("do not match", exception.Message);
            Assert.False(File.Exists(Path.Combine(_clientDirectory, "logos", "agency.png")));
        }

        [Fact]
        public async Task SaveAsync_RejectsDirectoryPathsFromConfig()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            WriteConfig("../agency.png", "implementation.svg");
            var service = new ClientLogoService(_apiDirectory);

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
                service.SaveAsync(
                    ClientLogoKind.Agency,
                    CreateFormFile(PngBytes, "agency.png", "image/png"),
                    cancellationToken));

            Assert.Contains("without a directory path", exception.Message);
        }

        [Fact]
        public async Task ReadAsDataUrlAsync_ReadsFromTheClientLogoFile()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var service = new ClientLogoService(_apiDirectory);
            await service.SaveAsync(
                ClientLogoKind.Agency,
                CreateFormFile(PngBytes, "agency.png", "image/png"),
                cancellationToken);

            var result = await service.ReadAsDataUrlAsync(ClientLogoKind.Agency, cancellationToken);

            Assert.Equal($"data:image/png;base64,{Convert.ToBase64String(PngBytes)}", result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_deploymentRoot))
                Directory.Delete(_deploymentRoot, true);
        }

        private void WriteConfig(
            string agencyFilename = "agency.png",
            string implementationFilename = "implementation.svg")
        {
            var json = $$"""
                {
                  "logos": {
                    "agencyFilename": "{{agencyFilename}}",
                    "implementationFilename": "{{implementationFilename}}"
                  }
                }
                """;
            File.WriteAllText(Path.Combine(_clientDirectory, "config.json"), json);
        }

        private static FormFile CreateFormFile(byte[] bytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
