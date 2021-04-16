using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SourceTypeSelectVM
    {
        public int SourceType { get; set; }
        public long SourceId { get; set; }
        public bool CanChange { get; set; }
        public bool TaskRequired { get; set; }

        public SourceTypeSelectVM()
        {

        }
        public SourceTypeSelectVM(int sourceType, long sourceId, bool canChange = true)
        {
            this.SourceType = sourceType;
            this.SourceId = sourceId;
            this.CanChange = canChange;
        }

        public const int Document = 1;
        public const int DocumentPdf = 102;
        public const int CaseMigration = 10;
        public const int CaseMovement = 11;
        public const int DocumentTemplate = 12;
        public const int CaseLifecycle = 13;
        public const int CaseEvidence = 14;
        public const int CaseClassification = 15;
        public const int CaseMoney = 16;

        public const int DocumentObligation = 103;
        public const int DocumentResolution = 104;
        public const int DocumentResolutionBlank = 1041;
        public const int DocumentResolutionPdf = 1042;
        public const int Case = 2;
        public const int CaseInforcedDate = 20021;
        public const int CaseSelectionProtokol = 20;
        public const int CaseSelectionProtokolFile = 20020;
        public const int CaseInDoc = 21;
        public const int DocumentDecision = 22;
        public const int CaseSessionActDivorce = 23;
        //Изпълнителен лист
        public const int ExecList = 24;
        public const int DocumentPerson = 25;
        public const int LawUnit = 26;
        //Плащане
        public const int Payment = 27;
        //Бюлетин за съдимост
        public const int CasePersonBulletin = 28;
        //протоколи за ИЛ
        public const int ExchangeDoc = 29;

        public const int CaseFastProcess = 200;
        public const int CaseBankAccount = 201;
        public const int CaseMoneyClaim = 202;
        public const int CaseMoneyCollection = 203;
        public const int CaseMoneyExpense = 204;
        public const int CaseSession = 3;
        public const int CaseLoadIndex = 30;
        public const int CaseLoadCorrection = 31;
        public const int CaseCrime = 32;
        public const int CasePersonCrime = 33;
        public const int CasePersonSentence = 34;
        public const int CasePersonSentencePunishment = 35;
        public const int CasePersonSentencePunishmentCrime = 36;
        public const int CasePersonInheritance = 37;
        public const int CasePersonMeasure = 38;
        public const int CasePersonDocument = 39;
        public const int CaseSessionNotificationList = 301;
        public const int CaseSessionNotificationListMessage = 315;
        public const int CaseSessionNotificationListNotification = 316;
        public const int CaseSessionNotificationListPerson = 306;
        public const int CaseSessionNotificationListLawUnit = 307;
        public const int CaseSessionNotificationListPersonLawUnit = 308;
        public const int CaseSessionResult = 302;
        public const int CaseSessionMeeting = 303;
        public const int CaseSessionMeetingUser = 304;
        public const int CaseSessionDoc = 310;
        public const int SessionObligation = 311;
        public const int SessionActObligation = 312;
        public const int CaseSessionFastDocument = 313;
        public const int CaseDeadLine = 305;
        public const int CasePerson = 4;
        public const int CaseSessionActCompany = 40;
        public const int RegixReport = 41;
        public const int CaseDeactivate = 42;
        public const int ExpenseOrder = 43;
        public const int ExecListBlank = 44;
        public const int CasePersonAddress = 401;
        public const int CaseSessionPerson = 402;
        public const int CasePersonLink = 403;
        public const int CaseLawUnit = 5;
        public const int CaseSessionLawUnit = 502;
        public const int CaseLawUnitDismisal = 501;
        public const int CaseLawUnitDismisalList = 503;
        public const int CaseLawUnitTaskChange = 504;
        public const int CaseLawUnitManualJudge = 505;
        public const int CaseNotification = 6;
        public const int CaseNotificationReturn = 601;
        public const int CaseNotificationPrint = 602;
        public const int CaseSessionNotification = 603;
        public const int CaseSessionActNotification = 604;
        public const int CaseSessionAct = 7;
        public const int CaseSessionActAllFiles = 7999;
        public const int CaseSessionActAutomatic = 72;
        public const int CaseSessionActManualUpload = 71;
        public const int CaseSessionActBlank = 701;
        public const int CaseSessionActBlankComplete = 7011;
        public const int CaseSessionActPdf = 702;
        public const int CaseSessionActDepersonalizedBlank = 703;
        public const int CaseSessionActDepersonalized = 704;
        public const int CaseSessionActPreparator = 770;
        public const int CaseSessionActLawBase = 705;
        public const int CaseSessionActComplain = 706;
        public const int CaseSessionActComplainResult = 707;
        public const int CaseSessionActComplainPerson = 709;
        public const int CaseSessionActDoc = 708;
        public const int Court = 70;
        public const int CourtDepartment = 71;
        public const int CaseEvidenceMovement = 14000;
        public const int CaseSessionActMotive = 8;
        public const int CaseSessionActMotiveBlank = 801;
        public const int CaseSessionActMotivePdf = 802;
        public const int CaseSessionActMotiveDepersonalizedBlank = 803;
        public const int CaseSessionActMotiveDepersonalized = 804;
        public const int Instutution = 80;
        public const int ExcelReportTemplate = 850;
        public const int CaseSessionActCoordination = 9;
        public const int CaseSessionActCoordinationPdf = 902;
        public const int TemporaryFile = 99;
        public const int WorkNotification = 919;


        public const string InstututionPrefix = "inst";
        public const string LawUnitPrefix = "lawu";
        public const string CourtPrefix = "court";
        public const string CasePersonPrefix = "caseperson";

        public const int Integration_EISPP = 10001;
        public const int Integration_EISPP_CardNP = 10002;
        public const int Integration_EISPP_Package = 10003;
        public const int Integration_EISPP_Response = 10004;
        public const int Integration_EISPP_CardTHN = 10005;
        public const int EisppPerson = 10100;
        public const int EpepUser = 11001;
        public const int EpepUserAssignment = 11002;
        public const int Files = 20000;
        public const int MobileApp = 19000;
        public const int Integration_ISPN = 10050;

        public static string GetSourceTypeName(int sourceType)
        {
            switch (sourceType)
            {
                case Document: return "Документ";
                case Case: return "Дело";
                case CaseFastProcess: return "Заповедни производства";
                case CaseBankAccount: return "Начин на плащане/изпълнение, Заповедни производства";
                case CaseMoneyClaim: return "Обстоятелства по заповедни производства";
                case CaseMoneyCollection: return "Вземане към обстоятелство по заповедни производства";
                case CaseMoneyExpense: return "Претендиран разноски по заповедни производства";
                case CaseSession: return "Заседание";
                case CaseSessionDoc: return "Съпровождащи документи";
                case CasePerson: return "Страна по дело";
                case CasePersonAddress: return "Адрес на страна по дело";
                case CaseSessionPerson: return "Страна по заседание";
                case CaseLawUnit: return "Състав по дело";
                case CaseSessionLawUnit: return "Състав по заседание";
                case CaseNotification: return "Уведомление";
                case CaseSessionNotification: return "Уведомление към заседание";
                case CaseSessionActNotification: return "Уведомление към акт";
                case CaseSessionAct: return "Съдебен акт";
                case CaseSessionActPreparator: return "Изготвил акта";
                case CaseSessionActMotive: return "Мотиви към съдебен акт";
                case CaseSessionActCoordination: return "Съгласуване на съдебен акт";
                case CaseSessionFastDocument: return "Съпровождащ документ представен в заседание";
                case CaseSelectionProtokol: return "Протокол за разпределяне";
                case CaseMigration: return "Движение на дело";
                case SessionObligation: return "Суми заседатели";
                case SessionActObligation: return "Суми по акт";
                case Court: return "Съд";
                case Instutution: return "Институция";
                case CaseMovement: return "Местоположение";
                case DocumentTemplate: return "Изходящи документи";
                case CaseLifecycle: return "Интервали";
                case CaseLawUnitDismisal: return "Преразпределение";
                case CaseLawUnitDismisalList: return "Преразпределение мъм дело";
                case CaseEvidence: return "Доказателства";
                case CaseEvidenceMovement: return "Доказателства движение";
                case CaseClassification: return "Индикатори";
                case CaseMoney: return "Суми";
                case CaseSessionResult: return "Резултат от заседание";
                case CaseSessionMeeting: return "Сесия/тайно съвещание";
                case CaseSessionMeetingUser: return "Секретари";
                case CaseInDoc: return "Съпровождащи документи";
                case DocumentDecision: return "Решение";
                case CaseSessionActDivorce: return "Прекратяване на граждански брак";
                case ExecList: return "Изпълнителен лист";
                case ExecListBlank: return "Изпълнителен лист";
                case CaseLoadIndex: return "Натовареност по дело";
                case CaseLoadCorrection: return "Коригиращи коефициенти по дело";
                case Payment: return "Плащане";
                case ExchangeDoc: return "Протокол";
                case CaseCrime: return "Престъпление";
                case CasePersonCrime: return "Лице към престъпление";
                case CasePersonLink: return "Връзки между лица";
                case CasePersonSentence: return "Присъда";
                case CasePersonSentencePunishment: return "Наложено наказание";
                case CasePersonSentencePunishmentCrime: return "Участие в престъпление";
                case CasePersonInheritance: return "Наследство";
                case CasePersonBulletin: return "Бюлетин за съдимост";
                case CaseSessionActLawBase: return "Нормативната текстове към акт";
                case CaseSessionActComplain: return "Обжалване на акт";
                case CaseSessionActComplainResult: return "Резултат от обжалване";
                case CaseSessionActComplainPerson: return "Обжалване на акт - жалбоподатели";
                case CaseDeadLine: return "Срокове";
                case CaseSessionNotificationList: return "Списък за призоваване";
                case CaseSessionNotificationListLawUnit: return "Списък за призоваване от съдебен състав";
                case CaseSessionNotificationListPerson: return "Списък за призоваване от страни";
                case CaseSessionNotificationListPersonLawUnit: return "Списък за призоваване";
                case CasePersonDocument: return "Лични документи на лице по дело";
                case CasePersonMeasure: return "Мерки към лице по НД";
                case DocumentObligation: return "Суми към документ";
                case CaseSessionActCompany: return "Заявление за регистрация";
                default: return "";
            }
        }

        public static string GetSourceTypePrefix(int sourceType)
        {
            switch (sourceType)
            {
                case Court: return CourtPrefix;
                case Instutution: return InstututionPrefix;
                default: return "";
            }
        }

        public static (int sourceType, long sourceId) GetSourceTypeSourceId(string id)
        {
            int sourceType = 0;
            long sourceId = 0;
            if (id.Contains(CourtPrefix))
            {
                sourceType = Court;
                sourceId = long.Parse(id.Replace(CourtPrefix, ""));
            }
            else if (id.Contains(InstututionPrefix))
            {
                sourceType = Instutution;
                sourceId = long.Parse(id.Replace(InstututionPrefix, ""));
            }
            else if (id.Contains(LawUnitPrefix))
            {
                sourceType = LawUnit;
                sourceId = long.Parse(id.Replace(LawUnitPrefix, ""));
            }
            else if (id.Contains(CasePersonPrefix))
            {
                sourceType = CasePerson;
                sourceId = long.Parse(id.Replace(CasePersonPrefix, ""));
            }
            return (sourceType: sourceType, sourceId: sourceId);
        }

        /// <summary>
        /// Попълване на SourceType за DropDown
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetDDL_SourceType()
        {
            var selectListItems = new List<SelectListItem>();

            selectListItems.Add(new SelectListItem() { Text = "Избери", Value = "-1" });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(Case), Value = Case.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CaseSessionAct), Value = CaseSessionAct.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CaseNotification), Value = CaseNotification.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CaseMigration), Value = CaseMigration.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CaseSessionActDivorce), Value = CaseSessionActDivorce.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(ExecList), Value = ExecList.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CasePersonBulletin), Value = CasePersonBulletin.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(CasePersonSentence), Value = CasePersonSentence.ToString() });
            selectListItems.Add(new SelectListItem() { Text = GetSourceTypeName(ExchangeDoc), Value = ExchangeDoc.ToString() });

            return selectListItems;
        }
    }
}
