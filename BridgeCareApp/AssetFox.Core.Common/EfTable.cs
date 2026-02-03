using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.Common
{
    public static class EfTable
    {
        public static string Stage(this string liveName) => $"{liveName}_Staging";
    }
}
