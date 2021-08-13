using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public class EpepConstants
    {
        public class UserTypes
        {
            public const int Person = 1;
            public const int Lawyer = 2;
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

        public const string SummonTypeCode_Prizovka = "4";
        public const string SummonKind_Generic = "1";
    }
}
