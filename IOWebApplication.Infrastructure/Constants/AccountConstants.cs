using System.Security.Claims;

namespace IOWebApplication.Infrastructure.Constants
{
    public class AccountConstants
    {
        /// <summary>
        /// Разрешен достъп с парола
        /// </summary>
        public const bool PasswordLoginEnabled = false;
        public class Roles
        {
            /// <summary>
            /// 1. Регистратура
            /// </summary>
            public const string DocumentInit = "DOC_INIT";

            /// <summary>
            /// 2. Деловодство
            /// </summary>
            public const string DocumentEdit = "DOC_EDIT";

            /// <summary>
            /// 3.1. Разпределение на дела
            /// </summary>
            public const string CaseInit = "CASE_INIT";

            /// <summary>
            /// 3.2. Управление на дела
            /// </summary>
            public const string CaseEdit = "CASE_EDIT";

            /// <summary>
            /// 3.3. Архивиране
            /// </summary>
            public const string CaseArhive = "CASE_ARH";

            /// <summary>
            /// 3.4. Призоваване
            /// </summary>
            public const string DeliveryUser = "DEL_USER";

            /// <summary>
            /// 4. Счетоводство
            /// </summary>
            public const string MoneyAccount = "MONEY";

            /// <summary>
            /// 5. Статистика
            /// </summary>
            public const string Statistic = "STAT";

            /// <summary>
            /// 6. Супервайзор
            /// </summary>
            public const string Supervisor = "SUPER";

            /// <summary>
            /// 6.1. Супервайзор на структура
            /// </summary>
            public const string PowerUser = "POWER_USER";

            /// <summary>
            /// 6.2. Управление на магистрати и служители
            /// </summary>
            public const string HrAdmin = "HR_ADMIN";

            /// <summary>
            /// 7. Ограничен достъп
            /// </summary>
            public const string RestrictedAccess = "RESTR";

            /// <summary>
            /// 8. Администриране
            /// </summary>
            public const string Administrator = "ADMIN";

            /// <summary>
            /// 9. Администратор на инфраструктура
            /// </summary>
            public const string GlobalAdministrator = "GLOBAL_ADMIN";
        }

        public class Features
        {
            /// <summary>
            /// Възстановяване на премахнати документи
            /// </summary>
            public const string DocumentReactivate = "DOCUMENT_REACTIVATE";

            public class Modules
            {
                /// <summary>
                /// Управление на съдебна регистратура
                /// </summary>
                public const string Documents = "MODULE_DOCUMENTS";

                /// <summary>
                /// Управление на обща информация за дела
                /// </summary>
                public const string CaseData = "MODULE_CASE_DATA";

                /// <summary>
                /// Достъп до дела
                /// </summary>
                public const string CaseAccessData = "MODULE_CASE_ACCESS_DATA";

                /// <summary>
                /// Управление на уведомления по дела
                /// </summary>
                public const string CaseNotification = "MODULE_CASE_NOTIFICATION";

                public const string HrAdministration = "HR_ADMINISTRATION";
            }

            public static bool IsInFeature(ClaimsPrincipal userClaimsPrincipal, string feature)
            {
                bool result = false;
                if (userClaimsPrincipal == null)
                {
                    return result;
                }
                switch (feature)
                {
                    case Modules.Documents:
                        result = userClaimsPrincipal.IsInRole(Roles.DocumentInit)
                            || userClaimsPrincipal.IsInRole(Roles.DocumentEdit)
                            || userClaimsPrincipal.IsInRole(Roles.CaseInit);
                        break;
                    case DocumentReactivate:
                        result = (userClaimsPrincipal.IsInRole(Roles.DocumentInit)
                            || userClaimsPrincipal.IsInRole(Roles.DocumentEdit))
                            && userClaimsPrincipal.IsInRole(Roles.Supervisor);
                        break;
                    case Modules.CaseData:
                        result = userClaimsPrincipal.IsInRole(Roles.CaseInit)
                            || userClaimsPrincipal.IsInRole(Roles.CaseEdit)
                            || userClaimsPrincipal.IsInRole(Roles.DocumentEdit);
                        break;
                    case Modules.CaseAccessData:
                        result = userClaimsPrincipal.IsInRole(Roles.CaseInit)
                            || userClaimsPrincipal.IsInRole(Roles.DocumentEdit);
                        break;
                    case Modules.CaseNotification:
                        result = userClaimsPrincipal.IsInRole(Roles.DeliveryUser)
                                || userClaimsPrincipal.IsInRole(Roles.DocumentEdit);
                        break;

                    case Modules.HrAdministration:
                        result = userClaimsPrincipal.IsInRole(Roles.Administrator)
                                || userClaimsPrincipal.IsInRole(Roles.HrAdmin);
                        break;
                }
                if (!result)
                {
                    result = userClaimsPrincipal.IsInRole(Roles.GlobalAdministrator);
                }
                return result;
            }
        }
    }
}
