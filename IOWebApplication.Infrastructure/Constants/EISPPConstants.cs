using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Constants
{
    public class EISPPConstants
    {
        public const int CountryBG = 8805;
        public const int UnknownPersonEGN = 1111111110;
        public class Gender
        {
            public const int Male = 706;
            public const int Female = 691;
        }
        public class EisppTableCode
        {
            /// <summary>
            /// ЕИСС - Видове престъпления
            /// </summary>
            public const string EISS_PNE = "eiss_pne";


            /// <summary>
            /// Мярка статус
            /// </summary>
            public const string MeasureType = "214";

            /// <summary>
            /// Вид мярка
            /// </summary>
            public const string MeasureStatus = "215";

            /// <summary>
            /// Вид документ
            /// </summary>
            public const string PersonalDocumentType = "254";

            /// <summary>
            /// Вид събитие
            /// sbesid
            /// </summary>
            public const string EventType = "238";

            /// <summary>
            /// Статус на НП за престъпление
            /// Номенклатура nmk_nprpnests
            /// </summary>
            public const string CiminalProceedingCrime = "200";

            /// <summary>
            /// dlosig
            /// точен вид дело
            /// системен код на елемент от nmk_dlosig
            /// </summary>
            public const string ExactCaseType = "204";

            /// <summary>
            /// dlovid
            /// вид дело
            /// </summary>
            public const string CaseType = "203";

            /// <summary>
            /// dloncnone
            /// начин на образуване на дело
            /// Системен код на елемент от nmk_dloncnone
            /// </summary>
            public const string CaseSetupType = "253";

            /// <summary>
            /// dlopmt
            /// Основание за възлагане на дело
            /// Не е задължително
            /// Номенклатура nmk_osnvzg
            /// </summary>
            public const string CaseReason = "15334";

            /// <summary>
            /// dlosts
            /// Статус на дело
            /// </summary>
            public const string CaseStatus = "205";

            /// <summary>
            /// dlosprvid
            /// вид съдебно производство
            /// системен код на елемент от nmk_dlosprvid
            /// </summary>
            public const string LegalProceedingType = "223";

            /// <summary>
            /// pneotdtip
            /// Тип на дата на престъпление
            /// Номенклатура nmk_pneotdtip
            /// </summary>
            public const string StartDateType = "1632";

            /// <summary>
            /// pnests
            /// Статус на престъпление
            /// Номенклатура nmk_pnests
            /// </summary>
            public const string CrimeStatus = "206";

            /// <summary>
            /// pnestpdvs
            /// Степен на довършеност на престъпление
            /// номенклатура nmk_pnestpdvs
            /// </summary>
            public const string CompletitionDegree = "207";

            /// <summary>
            /// scqrlq в САС и XSD
            /// fzlpnerlq в Описание на обектите
            /// Номенклатура nmk_scqrlq / nmk_fzlpnerlq
            /// </summary>
            public const string CrimeSanctionRole = "220";

            /// <summary>
            /// fzlpol
            /// Пол
            /// Номенклатура nmk_fzlpol
            /// </summary>
            public const string Gender = "216";

            /// <summary>
            /// adrtip
            /// Тип на адрес
            /// Номенклатура nmk_adrtip
            /// </summary>
            public const string AddressType = "217";

            /// <summary>
            /// scqvid
            /// Вид процесуална санкция
            /// Номенклатура nmk_scqvid
            /// </summary>
            public const string SanctionType = "225";

            /// <summary>
            /// nprfzlkcv
            /// Качество на лице в НП
            /// Номенклатура nmk_nprfzlkcv
            /// </summary>
            public const string PersonRole = "221";

            /// <summary>
            /// nprfzlsts
            /// Статус на НП за лице
            /// номенклатура nmk_nprfzlsts
            /// </summary>
            public const string PersonStatus = "244";

            // <summary>
            /// nprfzlosn
            /// Основание за статус на НП за лице
            /// Основания за налагане на административно наказание ??? 
            /// номенклатура nmk_osn_adm_nkz
            /// </summary>
            public const string PersonReasonAdmNkz = "15458";


            /// <summary>
            /// nprfzlosn
            /// Основание за статус на НП за лице
            /// public const string PersonStatusReason = "242";
            /// nflgrp
            /// Група на на НФЛ
            /// Номенклатура nmk_nflgrp
            /// </summary>
            public const string EntityGroup = "8621";

            /// <summary>
            /// nfljrdstt
            /// Юридически статут на НФЛ
            /// Номенклатура nmk_nfljrdstt
            /// </summary>
            public const string EntityStatus = "8620";

            /// <summary>
            /// nflvid
            /// Вид на НФЛ
            /// Номенклатура nmk_nflvid
            /// </summary>
            public const string EntityType = "8642";

            /// <summary>
            /// Събития за едно лице
            /// </summary>
            public const string OnePersonEvent = "272";

            /// <summary>
            /// Видове движение
            /// </summary>
            public const string MigrationType = "246";

            /// <summary>
            /// Видове срокове
            /// </summary>
            public const string SrokType = "230";

            /// <summary>
            /// pbcvid  nmk_vid_pbc Видове пробационни мярки
            /// </summary>
            public const string ProbationMeasureType = "13629";

            /// <summary>
            /// pbcmered nmk_pbcu  Мерни единици за количество на пробационна мярка 
            /// </summary>
            public const string MeasureUnit = "14488";

            /// <summary>
            /// nkztip  Типове наказания
            /// </summary>
            public const string PunishmentType = "208";

            /// <summary>
            /// nkzvid Видове наказания
            /// </summary>
            public const string PunishmentKind = "209";

            /// <summary>
            /// nkzncn 210	Начини на изтърпяване на наказание
            /// </summary>
            public const string ServingType = "210";

            /// <summary>
            /// nkzakt 	Активности на наказание	
            /// </summary>
            public const string PunishmentActivity = "211";

            /// <summary>
            ///  nkzrjm 234 Видове режими на изтърпяване на наказание
            /// </summary>
            public const string PunishmentRegime = "234";

            /// <summary>
            ///  sbhvid 228 Вид на характеристика
            /// </summary>
            public const string FeatureType = "228";

            /// <summary>
            /// sbhstn 231 Стойност на хараkтеристика
            /// </summary>
            public const string FeatureVal = "231";

            /// <summary>
            /// sbedkpvid
            /// Вид документ номенклатура 224 или 11993
            /// </summary>
            public const string DocumentType = "224";

            /// <summary>
            /// sbcetn
            /// Етническа група
            /// Номенклатура nmk_sbcetn 314
            /// </summary>
            public const string ЕthnicGroup = "314";

            /// <summary>
            /// sbcobr
            /// Образование
            /// Номенклатура nmk_sbcobr 311
            /// </summary>
            public const string Education = "311";

            /// <summary>
            /// sbcple
            /// Пълнолетие
            /// Номенклатура nmk_sbcple 309
            /// </summary>
            public const string LawfulAge = "309";

            /// <summary>
            /// sbcrcd
            /// Рецидив
            /// Номенклатура nmk_sbcrcd 308
            /// </summary>
            public const string Relaps = "308";


            /// <summary>
            /// sbcspj
            /// Семейно положение
            /// Номенклатура nmk_sbcspj 310
            /// </summary>
            public const string MeritalStatus = "310";

            /// <summary>
            /// sbctrd
            /// Трудова активност
            /// Номенклатура nmk_sbctrd 312
            /// </summary>
            public const string LaborActivity = "312";

            /// <summary>
            /// sbcznq
            /// Занятие
            /// Номенклатура nmk_sbcznq  nmk_fzlpne_znt 1504
            /// </summary>
            public const string Occupation = "1504";

            /// <summary>
            /// sbcrge
            /// Предишни регистрации
            /// Номенклатура nmk_sbcrge 12478
            /// </summary> 
            public const string FormerRegistrations = "12478";

            /// <summary>
            /// adrloc
            /// Видове локализация на място
            /// Номенклатура nmk_adrloc 1724
            /// </summary> 
            public const string Localization = "1724";

            /// <summary>
            /// Причини за изпращане на дело
            /// </summary> 
            public const string MigrationReason = "218";
        }
        public class EventType
        {
            /// <summary>
            /// Редактиране на събитие
            /// </summary>
            public const int DeleteEvent = -1;

            /// <summary>
            /// Редактиране на събитие
            /// </summary>
            public const int ChangeEvent = -2;

            /// <summary>
            /// Редактиране на дело
            /// </summary>
            public const int ChangeCase = -3;

            /// <summary>
            /// Редактиране на лице/престъпление
            /// </summary>
            public const int ChangePersonCrime = -4;

            /// <summary>
            /// Редактиране на лице
            /// </summary>
            public const int ChangePerson = -5;

            /// <summary>
            /// Редактиране на престъпление
            /// </summary>
            public const int ChangeCrime = -7;

            /// <summary>
            /// Образуване на съдебно дело
            /// </summary>
            public const int CreateCase = 871;

            /// <summary>
            /// Получаване на дело
            /// </summary>
            public const int GetCase = 913;

            /// <summary>
            /// Получаване на дело
            /// </summary>
            public const int SendCase = 859;
            /// <summary>
            /// Вземане на мярка за процесуална принуда
            /// </summary>
            public const int CoercionMeasureCreate = 947;

            /// <summary>
            /// Отменяне на мярка за процесуална принуда
            /// </summary>
            public const int CoercionMeasureCancellation = 888;

            /// <summary>
            /// Изменяне на мярка за процесуална принуда
            /// </summary>
            public const int CoercionMeasureChange = 863;


            /// <summary>
            /// Изменяне на мярка за процесуална принуда
            /// </summary>
            public const int CoercionMeasureRefused = 877;

            /// <summary>
            /// Искане за преместване на НД
            /// </summary>
            public const int AskForMove = 861;

            /// <summary>
            /// Връщане на НД за доразследване на прокурор
            /// </summary>
            public const int ReturnToProsecutor = 943;

            /// <summary>
            /// Връщане на НД за доразследване на прокурор
            /// </summary>
            public const int ReturnToFirst = 945;

            /// <summary>
            ///Приемане на жалба
            /// </summary>
            public const int ComplaintReceived = 915;


            /// <summary>
            ///Предсрочно освобождаване
            /// </summary>
            public const int EarlyRelease = 925;

            /// <summary>
            ///Изменение на присъда по решение на чужд съд	14127
            /// </summary>
            public const int ChangeExternalSentence = 14127;

            /// <summary>
            ///Образуване на съдебно производство при трансфер от друга държава	14130
            /// </summary>
            public const int CreateOnExternal = 14130;


            /// <summary>
            ///Отказ за признаване и изпълнение на присъда на чуждестранен съд	14128
            /// </summary>
            public const int RefuseOnExternal = 14128;

            /// <summary>
            ///Приемане на чужда присъда за изпълнение
            /// </summary>
            public const int ReciveSentence = 914;

            /// <summary>
            /// Обединяване на НД
            /// </summary>
            public const int CaseUnion = 898;
            public const int CaseUnionInner = 1721;
        }


        public class SidType
        {
            /// <summary>
            /// Събитие
            /// </summary>
            public const string Event = "Event";

            /// <summary>
            /// Дело
            /// </summary>
            public const string Case = "Case";

            /// <summary>
            /// Статус Дело
            /// </summary>
            public const string CaseStatus = "CaseStatus";

            /// <summary>
            /// Идентификатор за движение на дело
            /// </summary>
            public const string GetCaseMigration = "GetCaseMigration";

            /// <summary>
            /// Лице
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// МПП
            /// </summary>
            public const string Measure = "Measure";

            /// <summary>
            /// Престъпление
            /// </summary>
            public const string Crime = "Crime";

            /// <summary>
            /// Лице в Престъпление
            /// </summary>
            public const string PersonCrime = "PersonCrime";

            /// <summary>
            /// Наказание на Лице в Престъпление
            /// </summary>
            public const string CrimePunishment = "CrimePunishment";


            /// <summary>
            /// Санкция на Лице в Престъпление
            /// </summary>
            public const string CrimeSanction = "CrimeSanction";

            /// <summary>
            /// Срок за обжалване
            /// </summary>
            public const string Srok = "Srok";

            /// <summary>
            /// Характеристика на събитие
            /// </summary>
            public const string Feature = "Feature";

            /// <summary>
            /// Наказание
            /// </summary>
            public const string Punishment = "Punishment";

            /// <summary>
            /// Пробационна мярка
            /// </summary>
            public const string ProbationMeasure = "ProbationMeasure";

            /// <summary>
            ///Статистически данни за субект на престъпление
            /// </summary>
            public const string SubjectStatisticData = "SubjectStatisticData";
            /// <summary>
            ///Статистически данни за субект на престъпление
            /// </summary>
            public const string SubjectStatisticDataXML = "SubjectStatisticDataXML";

            /// <summary>
            /// Свързани дела от институции
            /// </summary>
            public const string CaseCauseInstitution = "CaseCauseInstitution";

            /// <summary>
            /// Свързани дела
            /// </summary>
            public const string CaseCause = "CaseCause";

            /// <summary>
            /// Адреси
            /// </summary>
            public const string Address = "Address";
        }
        /// <summary>
        /// Тип Санкция на Лице в Престъпление
        /// Номенклатура nmk_scqvid
        /// </summary>
        public class SanctionType {
            /// <summary>
            /// свързва с НП
            /// </summary>
            public const int ConnectToCrime = 987;

            /// <summary>
            /// оправдава
            /// </summary>
            public const int Innocence = 978;

            /// <summary>
            /// осъжда
            /// </summary>
            public const int Guilty = 979;
        }
        /// <summary>
        /// Вид наказание
        /// </summary>
        public class PunishmentType
        {
            /// <summary>
            /// Наказание, което се изтърпява
            /// </summary>
            public const int InЕffect = 1354;

            /// <summary>
            /// Наказание за изтърпяване
            /// </summary>
            public const int ForExecution = 721;

            /// <summary>
            /// Общо наказание по присъда
            /// </summary>
            public const int Union = 745;
        }

        /// <summary>
        /// Вид наказание
        /// </summary>
        public class PunishmentActivity
        {
            /// <summary>
            /// наложено
            /// </summary>
            public const int Imposed = 1108;
            /// <summary>
            /// кумулирано
            /// </summary>
            public const int Cumulated = 1106;
        }
        /// <summary>
        /// Резултат от присъда оправдателна/осъдителна
        /// </summary>
        public class SentenceResultType {
            /// <summary>
            /// oправдателна
            /// </summary>
            public const int Innocence = 824;
            /// <summary>
            /// осъдителна
            /// </summary>
            public const int Guilty = 825;
        }

        
        public class PunishmentVal {
            /// <summary>
            /// Наказание за което се въвежда период
            /// </summary>
            public const string period = "period";

            /// <summary>
            /// Наказание за което се въвежда ефективен период 
            /// </summary>
            public const string effective_period = "effective_period";

            /// <summary>
            /// Наказание за което се въвежда начало и режим
            /// </summary>
            public const string efective = "efective";

            /// <summary>
            /// Наказание за което се въвежда условен период
            /// </summary>
            public const string probation_period = "probation_period";

            /// <summary>
            /// Глоба
            /// </summary>
            public const string fine = "fine";

            /// <summary>
            /// Наказание за което се въвеждат данни за пробация
            /// </summary>
            public const string probation = "probation";
        }

        public class FeatureType
        {
            /// <summary>
            /// Тип на присъда
            /// </summary>
            public const int SentenceType = 971;

            /// <summary>
            /// Прокуратура
            /// </summary>
            public const int StructurePrk = 1102;


            /// <summary>
            /// Прокуратура
            /// </summary>
            public const int StructureCourt = 1103;

            /// <summary>
            /// Държава Гражданства
            /// </summary>
            public const int Country = 8619;
        }

     
        public class PersonProceduralCoercionMeasureStatus
        {
            /// <summary>
            /// наложена мярка
            /// </summary>
            public const string Imposed = "1107";

            /// <summary>
            /// отменена мярка
            /// </summary>
            public const string Canceled = "1116";

            /// <summary>
            /// отказана мярка
            /// </summary>
            public const string Refused = "1115";
        }

       
        public class EisppMapping
        {
            public const string CaseTypes = "eispp_casetype";

            public const string PersonInCrimeRole = "eispp_person_in_crime_role";

            public const string ResSid = "eispp_res_sid";

            public const string CaseTypeCause = "eispp_dloosn_dlovid";

            public const string CaseTypeCauseIstitution = "eispp_dloosn_institution";

            public const string PunismentKindMap = "eispp_punisment_kind";

            public const string PunismentPeriodMap = "eispp_punisment_period";

            public const string MappingPBC_TypeValue = "eispp_pbc_type_value";

            public const string MappingActType = "eispp_act_type";

            public const string GeneriraneNumDoc = "eispp_new_num_doc";

            public const string NumberPrefix = "eispp_num_prefix";

            public const string CaseTypeCauseExactType = "eispp_dlovid_dlosig";

            public const string Relaps = "eispp_relaps";

            public const string ComplaintType = "eispp_complaint_type";
        }
        public class EventKind 
        {
            public const string OldEvent = "sbezvk";

            public const string NewEvent = "sbereg";
        }
        public class GeneriraneDocGroup
        {
            public const string Iskane = "eispp_iskane";
            public const string Tujba = "eispp_tujba";
        }
        public class ServingType
        {
            public const int Efective = 763;
            public const int Probation = 764;
            public const int EarlyRelease = 789;
        }

        public class PunismentKind
        {
            public const int nkz_dojiv_zatvor = 654;
            public const int nkz_dojiv_zatvor_bez_zamiana = 655;
            public const int nkz_lishavane_ot_svoboda = 704;
            public const int probation = 14344;
        }
        public class StartDateType
        {
            // pneotdper
            public const int pneotdper = 1630;
        }
        public class InitDocumentType
        {
            public const int IskaneExternal = 54;
        }
        public class LegalProceedingType
        {
            //ПИС - Производство на ПИС по наказателни дела
            public const int PIS_ND = 1018;
            //ПИС - Производство по изпълнение на ЕЗА
            public const int PIS_EZA = 1900001825;

            //ПИС - Производство във връзка с международното сътрудничество по наказателни дела	14119
            public const int PIS_ExternalND = 14119;

        }
    } 
}
