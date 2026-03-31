using System;
using System.Runtime.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта Entity, използван в уеб услугата Юридическо лице
    /// </summary>
    
    public class Entity
    {
        /// <summary>
        /// Име на юридическото лице
        /// Полето е задължително
        /// </summary>
       
        public string Name { get; set; }

        /// <summary>
        /// ЕИК / Булстат
        /// Полето не е задължително
        /// </summary>
       
        public string Bulstat { get; set; }

        /// <summary>
        /// Адрес
        /// Полето не е задължително
        /// </summary>
       
        public string Address { get; set; }
    }
}