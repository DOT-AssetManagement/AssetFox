using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DTOs
{
    public class KeyFieldValues
    {
        public List<string> FieldNames { get; set; }
        public List<List<string>> Values { get; set; }
    }
}
