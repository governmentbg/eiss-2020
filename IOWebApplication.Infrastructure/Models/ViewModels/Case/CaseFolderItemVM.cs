using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Елемент от електронна папка на дело
    /// </summary>
    public class CaseFolderItemVM
    {
        public string ItemName { get; set; }
        public int SourceType { get; set; }
        public string SourceId { get; set; }
        public DateTime? Date { get; set; }
        public string DateText { get 
            { 
                if (this.Date == null) return string.Empty;
                if (this.Date.Value.Hour == 0 && this.Date.Value.Minute == 0)
                    return this.Date.Value.ToString("dd.MM.yyyy");
                else
                    return this.Date.Value.ToString("dd.MM.yyyy HH:mm");
            } }
        public string Title { get; set; }
        public string TypeName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }

        public string ElementUrl { get; set; }
    }
}
