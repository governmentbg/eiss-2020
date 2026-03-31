using System;
using System.Runtime.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта Person, използван в уеб услугата Физическо лице
    /// </summary>
    
    public class Person
    {
        /// <summary>
        /// Име
        /// Полето не е задължително
        /// </summary>
       
        public string Firstname { get; set; }

        /// <summary>
        /// Бащино име
        /// Полето не е задължително
        /// </summary>
       
        public string Secondname { get; set; }

        /// <summary>
        /// Фамилно име
        /// Полето не е задължително
        /// </summary>
       
        public string Lastname { get; set; }

        /// <summary>
        /// ЕГН
        /// Полето не е задължително
        /// </summary>
       
        public string EGN { get; set; }

        /// <summary>
        /// Адрес
        /// Полето не е задължително
        /// </summary>
       
        public string Address { get; set; }
    }
}