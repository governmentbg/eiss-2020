// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    public class RnflConstants
    {
        public class TargetMethods
        {
            public const string Case = "case";
            public const string Debtor = "debtor";
            public const string Syndic = "syndic";
            public const string Act = "act";
            public const string Document = "document";
            public const string DocumentFile = "document_file";
            public const string ActPrivate = "act_private";
            public const string ActPublic = "act_public";
            public const string Appeal = "appeal";
            public const string AppealFile = "appeal_file";
            public const string AppealActPrivate = "appeal_act_private";
            public const string AppealActPublic = "appeal_act_public";
            public const string Summon = "summon";
            public const string SummonFile = "summon_file";
        }

        public class CodeMapping
        {
            /// <summary>
            /// Правни основания, стартиращи производството в РНФЛ
            /// </summary>
            public const string StartLegalBase = "rnfl_start_legal_base";


            /// <summary>
            /// Видове роли на страна
            /// </summary>
            public const string PersonRoles = "rnfl_person_roles";

            /// <summary>
            /// Видове адреси
            /// </summary>
            public const string AddressTypes = "rnfl_address_types";

            /// <summary>
            /// Видове документи - молба за стартиране на процедурата
            /// </summary>
            public const string DocumentTypes = "rnfl_document_types";

            /// <summary>
            /// Видове идентификатори на лица
            /// </summary>
            public const string UicTypes = "rnfl_uic_types";

            /// <summary>
            /// Видове актове
            /// </summary>
            public const string ActTypes = "rnfl_act_types";

            /// <summary>
            /// Видове основания
            /// </summary>
            public const string LegalBases = "rnfl_legal_bases";

            /// <summary>
            /// Видове призовки/уведомления
            /// </summary>
            public const string NotificationTypes = "rnfl_notification_types";

            /// <summary>
            /// Видове жалби/Appeal
            /// </summary>
            public const string AppealTypes = "rnfl_appeal_types";
        }

        public class RnflSourceTypeSelectVM
        {
            public const int InsolvencyDocument = 1;

            /// <summary>
            /// Други файлове към документ
            /// </summary>
            public const int InsolvencyDocumentFile = 2;

            public const int EpzeuApplication = 20;

            /// <summary>
            /// Необезличен файл към акт
            /// </summary>
            public const int InsolvencyActPrivate = 31;

            /// <summary>
            /// Обезличен файл къв акт
            /// </summary>
            public const int InsolvencyActPublic = 32;

            /// <summary>
            /// Прикачени файлове
            /// </summary>
            public const int InsolvencyActFile = 33;

            /// <summary>
            /// Прикачени файлове към жалба
            /// </summary>
            public const int InsolvencyAppeal = 41;

            /// <summary>
            /// Прикачени файлове към въззивен/касационен акт към жалба
            /// </summary>
            public const int InsolvencyAppealActPrivate = 42;

            /// <summary>
            /// Прикачени файлове към въззивен/касационен акт към жалба - Обезличени
            /// </summary>
            public const int InsolvencyAppealActPublic = 43;

            /// <summary>
            /// Прикачени файлове към въззивен/касационен акт към жалба
            /// </summary>
            public const int InsolvencySummon = 51;

            /// <summary>
            /// Допустими прикачени типове файлове към InsolvencyAct
            /// </summary>
            public static int[] InsolvencyActEiss = { InsolvencyActPrivate, InsolvencyActPublic, InsolvencyActFile, InsolvencyAppeal, InsolvencyAppealActPrivate, InsolvencyAppealActPublic, InsolvencySummon };
        }
    }
}
