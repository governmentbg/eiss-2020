namespace IOWebApplication.Infrastructure.Constants
{
    public class AuditConstants
    {
        public class Operations
        {
            public const string Append = "Добавяне";
            public const string Update = "Редакция";
            public const string Delete = "Изтриване";
            public const string Init = "Образуване";
            public const string View = "Преглед";
            public const string List = "Списък";
            public const string ChoiceByList = "Добавяне/премахване от списък";
            public const string Print = "Печат";
            public const string GeneratingFile = "Генериране на файл";

            public static string[] ChangingOperations = { Append, Update, Delete };
        }
    }
}
