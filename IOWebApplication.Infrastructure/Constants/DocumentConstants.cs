using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public class DocumentConstants
    {
        public class DocumentKind
        {
            /// <summary>
            /// Иницииращ документ
            /// </summary>
            public const int InitialDocument = 1;

            /// <summary>
            /// Съпровождащ документ
            /// </summary>
            public const int CompliantDocument = 2;

            /// <summary>
            /// Обща администрация
            /// </summary>
            public const int InAdministrationDocument = 3;

            /// <summary>
            /// Изходящ документ по дело
            /// </summary>
            public const int OutCompliantDocument = 6;

            public static int[] InDocsForEPEP = { InitialDocument, CompliantDocument, InAdministrationDocument };
        }

        public class DocumentDirection
        {
            /// <summary>
            /// Входящи документи
            /// </summary>
            public const int Incoming = 1;

            /// <summary>
            /// Изходящи документи
            /// </summary>
            public const int OutGoing = 2;

            /// <summary>
            /// Вътрешни документи
            /// </summary>
            public const int Internal = 3;
        }
        public class TemplateStates
        {
            /// <summary>
            /// Проект
            /// </summary>
            public const int Draft = 1;
        }

        public class ProcessPriority
        {
            /// <summary>
            /// По общия ред
            /// </summary>
            public const int Common = 1;

            /// <summary>
            /// Дело с кратки процесуални срокове
            /// </summary>
            public const int ShortNoticeCase = 2;

        }

        public class DeliveryGroups
        {
            /// <summary>
            /// По пощата
            /// </summary>
            public const int PostOffice = 1;

            /// <summary>
            /// През ЕПЕП
            /// </summary>
            public const int WebPortal = 5;

        }

        /// <summary>
        /// Константи за точен вид документ
        /// </summary>
        public class Types
        {
            /// <summary>
            /// Молба за кумулация
            /// </summary>
            public const int Init_MolbaKumulacia = 39;

            public const int VKS_MolbaOtmyana47ZMTA = 318;

            /// <summary>
            /// Становище
            /// </summary>
            public const int Opinion = 259;

            public static int[] DocumentsNoNeedCaseInfo = { VKS_MolbaOtmyana47ZMTA };

            public static int[] DocumentsMustHaveCaseInfo = { Init_MolbaKumulacia };


        }

        public class ResolutionTypes
        {
            public const int Resolution = 1;

            /// <summary>
            /// Разпореждане за преразпределение
            /// </summary>
            public const int ResolutionForSelection = 2;

            /// <summary>
            /// Разпореждане за насрочване
            /// </summary>
            public const int ResolutionForScheduling = 3;

        }

        /// <summary>
        /// Константи за DocumentRequestTypes
        /// </summary>
        public class ElectronicDocumentRequestTypes
        {
            /// <summary>
            /// 410
            /// </summary>
            public const string FastProcess410 = "R04410";

            /// <summary>
            /// 417
            /// </summary>
            public const string FastProcess417 = "R04417";

            /// <summary>
            /// Списък с 410 и 417
            /// </summary>
            public static string[] FastProcess = { FastProcess410, FastProcess417 };

            public class IDs
            {
                /// <summary>
                /// Заявление по чл.410
                /// </summary>
                public const int FastProcess410 = 1;

                /// <summary>
                /// Заявление по чл.417
                /// </summary>
                public const int FastProcess417 = 2;
                /// <summary>
                /// Заявление по чл. 417, ал. 1, т 3, 6 и 10
                /// </summary>
                public const int FastProcess417a1t3610 = 5;
            }
        }

        /// <summary>
        /// Константи за основен вид документи
        /// </summary>
        public class DocumentGroupConstants
        {
            /// <summary>
            /// Възражение  - за съпровождащ документ kind - 2
            /// </summary>
            public const int Objection = 20;

            /// <summary>
            /// Други - за съпровождащ документ kind - 2
            /// </summary>
            public const int Other = 26;
        }
    }
}
