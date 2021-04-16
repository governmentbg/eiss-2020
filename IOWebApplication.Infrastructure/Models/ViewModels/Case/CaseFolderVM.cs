using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Електронна папка на дело
    /// </summary>
    public class CaseFolderVM : CaseVM
    {
        public List<CaseFolderItemVM> Items { get; set; }

        public CaseFolderVM()
        {
            Items = new List<CaseFolderItemVM>();
        }
    }
}
