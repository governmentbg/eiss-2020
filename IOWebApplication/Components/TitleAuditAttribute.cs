using System;

namespace IOWebApplication
{
    /// <summary>
    /// Атрибут, с който се добавя запис в лога на заглавието на страницата за списъчни полета
    /// </summary>
    public class TitleAuditAttribute : Attribute
    {
        /// <summary>
        /// Вид операция за добавяне към лога
        /// </summary>
        public string Operation { get; set; }
    }
}
