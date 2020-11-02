// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Core.Helper.GlobalConstants
{
    public static class FormattingConstant
    {
        /// <summary>
        /// Formatting of date dd.MM.yyyy - HH:mm:ss;
        /// </summary>
        public const string DateFormat = "dd.MM.yyyy - HH:mm:ss";

        /// <summary>
        /// Formatting of date dd.MM.yyyy;
        /// </summary>
        public const string NormalDateFormat = "dd.MM.yyyy";
        
        /// <summary>
        /// Formatting of date dd.MM.yyyy - HH:mm;
        /// </summary>
        public const string NormalDateFormatHHMM = "dd.MM.yyyy - HH:mm";

        /// <summary>
        /// Formatting time to HH:mm
        /// </summary>
        public const string NormalTimeFormat = "HH:mm";

        /// <summary>
        /// Formatting decimal to #00.00
        /// </summary>
        public const string DecimalValueFormat = "#0.00";

        /// <summary>
        /// Formatting decimal to #00
        /// </summary>
        public const string IntValueFormat = "N";

        public const string DateFormatJS = "DD.MM.YYYY - HH:mm:ss";
        public const string DateFormatJSHHMM = "DD.MM.YYYY - HH:mm";
        public const string NormalDateFormatJS = "DD.MM.YYYY";
    }
}
