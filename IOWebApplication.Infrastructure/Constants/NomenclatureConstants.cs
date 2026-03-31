using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Constants
{
    public static class NomenclatureConstants
    {
        public const string AssemblyQualifiedName = "IOWebApplication.Infrastructure.Data.Models.Nomenclatures.{0}, IOWebApplication.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        public const string CountryBG = "BG";
        public const int CountryBGID = 24;
        public const int NullVal = -1;
        public const string NullText = "-1";

        /// <summary>
        /// Неопределено
        /// </summary>
        public const string CountryNA = "AA";

        public const string EkattCitySofiq = "68134";

        public const bool IsAutomationLoadIndex = false;

        /// <summary>
        /// id На Common_Court ВКС
        /// </summary>
        public const int VKScourtId = 39;

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
            /// ЧСИ
            /// </summary>
            public const int CHSI = 6;

            /// <summary>
            /// ДСИ
            /// </summary>
            public const int DSI = 7;


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


            /// <summary>
            /// Съдия по вписванията
            /// </summary>
            public const int JudgeRegistry = 22;

            public static int[] StatisticsFromInstitution = { CHSI, DSI, JudgeRegistry };

            /// <summary>
            /// Организации, задължени да подават през ЕПЕП
            /// </summary>
            public const int OrgEpepOnly = 23;

        }

        public class InstitutionCaseTypes
        {
            /// <summary>
            /// Изпълнително дело
            /// </summary>
            public const int ExecutiveCase = 14;

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
            public static int[] ListLeftRightSide = { LeftSide, RightSide };
        }
        public class LawUnitTypes
        {
            public const int Judge = 1;

            /// <summary>
            /// Заседател
            /// </summary>
            public const int Jury = 2;
            /// <summary>
            /// Прокурор
            /// </summary>
            public const int Prosecutor = 3;
            /// <summary>
            /// Вещо лице
            /// </summary>
            public const int Expert = 4;

            /// <summary>
            /// Служител
            /// </summary>
            public const int OtherEmployee = 5;

            /// <summary>
            /// Призовкар
            /// </summary>
            public const int MessageDeliverer = 6;
            /// <summary>
            /// Адвокат
            /// </summary>
            public const int Lawyer = 7;

            public static int[] EissUserTypes = { Judge, Jury, OtherEmployee, MessageDeliverer };
            public static int[] CasePersonSelectables = { Prosecutor, Expert, Lawyer };
            public static int[] NoApointmentPersons = { Prosecutor, Lawyer };
            public static int[] DepartmentPersons = { Prosecutor, Lawyer };
            public static int[] LocalViewOnly = { Judge, OtherEmployee, MessageDeliverer, Jury, };
            public static int[] HasSpecialities = { Jury, Expert };
            public static int[] CanActAsPersons = { Judge, Jury, OtherEmployee, MessageDeliverer, Expert };
            public static int[] CaseSelectable = { Judge, Jury };
            public static int[] SpecialAccess = { Judge, OtherEmployee };
            public static int[] ChangeableTypes = { Judge, Jury, MessageDeliverer, OtherEmployee };
            public static int[] WorkTaskCreators = { Judge, OtherEmployee };

        }
        public class LawUnitSelectMode
        {
            public const string All = "all";
            /// <summary>
            /// Назначени/командировани текущо в съда
            /// </summary>
            public const string Current = "current";

            /// <summary>
            /// Назначени/командировани текущо в съда или някога са били - за стари дела
            /// </summary>
            public const string CurrentWithHistory = "history";

            /// <summary>
            /// Назначени/командировани текущо в съда или някога са били - за стари дела без смесени състави на ВАС
            /// </summary>
            public const string CurrentWithHistoryNoVAS = "history_novas";
            /// <summary>
            /// Назначени/командировани някога в съда - за стари дела без смесени състави на ВАС, както и неактивни вече
            /// </summary>
            public const string AllWithHistoryNoVAS = "all_history_novas";
        }

        public class UicTypes
        {
            public const int EGN = 1;
            public const int LNCh = 2;
            public const int EIK = 3;
            public const int BirthDate = 4;
            public const int Bulstat = 5;
            public const int LN = 6;

            public static int[] PersonTypes = { EGN, LNCh, LN, BirthDate };
        }

        public class DepartmentType
        {
            public const int Kolegia = 1;
            public const int Otdelenie = 2;
            public const int Napravlenie = 3;
            public const int Systav = 4;
            public static int[] RealJudgeDepartments = { Otdelenie, Systav };
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
            /// Смесен състав
            /// </summary>
            public const int MixedLawUnit = 7;

            /// <summary>
            /// Статуси за проверка за наличност
            /// </summary>
            public static int[] CurrentlyAvailable = { Appoint, Move, MixedLawUnit };

            /// <summary>
            /// Статуси за проверка за наличност - фактическо наличие в съда
            /// </summary>
            public static int[] CurrentlyCourtActions = { Appoint, Move };

            /// <summary>
            /// Статуси за проверка за наличност - разширени
            /// </summary>
            public static int[] CurrentlyAvailableExtended = { Appoint, Move, ActAs, MixedLawUnit };

            /// <summary>
            /// Статуси за проверка за наличност - разширени - без ВАС
            /// </summary>
            public static int[] CurrentlyAvailableExtendedNoVas = { Appoint, Move, ActAs };

            /// <summary>
            /// Видове периоди, при които не може да се сортива в списъка, заради повторенията и разместването на втора страница
            /// </summary>
            public static int[] DisableSortingOnList = { Ill, Holiday };
            public static int[] ExcludeForSelection = { Ill, Holiday, Move };
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
            /// Обявено за решаване
            /// </summary>
            public const int AnnouncedForResolution = 6;

            /// <summary>
            /// Решено
            /// </summary>
            public const int Resolution = 7;

            /// <summary>
            /// Обжалвано
            /// </summary>
            public const int Appealed = 8;

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

            /// <summary>
            /// Върнато за администриране
            /// </summary>
            public const int ReturnForAdministration = 14;

            //не са в таблица. Има ги в разширеното търсене
            public const int WithoutArchive = 1000;
            public const int WithArchive = 2000;

            /// <summary>
            /// Статуси, при които се забранява редакцията на делото
            /// </summary>
            public static int[] DisableEditStates = { Deleted, Destroy };
            public static int[] CanDeleteStates = { New };
            public static int[] AutomatedStates = { Deleted, Destroy, Archive };
            public static int[] CantChangeSelection = { Draft, Deleted, Destroy, Rejected, ReturnForAdministration };

            //Необразувани дела, които се администрират 
            public static int[] UnregisteredManageble = { Rejected, ReturnForAdministration };
            public static int[] FakeCase = { Rejected, ReturnForAdministration, Draft };
        }

        /// <summary>
        /// константи за група на дело
        /// </summary>
        public class CaseGroups
        {
            /// <summary>
            /// Гражданско
            /// </summary>
            public const int GrajdanskoDelo = 1;

            /// <summary>
            /// Наказателно
            /// </summary>
            public const int NakazatelnoDelo = 2;

            /// <summary>
            /// Търговско
            /// </summary>
            public const int Trade = 3;

            /// <summary>
            /// Фирменто
            /// </summary>
            public const int Company = 4;

            /// <summary>
            /// Административно
            /// </summary>
            public const int Administrative = 5;

            /// <summary>
            /// Списък с дела за справка свършени дела за период – първоинстанционни дела
            /// </summary>
            public static int[] GrajdanskoTradeDelo = { GrajdanskoDelo, Trade };

            /// <summary>
            /// Списък с дела за справка свършени дела за период – първоинстанционни дела
            /// </summary>
            public static int[] GrajdanskoTradeNakazatelnoDelo = { NakazatelnoDelo, GrajdanskoDelo, Trade };
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

            /// <summary>
            /// UnDeliveredMail
            /// </summary>
            public const int UnDeliveredMail = 13;

            /// <summary>
            /// ненасочен
            /// </summary>
            public const int NoDeliveryArea = 66;
            /// <summary>
            /// Всички статус в приемане
            /// </summary>
            public const int AllForReceived = 77;

            public static int[] NotificationDelivered()
            {
                return new int[] {
                     Delivered,
                     //Delivered47,
                     //Delivered50,
                     //Delivered51
                };
            }
            public static int[] NotificationEndState()
            {
                return new int[] {
                     Delivered,
                     //Delivered47,
                     //Delivered50,
                     //Delivered51,
                     UnDelivered
                };
            }

            public static int[] NotificationEndState475051()
            {
                return new int[] {
                     Delivered,
                     Delivered47,
                     Delivered50,
                     Delivered51,
                     UnDelivered
                };
            }
            public static int[] NotificationForDelivery()
            {
                return new int[] {
                     Visited,
                     Delivered47,
                     Delivered50,
                     Delivered51,
                     ForDelivery
                };
            }

            public static int[] NotificationForVisit()
            {
                return new int[] {
                     Visited,
                     Delivered47,
                     Delivered50,
                     Delivered51,
                };
            }
            public static int[] NotificationStateEndAndVisited()
            {
                return new int[] {
                     Delivered,
                     Delivered47,
                     Delivered50,
                     Delivered51,
                     UnDelivered,
                     Visited
                };
            }
            public static int[] NotificationEndStateAndUdeliveredMail()
            {
                return new int[] {
                     Delivered,
                     Delivered47,
                     Delivered50,
                     Delivered51,
                     UnDelivered,
                     Visited,
                     UnDeliveredMail
                };
            }
            public static int[] NotificationStateStuckMessage()
            {
                return new int[] {
                     Delivered47,
                     Delivered50,
                };
            }
        }
        public class DeliveryOper
        {
            /// <summary>
            /// Изготвена
            /// </summary>
            public const int Prepared = 1;
            /// <summary>
            /// Изпратена
            /// </summary>
            public const int Send = 7;

            /// <summary>
            /// Предадена на призовкар "За връчване:
            /// </summary>
            public const int ToLawUnit = 4;

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

            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther1 = 24;
            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther2 = 25;
            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther3 = 26;
            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther4 = 27;
            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther5 = 28;
            /// <summary>
            /// Друго посещение
            /// </summary>
            public const int VisitOther6 = 29;

            public static int?[] Visits()
            {
                return new int?[]{
                 Visit1,
                 Visit2,
                 Visit3,
                 VisitOther1,
                 VisitOther2,
                 VisitOther3,
                 VisitOther4,
                 VisitOther5,
                 VisitOther6,
               };
            }
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

        /// <summary>
        /// Константи за типове нотификации
        /// </summary>
        public class WorkNotificationType
        {
            /// <summary>
            /// Известени са всички лица в списъка за известяване
            /// </summary>
            public const int ListDelivered = 1;

            /// <summary>
            /// Недоставено известие
            /// </summary>
            public const int UnDeliveredNotification = 2;

            /// <summary>
            /// Доставено известие
            /// </summary>
            public const int DeliveredNotification = 3;

            /// <summary>
            /// Изтичащ срок
            /// </summary>
            public const int DeadLine = 4;

            /// <summary>
            /// Ново разпределено дело
            /// </summary>
            public const int NewCase = 5;

            /// <summary>
            /// Заместване в заседание
            /// </summary>
            public const int JudgeChangeInSession = 6;

            /// <summary>
            /// Посочен адвокат по искане за правна помощ
            /// </summary>
            public const int CaseLawyerHelpAssigned = 7;

            /// <summary>
            /// Новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int N1 = 8;

            /// <summary>
            /// Липса на предприети действия
            /// </summary>
            public const int N3 = 9;

            /// <summary>
            /// Липса на предприети действия
            /// </summary>
            public const int N3_From_Result = 99999;

            /// <summary>
            /// Постъпване на съпровождащ документ по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int CompliantDocumentCaseFastProcess = 10;

            /// <summary>
            /// Връчено съобщение по дело по чл. 410 ГПК или чл. 417 ГПКК
            /// </summary>
            public const int MessageDeliveredFastProcess = 11;

            /// <summary>
            /// Залепено съобщение по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int StuckMessagesFastProcess = 12;

            /// <summary>
            /// Липса на подадено в срок възражение по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int LackSubmittedObjectionFastProcess = 13;

            /// <summary>
            /// Обжалване на акт по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int AppealActFastProcess = 14;

            /// <summary>
            /// Влязъл в сила финализиращ съдебен акт по исково дело
            /// </summary>
            public const int ActInforcedAnotherInstanceFastProcess = 15;

            /// <summary>
            /// Известяване за постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК
            /// </summary>
            public const int N11 = 16;

            /// <summary>
            /// Образуване на свързано дело на горна инстанция по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int NewCaseHigherInstanceWithout0604_1_2FastProcess = 17;

            /// <summary>
            /// Потвърждение за подаване на иск по чл. 422
            /// </summary>
            public const int NewCaseHigherInstanceWith0604_1_2FastProcess = 18;

            /// <summary>
            /// Изтекъл срок за изразяване на становище по възражение по чл. 414 а ГПК по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int ExpressingOpinionObjectionFastProcess = 20;

            /// <summary>
            /// Изтекъл срок за предявяване на иск по чл. 422 ГПК по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int FilingClaimFastProcess = 21;

            /// <summary>
            /// Постановен акт за отвод/самоотвод по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int DecreeRecusalSelfRecusalFastProcess = 22;

            /// <summary>
            /// Получено съобщение за връчване по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int ReceivedMessageDeliveryFastProcess = 23;

            /// <summary>
            /// Връчен изпълнителен лист от съдебен изпълнител по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int DeliveredЕxecutiveListFastProcess = 24;

            /// <summary>
            /// Не връчено съобщение по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int NotDeliveredMessageDeliveryFastProcess = 25;

            /// <summary>
            /// Липса на предприето процесуално действие от страна по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int NoProceduralActionTakenFastProcess = 26;

            /// <summary>
            /// Известяване за получено съобщение за връчване по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int NoticeServiceReceivedFastProcess = 27;

            /// <summary>
            /// Предприемане на действия от съдебен служител, при постановяване на съдебен акт по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int ActionTakenCourtOfficerDeclatActFastProcess = 28;

            /// <summary>
            /// Изтекъл, определения от потребителя, срок от постановяване на акт
            /// </summary>
            public const int N23 = 29;

            /// <summary>
            /// Предприемане на действия от съдебен служител след подписване на писмо/удостоверение
            /// </summary>
            public const int N24 = 30;
        }

        public class NotificationKinds
        {
            public const int FastProcess = 2;
        }
        public class Courts
        {
            public const int VKS = 39;
            public const int VSS = 182;
            public const int RandomAssignment = 184;
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

            public static int[] CanceledSessions = { Prenasrocheno, Cancel };
        }

        /// <summary>
        /// Резултат от заседание
        /// </summary>
        public class CaseSessionResult
        {
            /// <summary>
            /// Без движение - с определение
            /// </summary>
            public const int WithoutMovementByDefinition = 1;

            /// <summary>
            /// Без движение - с разпореждане
            /// </summary>
            public const int WithoutMovementByOrder = 2;

            /// <summary>
            /// Върнато за доразследване
            /// </summary>
            public const int Investigation = 5;

            /// <summary>
            /// Прекратява поради изпращане по подсъдност
            /// </summary>
            public const int SendJurisdiction = 6;

            /// <summary>
            /// Обявено за решаване
            /// </summary>
            public const int AnnouncedForResolution = 8;

            /// <summary>
            /// Отложено в I-во заседание
            /// </summary>
            public const int ProcrastinationFirstSession = 11;

            /// <summary>
            /// Прекратено производство
            /// </summary>
            public const int StopProduction = 18;

            /// <summary>
            /// Прекратено производство за доразследване
            /// </summary>
            public const int SuspendedInvestigation = 19;

            /// <summary>
            /// Прекратено производство по спогодба
            /// </summary>
            public const int Agreement = 21;

            /// <summary>
            /// С определение
            /// </summary>
            public const int WithDefinition = 24;

            /// <summary>
            /// С определение за отвод
            /// </summary>
            public const int S_opredelenie_za_otvod = 26;

            /// <summary>
            /// С определение по привременни мерки
            /// </summary>
            public const int TemporarilyAction = 28;

            /// <summary>
            /// С определение, приключващо делото
            /// </summary>
            public const int OpredeleniePrikluchvane = 30;

            /// <summary>
            /// С отменен ход по същество
            /// </summary>
            public const int StopedMoveWithSubstantialReason = 31;

            /// <summary>
            /// С присъда
            /// </summary>
            public const int WithSentence = 33;

            /// <summary>
            /// С разпореждане
            /// </summary>
            public const int WithWrit = 36;

            /// <summary>
            /// С разпореждане за отвод
            /// </summary>
            public const int S_razporejdane_za_otvod = 37;

            /// <summary>
            /// С разпореждане, приключващо делото
            /// </summary>
            public const int RazporejdanePrikluchvane = 39;

            /// <summary>
            /// С решение
            /// </summary>
            public const int WithDecision = 42;

            /// <summary>
            /// Спряно производство
            /// </summary>
            public const int DiscontinuedProduction = 44;

            /// <summary>
            /// Със споразумение
            /// </summary>
            public const int WithAgreement = 47;

            /// <summary>
            /// Насрочено за I-во заседание
            /// </summary>
            public const int ScheduledFirstSession = 48;

            /// <summary>
            /// Отказ от наследство
            /// </summary>
            public const int RefuseHeritage = 49;

            /// <summary>
            /// Приемане на наследство
            /// </summary>
            public const int AcceptHeritage = 50;

            /// <summary>
            /// С решение и мотиви
            /// </summary>
            public const int WithDecisionAndMotives = 56;

            /// <summary>
            /// Прекратено след насрочване
            /// </summary>
            public const int CanceledAfterScheduling = 59;

            /// <summary>
            /// Прекратено преди насрочване
            /// </summary>
            public const int CancelledBeforeSchedule = 65;

            /// <summary>
            /// С решение за допускане на делба
            /// </summary>
            public const int Partition = 70;

            /// <summary>
            /// Насрочено в I-во заседание БЕЗ РАЗМЯНА НА КНИЖА
            /// </summary>
            public const int ScheduledFirstSessionWithoutDocuments = 71;

            /// <summary>
            /// Прекратява съдебното производство и връща на прокурора
            /// </summary>
            public const int TerminatesCourtProceedingsReturnsProsecutor = 75;

            /// <summary>
            /// С акт за отвод
            /// </summary>
            public const int S_act_za_otvod = 224;

            /// <summary>
            /// Препращане за провеждане на информационна среща по медиация
            /// </summary>
            public const int ReferralInformationalMeetingMediation = 228;

            /// <summary>
            /// Частично прекратено със спогодба  след проведена медиация
            /// </summary>
            public const int PartiallyTerminatedSettlementAfterMediation = 229;

            /// <summary>
            /// Частично прекратено поради частично оттегляне или отказ от иска, след проведена медиация
            /// </summary>
            public const int PartiallyTerminatedDuePartialWithdrawalWaiverClaimFollowingMediation = 230;

            /// <summary>
            /// С акт по хода на делото без уведомяване на страните
            /// </summary>
            public const int ByActOnProgressCaseWithoutNotifyingParties = 231;

            public static readonly int[] ScheduledFirstSessionList = { ScheduledFirstSession, ScheduledFirstSessionWithoutDocuments };

            /// <summary>
            /// Резултати актове за отвод
            /// </summary>
            public static readonly int[] ActZaOtvod = { S_opredelenie_za_otvod, S_razporejdane_za_otvod, S_act_za_otvod };

            /// <summary>
            /// Реазултати за справка постъпили дела за период – първоинстанционни дела
            /// </summary>
            public static readonly int[] FiledCasesFirstInstance = { SuspendedInvestigation, TerminatesCourtProceedingsReturnsProsecutor };

            /// <summary>
            /// резултати за заседания с ненаписани съдебни актове от всички съдии
            /// </summary>
            public static readonly int[] CaseSessionWithActProject = { 1, 2, 13, 16, 17, 24, 25, 26, 35, 161, 162, 163, 164, 165, 167, 168, 169, 170, 174, 176, 182, 188, 192, 193, 195, 196, 201, 204, 209, 210, 222, 224 };

            /// <summary>
            /// За Справка за Дела с ненаписани съдебни актове от всички съдии
            /// </summary>
            public static readonly int[] CaseWithoutFinalAct = { 3, 31 };

            public static readonly int[] DecreeRecusalSelfRecusalFastProcessArray = { S_opredelenie_za_otvod, S_razporejdane_za_otvod, S_act_za_otvod };

            /// <summary>
            /// Прекратяване на дело със спогодба
            /// </summary>
            public static readonly int[] TerminationCaseSettlement = { Agreement, CanceledAfterScheduling };

            /// <summary>
            /// Прекратяване на дело поради оттегляне или отказ
            /// </summary>
            public static readonly int[] TerminationCaseTerminationWithdrawalRefusal = { CanceledAfterScheduling, CancelledBeforeSchedule };

            /// <summary>
            /// Спиране на интервал за медиация
            /// </summary>
            public static readonly int[] LifecycleStopOne = { Agreement, CanceledAfterScheduling };

            /// <summary>
            /// Спиране на интервал за медиация
            /// </summary>
            public static readonly int[] LifecycleStopTwo = { CanceledAfterScheduling, CancelledBeforeSchedule };

            /// <summary>
            /// Разултати за стартиране на нотификация N3
            /// </summary>
            public static readonly int[] ResultsStartNotificationN3 = { 151, 222, 2, 51, 3, 221, 7, 73, 68, 65, 223, 18, 20, 59, 6, 66, 206, 205, 174, 171, 172, 167, 24, 191, 57, 30, 179, 44, 203, 36, 180, 37, 39, 43, 45 };
        }

        /// <summary>
        /// Основания за резултат от заседание
        /// </summary>
        public class CaseSessionResultBase
        {
            /// <summary>
            /// За доразследване
            /// </summary>
            public const int ForFurtherInvestigation = 8;

            /// <summary>
            /// За отстраняване на процесуални нарушения
            /// </summary>
            public const int ToEliminateProceduralViolations = 14;

            /// <summary>
            /// Постигната спогодба между страните
            /// </summary>
            public const int AgreementReachedBetweenParties = 38;

            /// <summary>
            /// Оттегляне/отказ от иска
            /// </summary>
            public const int WithdrawalWaivingClaim = 63;

            /// <summary>
            /// Върнато за доразследване
            /// </summary>
            public const int ReturnedFurtherInvestigation = 66;

            /// <summary>
            /// Поради отлагане или отказ от иска
            /// </summary>
            public const int PostponeCancel = 89;

            /// <summary>
            /// Постигната спогодба между страните след проведена медиация
            /// </summary>
            public const int AgreementReachedBetweenPartiesAfterMediation = 90;

            /// <summary>
            /// Оттегляне или отказ от иска, след процедура по медиация
            /// </summary>
            public const int WithdrawalAbandonmentClaimAfterMediationProcedure = 91;

            /// <summary>
            /// Основания за справка постъпили дела за период – първоинстанционни дела
            /// </summary>
            public static readonly int[] FiledCasesFirstInstance = { ForFurtherInvestigation, ToEliminateProceduralViolations, ReturnedFurtherInvestigation };
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

            public static readonly int[] EnforcedStatesWithOutCanceled = { Enforced, ComingIntoForce, Appeal };

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

            public static readonly int[] WithOpinion = { AcceptWithOpinion, DontAccept };
        }

        public class CoordinationTypes
        {
            public const int Act = 1;
            public const int Motive = 2;
        }

        public class JudgeRole
        {
            /// <summary>
            /// Съдия-докладчик
            /// </summary>
            public const int JudgeReporter = 1;

            /// <summary>
            /// Член на състав
            /// </summary>
            public const int Judge = 2;

            /// <summary>
            /// Заседател
            /// </summary>
            public const int Jury = 3;

            /// <summary>
            /// Резервен съдия
            /// </summary>
            public const int ReserveJudge = 4;

            /// <summary>
            /// Резервен заседател
            /// </summary>
            public const int ReserveJury = 5;

            /// <summary>
            /// Член-съдия разширен състав
            /// </summary>
            public const int ExtJudge = 6;

            /// <summary>
            /// Заседател разширен състав
            /// </summary>
            public const int ExtJury = 7;

            /// <summary>
            /// Съдия ВАС
            /// </summary>
            public const int JudgeVAS = 8;

            /// <summary>
            /// Секретар
            /// </summary>
            public const int Secretary = 9;

            /// <summary>
            /// Съдебен помощник
            /// </summary>
            public const int CaseAssistant = 10;

            /// <summary>
            /// Деловодител
            /// </summary>
            public const int DocumentRegister = 11;

            /// <summary>
            /// Друг съдебен служител
            /// </summary>
            public const int OtherUser = 12;

            public const int ReserveJudgeAndJury = 45;

            public static int[] JudgeRolesActiveList = { JudgeReporter, Judge, ExtJudge };
            public static int[] JudgeRolesActiveChangeDepRolList = { JudgeReporter, Judge, ExtJudge, JudgeVAS, Jury, ReserveJury, ExtJury };
            public static int[] JudgeRolesList = { JudgeReporter, Judge, ReserveJudge, ExtJudge, JudgeVAS };
            public static int[] JudgeRolesListMain = { JudgeReporter, Judge, ExtJudge, JudgeVAS };
            public static int[] JudgeAndJuryRolesListMain = { JudgeReporter, Judge, ExtJudge, JudgeVAS, Jury, ExtJury };
            public static int[] JudgeAndJuryRolesListMainForActPrint = { JudgeReporter, Judge, ExtJudge, JudgeVAS, Jury, ExtJury, ReserveJury };
            public static int[] JuriRolesList = { Jury, ReserveJury, ExtJury };
            public static int[] JuriRolesListMain = { Jury, ExtJury };
            public static int[] ExtRolesList = { ExtJudge, ExtJury };
            public static int[] ReserveRolesList = { ReserveJudge, ReserveJury };
            /// <summary>
            /// Ръчни роли на служители - за добавяне в дело, без разпределение
            /// </summary>
            public static int[] ManualRoles = { Secretary, CaseAssistant, DocumentRegister, OtherUser };

            //Всички съдии, секретари и помощници по делото
            public static int[] JudgeAndManualRoles = { JudgeReporter, Judge, ReserveJudge, ExtJudge, JudgeVAS, Secretary, CaseAssistant, DocumentRegister, OtherUser };
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
            public const int Summary = 4;
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
            public const int EisppNP = 16;
            public const int EisppCrime = 17;
            public const int CasePersonInheritance = 18;
            public const int CasePersonBulletin = 19;
            public static int[] OtherCounters = { Document, Case, SessionAct };
        }

        public class CounterResults
        {
            public const string Register = "register";
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

        /// <summary>
        /// Константи за направление на движение на дело
        /// </summary>
        public class CaseMigrationDirections
        {
            /// <summary>
            /// Изходящи
            /// </summary>
            public const int Outgoing = 1;

            /// <summary>
            /// Входящи
            /// </summary>
            public const int Incoming = 2;

            /// <summary>
            /// Вътрешни
            /// </summary>
            public const int UnionCase = 3;

            public static int[] DirectionsForAccess = { Outgoing, UnionCase };

        }
        public class CaseMigrationTypes
        {
            /// <summary>
            /// Изпращане в равен по степен съд по подсъдност
            /// </summary>
            public const int SendJurisdiction = 1;

            /// <summary>
            /// Изпращане в по-висша инстанция за обжалване
            /// </summary>
            public const int SendNextLevel = 2;
            /// <summary>
            /// Изпращане в по-висша инстанция за определяне на компетентен съд
            /// </summary>
            public const int SendNextLevelCompetency = 3;

            /// <summary>
            /// Изпращане в по-висша инстанция при масов отвод
            /// </summary>
            public const int SendNextLevelDismissal = 4;

            /// <summary>
            /// Изпращане в подчинен съд по компетентност
            /// </summary>
            public const int SendCompetence = 6;

            /// <summary>
            /// Приемане в равен по степен съд по подсъдност
            /// </summary>
            public const int AcceptJurisdiction = 7;

            /// <summary>
            /// Приемане от подчинен съд за обжалване
            /// </summary>
            public const int AcceptanceSubordinateCourtAppeal = 8;

            /// <summary>
            /// Връщане в подчинен съд с резултат от инстанционна проверка
            /// </summary>
            public const int ReturnCase_AfterComplain = 12;

            /// <summary>
            /// Приемане от по-висша инстанция с резултат от инстанционна проверка
            /// </summary>
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
            /// Изпращане за послужване
            /// </summary>
            public const int Acceptance_ForUse = 17;

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

            /// <summary>
            /// Изпращане на дело към друга система
            /// </summary>
            public const int SendCase_ToOtherSystem = 23;

            /// <summary>
            /// Връщане на дело от друга система
            /// </summary>
            public const int SendCase_FromOtherSystem = 25;

            /// <summary>
            /// Изпращане на делото за централно преразпределение 
            /// </summary>
            public const int SendCase_FromRandomAssignment = 27;

            /// <summary>
            /// Изпращане за преразпределение по подсъдност
            /// </summary>
            public const int SendCase_FromAssignmentByAddress = 29;

            /// <summary>
            /// Свързване на дела
            /// </summary>
            public const int CaseConnection = 31;

            /// <summary>
            /// Изпращане на дело по чл. 80 ал.10 от ПАС
            /// </summary>
            public const int SendCase_ch80PAS = 32;

            /// <summary>
            /// Приемане на дело по чл. 80 ал.10 от ПАС
            /// </summary>
            public const int AcceptCase_ch80PAS = 33;

            public static int[] SendCaseTypesCanAccept = { SendCase_ForUse, SendCase_ToOtherSystem, SendCase_FromOtherSystem, SentToProsecutors, ReturnCase_ForUse };
            public static int[] SendCaseTypesCanAcceptToInstitution = { SendCase_ch80PAS };
            public static int[] RequireActs = { SendNextLevel };
            public static int[] ReturnCaseTypes = { ReturnCase_AfterComplain, ReturnCase_ForAdmin, ReturnCase_ForUse, SendCase_ch80PAS };
            public static int[] RequireReturnCaseTypes = { ReturnCase_AfterComplain, ReturnCase_ForAdmin, ReturnCase_ForUse };
            public static int[] SendCase_FromAssignment = { SendCase_FromRandomAssignment, SendCase_FromAssignmentByAddress };
            public static int[] ConnectCaseMigrations = { SendJurisdiction, SendNextLevel, SendNextLevelCompetency, SendNextLevelDismissal, SendCompetence, SendCase_ForUse };
            public static int[] NewCaseHigherInstanceFastProcess = { AcceptanceSubordinateCourtAppeal, CaseConnection, Acceptance_ForUse };
            public static int[] HasAcceptWithInterval = { SendCompetence, SendCase_ch80PAS };

            /// <summary>
            /// Обединяване и свързване на дела
            /// </summary>
            public static int[] CaseUnionConnection = { CaseUnion, CaseConnection };
        }

        public class CaseMigrationKinds
        {
            public const int EpepInMigration = 1;
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
            public const int WillBeen = 11;
            public const int WithRegistry = 12;
            public const int WithSecurity = 13;
            public const int ByRNFL = 14;

            public static bool OnMoment(int? notificationDeliveryGroupId)
            {
                return (
                notificationDeliveryGroupId == OnSession ||
                notificationDeliveryGroupId == OnPhone ||
                notificationDeliveryGroupId == OnEMail ||
                notificationDeliveryGroupId == OnMember50 ||
                notificationDeliveryGroupId == OnMember56 ||
                notificationDeliveryGroupId == WillBeen ||
                notificationDeliveryGroupId == ByRNFL);
            }
            public static bool WithCourierLike(int? notificationDeliveryGroupId)
            {
                return (
                notificationDeliveryGroupId == WithCourier ||
                notificationDeliveryGroupId == WithCityHall ||
                notificationDeliveryGroupId == WithRegistry ||
                notificationDeliveryGroupId == WithSecurity);
            }
            public static bool OnMomentWithOutMail(int? notificationDeliveryGroupId)
            {
                return OnMoment(notificationDeliveryGroupId) &&
                       (notificationDeliveryGroupId != OnEMail) &&
                       (notificationDeliveryGroupId != ByRNFL);
            }
            /// <summary>
            /// Видове за които да се генерира DeliveryItem
            /// </summary>
            public static int[] DeliveryGroupForDeliveryItem = { WithSummons, WithCourier, WithCityHall, WithRegistry, WithSecurity, OnEMail, ByEPEP, ByRNFL };
        }

        public class DeliveryItemAuditLog
        {
            public const string ChangeRaion = "Смяна на район и призовкар";
            public const string Visit = "Посещения";
            public const string VisitView = "Посещение";
            public const string VisitEdit = "Редакция на  посещение";
            public const string VisitHistory = "Проследяване";
            public const string EditReturn = "Връщане на отрязък";

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

            public static int[] ResidentAddresses = { Permanent };
            public static int[] CurrentManageAddresses = { Current, CompanyPermanent };

        }
        public class PaymentType
        {
            public const int Pos = 1;
            public const int Bank = 2;
            public const int Cash = 3;
            public const int EPEP = 4;

            /// <summary>
            /// Незабавни плащания, при които се пише и плащане
            /// </summary>
            public static int[] InstantPayments = { Pos, EPEP, Cash };
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
            public const int RelationsSearch = 12;
            public const int CriminalRecordsReport = 13;
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
            public static int[] EproDismissalTypes = { Otvod, SamoOtvod };
        }

        public class DismissalStates
        {
            /// <summary>
            /// Уважено искане за отвод
            /// </summary>
            public const int Confirmed = 1;

            /// <summary>
            /// Неуважено искане за отвод
            /// </summary>
            public const int Declined = 2;
        }

        public class DismissalRequestTypes
        {
            /// <summary>
            /// Подадено искане за отвод на регистратура
            /// </summary>
            public const int Document = 1;

            /// <summary>
            /// Искане за отвод по време на заседание
            /// </summary>
            public const int Session = 2;
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

            //Възнагражденията Бюджетна
            public const int Earnings = 4;

            //Гаранция
            public const int Warranty = 6;

            //Депозит за вещо лице
            public const int ExpertDeposit = 7;

            //Депозит за правна помощ
            public const int LegalDeposit = 8;

            //Други суми по сметка Депозити
            public const int OtherDeposit = 9;

            //Обезпечение
            public const int Collateral = 41;

            /// <summary>
            /// Възнаграждение на медиатор към съдебен център по медиация за участие в информационна среща
            /// </summary>
            public const int Mediation = 43;

            //Видове суми, които да се зареждат при съпровождащ документ
            public static int[] MoneyCompliantDocumentList = { StateFee, Fine, Warranty, ExpertDeposit, LegalDeposit, OtherDeposit };
        }

        public class PersonRole
        {
            /// <summary>
            /// Адвокат
            /// </summary>
            public const int Lawyer = 1;

            /// <summary>
            /// Административно наказващ орган
            /// </summary>
            public const int AdministrativeMember = 2;

            /// <summary>
            /// Вещо лице
            /// </summary>
            public const int Expert = 4;

            /// <summary>
            /// Заинтересовано лице
            /// </summary>
            public const int InterestedPerson = 11;

            /// <summary>
            /// Защитник
            /// </summary>
            public const int Defender = 13;

            /// <summary>
            /// Заявител
            /// </summary>
            public const int Notifier = 14;

            /// <summary>
            /// Ищец
            /// </summary>
            public const int Plaintiff = 15;

            /// <summary>
            /// Комитет на кредиторите
            /// </summary>
            public const int KomitetKreditor = 17;

            /// <summary>
            /// Молител
            /// </summary>
            public const int Petitioner = 20;

            /// <summary>
            /// Нарушител
            /// </summary>
            public const int Offender = 22;

            /// <summary>
            /// наследник
            /// </summary>
            public const int Inheritor = 23;

            /// <summary>
            /// наследодател
            /// </summary>
            public const int Legator = 24;

            /// <summary>
            /// Обвиняем
            /// </summary>
            public const int Defendant = 27;

            /// <summary>
            /// Особен представител
            /// </summary>
            public const int SpecialRepresentative = 29;

            /// <summary>
            /// Ответник
            /// </summary>
            public const int Libellee = 30;

            /// <summary>
            /// Подсъдим
            /// </summary>
            public const int Prisoner = 35;

            /// <summary>
            /// Пострадал
            /// </summary>
            public const int Suffered = 39;

            /// <summary>
            /// Преводач
            /// </summary>
            public const int Translator = 40;

            /// <summary>
            /// Представляващ
            /// </summary>
            public const int Representing = 41;

            /// <summary>
            /// Прокурор
            /// </summary>
            public const int Prokuror = 43;

            /// <summary>
            /// Прокурор
            /// </summary>
            public const int Prosecutor = 43;

            /// <summary>
            /// Процесуален представител
            /// </summary>
            public const int ProceduralRepresentative = 44;

            /// <summary>
            /// Синдик
            /// </summary>
            public const int Sindik = 47;

            /// <summary>
            /// Служебен защитник
            /// </summary>
            public const int OfficialDefender = 48;

            /// <summary>
            /// Трето лице помагач
            /// </summary>
            public const int ThirdPersonHelper = 49;

            /// <summary>
            /// Вносител
            /// </summary>
            public const int Vnositel = 57;

            /// <summary>
            /// Кредитор
            /// </summary>
            public const int Kreditor = 59;

            /// <summary>
            /// Длъжник
            /// </summary>
            public const int Debtor = 60;

            /// <summary>
            /// Напълномощен защитник
            /// </summary>
            public const int AuthorizedDefender = 68;

            /// <summary>
            /// Резервен защитник
            /// </summary>
            public const int ReserveDefender = 69;

            /// <summary>
            /// Фирма
            /// </summary>
            public const int Company = 70;

            /// <summary>
            /// Освидетелстван
            /// </summary>
            public const int Certified = 71;

            /// <summary>
            /// Искано лице
            /// </summary>
            public const int WantedPerson = 88;

            /// <summary>
            /// Засегнато лице
            /// </summary>
            public const int AffectedPerson = 93;

            /// <summary>
            /// Експерт
            /// </summary>
            public const int Consultant = 94;

            /// <summary>
            /// Лице по принудително действие
            /// </summary>
            public const int CoerciveActionPerson = 105;

            public static int[] ListForLawyerHelp_Lawyer = { Lawyer, Defender, SpecialRepresentative, Representing, ProceduralRepresentative, OfficialDefender, AuthorizedDefender, ReserveDefender };
            public static int[] PersonFastProcess = { Notifier, Debtor };
        }
        public class RoleKind
        {
            public const int LeftSide = 1;
            public const int RightSide = 2;
            public const int Representative = 3;

            public static int[] MainSides = { LeftSide, RightSide };
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
        public class YesNo
        {
            public const string Yes = "Y";
            public const string No = "N";
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
            public const int CHF = 4;
            public const int GBP = 5;

            public const int TRY = 6;
            public const int RON = 7;
            public const int RSD = 8;

            public const int N_A = 9;
        }
        public class CurrencyCode
        {
            public const string BGN = "BGN";
            public const string EUR = "EUR";
            public const string USD = "USD";
            public const string CHF = "CHF";
            public const string GBP = "GBP";

            public const string TRY = "TRY";
            public const string RON = "RON";
            public const string RSD = "RSD";
            public const string N_A = "N/A";
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
            public const int SpecialAccess = 16;
            /// <summary>
            /// лица по чл.50 и 52 (лица задължени да подават по ел. път) обвързано с ред 41)
            /// </summary>
            public const int FP_5152 = 17;

            public static int[] RestictedAccess = { Secret, Restriction, SpecialAccess };
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

        /// <summary>
        /// Константи за шифри
        /// </summary>
        public class CaseCode
        {
            public const int Case410 = 171;
            public const int Case417 = 172;
            public const int AdmissionOfAdoption_0107_1 = 15;

            /// <summary>
            /// 0604-1
            /// </summary>
            public const int CaseCode_0604_1 = 2555;

            /// <summary>
            /// 0604-2
            /// </summary>
            public const int CaseCode_0604_2 = 2556;

            /// <summary>
            /// 0602-1
            /// </summary>
            public const int CaseCode_0602_1 = 104;

            /// <summary>
            /// 0602-2
            /// </summary>
            public const int CaseCode_0602_2 = 103;

            /// <summary>
            /// Списък от шифри 0604-1 и 0604-2
            /// </summary>
            public static int[] CaseCode_0604_1_2 = { CaseCode_0604_1, CaseCode_0604_2 };

            /// <summary>
            /// Списък от шифри 0602-1, 0602-2, 0604-1 и 0604-2
            /// </summary>
            public static int[] CaseCode_0602_1_2_0604_1_2 = { CaseCode_0604_1, CaseCode_0604_2, CaseCode_0602_1, CaseCode_0602_2 };

            /// <summary>
            /// Заявление по чл. 410
            /// </summary>
            public const int FP410 = 170;

            /// <summary>
            /// Заявление по чл. 417
            /// </summary>
            public const int FP417 = 172;
            /// <summary>
            /// Заявление по чл. 417, ал. 1, т 3, 6 и 10
            /// </summary>
            public const int FP417_t3610 = 3037;

            public static int?[] CaseCode_FastProcessNull = { FP410, FP417, FP417_t3610 };
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
            public const int KNOHD = 10;
            public const int KNChHD = 12;
            public const int KNChD = 11;
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

        /// <summary>
        /// Видове интервали
        /// </summary>
        public class LifecycleType
        {
            /// <summary>
            /// Главен интервал
            /// </summary>
            public const int InProgress = 1;

            /// <summary>
            /// Спиране
            /// </summary>
            public const int Stop = 2;

            /// <summary>
            /// Рестартиране
            /// </summary>
            public const int Restart = 3;

            /// <summary>
            /// Рестартиране
            /// </summary>
            public const int Mediation = 99;

            /// <summary>
            /// Типове които важат за сроковете
            /// </summary>
            public static int[] Deadline = { InProgress, Stop };
        }

        public class LinkDirectionType
        {
            public const int Represent = 1;
            public const int RepresentBy = 2;
            public const int RepresentSecond = 7;
        }

        public class CaseSessionTypeGroup
        {
            public const int PublicSession = 1;
            public const int PrivateSession = 2;
        }

        public class DocumentGroup
        {
            //Протест
            public const int Protest = 8;

            public const int DocumentForComplain_AccompanyingDocument = 29;

            public static int[] N24 = { 38, 39 };
        }

        public class DocumentType
        {
            /// <summary>
            /// Молба за опис насл./връщ. на дете
            /// </summary>
            public const int RequestForInventoryInheritanceReturnOfAChild = 13;

            // Молба по чл.51 от Закона за наследството
            public const int Request51LawInheritance = 14;

            // Молба за отказ от наследство
            public const int RequestAcceptanceInheritance = 15;

            // Молба за отказ от наследство
            public const int RequestRefusalInheritance = 16;

            //Молба за кумулация
            public const int RequestForAggregate = 39;
            public const int RequestForAggregate2 = 234;

            // Искане по чл.368 НПК
            public const int Request368 = 44;

            //Заявление за регистрация
            public const int ApplicationForCompanyRegister = 91;

            //Заявление за промяна в обстоятелствата
            public const int ApplicationForCompanyChange = 215;

            //Молба (искане) за възобновяване
            public const int RequestForRenewing = 264;

            //Молба за определяне срок при бавност
            public const int AppealForSlowProcess = 265;

            // Касационна жалба
            public const int CassationAppeal = 272;
            // Касационна частна жалба
            public const int CassationPrivateAppeal = 273;

            /// <summary>
            /// Писмо за изпращане за обжалване
            /// </summary>
            public const int LetterOfTransmittalForAppeal = 281;

            //заявленията за достъп до обществена информация
            public const int PublicInformation = 298;

            public const int IspnLetter760 = 355;
            public const int IspnLetter800 = 356;
            public const int IspnLetter760Papers = 357;
            public const int IspnLetter800Papers = 358;

            /// <summary>
            /// Молба за проверка на предпоставките за погасяване на задълженията от предприемач
            /// </summary>
            public const int Request760Entrepreneur = 361;

            /// <summary>
            /// Молба за проверка на предпоставките за погасяване на задълженията по ЕТ
            /// </summary>
            public const int Request760ET = 362;

            public static int[] ComplainDocsWithoutAct = { AppealForSlowProcess };
            public static int[] IspnLetter = { IspnLetter760, IspnLetter800, IspnLetter760Papers, IspnLetter800Papers };

            public static int[] BlankaRazporejdane = { 210, 364 };
        }

        public class DocumentTypeGroupings
        {
            //Книга по чл. 634в от ТЗ
            // public const int Insolvency = 1;

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

            //За писмата F_PRESENSE_REQUIRED
            public const int PrintLetterRequired = 9;

            //Книга по чл. 634в от ТЗ
            public const int Insolvency = 10;
            //Книга по чл. 634в от ТЗ на който се праща акт

            public const int InsolvencyAct = 11;

            //Книга по чл. 634в от ТЗ - само за книгата е това
            public const int InsolvencyReport = 12;


            //Молба за откриване на производство - 21110, 21111 24100, 24111 -1/2
            //За проверка за визуализация на поле Брой длъжници в екран Образуване на дело
            public const int CaseCodesForDebtorsCountField = 13;

            //ИСПН - РНФЛ
            public const int InsolvencyRNFL = 14;
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

        /// <summary>
        /// Видове срокове
        /// </summary>
        public class DeadlineType
        {
            /// <summary>
            /// Срок за изготвяне на решение
            /// </summary>
            public const int DeclaredForResolve = 1;

            /// <summary>
            /// Срок за изготвяне на протокол от открито съдебно заседание
            /// </summary>
            public const int OpenSessionResult = 2;

            /// <summary>
            /// Срок за изготвяне на мотиви към "Присъда"
            /// </summary>
            public const int Motive = 3;

            /// <summary>
            /// Срок за разглеждане на заявления за регистрация по фирмено дело
            /// </summary>
            public const int CompanyCaseRegister = 4;

            /// <summary>
            /// Срок за разглеждане на заявления за промяна по фирмено дело
            /// </summary>
            public const int CompanyCaseChange = 5;

            /// <summary>
            /// Предприемане на действия по ново образувано дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int TakingActionFastProcess = 6;

            /// <summary>
            /// Липса на произнасяне по съпровождащ документ по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int MissingActForCompliantDocumentFastProcess = 7;

            /// <summary>
            /// Невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public const int UnreturnedMessageFastProcess = 8;
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
            //Това за момента спира да се - от само 4 резултат станаха всички маркирани като прекрате в SessionResult
            public const int StatisticsStopCase = 8;

            //Статистика - прекратени по спогодба
            public const int StatisticsStopCaseAgreement = 9;

            //Статистика - прекратени по други причини
            //Това спира да се ползва по същите причини и като 8. Ще се взимат всички приключени и ще се изключват тези по спогодна
            public const int StatisticsStopCaseOtherReason = 10;

            //Статистика Отлагания на дела в открити заседания Граждански
            public const int StatisticsCaseDelayGD = 11;

            //Статистика В първо по делото заседание и помирително
            public const int StatisticsCaseDelayFirstSessionGD = 12;

            //Статистика Наказателни дела Прекратени и споразумения
            //Това спира да се ползва по същите причини и като 8 и 10
            public const int StatisticsCaseStopND = 13;

            //Статистика Наказателни дела в т.ч. свърш.споразум.- чл.381-384
            public const int StatisticsCaseStop381ND = 14;

            //Статистика Отлагания на дела в открити заседания Наказателни
            public const int StatisticsCaseDelayND = 15;

            //Срочна книга - обявено за решаване
            public const int CaseSessionPublicReportDecision = 16;

            //Без движение
            public const int NoMove = 17;

            //Статистика - дела за доразследване
            public const int StatisticsInvestigateND = 18;

            //Статистика - дела за доразследване Приложение 1 Общо
            public const int StatisticsInvestigate = 19;

            /// <summary>
            /// Повт. вненсени и образувани под нов номер след прекр. на съд.пр-во (чл. 42, ал. 2 , чл. 249 и чл. 288, т. 1 от НПК)
            /// </summary>
            public const int StatisticOsSheet1Col2b = 20;

            /// <summary>
            /// Статистика дела по несъстоятелност Прекратени
            /// </summary>
            public const int StatisticsIspnTerminate = 21;
        }

        public class SessionResultBaseGroupings
        {
            //РЕГИСТЪР ПО ЧЛ.39 Т.13 ОТ ПАС - прекратено производство
            public const int CaseMigrationReturnReport_FinishCaseBase = 1;

            //Статистика - дела за доразследване
            public const int StatisticsInvestigateND = 2;
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

            /// <summary>
            /// ЕПРО - Регистър на отводите
            /// </summary>
            public const int EPRO = 6;

            /// <summary>
            /// Индексиране на данни за съдебни актове
            /// </summary>
            public const int ElasticService = 7;

            /// <summary>
            /// Изчисляване на годишна статистика по дела
            /// </summary>
            public const int Statistics = 8;

            /// <summary>
            /// ЕЕСПП - Правна помощ
            /// </summary>
            public const int Eespp = 9;

            /// <summary>
            /// ЕИСС СИСМА
            /// </summary>
            public const int Sisma = 10;

            /// <summary>
            /// cais.mjs.bg  - Бюлетин съдимост
            /// </summary>
            public const int Cais = 11;

            /// <summary>
            /// Разпределение на електронни документи
            /// </summary>
            public const int EpepDocuments = 12;

            /// <summary>
            /// Обработка на нотификации и срокове
            /// </summary>
            public const int EissProcess = 13;

            /// <summary>
            /// Регистър на несъстоятелността на ФЛ
            /// </summary>
            public const int Rnfl = 14;

            /// <summary>
            /// Възстановяване на заявки по списък от id_list
            /// </summary>
            public const int MqRecover = 15;
        }

        /// <summary>
        /// Видове асинхронни обработки след извършване на действие, които отнемат дълго време
        /// </summary>
        public class EissProcessTypes
        {
            /// <summary>
            /// Запис на дело
            /// </summary>
            public const string CaseSave = "case_save";

            /// <summary>
            /// Деклариране на Съдебен акт
            /// </summary>
            public const string ActDeclared = "act_declared";
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

        /// <summary>
        /// Типове актове
        /// </summary>
        public class ActType
        {
            /// <summary>
            /// Протокол
            /// </summary>
            public const int Protokol = 1;

            /// <summary>
            /// Решение
            /// </summary>
            public const int Answer = 2;

            /// <summary>
            /// Определение
            /// </summary>
            public const int Definition = 3;

            /// <summary>
            /// Разпореждане
            /// </summary>
            public const int Injunction = 4;

            /// <summary>
            /// Протоколно определение
            /// </summary>
            public const int ProtokolOpredelenie = 5;

            /// <summary>
            /// Заповед за изпълнение
            /// </summary>
            public const int CommandmentForExec = 6;

            /// <summary>
            /// Заповед за защита
            /// </summary>
            public const int CommandmentProtection = 8;

            /// <summary>
            /// Заповед за незабавна защита
            /// </summary>
            public const int CommandmentimmediatelyProtection = 7;

            /// <summary>
            /// Присъда
            /// </summary>
            public const int Sentence = 10;

            /// <summary>
            /// Споразумение
            /// </summary>
            public const int Agreement = 11;

            /// <summary>
            /// Изпълнителен лист в полза на частни лица
            /// </summary>
            public const int ExecListPrivatePerson = 12;

            /// <summary>
            /// Изпълнителен лист в полза на държавата
            /// </summary>
            public const int ExecListForState = 13;

            /// <summary>
            /// Обезпечителна заповед
            /// </summary>
            public const int ObezpechitelnaZapoved = 14;

            /// <summary>
            /// Протокол с решение от ОСЗ
            /// </summary>
            public const int ProtokolAnswerOSZ = 16;


            public static int[] SecretarySign = { Protokol, ProtokolOpredelenie, Agreement, ProtokolAnswerOSZ };
            public static int[] JurySign = { Definition, ProtokolOpredelenie, Sentence, Answer, Agreement };
            public static int[] WithMotives = { Answer, Sentence, Definition };
            public static int[] HasInTheNameOfPeople = { Answer, Sentence };
            public static int[] HasSignJudge = { ExecListPrivatePerson, ExecListForState };
            public static int[] ExecListActs = { ExecListPrivatePerson, ExecListForState };
            public static int[] DontFlattenForEPEP = { ExecListPrivatePerson, ExecListForState, CommandmentForExec };

            /// <summary>
            /// Типове актове за стартиране на нотификация N3
            /// </summary>
            public static int[] NotificationN3 = { Injunction, Definition };

            /// <summary>
            /// Типове актове за стартиране на нотификация N4
            /// </summary>
            public static int[] NotificationN4 = { CommandmentForExec, Injunction, Definition };

            public static string FinalByDefaultStr = $"[{Answer},{Sentence},{CommandmentProtection}, {CommandmentForExec}]";
            public static string AppealByDefaultStr = $"[{CommandmentForExec}]";

            /// <summary>
            /// Актове, които могат да се подписват и преди да е проведено заседанието
            /// </summary>
            public static int[] CanSignBeforeSessionEnd = { Sentence };

            public static int[] AllowActTypesISPN = { Answer, Definition, Injunction };

            /// <summary>
            /// За решение, определение и разпореждане отразено като финализиращ акт
            /// </summary>
            public static int[] SignComfirmMessage1 = { Answer, Definition, Injunction };

            /// <summary>
            /// За решение, присъда, които НЕ са отразени като финализиращи актове
            /// </summary>
            public static int[] SignComfirmMessage2 = { Answer, Sentence };
        }

        public class ActKindBlankName
        {
            public const string execlist410itemNew = "execlist410itemNew";
            public const string execlist410moneyNew = "execlist410moneyNew";
            public const string execlist417itemNew = "execlist417itemNew";
            public const string execlist417moneyNew = "execlist417moneyNew";

            public static string[] execlist = { execlist410itemNew, execlist410moneyNew, execlist417itemNew, execlist417moneyNew };
        }

        public class ActFormatType
        {
            public const string Protokol = "protokol";
            public const string Act = "act";
        }

        public class ActBlankNames
        {
            public const string CommandProtection = "CProtection";
            public const string CommandImmidiateProtection = "CIProtection";
            public const string ProtectiveOrder = "ProtectiveOrder";

            public static string[] ActDirectionAlter = { CommandProtection, CommandImmidiateProtection, ProtectiveOrder };
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

            //Уважени за статистиката
            public const int StatisticsCaseSuccessful = 20;

            //Споразу- мения по чл.382 НПК
            public const int StatisticsCaseStop382 = 21;

            //Споразум. по чл.384 НПК , спог. по чл.24 ал. 3 НПК или чл.234 ГПК
            public const int StatisticsCaseStop384 = 22;
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

            /// <summary>
            /// Уведомление за доброволно изпълнение
            /// </summary>
            public static string[] NtVolexs = { "NT_VOLEX_P", "NT_VOLEX_C" };
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

        /// <summary>
        /// Групи резултати
        /// </summary>
        public class CaseSessionResultGroups
        {
            /// <summary>
            /// Основания за прекратяване
            /// </summary>
            public const int Suspended = 1;

            /// <summary>
            /// Основания за спиране
            /// </summary>
            public const int Stop = 3;

            /// <summary>
            /// Основания за отлагане
            /// </summary>
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

        public class SessionTypeGroupe
        {
            public const int OpenSession = 1;

            public const int ClosedSession = 2;
        }

        public class SessionType
        {
            // първо заседание
            public const int FirstSession = 1;

            // второ заседание
            public const int SecondSession = 2;

            // Закрито заседание
            public const int ClosedSession = 3;

            // Разпоредително заседание
            public const int DispositionalSession = 4;

            // Предварително изслушване
            public const int PreHearing = 5;

            // Събрание на кредиторите
            public const int KreditorSession = 6;

            // Открито заседание
            public const int OpenSession = 7;

            // Открито заседание при закрити врата
            public const int OpenSessionOpenDoors = 8;

            // Заседание по привременни мерки
            public const int TemporarilyAction = 10;

            // Помирително заседание
            public const int Conciliatory = 12;


            // Открито заседание след първо
            public const int OpenSessionAfterFirst = 14;

            public static int[] OpenSessionsForPastSessions = { OpenSession, OpenSessionOpenDoors, OpenSessionAfterFirst };
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
        public class PersonRoleInCrime
        {
            public const int Unknown = 3;
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

        public class ActMotiveToDateValue
        {
            public const int ActMotiveDateTo15DayValue = 1;
            public const int ActMotiveDateTo60DayValue = 2;
            public const int ActMotiveDateToUp60DayValue = 4;
        }

        public class ActMotiveToDateLabel
        {
            public const string ActMotiveDateTo15DayLabel = "До 15 дни";
            public const string ActMotiveDateTo60DayLabel = "До 60 дни";
            public const string ActMotiveDateToUp60DayLabel = "Над 60 дни";
        }

        /// <summary>
        /// Номенклатура за разлика между дата на заседание и дата на обявяване на акта - стойност
        /// </summary>
        public class ActToDateValue
        {
            /// <summary>
            /// До 1 месец
            /// </summary>
            public const int ActDateTo1MonthValue = 1;

            /// <summary>
            /// От 1 до 2 месеца
            /// </summary>
            public const int ActDateTo2MonthValue = 2;

            /// <summary>
            /// От 2 до 3 месеца
            /// </summary>
            public const int ActDateTo3MonthValue = 3;

            /// <summary>
            /// От 3 месеца до година
            /// </summary>
            public const int ActDateTo1YeаrValue = 4;

            /// <summary>
            /// Над година
            /// </summary>
            public const int ActDateUo1YeаrValue = 5;
        }

        /// <summary>
        /// Номенклатура за разлика между дата на заседание и дата на обявяване на акта - лейбъл
        /// </summary>
        public class ActToDateLabel
        {
            /// <summary>
            /// До 1 месец
            /// </summary>
            public const string ActDateTo1MonthLabel = "До 1 месец";

            /// <summary>
            /// От 1 до 2 месеца
            /// </summary>
            public const string ActDateTo2MonthLabel = "От 1 до 2 месеца";

            /// <summary>
            /// От 2 до 3 месеца
            /// </summary>
            public const string SessionTo3MonthLabel = "От 2 до 3 месеца";

            /// <summary>
            /// От 3 месеца до 1 година
            /// </summary>
            public const string ActDateTo1YeаrLabel = "От 3 месеца до 1 година";

            /// <summary>
            /// Над 1 година
            /// </summary>
            public const string ActDateUo1YeаrLabel = "Над 1 година";
        }

        public class MoneyCollectionEndDateType
        {
            public const int WithDate = 1;
            public const int Nothing = 2;
            public const int PaymentOfTheReceivable = 3;
        }

        public class CaseMoneyCollectionType
        {
            public const int Other = 2;
        }

        public class CaseMoneyCollectionKind
        {
            // Лихва
            public const int Interest = 1;

            // Договорна лихва
            public const int ContractualInterest = 4;

            // Друго допълнително вземане
            public const int Other = 3;

            // Законна лихва
            public const int LegalInterest = 6;

            // Мораторна лихва
            public const int MoratoriumInterest = 8;

            public static int[] Primary = { Interest, ContractualInterest, LegalInterest, MoratoriumInterest };
        }

        public class PersonRoleGroupings
        {
            public const int RoleArrested = 1;

            //За Справка съдени и осъдени лица
            public const int RoleCasePersonDedendantList = 2;

            //Книга за приемане и отказ от наследство - лицата които да се зареждат в заявителя
            public const int HeritageReportPersonNotifier = 3;

            //Описна книга - роли в полето Страни
            public const int CaseFirstInstanceReportPersonRole = 4;

            //Азбучник - за наказателно дело страни различни от десни, които влизат в справка
            public const int CaseAlphabeticalNakazatelnoDelo = 5;

            /// <summary>
            /// Регистър по чл. 10, ал. 2 от ЗЗДН
            /// </summary>
            public const int ZzdnReport = 6;

            /// <summary>
            /// Статистика дела несъстоятелност Длъжници
            /// </summary>
            public const int StatisticsIspnDebtor = 7;
        }

        public class HtmlTemplateTypes
        {
            public const int Letter = 9;

            //удостоверение
            public const int Certificate = 24;


            //Съобщение+Призовка уведомление
            public const int All3Notification = 29;

            public const int MessageResolution = 31;
            public const int NotificationResolution = 30;
            public const int SubpoenaMediation = 32;
        }

        /// <summary>
        /// Константи за HtmlTemplate
        /// </summary>
        public class HtmlTemplateConstants
        {
            /// <summary>
            /// Съобщение по чл.414a, ал. 3 ГПК
            /// </summary>
            public const int NotificationArt414A = 44;

            /// <summary>
            /// Съобщение по чл.415, ал. 1, т. 2 ГПК - ЗП
            /// </summary>
            public const int NotificationArt415Paragraph1Point2 = 45;

            /// <summary>
            /// Съобщение по чл.415, ал. 1, т. 3 ГПК - ЗП
            /// </summary>
            public const int NotificationArt415Paragraph1Point3 = 47;

            /// <summary>
            /// Съобщение по чл.415 ГПК - ЗАПОВЕДНО ПРОИЗВОДСТВО
            /// </summary>
            public const int NotificationArt415FastProcess = 47;

            /// <summary>
            /// Уведомление по чл.47, ал.1 и 7 ГПК
            /// </summary>
            public const int NT_47_1 = 132;

            /// <summary>
            /// Уведомление по чл.47, ал.8 ГПК
            /// </summary>
            public const int NT_47_8 = 133;

            /// <summary>
            /// Списък за нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК по дело по чл. 410 ГПК или чл. 417 ГПК
            /// </summary>
            public static int[] FilingClaimFastProcessArray = { NotificationArt415Paragraph1Point2, NotificationArt415Paragraph1Point3, NotificationArt415FastProcess };

            /// <summary>
            /// Уведомление по чл.47
            /// </summary>
            public static int[] NT_47s = { NT_47_1, NT_47_8 };
        }

        /// <summary>
        /// Константи за Източник на постъпване на делото
        /// </summary>
        public class CaseCreateFroms
        {
            /// <summary>
            /// Новообразувано
            /// </summary>
            public const int New = 1;

            /// <summary>
            /// По подсъдност
            /// </summary>
            public const int Jurisdiction = 2;

            /// <summary>
            /// За ново разглеждане
            /// </summary>
            public const int NewNumber = 3;

            /// <summary>
            /// Да продължи под същия номер
            /// </summary>
            public const int OldNumber = 4;

            /// <summary>
            /// След връщане за доразследване
            /// </summary>
            public const int Prosecutors = 5;

            /// <summary>
            /// Връщане след доразследване
            /// </summary>
            public const int ReturnAfterFurtherInvestigation = 6;

            /// <summary>
            /// Върнато след администриране
            /// </summary>
            public const int ReturnedAfterAdministration = 7;

            /// <summary>
            /// Приемане на дело по чл. 80 ал.10 от ПАС
            /// </summary>
            public const int AcceptedCh80 = 8;
        }

        /// <summary>
        /// Стойности на вид съдържание
        /// </summary>
        public class ContentTypes
        {
            public const string Html = "text/html";
            public const string Pdf = "application/pdf";
            public const string Json = "application/json";
            public const string Xml = "text/xml";
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

        public static char VisualLetterEnBgGetLatin(char bgChar)
        {
            foreach (var kvp in VisualLetterEnBg)
            {
                if (kvp.Value == bgChar)
                {
                    return kvp.Key;
                }
            }
            return bgChar;
        }

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

            //Освобождаване от наказателна отговорност - чл. 78а НК
            public const int Justified78 = 5;

            //С наложена имуществена санкция по чл.83а от ЗАНН
            public const int PropertySanction83 = 7;
        }

        public class SentenceTypes
        {
            //Лишаване от свобода - условно
            public const int ImprisonmentConditional = 1;

            //Лишаване от свобода - ефективно
            public const int ImprisonmentEffectively = 2;

            //Доживотен затвор
            public const int LifeSentence = 4;

            //Доживотен затвор без замяна
            public const int LifeSentenceNoChange = 5;

            //Глоба
            public const int Fine = 6;

            //Пробация
            public const int Probation = 7;

            //Обществено порицание
            public const int Reprimand = 8;

            //Лишаване от права
            public const int DeprivationOfRights = 9;

            //Други наказания
            public const int Other = 10;

            //Поправителен труд
            public const int CorrectiveWork = 11;

            //Задължително заселване
            public const int Settlement = 12;

            //Не се наказва
            public const int NotPunished = 13;

            //Други наказания – условно
            public const int OtherConditional = 14;

            //ТВУ – само за непълнолетни
            public const int TVU = 15;

            //Лишаване от право за управление на МПС
            public const int DeprivationLicenseToDrive = 16;

            //Отнемане в полза на държавата средство на престъпление(вр.чл.53НК)
            public const int DeprivationOfMps = 17;

            //Присъждане в полза на държавата равностойността в лева на средство на престъпление(вр.чл.53НК)
            public const int DeprivationOfMpsValue = 18;

            public static int?[] LishavaneOtSvoboda = { ImprisonmentConditional, ImprisonmentEffectively, LifeSentence, LifeSentenceNoChange };
        }

        public class SentenceLawbases
        {
            //чл.25 НК
            public const int LawBase25 = 3;
        }

        public class DecisionRequestTypes
        {
            public const int RequestCase = 1;
            public const int RequestNotification = 2;
        }
        public class DecisionTypes
        {
            //Дава достъп
            public const int FullAccess = 1;
        }

        public class ExecListStates
        {
            //Изготвен
            public const int Ready = 1;

            //Обезсилен
            public const int Cancel = 2;
        }

        public class ActComplainResults
        {
            public const int AcceptAll = 1;
            public const int AcceptNotAll = 2;
            public const int Cancel = 3;
            public const int Respect = 8;
        }

        //За статистиката
        public class StatisticReportTypes
        {
            //Несвършени
            public const int Unfinished = 1;

            //Постъпили за периода
            public const int Incoming = 2;

            //свършени по същество
            public const int FinishedNoStop = 3;

            //свършени прекратени
            public const int FinishedStop = 4;

            //свършени в 3 месечен срок
            public const int Finished3months = 5;
        }

        public class ExcelReportCellValueTypes
        {
            public const int StringValue = 1;

            public const int IntValue = 2;

            public const int IntervalValue = 3;
        }

        public class RegixRequestTypes
        {
            //Регистратура
            public const int FromDocument = 1;

            //Дело
            public const int FromCase = 2;

            //Справка външни системи
            public const int FromReport = 3;

        }
        public class VKS_const
        {


            public static int[,] MonthlyMatrix2 = { { 1, 1 }, { 1, 4}, { 1, 7 }, {1, 10 },{1, 14 },{ 1, 16 },{ 1, 19 },
                                               {2, 2 },{ 2, 5 },{ 2, 8 },{2, 11 },{ 2, 15 },{ 2, 18},{ 2, 20 },
                                               { 3, 3 },{ 3, 4 },{ 3, 9 },{ 3, 12 },{ 3, 13 },{ 3, 17 },{ 3, 19 },
                                                { 4, 1 }, { 4, 6}, { 4, 8 }, {4, 11 },{4, 13 },{ 4, 18 },{ 4, 21 },
                                                { 5, 2 }, { 5, 6}, { 5, 9 }, {5, 12},{5, 14 },{ 5, 16 },{ 5, 20},
                                                { 6, 3 }, { 6, 5}, { 6, 7 }, {6, 10 },{6, 15 },{ 1, 17 },{ 1, 21 }
                                              };

            public static string[] StringMatrix = { "1,4,7,10,14,16,19",
                                              "2,5,8,11,15,18,20",
                                              "3,4,9,12,13,17,19",
                                              "1,6,8,11,13,18,21",
                                              "2,6,9,12,14,16,20",
                                              "3,5,7,10,15,21"
                                              };


            public class SelectionState
            {
                /// <summary>
                /// Нов
                /// </summary>
                public const int New = 1;

                /// <summary>
                /// Затворен
                /// </summary>
                public const int Closed = 2;



            }


        }

        public class SessionWornings
        {
            // Не е минал час на провеждане или си има и резултат и акт
            public const int AllOk = 0;

            // Няма резултат
            public const int NotExistResult = 1;

            // Няма акт
            public const int NotExistAct = 2;
        }

        public class CasePersonInheritanceResults
        {
            //Приема
            public const int Accept = 1;

            //Отказва
            public const int Refuse = 2;

            //Загубва право
            public const int Lose = 3;
        }

        public class VksSessionLawunitChange
        {
            /// <summary>
            /// С промяна на състава по делото в зависимост от датата на заседанието
            /// </summary>
            public const int WithChange = 1;

            /// <summary>
            /// Без промяна на състава, каквото последно е валидно по делото
            /// </summary>
            public const int NoChange = 2;
        }

        public class SystemParamName
        {
            ///// <summary>
            ///// Активирана Заявка 4 - 1 за стартирана и 0 за спряна
            ///// </summary>
            //public const string req_4_2021 = "req_4_2021";
            public const string SystemFeatures = "system_features";
            /// <summary>
            /// ID-та на NomCaseType, които не се зареждат в календара на ВКС
            /// </summary>
            public const string VKS_CaseType_CalendarExclude = "vks_ctypes_cal_exc";

            /// <summary>
            /// Разрешените файлови разширения при прикачване на документи
            /// </summary>
            public const string FileUpload_IncludeExtentions = "file_include_ext";

            /// <summary>
            /// Адрес на услуга прокси услуга за достъп до външни системи
            /// </summary>
            public const string URL_PROXY_EISS = "url_proxy_eiss";

            /// <summary>
            /// Настройка за качество на сканираните документи
            /// </summary>
            public const string ScannerConfiguration = "scanner_configuration";

            /// <summary>
            /// Начало на междинен период при приемане на еврото
            /// </summary>
            public const string InterimPeriodEuroStart = "interim_period_euro_start";

            /// <summary>
            /// Край на междинен период при приемане на еврото
            /// </summary>
            public const string InterimPeriodEuroEnd = "interim_period_euro_end";

            /// <summary>
            /// Курс евро към лев
            /// </summary>
            public const string EuroExchangeRate = "euro_exchange_rate";


            /// <summary>
            /// Дата на регистриране на дела за образуване на партиди по ИЛ
            /// </summary>
            public const string ZP_StartRegDate = "zp_start_regdate";

            public const string ProtocolCheckInterval = "selection_protocol_check_sec";

            /// <summary>
            /// Първия document.id, от който започва Централизирана регистратура
            /// </summary>
            public const string ZP_StartDocumentId = "zp_start_document_id";

            /// <summary>
            /// Активирани броячи през EF core
            /// </summary>
            public const string Test_EF_Counters = "test_ef_counters";
        }

        public class SystemParamValue
        {
            ///// <summary>
            ///// Активирана на Заявка 4
            ///// </summary>
            //public const string req_4_2021_Start = "1";
        }

        public class SystemFeatures
        {

            #region Влизат във версията от 31.05
            //Влизат във версията
            ///// <summary>
            ///// Заявка за промяна №1 (Управление на работния поток за образуване и насрочване на дела от Председател отделение във ВКС)
            ///// </summary>
            //public const string VksReq1 = "vksr1";

            ///// <summary>
            ///// Заявка за промяна №2 (ВКС разпределение наказателно отделение)
            ///// </summary>
            //public const string VksReq2 = "vksr2";
            #endregion

            ///// <summary>
            ///// Заявка за промяна №3 (ВКС призоваване с ДВ)
            ///// </summary>
            //public const string VksReq3 = "vksr3";

            ///// <summary>
            ///// Заявка за промяна №6-12 (Промяна на описни книги и регистри)
            ///// </summary>
            //public const string ReqBooks = "rbooks";

            /// <summary>
            /// Завъртане на втората страница на призовки на 180 градуса ако се печата Landscape
            /// </summary>
            public const string PrintLandscapeRotate180 = "prn180";


            /// <summary>
            /// Разрешено коригиране на постановени актове
            /// </summary>
            public const string CaseSessionActCorrection = "actchg";

            ///// <summary>
            ///// Разрешена интеграция с ЕЕСПП
            ///// </summary>
            //public const string EesppActivated = "eespp";

            ///// <summary>
            ///// Активиране на надградени функционалности на ЕПЕП 2023
            ///// </summary>
            //public const string Epep2023 = "ep23";

            /// <summary>
            /// Промяна на критични данни: шифри, групи по натовареност
            /// </summary>
            public const string CriticalDataChange = "cds";


            public const string CheckDoublePostback = "cdp";


            ///// <summary>
            ///// Разрешен специален достъп
            ///// </summary>
            //public const string SpecialAccess = "sa";

            ///// <summary>
            ///// Разрешен Друго случайно разпределение
            ///// </summary>
            //public const string OtherElection = "oel";

            ///// <summary>
            ///// Заявка 4, 2023г - номенклатури, резултати, основания
            ///// </summary>
            //public const string Request4_2023 = "req423";

            ///// <summary>
            ///// Изпращане на движения на дела към ЕПЕП - ЕДИС
            ///// </summary>
            //public const string EpepCaseMigrations = "ecm";

            ///// <summary>
            ///// налична връзка към Voice 2 Text
            ///// </summary>
            //public const string Voice2Text = "v2t";

            ///// <summary>
            ///// Заявка 7, 2023г - Индекс степен на обжалване
            ///// </summary>
            //public const string Request7_2023 = "req723";

            ///// <summary>
            ///// Заявка 12 - Справки
            ///// </summary>
            //public const string Request12_2023 = "req1223";


            /// <summary>
            /// Показване на бройя задачи и движения на дела на всеки екран
            /// </summary>
            public const string MainMenuCounts = "mmc";

            /// <summary>
            /// Допълнително зачистване на контекста и премахване на тракнатите потребители
            /// </summary>
            public const string FearProtectsVineyard = "fpv";

            /// <summary>
            /// Премахване на тракнатите потребители
            /// </summary>
            public const string ClearTrackedUsers = "ctu";

            /// <summary>
            /// Заявка 9
            /// </summary>
            public const string Request9_2024 = "req0924";


            public const string Request9_2024_SpavkaSadimost = "req0924rep";

            /// <summary>
            /// Заявка 1 - Електронен печат с номер на акт
            /// </summary>
            public const string Request1_2024 = "req0124";

            /// <summary>
            /// Справка 5 и 6 ОС sheet2 
            /// </summary>
            public const string StatisticsIspn = "statIspn";

            /// <summary>
            /// Заявка 1 - Предварително валидиране на бюлетина при изпращане за подпис
            /// </summary>
            public const string CaisBulletinValidate = "cbv";

            /// <summary>
            /// Разрешено показване Справка трудови договори
            /// </summary>
            public const string EmploymentContractsEnabled = "ece";

            /// <summary>и
            /// Изпращане на хартиени призовки към ЕПЕП
            /// </summary>
            public const string SendPаperNotifications = "spn";

            /// <summary>
            /// Медиация
            /// </summary>
            public const string Mediation = "mediation";

            /// <summary>
            /// Заместване
            /// </summary>
            public const string Replacement = "replacement";

            /// <summary>
            /// Деактивиране на проверката за версия на дело
            /// </summary>
            public const string DisableRowVersion = "drv";

            /// <summary>
            /// Избор на 3 случайни акта
            /// </summary>
            public const string CheckThreeAct = "c3a";

            /// <summary>
            /// Заявка 9 статистика
            /// </summary>
            public const string Request9Stats = "req9stats";
        }

        public class TaskStates
        {
            /// <summary>
            /// Отменена задача
            /// </summary>
            public const int Cancel = 5;
        }

        public class MoneyTypeGroupings
        {
            //Уведомление за доброволно изпълнение - държавна такса
            public const int NtVolexStateFee = 1;

            //Уведомление за доброволно изпълнение - глоби
            public const int NtVolexFine = 2;

            //Уведомление за доброволно изпълнение - разноски
            public const int NtVolexExpense = 3;

            //Уведомление за доброволно изпълнение - възнаграждение за вещо лице/особен представител/
            public const int NtVolexEarning = 4;
        }

        public class JudgeLoadActivityCode
        {
            public const string Col1 = "1";
            public const string Col2 = "2";
            public const string Col3 = "3";
            public const string Col4 = "4";
            public const string Col5 = "5";
            public const string Col6 = "6";
            public const string Col7 = "7";
            public const string Col8 = "8";
            public const string Col9 = "9";
        }

        public class PersonMeasureKinds
        {
            public const int Probation = 2;
        }

        public class CaseSelectionChangeTypes
        {
            /// <summary>
            /// Преразпределение: дела на един съдия се преразпределят на нов
            /// </summary>
            public const int Prerazpredelenie = 1;
            public const int PrerazpredelenieJudge = 2;
        }
        public class CaseSelectionChangeStates
        {
            /// <summary>
            /// Ново
            /// </summary>
            public const int New = 1;

            public const int Declared = 2;
            public const int Saved = 3;
        }

        public class EesppPersonState
        {
            public const int Sent = 1;
            public const int Assigned = 2;
            public const int Confirmed = 3;
            public const int Declined = 4;
        }
        public class EesppLawyerState
        {
            public const int Assigned = 1;
            public const int Confirmed = 2;
            public const int Declined = 3;
        }
        public const bool Eispp2InstanceStructure = false;

        public class ElectionPersonStates
        {
            public const int Include = 1;
            public const int Exclude = 2;
            public const int TechError = 3;
        }
        public class ElectionGroupType
        {
            public const int Razsledvane = 1;
            public const int Control = 2;

        }
        public class ElectionDismissalTypes
        {
            public const int Otvod = 1;
            public const int SamoOtvod = 2;
            public const int Bolnichen = 3;
            public static int?[] FinalStates = { Otvod, SamoOtvod };
        }
        public class ElectionLogOperation
        {
            public const string AddGroup = "Добавяне на избор";
            public const string EditGroup = "Редакция на избор";
            public const string AddPerson = "Добавяне на лице";
            public const string EditPerson = "Редакция на лице";
            public const string DeclarationPerson = "Добавена декларация";
            public const string FinalizeList = "Финализиране на списък";
            public const string ElectionAdd = "Добавяне на сесия";
            public const string ElectionEdit = "Редактиране на сесия";
            public const string ElectionProtocolAdd = "Добавяне на протокол за избор";
            public const string ElectionProtocolSign = "Подписване на протокол за избор";
        }

        public class CourtRestrictionTypes
        {
            /// <summary>
            /// Забранява се регистрирането на иницииращи документи
            /// </summary>
            public const int DisableInitDocument = 1;

            /// <summary>
            /// Забранява се образуването на нови дела
            /// </summary>
            public const int DisableInitCase = 2;

            /// <summary>
            /// Забранява се редакция в разпределение ВКС
            /// </summary>
            public const int VKSPreviewEdit = 3;

            /// <summary>
            /// Забранява се редакция в критични данни
            /// </summary>
            public const int CriticalData = 4;

        }

        public class MainTransactionTypes
        {
            public const int Waiting = 1;
            public const int InUser = 2;
            public const int Finish = 3;
            public const int FastProcessForAssignment = 4;
            public const int FastProcessAssignedInCourt = 5;
            public const int FastProcessWatingForPayment = 6;

            public static int[] NotFinished = { Waiting, InUser };
        }

        /// <summary>
        /// Посока на бланката - Ищец-Ответник
        /// За обезпечителни заповеди
        /// </summary>
        public class ActBlankDirection
        {
            /// <summary>
            /// Права бланка - Лява страна ИЩЕЦ
            /// </summary>
            public const int LeftToRight = 1;

            /// <summary>
            /// Обратна бланка - Лява страна ОТВЕТНИК
            /// </summary>
            public const int RightToLeft = 2;
        }

        public enum ActAccessMode
        {
            ActBlank,
            ActDefaceBlank,
            MotiveBlank,
            MotiveDefaceBlank
        }

        public class CourtGroupKinds
        {
            /// <summary>
            /// Групи за разпределение на съдии - съществуващи функционалности
            /// </summary>
            public const int JudgeSelection = 1;

            /// <summary>
            /// Групи съдии/служители Специален достъп
            /// </summary>
            public const int SpecialAccess = 2;

            /// <summary>
            /// Група Централизирано разпределение на дела Заповедни производства
            /// </summary>
            public const int FastProcessCentral = 3;

            /// <summary>
            /// Група централизирано разпределение по подсъдност на дела Заповедни производства
            /// </summary>
            public const int FastProcessCentralDistributionJurisdiction = 4;

            /// <summary>
            /// Заместване
            /// </summary>
            public const int Replacement = 5;
        }

        /// <summary>
        /// Константи за тип ден
        /// </summary>
        public class DayTypeConstants
        {
            /// <summary>
            /// Работен ден
            /// </summary>
            public const int WorkingDay = 1;

            /// <summary>
            /// Почивен ден
            /// </summary>
            public const int DayOff = 2;
        }

        public class CaseFeatures
        {
            /// <summary>
            /// ИСПН, Избор на компетентност за образуване на дела
            /// </summary>
            public const string ISPN_HasCaseCompetence = "ispn_has_case_competence";

            /// <summary>
            /// ИСПН, Критерии за вписване на акт
            /// </summary>
            public const string ISPN_ActHasForRegistration = "ispn_act_has_for_registration";
        }

        /// <summary>
        /// Константи за шаблон за филтър на справка
        /// </summary>
        public class FilterTemplateTypeConstants
        {
            /// <summary>
            /// Специализирана справка
            /// </summary>
            public const int SpecializedReport = 1;
        }

        public class ActISPNReasonGroupings
        {
            /// <summary>
            /// Подадени молби шифри 21
            /// </summary>
            public const int StatisticsIspn21 = 1;

            /// <summary>
            /// Подадени молби шифри 24
            /// </summary>
            public const int StatisticsIspn24 = 2;

            /// <summary>
            /// Приключени производства шифри 21
            /// </summary>
            public const int StatisticsIspnFinish21 = 3;

            /// <summary>
            /// Приключени производства шифри 24
            /// </summary>
            public const int StatisticsIspnFinish24 = 4;

            /// <summary>
            /// отхвърлени с акт по същество шифри 21
            /// </summary>
            public const int StatisticsIspnRejected21 = 5;

            /// <summary>
            /// отхвърлени с акт по същество шифри 24
            /// </summary>
            public const int StatisticsIspnRejected24 = 6;

            /// <summary>
            /// Производства по подадени молби чл. 760з - Приключени
            /// </summary>
            public const int StatisticsRequest760Finish = 7;

            /// <summary>
            /// Производства по подадени молби чл. 760з - Брой решения за опрощаване 
            /// </summary>
            public const int StatisticsRequest760Forgive = 8;

            /// <summary>
            /// Производства по подадени молби чл. 760з - Брой решения за отхвърляне
            /// </summary>
            public const int StatisticsRequest760Reject = 9;

            /// <summary>
            /// Всички основания - за състояние преди 01.2026
            /// </summary>
            public const int CaseSessionAct_ISPN = 101;

            /// <summary>
            /// Основания за РНФЛ 
            /// </summary>
            public const int CaseSessionAct_RNFL = 102;
        }

        public class IspnKinds
        {
            /// <summary>
            /// ИСПН Дела, до 2025
            /// </summary>
            public const int LegacyISPN = 1;

            /// <summary>
            /// ИСПН Дела, Предприемачи
            /// </summary>
            public const int Entrepreneur = 2;

            /// <summary>
            /// ИСПН Дела, РНФЛ
            /// </summary>
            public const int Rnfl = 3;
        }

        public class RnflProcessTypes
        {
            /// <summary>
            /// Главно
            /// </summary>
            public const int Main = 1;
        }

        public class FPaliases
        {
            public const string FP_CompetencyBases = "fp_competencybases";
            public const string FP_MoneyClaimTypes = "fp_moneyclaimtypes";
            public const string FP_ClaimCircumstances = "fp_claimcircumstances";
            public const string FP_ExpenseTypes = "fp_expensetypes";
            public const string FP_DebtList = "fp_debtlist";

            public const string FileTypes = "filetypes";

            public static string[] FastProcessNomenclatures = { FP_CompetencyBases, FP_MoneyClaimTypes, FP_ClaimCircumstances, FP_ExpenseTypes, FP_DebtList };
        }

        public class BankFilePaymentTypeCodes
        {
            /// <summary>
            /// Приход
            /// </summary>
            public const int Credit = 1;

            /// <summary>
            /// Разход
            /// </summary>
            public const int Debit = 2;

            /// <summary>
            /// Сторно Приход
            /// </summary>
            public const int StornoCredit = 3;

            /// <summary>
            /// Сторно Разход
            /// </summary>
            public const int StornoDebit = 4;

        }

        public class BankFilePaymentTypes
        {
            /// <summary>
            /// Платежно нареждане
            /// </summary>
            public const int PaymentOrder = 1;

            /// <summary>
            /// ВПОС
            /// </summary>
            public const int VPos = 2;

            /// <summary>
            /// ПОС
            /// </summary>
            public const int Pos = 3;

            /// <summary>
            /// Наличен паричен превод
            /// </summary>
            public const int CashTransfer = 4;

            /// <summary>
            /// Други - не ги обработваме
            /// </summary>
            public const int Other = 5;

        }

        public class DeliveryItemMessage
        {
            public const string FastProcess = "Електронно заповедно дело";
        }

        /// <summary>
        /// Константи за статуси на среща за медиация
        /// </summary>
        public class MediationStateConstants
        {
            /// <summary>
            /// Насрочена
            /// </summary>
            public const int Scheduled = 1;

            /// <summary>
            /// Проведена
            /// </summary>
            public const int Held = 2;

            /// <summary>
            /// Пренасрочена
            /// </summary>
            public const int Rescheduled = 3;

            /// <summary>
            /// Непроведена
            /// </summary>
            public const int NotHeld = 4;

            /// <summary>
            /// Статуси, които позволяват редакция на среща
            /// </summary>
            public static int[] StateForEdit = { Scheduled, Rescheduled };

            /// <summary>
            /// Статуси за справка за срещи за медиация
            /// </summary>
            public static int[] ReportState = { Held, NotHeld };
        }

        /// <summary>
        /// Константи за място на което се провеждат срещите за медиация
        /// </summary>
        public class MediationLocationConstants
        {
            /// <summary>
            /// Съдебен център
            /// </summary>
            public const int CourtCenter = 1;
        }

        /// <summary>
        /// Константи за групи на резултати
        /// </summary>
        public class MediationResultGroupConstants
        {
            /// <summary>
            /// Прекратителни
            /// </summary>
            public const int Termination = 1;

            /// <summary>
            /// За спиране
            /// </summary>
            public const int Suspension = 2;

            /// <summary>
            /// Отложени
            /// </summary>
            public const int Postponed = 3;
        }

        /// <summary>
        /// Константи за видове срещи
        /// </summary>
        public class MediationTypeConstants
        {
            /// <summary>
            /// Информационна среща за процедура по медиация
            /// </summary>
            public const int MediationInformationMeeting = 1;

            /// <summary>
            /// Обща информационна среща в този център 
            /// </summary>
            public const int GeneralMediationInformationMeeting = 2;

            /// <summary>
            /// Среща за процедура по медиация
            /// </summary>
            public const int MediationMeeting = 3;

            /// <summary>
            /// Обща среща за процедура по медиация по повече дела в този център
            /// </summary>
            public const int GeneralMediationMeeting = 4;

            /// <summary>
            /// Информационна срещи
            /// </summary>
            public static int[] InformationMeetings = { MediationInformationMeeting, GeneralMediationInformationMeeting };

            /// <summary>
            /// Процедури
            /// </summary>
            public static int[] Meetings = { MediationMeeting, GeneralMediationMeeting };
        }

        /// <summary>
        /// Константи за тип оценка
        /// </summary>
        public class MediationCaseMediatorAppraisalTypeConstants
        {
            /// <summary>
            /// Подробна
            /// </summary>
            public const int Detailed = 1;

            /// <summary>
            /// Обобщена
            /// </summary>
            public const int Summary = 2;
        }

        /// <summary>
        /// Константи за вид в CourtDuty
        /// </summary>
        public class CourtDutyKindConstants
        {
            /// <summary>
            /// Заместване
            /// </summary>
            public const int Replacement = 1;
        }

        /// <summary>
        /// Константи за вид избор на медиатора в делото
        /// </summary>
        public class MediationTypeChoiceMediatorConstants
        {
            /// <summary>
            /// Избран от страна на съдебен център
            /// </summary>
            public const int ElectedCourtCenter = 1;

            /// <summary>
            /// Избран от страна по делото
            /// </summary>
            public const int ElectedPartyCase = 2;
        }

        /// <summary>
        /// Константи за основание към резултат на среща за медиация
        /// </summary>
        public class MmediationResultBaseConstants
        {
            /// <summary>
            /// С постигане на споразумение
            /// </summary>
            public const int ByReachingAagreement = 1;

            /// <summary>
            /// Справка за информационните срещи и процедурите по медиация - за колона: Причина за прекратяване
            /// </summary>
            public static int[] MediationProcedureReasonTermination = { 2, 3, 4, 5, 6 };
        }

        public class ExcelReportTemplateReportTypes
        {
            /// <summary>
            /// Нормалната статистика
            /// </summary>
            public const int Normal = 1;

            /// <summary>
            /// Медиация статистика
            /// </summary>
            public const int Mediation = 2;
        }

        public class InterestRateTypes
        {
            /// <summary>
            ///  Основен лихвен процент
            /// </summary>
            public const int OLP = 1;
        }

        public class NotificationAddressError
        {
            public const string Message = "Адресът е валиден при попълнени: Населено място и Улица № или Квартaл Блок/№";
        }
    }
}