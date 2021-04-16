using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид съдебен акт
    /// </summary>
    [Table("nom_act_type")]
    public class ActType : BaseCommonNomenclature
    {
        /// <summary>
        /// Вид форматиране на акт: protokol,act
        /// 10.05.2020
        /// </summary>
        [Column("act_format_type")]
        public string ActFormatType { get; set; }


        /// <summary>
        /// Наименование акт за бланката на съдебните актове
        /// 10.05.2020
        /// </summary>
        [Column("blank_label")]
        public string BlankLabel { get; set; }

        /// <summary>
        /// В името на народа, за присъди
        /// 10.05.2020
        /// </summary>
        [Column("blank_header_text")]
        public string BlankHeaderText { get; set; }

        /// <summary>
        /// Текст за бланка: ОПРЕДЕЛИ/РАЗПОРЕДИ/РЕШИ:
        /// </summary>
        [Column("blank_decision_text")]
        public string BlankDecisionText { get; set; }

        [Column("eispp_code")]
        public string EISPPCode { get; set; }

    }
}
