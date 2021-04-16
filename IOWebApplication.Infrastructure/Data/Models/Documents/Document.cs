using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Regix;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Деловодни документи
    /// </summary>
    [Table("document")]
    public class Document : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("court_organization_id")]
        public int? CourtOrganizationId { get; set; }

        /// <summary>
        /// Посока на движение на документ: Входящи, Изходящи, вътрешен
        /// </summary>
        [Column("document_direction_id")]
        public int DocumentDirectionId { get; set; }

        [Display(Name = "Основен вид документ")]
        [Column("document_group_id")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ")]
        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [Column("document_number_value")]
        public int? DocumentNumberValue { get; set; }

        [Column("document_number")]
        public string DocumentNumber { get; set; }

        [Column("document_date")]
        public DateTime DocumentDate { get; set; }

        [Column("actual_document_date")]
        public DateTime? ActualDocumentDate { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("is_resticted_access")]
        public bool IsRestictedAccess { get; set; }

        [Column("is_secret")]
        public bool? IsSecret { get; set; }

        [Column("is_old_number")]
        public bool? IsOldNumber { get; set; }

        /// <summary>
        /// Начин на изпращане: Призовкар,Поща,куриер,факс
        /// </summary>
        [Column("delivery_group_id")]
        public int? DeliveryGroupId { get; set; }

        /// <summary>
        /// Указания за изпращане: Обикновено,препоръчано,колет
        /// </summary>
        [Column("delivery_type_id")]
        public int? DeliveryTypeId { get; set; }

        [Column("post_office_date")]
        public DateTime? PostOfficeDate { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("multi_registration_id")]
        public string MultiRegistationId { get; set; }

        [ForeignKey(nameof(CourtOrganizationId))]
        public virtual CourtOrganization CourtOrganization { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentDirectionId))]
        public virtual DocumentDirection DocumentDirection { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }

        [ForeignKey(nameof(DeliveryGroupId))]
        public virtual DeliveryGroup DeliveryGroup { get; set; }

        [ForeignKey(nameof(DeliveryTypeId))]
        public virtual DeliveryType DeliveryType { get; set; }

        public virtual ICollection<DocumentPerson> DocumentPersons { get; set; }
        public virtual ICollection<DocumentCaseInfo> DocumentCaseInfo { get; set; }
        public virtual ICollection<DocumentInstitutionCaseInfo> DocumentInstitutionCaseInfo { get; set; }
        public virtual ICollection<Cases.Case> Cases { get; set; }
        public virtual ICollection<DocumentTemplate> DocumentTemplates { get; set; }
        public virtual ICollection<DocumentResolution> DocumentResolutions { get; set; }

        [InverseProperty(nameof(DocumentLink.Document))]
        public virtual ICollection<DocumentLink> DocumentLinks { get; set; }

        public virtual ICollection<RegixReport> RegixReports { get; set; }

        public Document()
        {
            DocumentPersons = new HashSet<DocumentPerson>();
            DocumentCaseInfo = new HashSet<DocumentCaseInfo>();
            DocumentInstitutionCaseInfo = new HashSet<DocumentInstitutionCaseInfo>();
            DocumentLinks = new HashSet<DocumentLink>();
            DocumentResolutions = new HashSet<DocumentResolution>();
            RegixReports = new HashSet<RegixReport>();
        }
    }
}
