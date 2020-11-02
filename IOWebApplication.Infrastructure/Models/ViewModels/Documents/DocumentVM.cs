// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using AutoMapper;
using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за документ, страни по документите и връзка към дела
    /// </summary>
    public class DocumentVM
    {

        [IORequired]
        [Display(Name = "Деловодна регистратура")]
        public int? CourtOrganizationId { get; set; }

        public bool CanChange { get; set; }
        public long Id { get; set; }
        public int TemplateId { get; set; }

        [Display(Name = "Стар номер")]
        public string DocumentNumber { get; set; }
        public int DocumentNumberValue { get; set; }
        [Display(Name = "Дата документ")]
        [Required(ErrorMessage = "Въведете '{0}'.")]
        public DateTime DocumentDate { get; set; }

        [Display(Name = "Стар номер")]
        [RegularExpression("[0-9]*$", ErrorMessage = "Невалиден {0}.")]
        public string OldDocumentNumber { get; set; }
        [Display(Name = "Дата документ")]
        public DateTime? OldDocumentDate { get; set; }
        [Display(Name = "Документ номер")]
        public string RegNumber
        {
            get
            {
                if (DocumentDate.Hour == 0 && DocumentDate.Minute == 0)
                {
                    return $"{DocumentNumber} / {DocumentDate:dd.MM.yyyy}";
                }
                else
                {
                    return $"{DocumentNumber} / {DocumentDate:dd.MM.yyyy HH:mm}";
                }
            }
        }

        public DateTime? ActualDocumentDate { get; set; }


        public int DocumentDirectionId { get; set; }

        [Display(Name = "Вид документ")]
        [IORequired]
        public int DocumentKindId { get; set; }

        [Display(Name = "Основен вид документ")]
        [Range(0, 9999999, ErrorMessage = "Изберете '{0}'.")]
        [IORequired]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ")]
        [Required(ErrorMessage = "Изберете '{0}'.")]
        public int? DocumentTypeId { get; set; }

        /// <summary>
        /// Начин на изпращане: Призовкар,Поща,куриер,факс
        /// </summary>
        [Display(Name = "Начин на получаване")]
        public int? DeliveryGroupId { get; set; }

        /// <summary>
        /// Указания за изпращане: Обикновено,препоръчано,колет
        /// </summary>
        [Display(Name = "Указания")]
        public int? DeliveryTypeId { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Ограничен достъп")]
        public bool IsRestictedAccess { get; set; }

        [Display(Name = "Секретен документ")]
        public bool IsSecret { get; set; }

        [Display(Name = "Въвеждане на стар номер")]
        public bool IsOldNumber { get; set; }


        [Display(Name = "Множествено регистриранe")]
        public bool IsMultiNumber { get; set; }

        [Display(Name = "Брой документи")]
        public int MultiDocumentCounter { get; set; }
        public string MultiRegistationId { get; set; }
        [Display(Name = "Множествено регистриранe")]
        public string MultiRegistationInfo { get; set; }
        //---------------По дело-----------------------

        public int? CaseId { get; set; }
        public string CaseRegisterNumber { get; set; }
        [Display(Name = "ЕИСПП номер на НП")]
        [RegularExpression("[а-яА-Я]{3}[0-9]{8}[а-яА-Я]{3}", ErrorMessage = "Невалиден {0}.")]
        public string EISSPNumber { get; set; }

        [Display(Name = "Вид производство")]
        [IORequired]
        public int? ProcessPriorityId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int? CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        [IORequired]
        public int? CaseTypeId { get; set; }

        [Display(Name = "Предмет на дело")]
        public int? CaseCodeId { get; set; }
        public IList<CheckListVM> CaseClassifications { get; set; }

        //---------------По дело-----------------------
        public IList<DocumentPersonVM> DocumentPersons { get; set; }
        [Display(Name = "Избор на свързано дело")]
        public bool HasCaseInfo { get; set; }

        //Примерно когато се прави изходящ по входящ документ, да се дръпнат лицата от входящия
        public long PriorDocumentId { get; set; }

        public bool HasDocumentResolutions { get; set; }


        public DocumentCaseInfoVM DocumentCaseInfo { get; set; }
        //public IList<DocumentCaseInfoVM> CaseInfo { get; set; }
        public IList<DocumentInstitutionCaseInfoVM> InstitutionCaseInfo { get; set; }
        public IList<DocumentLinkVM> DocumentLinks { get; set; }

        public DocumentVM()
        {
            DocumentDate = DateTime.Now;
            DocumentCaseInfo = new DocumentCaseInfoVM();
            DocumentPersons = new List<DocumentPersonVM>();
            InstitutionCaseInfo = new List<DocumentInstitutionCaseInfoVM>();
            DocumentLinks = new List<DocumentLinkVM>();
            CaseClassifications = new List<CheckListVM>();
            IsOldNumber = false;
            IsMultiNumber = false;
            MultiDocumentCounter = 1;
        }

        public static MapperConfiguration GetMapping()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Document, DocumentVM>();
            });

        }
    }
}
