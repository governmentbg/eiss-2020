using System.Collections.Generic;

namespace IOWebApplication.Core.Models
{
    public class DataTableResponseVM<T>
    {
        public int TotalCount { get; set; } = 0;

        /// <summary>
        /// Регистър
        /// </summary>
        public List<T> Records { get; set; } = new();
    }
}
