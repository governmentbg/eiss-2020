// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Integrations
{
    /// <summary>
    /// ЕИСС модел с изчетени данни за бюлетин за съдимост, 2024г
    /// </summary>
    public class CaisBuletinModel
    {

        public CaisBuletinContextModel Context { get; set; }

        public CaisBuletinInfoModel BuletinInfo { get; set; }

        public CaisBuletinPersonModel MainPerson { get; set; }
        public CaisBuletinPersonModel[] PrevNames { get; set; }

        public CaisBuletinAddressModel MainPersonBirthPlace { get; set; }

        public CaisBuletinAddressModel OtherUicAddress { get; set; }

        public CaisBuletinIdentityDocumentModel[] IdentityDocuments { get; set; }

        /// <summary>
        /// Гражданство - използват се само кодовете на страните
        /// </summary>
        public CaisBuletinAddressModel[] Citizenship { get; set; }
        public CaisBuletinPersonModel MotherNames { get; set; }
        public CaisBuletinPersonModel FatherNames { get; set; }

        public CaisBuletinActModel Act { get; set; }
        public CaisBuletinCaseModel Case { get; set; }

        public CaisBuletinCrimeModel[] Crimes { get; set; }

        //public CaisBuletinProbationModel[] Probations { get; set; }

        public string ErrorMessage { get; set; }
    }
    public class CaisBuletinContextModel
    {
        public DateTime CreateDate { get; set; }
        public CaisBuletinPersonModel CreatorPerson { get; set; }
        public CaisBuletinPersonModel ApproverPerson { get; set; }
        public string AuthorityEIK { get; set; }
        public string AuthorityEISPP { get; set; }
        public string AuthorityName { get; set; }
    }

    public class CaisBuletinInfoModel
    {
        public string BuletinName { get; set; }
        public string BuletinNumber { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? RecieveDate { get; set; }
        public string BureauName { get; set; }
        public string NumberEISPP { get; set; }
        public string NumberAFIS { get; set; }
        public string SentenceDescription { get; set; }
        public bool IsAdministrative78a { get; set; }
        public bool IsConvicted { get; set; }

        public CaisBuletinPersonModel CreatorNames { get; set; }
        public CaisBuletinPersonModel ApproverNames { get; set; }
    }

    public class CaisBuletinPersonModel
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }
        public string FullName { get; set; }
        public int GenderId { get; set; }
        public string Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string UIC { get; set; }
        public int UicTypeId { get; set; }
        public string OtherUic { get; set; }

        public string EGN
        {
            get { return UicTypeId == NomenclatureConstants.UicTypes.EGN ? UIC : string.Empty; }
        }

        public string LNCh
        {
            get { return UicTypeId == NomenclatureConstants.UicTypes.LNCh ? UIC : string.Empty; }
        }

        public string LN
        {
            get { return UicTypeId == NomenclatureConstants.UicTypes.LN ? UIC : string.Empty; }
        }

        public string LatinFirstName { get; set; }
        public string LatinMiddleName { get; set; }
        public string LatinFamilyName { get; set; }
        public string LatinFullName { get; set; }

        /// <summary>
        /// Длъжност
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// Тип предходно име
        /// </summary>
        public string PrevNameType { get; set; }

        public void ParseNames(string names, bool latinNames = false)
        {
            var person = new NamesBase();
            person.SplitPersonNames(names);
            if (latinNames)
            {
                LatinFirstName = person.FirstName;
                LatinMiddleName = person.MiddleName;
                LatinFamilyName = person.FamilyName;
                if (!string.IsNullOrEmpty(person.Family2Name))
                {
                    LatinFamilyName += "-" + person.Family2Name;
                }
                LatinFullName = names;
            }
            else
            {
                FirstName = person.FirstName;
                MiddleName = person.MiddleName;
                FamilyName = person.FamilyName;
                if (!string.IsNullOrEmpty(person.Family2Name))
                {
                    FamilyName += "-" + person.Family2Name;
                }
                FullName = names;
            }
        }
    }

    public class CaisBuletinIdentityDocumentModel
    {
        /// <summary>
        /// Тип на личен документ: nom_eispp_tbl_element(tbl_name:254)
        /// </summary>
        public string PersonalDocumentId { get; set; }
        public string TypeName { get; set; }
        public string Issuer { get; set; }
        public string Number { get; set; }
        public DateTime DateIssue { get; set; }
        public DateTime? ValidTo { get; set; }

        public CaisBuletinAddressModel Country { get; set; }
    }

    public class CaisBuletinAddressModel
    {
        public string CountryName { get; set; }

        /// <summary>
        /// Трицифрен код на държава
        /// </summary>
        public string CountryCodeD { get; set; }
        /// <summary>
        /// Трибуквен код на държава
        /// </summary>
        public string CountryCodeL { get; set; }

        public string CityName { get; set; }
        public string CityEkatte { get; set; }
        public string CityDescription { get; set; }
        public string CityDescriptionLat { get; set; }
    }

    public class CaisBuletinActModel
    {
        public string ActNumber { get; set; }
        public DateTime ActDate { get; set; }
        public DateTime? ActInforceDate { get; set; }
        public string ECLInumber { get; set; }
        public int ActTypeId { get; set; }
        public string ActType { get; set; }
        public string ActTypeCode { get; set; }
        public CaisBuletinCourtModel Court { get; set; }

        public int CaseId { get; set; }
    }

    public class CaisBuletinCaseModel
    {
        public int CaseTypeId { get; set; }
        public string CaseType { get; set; }
        public string CaseNumber { get; set; }
        public int CaseYear { get; set; }
        public CaisBuletinCourtModel Court { get; set; }
    }

    public class CaisBuletinCourtModel
    {
        public string CourtCode { get; set; }
        public string CourtName { get; set; }
        public string CourtEispp { get; set; }
    }

    /// <summary>
    /// Престъпление
    /// </summary>
    public class CaisBuletinCrimeModel
    {
        public int CaseCrimeId { get; set; }

        /// <summary>
        /// 3.1 Категория деяние
        /// </summary>
        public string CategoryDeedName { get; set; }
        public string CategoryDeedCode { get; set; }

        /// <summary>
        /// Обща категория 
        /// </summary>
        public string CategoryCommonDeedName { get; set; }
        public string CategoryCommonDeedCode { get; set; }
        /// <summary>
        /// Описание на деянието
        /// </summary>
        public string CrimeDescription { get; set; }

        /// <summary>
        /// Правна квалификация
        /// </summary>
        public string LegalQualificationText { get; set; }

        /// <summary>
        /// Място на престъплението
        /// </summary>
        public CaisBuletinAddressModel Address { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Форма на вината
        /// </summary>
        public int FormOfGuiltId { get; set; }
        public string FormOfGuilt
        {
            get
            {
                switch (FormOfGuiltId)
                {
                    case 1:
                        return "Умишлено";
                    case 2:
                        return "Непредпазливо";
                    default:
                        return "";
                }
            }
        }

        public bool NotPunished { get; set; }
        public bool DeclaredPunishmentPriorSentence { get; set; }

        public string ActPriorProbation { get; set; }

        public string EISSPNumber { get; set; }

        public int RecidiveTypeId { get; set; }

        /// <summary>
        /// Рецидив на лице
        /// </summary>
        public string RecidiveTypeLabel { get; set; }

        /// <summary>
        /// Наказания към престъпления
        /// </summary>
        public CaisBuletinPunishmentModel[] Punishmets { get; set; }
    }

    /// <summary>
    /// Наказание
    /// </summary>
    public class CaisBuletinPunishmentModel
    {
        public int Id { get; set; }
        public int CasePersonSentencePunishmentId { get; set; }
        public string SentenceTypeCode { get; set; }
        public string SentenceTypeCode2 { get; set; }
        public string SentenceTypeName { get; set; }
        public string SentenceText { get; set; }
        public string PunishmentGeneralCategoryCode { get; set; }
        public string PunishmentGeneralCategoryName { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Наложена глоба
        /// </summary>
        public decimal? ChargeAmount { get; set; }
        public decimal? ChargeAmountEUR { get; set; }


        //Срок
        public int SentenceYears { get; set; }
        public int SentenceMonths { get; set; }
        public int SentenceWeeks { get; set; }
        public int SentenceDays { get; set; }


        //Приспадане и задържане
        public int PreliminaryDetentionDays { get; set; }
        public int PreliminaryDetentionWeeks { get; set; }
        public int PreliminaryDetentionMonths { get; set; }
        public int PreliminaryDetentionYears { get; set; }

        //Пробация
        public int? ProbationYears { get; set; }
        public int? ProbationMonths { get; set; }
        public int? ProbationDays { get; set; }
        public int? ProbationHours { get; set; }

        public CaisBuletinProbationModel[] Probations { get; set; }

        /// <summary>
        /// Наказанието е лишаване от свобода
        /// </summary>
        public bool YesLOS { get; set; }

        public DateTime DateWrt { get; set; }
    }
    public class CaisBuletinProbationModel
    {
        public int Id { get; set; }
        public string MeasureType { get; set; }
        public string MeasureTypeCode { get; set; }
        public string MeasureTypeName { get; set; }
        public int? MeasureQuantity { get; set; }
        public string MeasureUnitCode { get; set; }
        public string MeasureUnitName { get; set; }

        public DateTime MeasureDate { get; set; }
        public string MeasureStatus { get; set; }

        //Пробация
        public int? ProbationYears { get; set; }
        public int? ProbationMonths { get; set; }
        public int? ProbationWeeks { get; set; }
        public int? ProbationDays { get; set; }

    }
}
