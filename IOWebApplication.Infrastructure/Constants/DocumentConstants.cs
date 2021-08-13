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

            public static int[] InDocsForEPEP = { InitialDocument, CompliantDocument };
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

        public class Types
        {
            public const int VKS_MolbaOtmyana47ZMTA = 318;
            public static int[] DocumentsNoNeedCaseInfo = { VKS_MolbaOtmyana47ZMTA };
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
    }
}
