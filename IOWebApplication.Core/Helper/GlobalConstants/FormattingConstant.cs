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

        public const string TinyMceTableDefStyle = @" 
             table.bordered {
                border-collapse: collapse;
            }
            table.bordered td {
               border: 1px solid #777;
            } 
            table.bordered th {
               border: 1px solid #777;
            } 

";
        public const string PrintTableDefStyle = @"
        table.table-report {
            border-collapse: collapse;
            table-layout: fixed;
            width: 190mm;
        }
            table.table-report th {
                padding: 3px 5px;
                border: 1px solid #777;
            }
            table.table-report td {
                padding: 3px 5px;
                border: 1px solid #777;
                white-space: -moz-pre-wrap !important; /* Mozilla, since 1999 */
                white-space: -webkit-pre-wrap; /* Chrome & Safari */
                white-space: -pre-wrap; /* Opera 4-6 */
                white-space: -o-pre-wrap; /* Opera 7 */
                white-space: pre-wrap; /* CSS3 */
                word-wrap: break-word; /* Internet Explorer 5.5+ */
                word-break: break-all;
                white-space: normal;
            }

.notification-table {
    width: 100%;
    height: 100%;
    border: 0;
    margin: 0px;
    border-collapse: collapse;
}

.notification-td1 {
    width: 40%;
}
.notification-td2 {
    width: 40%;
}
.notification-td3 {
    width: 20%;
    align: ""center"";
}
.header-notification-table{
   padding: 0px !important;
   border: 0px !important;
}
";
    }
}
