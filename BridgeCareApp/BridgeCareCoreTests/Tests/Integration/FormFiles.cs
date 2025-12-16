using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace AssetFoxCoreTests.Tests.Integration
{
    public static class FormFiles
    {
        public static FormFile FromFileInfo(FileInfoDTO fileInfo)
        {
            var bytes = Convert.FromBase64String(fileInfo.FileData);
            var stream = new MemoryStream(bytes);
            var formFile = new FormFile(stream, 0, stream.Length, fileInfo.FileName, fileInfo.FileName);
            return formFile;
        }
    }
}
