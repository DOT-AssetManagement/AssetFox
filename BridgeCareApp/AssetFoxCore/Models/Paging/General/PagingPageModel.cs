using System.Collections.Generic;

namespace AssetFoxCore.Models
{
    public class PagingPageModel<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
    }
}
