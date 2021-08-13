﻿using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// EISPPPAKET
    /// Пакет за ЕИСПП
    /// </summary>
    [Serializable]
    [XmlRoot("EISPPPAKET")]
    public class EisppPackage : EisppProperties
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlIgnore]
        public int SourceType { get; set; }
        [XmlIgnore] 
        public int SourceId { get; set; }
        [XmlIgnore]
        public int CaseId { get; set; }

        [XmlIgnore]
        public int EventTypeId { get; set; }
        [XmlIgnore]
        public string Mode { get; set; }
        [XmlIgnore]
        public bool IsGeneratedEisppNumber { get; set; }



        [XmlIgnore]
        [Display(Name = "Изпращане към ЕИСПП")]
        public bool IsForSend { get; set; }

        [XmlIgnore]
        public bool IsForEdit { get; set; }
        [XmlIgnore]
        public int? PersonOldMeasureId { get; set; }
        [XmlIgnore]
        public int? PersonMeasureId { get; set; }
        [XmlIgnore]
        public bool SaveIfHaveDiff { get; set; }
        [XmlIgnore]
        public List<EisppCrimePersonVM> EisppCrimePersonDiff { get; set; }
        /// <summary>
        /// Създава празен обект за десериализация на пакет
        /// </summary>
        public EisppPackage()
        {

        }

        /// <summary>
        /// Създава пакет от определена структура изпращач
        /// </summary>
        /// <param name="senderStructure">structura_izp
        /// структура, която изпраща пакета,
        /// елемент от номенклатурата в Стандарт 7</param>
        public EisppPackage(int senderStructure)
        {
            SenderStructure = senderStructure;
        }

        /// <summary>
        /// Създава готов за изпращане пакет
        /// </summary>
        /// <param name="data">DATA - Данни за пакета</param>
        public EisppPackage(Data data)
        {
            SenderStructure = data.Context.StructureId;
            Data = data;
        }

        /// <summary>
        /// DATA
        /// Данни за пакета
        /// </summary>
        [XmlElement(ElementName = "DATA")]
        public Data Data { get; set; }
    }
}
