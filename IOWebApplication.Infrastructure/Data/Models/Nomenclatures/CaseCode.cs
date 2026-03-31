using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Шифри на дело
    /// </summary>
    [Table("nom_case_code")]
    public class CaseCode : BaseCommonNomenclature
    {

        /// <summary>
        /// Правно основание: члХХ от НК
        /// </summary>
        [Column("lawbase_description")]
        [Display(Name = "Правно основание")]
        public string LawBaseDescription { get; set; }

        /// <summary>
        /// Само за ВКС
        /// </summary>
        [Column("load_index")]
        public decimal LoadIndex { get; set; }

        public virtual ICollection<CaseTypeCode> TypeCodes { get; set; }
        
        /// <summary>
        /// Подшифри
        /// </summary>
        public virtual ICollection<CaseCodeSub> CodeSubs { get; set; }

        public CaseCode()
        {
            TypeCodes = new HashSet<CaseTypeCode>();
            CodeSubs = new HashSet<CaseCodeSub>();
        }
    }
}
