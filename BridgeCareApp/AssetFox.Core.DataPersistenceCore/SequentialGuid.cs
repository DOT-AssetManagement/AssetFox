using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Common;
using Microsoft.EntityFrameworkCore;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssetFox.Core.DataPersistenceCore
{
    internal static class SequentialGuid
    {
        private static readonly SequentialGuidValueGenerator _gen = new();

        public static Guid NewGuid() => _gen.Next(null);   
    }
}
