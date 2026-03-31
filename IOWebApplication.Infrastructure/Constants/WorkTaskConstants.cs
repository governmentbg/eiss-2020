using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public class WorkTaskConstants
    {
        public class Types
        {
            /// <summary>
            /// Разпределение на дело
            /// </summary>
            public const int Case_SelectLawUnit = 1;

            /// <summary>
            /// За отказ от образуване
            /// </summary>
            public const int Case_ForReject = 22;

            /// <summary>
            /// За резолюция
            /// </summary>
            public const int For_Resolution = 2;

            /// <summary>
            /// Пренасочване по компетенция
            /// </summary>
            public const int ReassignmentByCompetence = 3;

            /// <summary>
            /// Предаване за съгласуване
            /// </summary>
            public const int CaseSessionAct_SentToCoordinate = 4;

            /// <summary>
            /// Съгласуване на документ
            /// </summary>
            public const int CaseSessionAct_Coordinate = 5;

            /// <summary>
            /// Изпращане за подписване
            /// </summary>
            public const int CaseSessionAct_SentToSign = 6;
            public const int CaseSessionAct_Sign = 7;
            public const int CaseSessionActCoordination_Sign = 8;
            public const int CaseSessionActMotives_SentToSign = 9;
            public const int CaseSessionActMotives_Sign = 10;
            public const int Document_Sign = 11;
            public const int ForReport = 12;
            public const int DocumentDecision = 13;
            public const int DocumentResolution_SentToSign = 14;
            public const int DocumentResolution_Sign = 15;
            public const int ForDocumentResolution = 16;
            public const int ExecList_SentToSign = 17;
            public const int ExecList_Sign = 18;
            public const int SendFor_NewSelection = 19;
            public const int SendFor_NewSession = 20;
            public const int SendFor_Competency = 21;
            public const int CasePersonBulletin_SentToSign = 23;
            public const int CasePersonBulletin_Sign = 24;
            public const int CasePersonBulletin_SentToSignNewNumber = 25;

            public const int CaseSessionAct_SentMotiveToCoordinate = 26;
            public const int CaseSessionAct_MotiveCoordinate = 27;
            public const int DocumentForGlobalAssignment = 28;

            /// <summary>
            /// Уведомяване по чл.41, ал. 1 от Наредба 12
            /// </summary>
            public const int Notification_41_1_12 = 30;

            /// <summary>
            /// Уведомяване по чл.41, ал. 2 от Наредба 12
            /// </summary>
            public const int Notification_41_2_12 = 31;

            /// <summary>
            /// Уведомяване по чл.41, ал. 5 от Наредба 12
            /// </summary>
            public const int Notification_41_5_12 = 32;

            /// <summary>
            /// Задачи, които се приключват сами и неможе да се приключват ръчно
            /// </summary>
            public static int[] SelfCompleteTasks = {
                                                    Document_Sign,
                                                    DocumentResolution_Sign,
                                                    CaseSessionAct_SentToSign,
                                                    CaseSessionAct_Sign,
                                                    CaseSessionActCoordination_Sign,
                                                    CaseSessionActMotives_SentToSign,
                                                    CaseSessionActMotives_Sign,
                                                    CasePersonBulletin_SentToSign,
                                                    CasePersonBulletin_Sign,
                                                    CasePersonBulletin_SentToSignNewNumber,
                                                    ExecList_Sign
            };


            /// <summary>
            /// Задачи, които се приключват сами при създаването
            /// </summary>
            public static int[] AutoCompleteTasks = { DocumentForGlobalAssignment };
            /// <summary>
            /// задачи, които немогат да бъдат редактирани и пренасочвани
            /// </summary>
            public static int[] TaskCantUpdate = { Document_Sign, DocumentResolution_Sign, CaseSessionAct_Coordinate, CaseSessionAct_MotiveCoordinate, CaseSessionAct_Sign, CaseSessionActCoordination_Sign, CaseSessionActMotives_Sign };
            public static int[] TaskCanChangeUser = { CaseSessionAct_Sign, CaseSessionActCoordination_Sign, CaseSessionActMotives_Sign, CasePersonBulletin_Sign };
            /// <summary>
            /// Задачи, които не могат да бъдат пренасочвани през Преглед на всички задачи
            /// </summary>
            public static int[] TaskCantReroute = { Document_Sign, CaseSessionAct_Sign, CaseSessionActCoordination_Sign, CaseSessionActMotives_Sign, CasePersonBulletin_Sign };

            public static int[] ExpireConnectedTasks = { CaseSessionAct_Sign, CaseSessionActMotives_Sign, DocumentResolution_Sign, CasePersonBulletin_Sign };
            public static int[] AutomatedTasks = { DocumentForGlobalAssignment };

            public static int[] Notification_41 = { Notification_41_1_12, Notification_41_2_12, Notification_41_5_12 };
        }

        public class States
        {
            /// <summary>
            /// Нова задача
            /// </summary>
            public const int New = 1;

            /// <summary>
            /// Приета
            /// </summary>
            public const int Accepted = 2;

            /// <summary>
            /// Приключила
            /// </summary>
            public const int Completed = 3;

            /// <summary>
            /// Пренасочена
            /// </summary>
            public const int Redirected = 4;

            /// <summary>
            /// Отменена
            /// </summary>
            public const int Deleted = 5;

            public static readonly int[] NotFinished = { New, Accepted };
            public const int NotFinishedId = -2;
        }
        public class TaskExecution
        {
            /// <summary>
            /// От потребител
            /// </summary>
            public const int ByUser = 1;

            /// <summary>
            /// От отдел
            /// </summary>
            public const int ByOrganization = 2;
        }

        public static int[] LongDescriptionTasks = { SourceTypeSelectVM.CasePersonBulletin };
    }
}
