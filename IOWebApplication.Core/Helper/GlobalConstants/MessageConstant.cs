namespace IOWebApplication.Core.Helper.GlobalConstants
{
    public static class MessageConstant
    {
        /// <summary>
        /// Грешка
        /// </summary>
        public const string ErrorMessage = "ErrorMessage";

        /// <summary>
        /// Внимание
        /// </summary>
        public const string WarningMessage = "WarningMessage";

        /// <summary>
        /// Успех
        /// </summary>
        public const string SuccessMessage = "SuccessMessage";

        /// <summary>
        /// Не е избран (за Display Template-ите)
        /// </summary>
        public const string NotSelected = "Не е избран";

        /// <summary>
        /// Да (за Display Template-ите)
        /// </summary>
        public const string Yes = "Да";

        /// <summary>
        /// Не (за Display Template-ите)
        /// </summary>
        public const string No = "Не";

        public static class Values
        {
            public const string SaveOK = "Записът премина успешно.";
            public const string SaveFailed = "Проблем по време на запис.";
            public const string TimeoutSelectProtokol = "Сървърът е претоварен. Изберете бутон 'Запис' тново";
            public const string UpdateOK = "Обновяването премина успешно.";
            public const string UpdateFailed = "Проблем при обновяването на данните.";
            public const string FileNotFound = "Файлът не е намерен!";
            public const string FileUploadFailed = "Грешка при запис на Файл!";
            public const string RecordNotFound = "Не съществува такъв запис!";
            public const string WorkDayExist = "Съществува запис за избрания ден и съд!";
            public const string ActExpireOK = "Съдебният акт е премахнат успешно.";
            public const string CasePersonExpireOK = "Лицето е премахнато успешно.";
            public const string CasePersonAddressExpireOK = "Адресът към лицето е премахнато успешно.";
            public const string CaseSessionExpireOK = "Заседанието е премахнато успешно.";
            public const string CaseCrimeExpireOK = "Престъплението е премахнато успешно.";
            public const string CaseSessionMeetingExpireOK = "Сесията е премахнато успешно.";
            public const string CaseSessionResultExpireOK = "Резултатът от заседанието е премахнат успешно.";
            public const string CaseSessionActComplainExpireOK = "Обжалването е премахнато успешно.";
            public const string CaseNotificationExpireOK = "Уведомлението е премахнато успешно.";
            public const string DocumentExpireOK = "Документът е премахнат успешно.";
            public const string DescriptionExpireRequired = "Въведете причина за премахването.";
            public const string DeliveryAccountExpireOK = "Мобилният токен е премахнат успешно.";
            public const string EisppEventItemExpireOK = "Събитието към ЕИСПП е премахнат успешно.";
            public const string CasePersonLinkExpireOK = "Връзката е премахнато успешно.";
            public const string CourtLawUnitActivityExpireOK = "Дейността е премахнато успешно.";
            public const string EisppPunismentMpsNotSend = "В номенклатурата на ЕИСПП няма съответстващо наказание и то ще бъде филтрирано при изпращане на събитието.";
            public const string CasePersonSentencePunishmentExpireOK = "Наказанието е премахнато успешно.";
            public const string CasePersonMeasureExpireOK = "Мярката за процесуална принуда е премахната успешно.";
            public const string DeliveryAreaAddressExpireOK = "Районирането е премахнато успешно.";
            public const string DeliveryAreaExpireOK = "Района е премахнат успешно.";
            public const string CaseLoadIndexExpireOK = "Натоварването е премахнато успешно.";
        }
        public static class ValidationErrors{
            public const string DeliveryDateFuture = "Не може да въвеждате посещение с бъдеща дата и час.";
            public const string DeliveryDateBeforeRegDate = "Посещение преди дата на изготвяне ";
        }
    }
}
