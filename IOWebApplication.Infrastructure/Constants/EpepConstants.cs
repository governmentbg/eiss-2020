namespace IOWebApplication.Infrastructure.Constants
{
    public class EpepConstants
    {
        public class UserTypes
        {
            public const int Person = 1;

            public const int Lawyer = 2;

            public const int Organization = 3;

            public const int ProcesutorOffice = 13;

            public static int[] AutoEpepUserAccess = { ProcesutorOffice };
        }
        /// <summary>
        /// Константи за поле "alias" от таблица "nom_code_mapping"
        /// </summary>
        public class Nomenclatures
        {
            /// <summary>
            /// Съдилища
            /// </summary>
            public const string Courts = "epep_courts";

            /// <summary>
            /// Видове входящи документи
            /// </summary>
            public const string IncommingDocumentTypes = "epep_in_doctype";

            /// <summary>
            /// Видове изходящи документи
            /// </summary>
            public const string OutgoingDocumentTypes = "epep_out_doctype";

            /// <summary>
            /// Основни видове дела
            /// </summary>
            public const string CaseGroups = "epep_casegroup";

            /// <summary>
            /// Точни видове дела
            /// </summary>
            public const string CaseTypes = "epep_casetype";

            /// <summary>
            /// Видове лица
            /// </summary>
            public const string PersonRoles = "epep_personrole";

            /// <summary>
            /// Видове лица - обратен мапинг от ЕПЕП
            /// </summary>
            public const string PersonRolesFromEPEP = "epep_personrole_fromepep";


            /// <summary>
            /// Видове адреси
            /// </summary>
            public const string AddressTypes = "epep_address_types";

            /// <summary>
            /// Видове адреси - обратен мапинг от ЕПЕП
            /// </summary>
            public const string AddressTypesFromEPEP = "epep_addresstypes_fromepep";

            /// <summary>
            /// Видове актове
            /// </summary>
            public const string ActTypes = "epep_acttype";

            /// <summary>
            /// Видове заседания
            /// </summary>
            public const string SessionTypes = "ispn_session_kind";

            /// <summary>
            /// Видове резултати заседания
            /// </summary>
            public const string SessionResults = "ispn_session_result";

            /// <summary>
            /// Вид на входящ документ за обжалване на съдебен акт
            /// </summary>
            public const string SessionActAppealDocType = "epep_appeal_doctype";

            /// <summary>
            /// ЕПРО - Роля на съдия в дело
            /// </summary>
            public const string EPRO_CaseRole = "epro_case_role";


            /// <summary>
            /// ЕПРО - Вид акт
            /// </summary>
            public const string EPRO_ActType = "epro_acttype";



            /// <summary>
            /// Вид движение на дело
            /// </summary>
            public const string CaseMigrationType = "epep_case_migration_type";

            /// <summary>
            /// Кодове на съдилища, обработвани в ЕИСС
            /// </summary>
            public const string CaseMigrationCourts = "epep_courts_with_eiss";


            public const string ExecProcessObligation = "epep_execprocess_obligation";
            public const string ExecListMoneyType = "epep_execlist_moneytype";

            public class PunishmentActivity
            {
            }
        }
        public enum ServiceMethod
        {
            Add = 1,
            Update = 2,
            Delete = 3
        }

        public class Methods
        {
            public const string Add = "add";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Manage = "manage";
            public const string Restart = "restart";
            public const string Result = "result";
            public const string DataChange = "datachange";

            public static string[] EditMethods = { Add, Update };

            public static string GetMethod(ServiceMethod enumValue)
            {
                switch (enumValue)
                {
                    case ServiceMethod.Add:
                        return Add;
                    case ServiceMethod.Update:
                        return Update;
                    case ServiceMethod.Delete:
                        return Delete;
                    default:
                        return string.Empty;
                }
            }

            public static ServiceMethod GetMethod(string methodName)
            {
                switch (methodName)
                {
                    case Add: return ServiceMethod.Add;
                    case Update: return ServiceMethod.Update;
                    case Delete: return ServiceMethod.Delete;
                    default:
                        return ServiceMethod.Update;
                }
            }
        }
        public class AttachedDocumentTypes
        {
            public const int IncommingDocument = 1;
            public const int OutgoingDocument = 2;
            public const int ActCoordination = 3;
            public const int ActCoordinationPublic = 4;
            public const int ActManualFile = 41;
            public const int SessionFastDocument = 5;
            public const int Summon = 10;
            public const int ElectronicDocument = 7;
            public const int ElectronicDocumentMain = 71;
            public const int ElectronicDocumentTimestamp = 72;
            /// <summary>
            /// json заявка от ЕПЕП
            /// </summary>
            public const int ElectronicDocumentRequest = 73;
        }
        public const int IntegrationMaxErrorCount = 20;
        public class IntegrationStates
        {
            public const int New = 1;
            public const int TransferOK = 2;
            public const int WaitForParentIdError = 3;
            public const int MissingCodeError = 4;
            public const int WaitingForReply = 5;
            public const int ReplyContainsError = 6;
            public const int MissingLawyerError = 17;
            public const int DataContentError = 18;
            public const int TransferError = 19;
            public const int TransferErrorLimitExceeded = 20;
            public const int MissingObjectEISS = 22;
            public const int DisabledByDelete = 30;

            public static int[] ReturnToMQStates = { WaitForParentIdError, TransferError };
            public static int?[] ReturnToMQStatesNulls = { WaitForParentIdError, TransferError };
            public static int[] ResetMQErrorStates = { MissingLawyerError, DataContentError, TransferError, TransferErrorLimitExceeded };
            public static int[] UnfinishedMQStates = { New, WaitForParentIdError, TransferError, TransferErrorLimitExceeded };
            public static int?[] UnfinishedMQStatesNulls = { New, WaitForParentIdError, TransferError, TransferErrorLimitExceeded };
        }

        public const string SummonTypeCode_CasesessionAct = "1";
        public const string SummonTypeCode_CaseSession = "4";
        public const string SummonKind_Generic = "1";

        public class AssignmentRoles
        {
            public const int Lawyer = 1;
            public const int Side = 2;
        }

        public class EpepDocumentMethods
        {
            /// <summary>
            /// Първоначално разпределяне, гледа компетенция
            /// </summary>
            public const string InitAssignment = "initassign";

            /// <summary>
            /// Централно Разпределение от движение
            /// </summary>
            public const string ForAssignment = "assign";

            /// <summary>
            /// Разпределение по компетенция от движение
            /// </summary>
            public const string ForAssignmentAddress = "assignadr";

            public static string[] RandomAssignmentMethods = { InitAssignment, ForAssignment, ForAssignmentAddress };
            public static string[] CheckCompetanceMethods = { InitAssignment, ForAssignmentAddress };
        }

        public class ExecProcessKinds
        {
            public const int FromExecList = 1;
            public const int FromCaseSessionAct = 2;
        }

        public class SubjectKinds
        {
            public const int Person = 1;
            public const int Entity = 2;
        }
    }
}
