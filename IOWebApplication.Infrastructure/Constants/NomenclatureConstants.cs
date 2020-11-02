// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public static class NomenclatureConstants
    {
        public const string AssemblyQualifiedName = "IOWebApplication.Infrastructure.Data.Models.Nomenclatures.{0}, IOWebApplication.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        public const string CountryBG = "BG";
        public const int NullVal = -1;

        /// <summary>
        /// Неопределено
        /// </summary>
        public const string CountryNA = "AA";

        public const string EkattCitySofiq = "68134";

        public const bool IsAutomationLoadIndex = false;

        public const int VSScourtId = 182;

        public const bool FilterPersonOnNotification = true;

        public class Environments
        {
            public const string Production = "prod";
            public const string Test = "test";
            public const string Development = "dev";
            public const string QA = "qa";
        }

        public class InstitutionTypes
        {
            /// <summary>
            /// Прокуратура
            /// </summary>
            public const int Attourney = 1;

            /// <summary>
            /// ОД на МВР
            /// </summary>
            public const int MVR = 2;

            /// <summary>
            /// Частен съдия изпълнител
            /// </summary>
            public const int Prison = 3;

            /// <summary>
            /// Адвокат
            /// </summary>
            public const int Laweyr = 4;


            /// <summary>
            /// ТД на НАП
            /// </summary>
            public const int NAP = 5;

            /// <summary>
            /// ВСС
            /// </summary>
            public const int Vss = 10;

            /// <summary>
            /// Нотариуси
            /// </summary>
            public const int Notary = 14;

            /// <summary>
            /// Синдици
            /// </summary>
            public const int Syndic = 15;

            /// <summary>
            /// Съд
            /// </summary>
            public const int Courts = 105;

        }

        public class PersonTypes
        {
            public const int Person = 1;
            public const int Entity = 2;
            public static int FromUicType(int uicType)
            {
                switch (uicType)
                {
                    case UicTypes.EIK:
                    case UicTypes.Bulstat:
                        return Entity;
                    default:
                        return Person;
                }
            }
        }
        /// <summary>
        /// Видове страни PersonRole.PersonKindId
        /// </summary>
        public class PersonKinds
        {
            /// <summary>
            /// Лява страна: ищец, въззивник, касатор
            /// </summary>
            public const int LeftSide = 1;

            /// <summary>
            /// Дясна страна: ответник, въззиваем
            /// </summary>
            public const int RightSide = 2;

            /// <summary>
            /// Представляваща страна
            /// </summary>
            public const int Represent = 3;


            public const int Prosecutor = 8;

        }
        public class LawUnitTypes
        {
            public const int Judge = 1;
            public const int Jury = 2;
            /// <summary>
            /// Прокурор
            /// </summary>
            public const int Prosecutor = 3;
            /// <summary>
            /// Вещо лице
            /// </summary>
            public const int Expert = 4;
            public const int OtherEmployee = 5;

            /// <summary>
            /// Призовкар
            /// </summary>
            public const int MessageDeliverer = 6;
            /// <summary>
            /// Адвокат
            /// </summary>
            public const int Lawyer = 7;

            public static int[] EissUserTypes = { Judge, OtherEmployee, MessageDeliverer };
            public static int[] CasePersonSelectables = { Prosecutor, Expert, Lawyer };
            public static int[] NoApointmentPersons = { Prosecutor, Lawyer };
            public static int[] DepartmentPersons = { Prosecutor, Lawyer };
            public static int[] LocalViewOnly = { Judge, OtherEmployee, MessageDeliverer, Jury, };
            public static int[] HasSpecialities = { Jury, Expert };
            public static int[] CanActAsPersons = { Judge, Jury, OtherEmployee, MessageDeliverer, Expert };

        }
        public class UicTypes
        {
            public const int EGN = 1;
            public const int LNCh = 2;
            public const int EIK = 3;
            public const int BirthDate = 4;
            public const int Bulstat = 5;
        }

        public class DepartmentType
        {
            public const int Kolegia = 1;
            public const int Otdelenie = 2;
            public const int Napravlenie = 3;
            public const int Systav = 4;
        }
        public class PeriodTypes
        {
            /// <summary>
            /// Назначен
            /// </summary>
            public const int Appoint = 1;

            /// <summary>
            /// Болен
            /// </summary>
            public const int Ill = 2;

            /// <summary>
            /// Командирован
            /// </summary>
            public const int Move = 3;

            /// <summary>
            /// Отпуска
            /// </summary>
            public const int Holiday = 4;

            /// <summary>
            /// Дежурен
            /// </summary>
            public const int onDuty = 5;

            /// <summary>
            /// Изпълнява длъжността на
            /// </summary>
            public const int ActAs = 6;

            /// <summary>
            /// Статуси за проверка за наличност
            /// </summary>
            public static int[] CurrentlyAvailable = { Appoint, Move };

            /// <summary>
            /// Статуси за проверка за наличност - разширени
            /// </summary>
            public static int[] CurrentlyAvailableExtended = { Appoint, Move, ActAs };
        }

        public class CaseState
        {
            /// <summary>
            /// Чернова
            /// </summary>
            public const int Draft = 1;

            /// <summary>
            /// Новообразувано дело
            /// </summary>
            public const int New = 2;

            /// <summary>
            /// Спряно
            /// </summary>
            public const int Stop = 4;

            /// <summary>
            /// Прекратено
            /// </summary>
            public const int Suspend = 5;

            /// <summary>
            /// Решено
            /// </summary>
            public const int Resolution = 7;

            /// <summary>
            /// Архивирано
            /// </summary>
            public const int Archive = 9;

            /// <summary>
            /// Анулирано
            /// </summary>
            public const int Deleted = 10;

            /// <summary>
            /// Отказ от образуване
            /// </summary>
            public const int Rejected = 11;

            /// <summary>
            /// Унищожено
            /// </summary>
            public const int Destroy = 12;

            /// <summary>
            /// Влязло в сила
            /// </summary>
            public const int ComingIntoForce = 13;

            //не са в таблица. Има ги в разширеното търсене
            public const int WithoutArchive = 1000;
            public const int WithArchive = 2000;

            /// <summary>
            /// Статуси, при които се забранява редакцията на делото
            /// </summary>
            public static int[] DisableEditStates = { Deleted, Destroy };
            public static int[] CanDeleteStates = { New };
            public static int[] AutomatedStates = { Deleted, Destroy, Archive };
        }

        public class CaseGroups
        {
            public const int GrajdanskoDelo = 1;
            public const int NakazatelnoDelo = 2;
            public const int Trade = 3;
            public const int Company = 4;
            public const int Administrative = 5;
        }

        public class CaseCharacters
        {
            public const int PyrvaInstanciaGrajdanskoDelo = 1;
        }

        public class NotificationState
        {
            /// <summary>
            /// Проект
            /// </summary>
            public const int Proekt = 11;

            /// <summary>
            /// Изготвен
            /// </summary>
            public const int Ready = 1;

            /// <summary>
            /// Изпратена
            /// </summary>
            public const int Send = 7;

            /// <summary>
            /// Получен
            /// </summary>
            public const int Received = 2;

            /// <summary>
            /// За връчване
            /// </summary>
            public const int ForDelivery = 4;

            /// <summary>
            /// Доставен
            /// </summary>
            public const int Delivered = 6;


            /// <summary>
            /// Уведомяване по чл. 47 ГПК
            /// </summary>
            public const int Delivered47 = 8;

            /// <summary>
            /// Уведомяване по чл. 50 ГПК
            /// </summary>
            public const int Delivered50 = 9;

            /// <summary>
            /// Уведомяване по чл. 51 ГПК
            /// </summary>
            public const int Delivered51 = 10;

            /// <summary>
            /// Невръчен
            /// </summary>
            public const int UnDelivered = 3;

            /// <summary>
            /// Посетен
            /// </summary>
            public const int Visited = 12;
        }
        public class DeliveryOper
        {
            /// <summary>
            /// Изпратена
            /// </summary>
            public const int Send = 7;

            /// <summary>
            /// Първо посещение
            /// </summary>
            public const int Visit1 = 21;

            /// <summary>
            /// Второ посещение
            /// </summary>
            public const int Visit2 = 22;

            /// <summary>
            /// Трето посещение
            /// </summary>
            public const int Visit3 = 23;

        }

        public class NotificationType
        {
            public const int Subpoena = 1;
            public const int Message = 2;
            public const int GovernmentPaper = 3;
            public const int Notification = 4;
            public static int ToListType(int? notificationTypeId)
            {
                switch (notificationTypeId)
                {
                    case Subpoena:
                        return SourceTypeSelectVM.CaseSessionNotificationList;
                    case Message:
                        return SourceTypeSelectVM.CaseSessionNotificationListMessage;
                    case Notification:
                        return SourceTypeSelectVM.CaseSessionNotificationListNotification;
                    default:
                        return 0;
                }
            }
            public static int FromListType(int? notificationListTypeId)
            {
                switch (notificationListTypeId)
                {
                    case SourceTypeSelectVM.CaseSessionNotificationList:
                        return Subpoena;
                    case SourceTypeSelectVM.CaseSessionNotificationListMessage:
                        return Message;
                    case SourceTypeSelectVM.CaseSessionNotificationListNotification:
                        return Notification;
                    default:
                        return Subpoena;
                }
            }

        }

        public class WorkNotificationType
        {
            public const int ListDelivered = 1;
            public const int UnDeliveredNotification = 2;
            public const int DeliveredNotification = 3;
            public const int DeadLine = 4;
            public const int NewCase = 5;
        }

        /// <summary>
        /// Видове съдилища
        /// </summary>
        public class CourtType
        {
            /// <summary>
            /// Апелативен Специализиран наказателен съд
            /// </summary>
            public const int ApealCriminal = 4;

            /// <summary>
            /// Специализиран наказателен съд
            /// </summary>
            public const int Criminal = 5;

            /// <summary>
            /// Военен съд
            /// </summary>
            public const int Millitary = 7;

            /// <summary>
            /// Военно-апелативен съд
            /// </summary>
            public const int MillitaryApeal = 6;

            /// <summary>
            /// апелативен съд
            /// </summary>
            public const int Apeal = 8;

            public static readonly int[] MillitaryCourts = { Millitary, MillitaryApeal };

            /// <summary>
            /// Административен съд
            /// </summary>
            public const int Аdministrative = 9;

            /// <summary>
            /// Върховен административен съд
            /// </summary>
            public const int VAS = 3;


            /// <summary>
            /// ВКС
            /// </summary>
            public const int VKS = 2;

            /// <summary>
            /// Окръжен
            /// </summary>
            public const int DistrictCourt = 10;

            /// <summary>
            /// Районен
            /// </summary>
            public const int RegionalCourt = 11;

            public static readonly int[] ApealCourts = { MillitaryApeal, ApealCriminal, Apeal };
        }

        public class SessionDocState
        {
            public const int Nerazgledan = 1;
            public const int Razgledan = 2;
            public const int FinalRazgledan = 3;
            public const int Presented = 4;
            public static readonly int[] UsedInSession = { Razgledan, FinalRazgledan };
        }

        public class SessionState
        {
            public const int Nasrocheno = 1;
            public const int Prenasrocheno = 3;
            //отложено
            public const int Cancel = 2;
            public const int Provedeno = 4;
        }

        public class CaseSessionResult
        {
            public const int Investigation = 5;
            public const int S_opredelenie_za_otvod = 26;
            public const int S_razporejdane_za_otvod = 37;
            public const int AnnouncedForResolution = 8;
            public const int OpredeleniePrikluchvane = 30;
            public const int StopedMoveWithSubstantialReason = 31;
            public const int RazporejdanePrikluchvane = 39;
            public const int RefuseHeritage = 49;
            public const int AcceptHeritage = 50;
            public const int ScheduledFirstSession = 48;
        }

        public class SessionActState
        {
            /// <summary>
            /// Проект
            /// </summary>
            public const int Project = 1;

            /// <summary>
            /// Съгласуван
            /// </summary>
            public const int Coordinated = 2;

            /// <summary>
            /// Постановен
            /// </summary>
            public const int Enforced = 3;

            /// <summary>
            /// Влязъл в сила
            /// </summary>
            public const int ComingIntoForce = 4;

            /// <summary>
            /// Обжалван
            /// </summary>
            public const int Appeal = 5;

            /// <summary>
            /// Отменен
            /// </summary>
            public const int Canceled = 6;

            /// <summary>
            /// Неподписан
            /// </summary>
            public const int Registered = 7;

            //Всички статуси, които се броят за постановен
            public static readonly int[] EnforcedStates = { Enforced, ComingIntoForce, Appeal, Canceled };

            /// <summary>
            /// Статуси, при които след подписване акта да влезне в сила
            /// </summary>
            public static readonly int[] StatesToEnforce = { Project, Coordinated, Registered };
        }

        public class ResolutionStates
        {
            public const int New = 1;
            public const int Enforced = 2;
        }

        public class ActCoordinationTypes
        {
            public const int New = 1;
            public const int Accept = 2;
            public const int AcceptWithOpinion = 3;
            public const int DontAccept = 4;
        }

        public class JudgeRole
        {
            public const int JudgeReporter = 1;
            public const int Judge = 2;
            public const int Jury = 3;
            public const int ReserveJudge = 4;
            public const int ReserveJury = 5;
            public const int ExtJudge = 6;
            public const int ExtJury = 7;
            public const int JudgeVAS = 8;
            /// <summary>
            /// Секретар
            /// </summary>
            public const int Secretary = 9;

            /// <summary>
            /// Съдебен помощник
            /// </summary>
            public const int CaseAssistant = 10;
            public const int ReserveJudgeAndJury = 45;
            public static int[] JudgeRolesActiveList = { JudgeReporter, Judge, ExtJudge };
            public static int[] JudgeRolesList = { JudgeReporter, Judge, ReserveJudge, ExtJudge, JudgeVAS };
            public static int[] JudgeRolesListMain = { JudgeReporter, Judge, ExtJudge, JudgeVAS };
            public static int[] JuriRolesList = { Jury, ReserveJury, ExtJury };
            public static int[] ExtRolesList = { ExtJudge, ExtJury };
            public static int[] ReserveRolesList = { ReserveJudge, ReserveJury };
            /// <summary>
            /// Ръчни роли на служители - за добавяне в дело, без разпределение
            /// </summary>
            public static int[] ManualRoles = { Secretary, CaseAssistant };

            //Всички съдии, секретари и помощници по делото
            public static int[] JudgeAndManualRoles = { JudgeReporter, Judge, ReserveJudge, ExtJudge, JudgeVAS, Secretary, CaseAssistant };
        }

        public class SelectionMode
        {
            public const int SelectByGroups = 1;
            public const int ManualSelect = 2;
            public const int SelectByDuty = 3;
        }

        public class SelectionProtokolLawUnitState
        {
            public const int Include = 1;
            public const int Exclude = 2;
            public const int AddedManually = 3;
            public const int Absent = 4;
            public static int[] ActiveState = { Include, AddedManually };
        }
        public class SelectionProtokolSelectyonType
        {
            public const int ByGroup = 1;
            public const int ByDuty = 2;

        }

        public class SelectionProtokolState
        {
            public const int Generated = 1;
            public const int CreatedDoc = 2;
            public const int Signed = 3;

        }

        public class ProcessPriority
        {
            public const int GeneralOrder = 1;
            public const int Short = 2;
            public const int Quick = 3;
        }

        public class CounterTypes
        {
            public const int Document = 1;
            public const int Case = 2;
            public const int SessionAct = 3;
            public const int Notification = 4;
            public const int Evidence = 5;
            public const int CaseArchive = 6;
            public const int Obligation = 7;
            public const int Payment = 8;
            public const int ExpenseOrder = 9;
            public const int DocumentDecision = 10;
            public const int Divorce = 11;
            public const int ExecListCountry = 12;
            public const int ExecListThirdPerson = 13;
            public const int ExchangeDoc = 14;
            public const int DocumentResolution = 15;

            public static int[] OtherCounters = { Document, Case, SessionAct };
        }

        public class CaseMovementType
        {
            public const int ToPerson = 1;
            public const int ToOtdel = 2;
            public const int ToOutStructure = 3;
        }
        public class CaseMigrationSendTo
        {
            public const int Court = 1;
            public const int Institution = 2;

        }
        public class CaseMigrationDirections
        {
            public const int Outgoing = 1;
            public const int Incoming = 2;
            public const int UnionCase = 3;

        }
        public class CaseMigrationTypes
        {
            /// <summary>
            /// Изпращане в равен по степен съд по подсъдност
            /// </summary>
            public const int SendJurisdiction = 1;

            /// <summary>
            /// Изпращане в по висша инстанция
            /// </summary>
            public const int SendNextLevel = 2;

            /// <summary>
            /// Връщане
            /// </summary>
            public const int ReturnCase = 4;

            /// <summary>
            /// Изпращане в подчинен съд по компетентност
            /// </summary>
            public const int SendCompetence = 6;

            /// <summary>
            /// Приемане в равен по степен съд по подсъдност
            /// </summary>
            public const int AcceptJurisdiction = 7;

            /// <summary>
            /// Връщане в подчинен съд с резултат от инстанционна проверка
            /// </summary>
            public const int ReturnCase_AfterComplain = 12;
            public const int AcceptCase_AfterComplain = 13;

            /// <summary>
            /// Връщане в подчинен съд за администриране
            /// </summary>
            public const int ReturnCase_ForAdmin = 14;

            /// <summary>
            /// Приемане от по-висша инстанция за администриране
            /// </summary>
            public const int AcceptCase_ForAdministration = 15;

            /// <summary>
            /// Изпращане за послужване
            /// </summary>
            public const int SendCase_ForUse = 16;

            /// <summary>
            /// Връщане след послужване
            /// </summary>
            public const int ReturnCase_ForUse = 18;

            /// <summary>
            /// Обединяване на дела
            /// </summary>
            public const int CaseUnion = 20;

            /// <summary>
            /// Връщане на делото за дораследване в прокуратурата
            /// </summary>
            public const int SentToProsecutors = 21;

            /// <summary>
            /// Връщане на делото от прокуратурата след доразследване
            /// </summary>
            public const int AcceptProsecutors = 22;

            public static int[] SendCaseTypesCanAccept = { SendCase_ForUse };
            public static int[] ReturnCaseTypes = { ReturnCase_AfterComplain, ReturnCase_ForAdmin, ReturnCase_ForUse };
        }
        public class DeliveryAddressNumberType
        {
            public const int OddNumber = 1;
            public const int EvenNumber = 2;
            public const int OddEvenNumber = 4;
            public const int NumberName = 8;
            public const int Block = 3;
            public const int BlockOdd = 5;
            public const int BlockEven = 6;
            public const int BlockName = 7;
        }

        public class NotificationDeliveryGroup
        {
            public const int WithSummons = 1;
            public const int WithGovernmentPaper = 2;
            public const int WithCourier = 3;
            public const int WithCityHall = 4;
            public const int OnSession = 5;
            public const int OnPhone = 6;
            public const int OnEMail = 7;
            public const int ByEPEP = 8;
            public const int OnMember56 = 9;
            public const int OnMember50 = 10;

            public static bool OnMoment(int? notificationDeliveryGroupId)
            {
                return (
                notificationDeliveryGroupId == OnSession ||
                notificationDeliveryGroupId == OnPhone ||
                notificationDeliveryGroupId == OnEMail ||
                notificationDeliveryGroupId == OnMember50||
                notificationDeliveryGroupId == OnMember56);
            }
            /// <summary>
            /// Видове за които да се генерира DeliveryItem
            /// </summary>
            public static int[] DeliveryGroupForDeliveryItem = { WithSummons, WithCourier, WithCityHall };
        }
        public class DeliveryItemFilterType
        {
            public const int Inner = 1;
            public const int FromOther = 2;
            public const int ToOther = 3;
        }
        public class AddressType
        {
            /// <summary>
            /// Постоянен адрес
            /// </summary>
            public const int Permanent = 1;

            /// <summary>
            /// Настоящ адрес
            /// </summary>
            public const int Current = 2;
            /// <summary>
            /// Съдебен адрес
            /// </summary>
            public const int Court = 3;
            /// <summary>
            /// Седалище и адрес на управление
            /// </summary>
            public const int CompanyPermanent = 4;
            /// <summary>
            /// Месторабота
            /// </summary>
            public const int Work = 6;
        }
        public class PaymentType
        {
            public const int Pos = 1;
            public const int Bank = 2;
            public const int Cash = 3;
        }

        public class NotificationPersonType
        {
            public const int CasePerson = 1;
            public const int CaseLawUnit = 2;
        }

        public class EvidenceType
        {
            public const int Materially = 1;
            public const int Electronically = 2;
        }

        public class EvidenceState
        {
            public const int Destroyed = 5;
        }

        public class TemplateGroup
        {
            public const int Notification = 1;
        }
        public class RegixType
        {
            public const int PersonData = 1;
            public const int PersonPermanentAddress = 2;
            public const int PersonCurrentAddress = 3;
            public const int EmploymentContracts = 4;
            public const int DisabilityCompensationByPaymentPeriod = 5;
            public const int UnemploymentCompensationByPaymentPeriod = 6;
            public const int PensionIncomeAmount = 7;
            public const int PersonalIdentityV2 = 8;
            public const int ActualStateV3 = 9;
            public const int StateOfPlay = 10;
            public const int PersonDataAddress = 11;
        }

        public class JudgeDepartmentRole
        {
            public const int Predsedatel = 1;
            public const int Member = 2;
        }

        public class DismisalType
        {
            public const int Otvod = 1;
            public const int SamoOtvod = 2;
            public const int Prerazpredelqne = 3;
            public const int SmqnaZasedatel = 4;
            public static int[] DismisalList = { Otvod, SamoOtvod };
        }

        public class EvidenceMovementType
        {
            public const int IzprashtaneDrugSyd = 2;
            public const int PoluchavaneDrugSyd = 3;
            public const int Destroyed = 6;
        }

        public class MoneyType
        {
            //Държавни такси
            public const int StateFee = 1;

            //Глоби
            public const int Fine = 2;

            //Транспортни разходи
            public const int Transport = 3;

            //Възнагражденията Бюджетна
            public const int Earnings = 4;

            //Възнагражденията Депозитна
            public const int EarningsDeposit = 20;

            //Гаранция
            public const int Warranty = 6;

            //Депозит за вещо лице
            public const int ExpertDeposit = 7;

            //Депозит за правна помощ
            public const int LegalDeposit = 8;

            //Други суми по сметка Депозити
            public const int OtherDeposit = 9;

            //Видове суми, които да се зареждат при съпровождащ документ
            public static int[] MoneyCompliantDocumentList = { StateFee, Fine, Warranty, ExpertDeposit, LegalDeposit, OtherDeposit };

            //Възнаградения
            public static int[] EarningList = { Earnings, EarningsDeposit };
        }

        public class PersonRole
        {
            //Административно наказващ орган
            public const int AdministrativeMember = 2;

            //Заявител
            public const int Notifier = 14;

            //Ищец
            public const int Plaintiff = 15;

            //Комитет на кредиторите
            public const int KomitetKreditor = 17;

            //Молител
            public const int Petitioner = 20;

            //Нарушител
            public const int Offender = 22;

            //наследник
            public const int Inheritor = 23;

            //наследодател
            public const int Legator = 24;

            //Обвиняем
            public const int Defendant = 27;

            //Ответник
            public const int libellee = 30;

            //Подсъдим
            public const int Prisoner = 35;

            //Прокурор
            public const int Prokuror = 43;

            //Прокурор
            public const int Prosecutor = 43;

            //Синдик
            public const int Sindik = 47;

            //Кредитор
            public const int Kreditor = 59;

            //Длъжник
            public const int Debtor = 60;

            //Фирма
            public const int Company = 70;
        }
        public class RoleKind
        {
            public const int LeftSide = 1;
            public const int RightSide = 2;
        }

        public class SessionMeetingType
        {
            public const int PublicMeeting = 1;
            public const int PrivateMeeting = 2;
        }

        public class CaseMoneyClaimGroup
        {
            public const int Contract = 1;
        }

        public class CaseMoneyCollectionGroup
        {
            public const int Money = 1;
            public const int Property = 2;
            public const int Movables = 3;
        }

        public class AnswerQuestionTextBG
        {
            public const string Yes = "Да";
            public const string No = "Не";
        }

        public class LawUnitPosition
        {
            //Председателя на съда
            public const int GeneralJudge = 1;
        }

        public class CaseBankAccountType
        {
            public const int BankAccount = 1;
            public const int Other = 2;
        }

        public class PriceColTypes
        {

            public const int DataFrom = 1;
            public const int DataTo = 2;
            public const int Value = 3;
            public const int Procent = 4;
            public const int RowKeyword = 5;

            public static Dictionary<int, string> Names
            {
                get
                {
                    Dictionary<int, string> result = new Dictionary<int, string>();
                    result.Add(DataFrom, "Стойност 'ОТ'");
                    result.Add(DataTo, "Стойност 'ДО'");
                    result.Add(Value, "Стойност");
                    result.Add(Procent, "Процент от стойност");
                    result.Add(RowKeyword, "Зона за ред");
                    return result;
                }
            }

            public static string LabelSign(int colType)
            {
                switch (colType)
                {
                    case DataFrom: return "<-";
                    case DataTo: return "->";
                    case Value: return "#";
                    case Procent: return "%";
                    case RowKeyword: return "id";
                    default: return "";
                }
            }
        }

        public class Currency
        {
            public const int BGN = 1;
            public const int EUR = 2;
            public const int USD = 3;
        }

        public class ExpenseOrderState
        {
            public const int StateReady = 1;
            public const int StateConfirm = 2;
            public const int StateDeliver = 3;
            public const int StatePaid = 4;
        }

        public class CaseCodeGroupAlias
        {
            public const string CaseFastProcess = "case_fast_process";
            public const string CaseCompanyRegister = "case_company_register";
        }

        public class CaseClassifications
        {
            public const int Secret = 1;
            public const int Restriction = 2;
            public const int UnderAge = 3;
            public const int CorruptCase = 9;
            public const int DoubleExchangeDoc = 11;

            public static int[] RestictedAccess = { Secret, Restriction };
        }

        public class SelectionProtocolConstants
        {
            public const string NoAvailableJudges = "Няма налични съдии за избор";

        }


        public class PriceDescKeyWord
        {
            public const string KeyMoneyFee = "MONEY_FEE";
            public const string KeyMoneyCase410 = "STATE_FEE410";
            public const string KeyMoneyCase417 = "STATE_FEE417";
            public const string KeyJudgeLoadActivity = "JUDGE_LOAD";
            public const string RowMoneyPercent = "tax_procent";
            public const string RowMoneyMinValue = "min_value";
            public const string ArchivePeriod = "Archive_Period";
        }

        public class CaseCode
        {
            public const int Case410 = 171;
            public const int Case417 = 172;
        }

        public class MoneySign
        {
            public const int SignPlus = 1;
            public const string SignPlusName = "Приход";


            public const int SignMinus = -1;
            public const string SignMinusName = "Разход";
        }

        public class PersonMaturity
        {
            /// <summary>
            /// Пълнолетни
            /// </summary>
            public const int Adult = 1;

            /// <summary>
            /// Малолетни
            /// </summary>
            public const int UnderAged = 3;

            /// <summary>
            /// Непълнолетни
            /// </summary>
            public const int UnderLegalAge = 2;
        }

        public class CaseInstanceType
        {
            public const int FirstInstance = 1;
            public const int SecondInstance = 2;
            public const int ThirdInstance = 3;
        }

        public class CaseTypes
        {
            public const int NOHD = 1;
            public const int NChHD = 2;
            public const int ChND = 3;
            public const int AND = 4;
            public const int VNOHD = 5;
            public const int VNChHD = 6;
            public const int VChND = 7;
            public const int VAND = 8;
            public const int VNR = 9;
            public const int KAND = 13;
            public const int GD = 14;
            public const int ChGD = 15;
            public const int VGD = 16;
            public const int VChGD = 17;
            public const int TD = 18;
            public const int ChTD = 19;
            public const int VTD = 20;
            public const int VChTD = 21;
            public const int FD = 22;

            public static int[] CaseTypeArrested = { NOHD, NChHD, ChND };
        }

        public class LifecycleType
        {
            public const int InProgress = 1;
            public const int Restart = 3;
            public const int Stop = 2;
        }

        public class LinkDirectionType
        {
            public const int Represent = 1;
            public const int RepresentBy = 2;
        }

        public class CaseSessionTypeGroup
        {
            public const int PublicSession = 1;
            public const int PrivateSession = 2;
        }

        public class DocumentGroup
        {
            public const int DocumentForComplain_AccompanyingDocument = 29;
        }

        public class DocumentType
        {
            // Молба по чл.51 от Закона за наследството
            public const int Request51LawInheritance = 14;

            // Молба за отказ от наследство
            public const int RequestAcceptanceInheritance = 15;

            // Молба за отказ от наследство
            public const int RequestRefusalInheritance = 16;

            // Искане по чл.368 НПК
            public const int Request368 = 44;

            //Заявление за регистрация
            public const int ApplicationForCompanyRegister = 91;

            //Заявление за промяна в обстоятелствата
            public const int ApplicationForCompanyChange = 215;

            //Молба (искане) за възобновяване
            public const int RequestForRenewing = 264;
        }

        public class DocumentTypeGroupings
        {
            //Книга по чл. 634в от ТЗ
            public const int Insolvency = 1;

            //Книга за приемане и отказ от наследство Само отказ
            public const int RefuseHeritage = 2;

            //Регистър по чл. 10, ал. 2 от ЗЗДН
            public const int Zzdn = 3;

            //Регистър на издадените европейски удостоверения за наследство
            public const int EuropeanHeritage = 4;

            //Книга за приемане и отказ от наследство Само приемане
            public const int AcceptHeritage = 5;

            //Книга за приемане и отказ от наследство Други
            public const int OtherHeritage = 6;

            //Регистър на издадените европейски удостоверения за наследство - Молба
            public const int RequestEuropeanHeritage = 7;

            //Регистър на заявленията за достъп до обществена информация
            public const int PublicInformation = 8;

            //Закрити заседания 2 инстанция наказателни дела
            public const int PrivateSessionSecondInstanceCriminal = 9;

            //Закрити заседания 2 инстанция граждански дела жалби
            public const int PrivateSessionSecondInstanceCivil = 10;

            //Закрити заседания 2 инстанция граждански дела бавност
            public const int PrivateSessionSlowSecondInstanceCivil = 11;

            //Книга втора инстанция Жалба
            public const int CaseSecondInstanceComplaint = 12;
            //Книга втора инстанция Протест
            public const int CaseSecondInstanceProtest = 13;
            //Книга втора инстанция Жалба и Протест
            public const int CaseSecondInstanceComplaintProtest = 14;

            //Заявление за регистрация на фирма
            public const int RegisterCompany = 15;

            //Статистика ТД и ГД - Жалби
            public const int StatisticsComplaintTDGD = 16;

            //Статистика ТД и ГД - Частни Жалби
            public const int StatisticsPrivateComplaintTDGD = 17;

            //Статистика ТД и ГД - Частни Жалби по 274
            public const int StatisticsPrivateComplaint274TDGD = 18;

            //Статистика ТД и ГД - Молба за определяне срок при бавност 
            public const int StatisticsRequestSlownessTDGD = 19;

            //Статистика НД - Протест
            public const int StatisticsProtestND = 20;

            //Статистика НД - Жалба“/ „Жалба и протест
            public const int StatisticsComplainND = 21;

            //Статистика НД - Частни жалби и протести
            public const int StatisticsPrivateProtestComplainND = 22;

            //Статистика НД - Възобновяване
            public const int StatisticsResumeND = 23;
        }

        public class CaseCodeGroupings
        {
            //Регистър на заявленията за достъп до обществена информация
            public const int PublicInformation = 1;

            //Закрити заседания Шифри първа инстанция наказателни дела
            public const int CaseSessionPrivateReportFirstInstanceCriminal = 2;

            //Закрити заседания Шифри втора инстанция наказателни дела
            public const int CaseSessionPrivateReportSecondInstanceCriminal = 3;

            //Съобщение за прекратен граждански брак
            public const int Divorce = 4;

            //Регистрация на сдружение, фондация, читалище, синдикална и работодателска организация - справка дела
            public const int RegisterAssociation = 5;

            //Регистрация на ЖСК - справка дела
            public const int RegisterJSK = 6;

            //Регистрации на адвокатско дружество - справка дела
            public const int RegisterLawyer = 7;

            //Регистрация на пенсионен фонд /фондове за допълнително  - справка дела
            public const int RegisterPensioner = 8;
        }

        public class DocumentDecisionStates
        {
            /// <summary>
            /// Чернова
            /// </summary>
            public const int Draft = 1;

            /// <summary>
            /// Новообразувано решение
            /// </summary>
            public const int New = 2;

            /// <summary>
            /// Решено
            /// </summary>
            public const int Resolution = 3;
        }
        public class DeadlineType
        {
            public const int DeclaredForResolve = 1;
            public const int OpenSessionResult = 2;
            public const int Motive = 3;
            public const int CompanyCaseRegister = 4;
            public const int CompanyCaseChange = 5;
        }
        public class DeadlineGroup
        {
            public const int ForJudge = 1;
        }

        public class SessionResultGroupings
        {
            //Закрити заседания Решени/Прекратени наказателни дела
            public const int CaseSessionReportFirstInstanceCriminal = 1;

            //Закрити заседания Решени/Прекратени граждански дела
            public const int CaseSessionReportFirstInstanceCivil = 2;

            //РЕГИСТЪР ПО ЧЛ.39 Т.13 ОТ ПАС - прекратено производство
            public const int CaseMigrationReturnReport_FinishCase = 3;

            //Описна Книга Въззивна инстанция - прекратени
            public const int CaseSecondInstanceStop = 4;

            //Книга за приемане и отказ от наследство
            public const int HeritageReport_Result = 5;

            // Дела с ненаписани съдебни актове към [дата] от всички съдии
            public const int CaseWithoutFinalAct_Result = 6;

            // Справка влезли в сила присъди
            public const int SentenceListReport_Result = 7;

            //Прекратени за статистиката
            public const int StatisticsStopCase = 8;

            //Статистика - прекратени по спогодба
            public const int StatisticsStopCaseAgreement = 9;

            //Статистика - прекратени по други причини
            public const int StatisticsStopCaseOtherReason = 10;

            //Статистика Отлагания на дела в открити заседания Граждански
            public const int StatisticsCaseDelayGD = 11;

            //Статистика В първо по делото заседание и помирително
            public const int StatisticsCaseDelayFirstSessionGD = 12;

            //Статистика Наказателни дела Прекратени и споразумения
            public const int StatisticsCaseStopND = 13;

            //Статистика Наказателни дела в т.ч. свърш.споразум.- чл.381-384
            public const int StatisticsCaseStop381ND = 14;

            //Статистика Отлагания на дела в открити заседания Наказателни
            public const int StatisticsCaseDelayND = 15;
        }

        public class SessionResultBaseGroupings
        {
            //РЕГИСТЪР ПО ЧЛ.39 Т.13 ОТ ПАС - прекратено производство
            public const int CaseMigrationReturnReport_FinishCaseBase = 1;
        }

        public class IntegrationTypes
        {
            public const int EISPP = 1;
            public const int EPEP = 2;
            /// <summary>
            /// ЦУБИПСА
            /// </summary>
            public const int LegalActs = 3;
            /// <summary>
            /// Производства по несъстоятелност
            /// </summary>
            public const int ISPN = 4;

            /// <summary>
            /// ЦСРД
            /// </summary>
            public const int CSRD = 5;
        }

        public class SentenceType_Select
        {
            public const int NoChoice = -1;
            public const int AllChoice = 3;
            public const int HasMoney = 2;
            public const int HasPeriod = 1;
        }

        public class ActResults
        {
            //Потвърдено изцяло
            public const int AcceptAll = 1;

            //Частично
            public const int AcceptNotAll = 2;

            //Отменено и постановено ново
            public const int CancelAndNew = 3;

            //Отменено и върнато
            public const int CancelAndReturn = 4;

            //Обезсилено
            public const int MakeNull = 5;
        }

        public class ExecListTypes
        {
            //В полза на държавата
            public const int Country = 1;

            //В полза на трети лица
            public const int ThirdPerson = 2;
        }

        public class ActType
        {
            public const int Protokol = 1;
            public const int Sentence = 10;
            public const int ExecListPrivatePerson = 12;
            // Решение
            public const int Answer = 2;
            public const int Definition = 3;
            public const int Injunction = 4;
            // Заповед за защита
            public const int CommandmentProtection = 8;
            // Заповед за незабавна защита
            public const int CommandmentimmediatelyProtection = 7;

            public static int[] WithMotives = { Answer, Sentence, Definition };
        }
        public class ActFormatType
        {
            public const string Protokol = "protokol";
            public const string Act = "act";
        }

        public class MoneyGroups
        {
            public const int Deposit = 1;
            public const int Budget = 2;
        }

        public class ActComplainResultGroupings
        {
            //Присъдата потвърдена
            public const int AcceptSentence = 1;

            //Приложен ч.л.66 НК
            public const int Applied66 = 2;

            //Отменен ч.л.66 НК
            public const int Cancel66 = 3;

            //Наказанието намалено
            public const int SentenceDown = 4;

            //Наказанието увеличено
            public const int SentenceUp = 5;

            //С др. промени в наказ. част
            public const int ChangeCriminalPart = 6;

            //С промяна в гражд. част
            public const int ChangeCivilPart = 7;

            //Прис. отменена отчасти с вр. за ново разгл.
            public const int AppliedNew = 8;

            //С връщане за ново разгл
            public const int ReturnNew = 9;

            //Произнасяне на нова присъда
            public const int SentenceNew = 10;

            //Прекратено
            public const int CaseStop = 11;

            public static int[] SecondInstanceReport = { AcceptSentence, Applied66, Cancel66, SentenceDown, SentenceUp, ChangeCriminalPart,
                           ChangeCivilPart, AppliedNew, ReturnNew, SentenceNew, CaseStop};

            //Потвърдено изцяло
            public const int AcceptAll = 12;

            //Частично
            public const int AcceptNotAll = 13;

            //Отменено и постановено ново
            public const int CancelAndNew = 14;

            //Отменено и върнато
            public const int CancelAndReturn = 15;

            //Обезсилено
            public const int MakeNull = 16;

            //Прекратено за ненаказателните
            public const int CaseStopNonCriminal = 17;

            public static int[] SecondInstanceNonCriminalReport = { AcceptAll, AcceptNotAll, CancelAndNew, CancelAndReturn,
                                            MakeNull, CaseStopNonCriminal};

            //Прекратени за статистиката
            public const int StatisticsCaseStop = 18;

            //Статистика Прекратява делото - граждански дела
            public const int StatisticsCaseStopGD = 19;
        }

        public class ActResultGroups
        {
            //Потвърдено изцяло
            public const int AcceptAll = 1;

            //Частично
            public const int AcceptNotAll = 2;

            //Отменено и постановено ново
            public const int CancelAndNew = 3;

            //Отменено и върнато
            public const int CancelAndReturn = 4;

            public static int[] SecondInstanceReport = { AcceptAll, AcceptNotAll, CancelAndNew, CancelAndReturn };

            //Потвърдено изцяло
            public const int AcceptAllNonCriminal = 5;

            //Частично
            public const int AcceptNotAllNonCriminal = 6;

            //Отменено и постановено ново
            public const int CancelAndNewNonCriminal = 7;

            //Отменено и върнато
            public const int CancelAndReturnNonCriminal = 8;

            public static int[] SecondInstanceReportNonCriminal = { AcceptAllNonCriminal, AcceptNotAllNonCriminal,
                             CancelAndNewNonCriminal, CancelAndReturnNonCriminal};
        }

        public class HtmlTemplateAlias
        {
            /// <summary>
            /// Писмата, които се пускат за наследство
            /// </summary>
            public static string[] HeritageLetters = { "CERTLEG", "CERTLEG_", "CERTLEG__" };

        }
        public class EMailMessageState
        {
            public const int ForSend = 1;
            public const int Pending = 2;
            public const int Sended = 3;
        }

        public class EkStreetTypes
        {
            public const int Street = 1;
            public const int Area = 2;
        }

        public class CaseSessionResultGroups
        {
            //Прекратено
            public const int Suspended = 1;
            public const int Stop = 3;
            public const int Procrastination = 2;
        }

        public class RecidiveTypes
        {
            public const int General = 1;
            public const int Danger = 2;
            public const int Special = 3;
            public const int None = 4;

            public static int[] Recidives = { General, Danger, Special };
        }

        public class DeliveryReason
        {
            public const int Other = 7;
        }

        public class SessionType
        {
            // Закрито заседание
            public const int ClosedSession = 3;

            // Разпоредително заседание
            public const int DispositionalSession = 4;
        }

        public class ComplainState
        {
            // Постъпила
            public const int Recived = 1;
        }

        public class CodeMappingAlias
        {
            public const string CasePersonCrimeRole = "eispp_person_in_crime_role";
        }

        public class SessionToDateLabel
        {
            public const string SessionTo1MonthLabel = "До 1 месец";
            public const string SessionTo2MonthLabel = "До 2 месеца";
            public const string SessionTo3MonthLabel = "До 3 месеца";
            public const string SessionUp3MonthLabel = "Над 3 месеца";
            public const string NoSessionLabel = "Без насрочване";
        }

        public class SessionToDateValue
        {
            public const int SessionTo1MonthValue = 1;
            public const int SessionTo2MonthValue = 2;
            public const int SessionTo3MonthValue = 3;
            public const int SessionUp3MonthValue = 4;
            public const int NoSessionValue = 5;
        }

        public class ActToDateLabel
        {
            public const string ActDateTo1MonthLabel = "До 1 месец";
            public const string ActDateTo2MonthLabel = "До 2 месеца";
            public const string SessionTo3MonthLabel = "До 3 месеца";
            public const string ActDateToUp3MonthLabel = "Над 3 месеца";
        }

        public class ActToDateValue
        {
            public const int ActDateTo1MonthValue = 1;
            public const int ActDateTo2MonthValue = 2;
            public const int ActDateTo3MonthValue = 3;
            public const int ActDateToUp3MonthValue = 4;
        }

        public class MoneyCollectionEndDateType
        {
            public const int WithDate = 1;
            public const int Nothing = 2;
        }

        public class CaseMoneyCollectionType
        {
            public const int Other = 2;
        }

        public class CaseMoneyCollectionKind
        {
            public const int Other = 3;
        }

        public class PersonRoleGroupings
        {
            public const int RoleArrested = 1;

            //За Справка съдени и осъдени лица
            public const int RoleCasePersonDedendantList = 2;
        }

        public class HtmlTemplateTypes
        {
            public const int Letter = 9;

            //удостоверение
            public const int Certificate = 24;
        }

        /// <summary>
        /// Константи за Източник на постъпване на делото
        /// </summary>
        public class CaseCreateFroms
        {
            //Новообразувано
            public const int New = 1;

            //По подсъдност
            public const int Jurisdiction = 2;

            //За ново разглеждане
            public const int NewNumber = 3;

            //Да продължи под същия номер
            public const int OldNumber = 4;

            //След връщане за доразследване
            public const int Prosecutors = 5;
        }

        /// <summary>
        /// Стойности на вид съдържание
        /// </summary>
        public class ContentTypes
        {
            public const string Html = "text/html";
            public const string Pdf = "application/pdf";
        }

        //ABCDEFGHIJKLMNOPQRSTUVWXYZ
        public static readonly Dictionary<char, char> VisualLetterEnBg = new Dictionary<char, char>()
        {
            { 'A', 'А' },
            { 'B', 'В' },
            { 'C', 'С' },
            { 'E', 'Е' },
            { 'H', 'Н' },
            { 'K', 'К' },
            { 'M', 'М' },
            { 'O', 'О' },
            { 'P', 'Р' },
            { 'T', 'Т' },
            { 'X', 'Х' },
            { 'Y', 'У' },
        };

        /// <summary>
        /// Константи за тип на реда - код или обект
        /// </summary>
        public class RegixMapActualStateTypeField
        {
            public const string FieldObject = "field_object";
            public const string FieldCode = "field_code";
        }

        public class SentenceResultTypes
        {
            //Оправдан
            public const int Justified = 2;

            //Осъден по споразумение
            public const int ConvictAgreement = 3;
        }

        public class SentenceTypes
        {
            //Лишаване от свобода - условно
            public const int ImprisonmentConditional = 1;

            //Лишаване от свобода - ефективно
            public const int ImprisonmentEffectively = 2;

            //Глоба
            public const int Fine = 4;

            //Доживотен затвор
            public const int LifeSentence = 5;

            //Доживотен затвор без замяна
            public const int LifeSentenceNoChange = 6;

            //Пробация
            public const int Probation = 7;

            //Поправителен труд
            public const int CorrectiveWork = 11;

            //Задължително заселване
            public const int Settlement = 12;

            //Обществено порицание
            public const int Reprimand = 8;

            //Лишаване от права
            public const int DeprivationOfRights = 9;

            //Други наказания
            public const int Other = 10;

            //Други наказания – условно
            public const int OtherConditional = 14;

            //ТВУ – само за непълнолетни
            public const int TVU = 15;

            //Не се наказва
            public const int NotPunished = 13;
        }

        public class SentenceLawbases
        {
            //чл.25 НК
            public const int LawBase25 = 3;
        }
    }
}