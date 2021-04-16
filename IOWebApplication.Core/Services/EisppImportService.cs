// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class EisppImportService : BaseService, IEisppImportService
    {
        public EisppImportService(
          IRepository _repo,
          ILogger<EisppImportService> _logger
   )
        {
            repo = _repo;
            logger = _logger;
        }
        public List<EisppImportEktte> GetEktteFromExcel(byte[] excelContent, int fromRow)
        {
            NPoiExcelService excelService = new NPoiExcelService(excelContent, 0);
            int row = fromRow;
            bool haveData = true;
            var ektteItems = new List<EisppImportEktte>();
            while (haveData)
            {
                var item = new EisppImportEktte();
                item.Row = row;
                excelService.rowIndex = row;
                excelService.colIndex = 0;
                item.Oblast = excelService.GetValue();
                item.Obstina = excelService.GetValue();
                item.TypeNM = excelService.GetValue();
                item.Name = excelService.GetValue();
                item.Rajon = excelService.GetValue();
                item.SystemCode = excelService.GetValue();
                item.SystemName = excelService.GetValue();
                item.EktteCode = excelService.GetValue();
                item.Active = excelService.GetValue();
                item.DateFrom = excelService.GetValue();
                item.DateTo = excelService.GetValue();
                haveData = !string.IsNullOrEmpty(item.Oblast);
                if (haveData)
                    ektteItems.Add(item);
                row++;
            }
            return ektteItems;
        }
        public bool ImportEktteRajon(List<EisppImportEktte> ektteItems)
        {
            foreach (var item in ektteItems.Where(x => !string.IsNullOrEmpty(x.Rajon)))
            {
                var rajons = repo.AllReadonly<EkRegion>()
                                 .Where(x => x.Raion == item.EktteCode)
                                 .ToList();
                if (rajons.Count == 1)
                {
                    var rajon = rajons.First();
                    rajon.EisppCode = item.SystemCode;
                    repo.Update(rajon);
                    repo.SaveChanges();
                }
                else
                {
                    logger.LogDebug($"Не намира район {item.Rajon}");
                }
            }
            return true;
        }

        public List<EisppImportNomenclature> GetNomenclatureFromExcel(byte[] excelContent, int fromRow)
        {
            NPoiExcelService excelService = new NPoiExcelService(excelContent, 0);
            int row = fromRow;
            bool haveData = true;
            var nomItems = new List<EisppImportNomenclature>();
            int blankRows = 0;
            string groupCode = string.Empty;
            while (blankRows < 10)
            {
                var item = new EisppImportNomenclature();
                item.Row = row;
                excelService.rowIndex = row;
                excelService.colIndex = 0;
                item.GroupName = excelService.GetValue();
                item.Name = excelService.GetValue();
                item.SystemCode = excelService.GetValue();
                item.SystemName = excelService.GetValue();
                item.Active = excelService.GetValue();
                item.DateFrom = excelService.GetValue();
                item.DateTo = excelService.GetValue();
                item.GroupCode = groupCode;
                haveData = !string.IsNullOrEmpty(item.GroupName) && !string.IsNullOrEmpty(item.SystemCode);
                if (haveData)
                {
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        groupCode = item.SystemCode;
                        item.GroupCode = null;
                    }
                    blankRows = 0;
                    nomItems.Add(item);
                }
                else
                {
                    blankRows++;
                }
                row++;
            }
            return nomItems;
        }
        public List<EisppImportStructure> GetStructureFromExcel(byte[] excelContent, int fromRow)
        {
            NPoiExcelService excelService = new NPoiExcelService(excelContent, 0);
            int row = fromRow;
            bool haveData = true;
            var structureItems = new List<EisppImportStructure>();
            int blankRows = 0;
            while (blankRows < 10)
            {
                var item = new EisppImportStructure();
                item.Row = row;
                excelService.rowIndex = row;
                excelService.colIndex = 0;
                item.SystemCode = excelService.GetValue();
                item.SystemName = excelService.GetValue();
                item.Name = excelService.GetValue();
                item.TypeName = excelService.GetValue();
                item.EisppCodePrefix = excelService.GetValue();
                item.Active = excelService.GetValue();
                item.DateFrom = excelService.GetValue();
                item.DateTo = excelService.GetValue();
                haveData = !string.IsNullOrEmpty(item.Name);
                if (haveData)
                {
                    blankRows = 0;
                    structureItems.Add(item);
                }
                else
                {
                    blankRows++;
                }
                row++;
            }
            return structureItems;
        }

        public List<EisppImportStructure> GetClosedStructure(List<EisppImportStructure> qList)
        {
            var result = new List<EisppImportStructure>();
            DateTime endDate = new DateTime(2999, 12, 31);
            foreach (var structure in qList)
            {
                if (!string.IsNullOrEmpty(structure.SystemCode))
                {
                    var institution = repo.AllReadonly<Institution>()
                                    .Where(x => x.EISPPCode == structure.SystemCode)
                                    .FirstOrDefault();
                    if (institution != null && institution.InstitutionTypeId == 1)
                    {
                        if (institution.DateTo?.Date != DateTime.FromOADate(structure.DateTo.ToInt()).Date)
                            if (institution.DateTo != null || DateTime.FromOADate(structure.DateTo.ToInt()).Date != endDate)
                                result.Add(structure);
                    }
                }
            }
            return result;
        }
        public void SaveClosedStructure(List<EisppImportStructure> qList)
        {
            DateTime endDate = new DateTime(2999, 12, 31);
            foreach (var structure in qList)
            {
                if (!string.IsNullOrEmpty(structure.SystemCode))
                {
                    var institution = repo.All<Institution>()
                                    .Where(x => x.EISPPCode == structure.SystemCode)
                                    .FirstOrDefault();
                    if (institution != null)
                    {
                        institution.DateTo = DateTime.FromOADate(structure.DateTo.ToInt()).Date;
                        repo.SaveChanges();
                    }
                }
            }
        }
        public List<EisppImportNomenclature> GetNewNomenclature(List<EisppImportNomenclature> nomList)
        {
            var result = new List<EisppImportNomenclature>();
            foreach(var nom in nomList)
            {
                if (string.IsNullOrEmpty(nom.GroupCode))
                {
                    var group = repo.AllReadonly<EisppTbl>()
                                    .Where(x => x.Code == nom.SystemCode)
                                    .FirstOrDefault();
                    if (group == null)
                        result.Add(nom);
                }
            }
            return result;
        }
        public List<EisppImportNomenclature> GetNewNomenclatureElement(List<EisppImportNomenclature> nomList)
        {
            var result = new List<EisppImportNomenclature>();
            foreach (var nom in nomList)
            {
                if (!string.IsNullOrEmpty(nom.GroupCode))
                {
                    var element = repo.AllReadonly<EisppTblElement>()
                                    .Where(x => x.Code == nom.SystemCode &&
                                                x.EisppTblCode == nom.GroupCode)
                                    .FirstOrDefault();
                    if (element == null)
                        result.Add(nom);
                }
            }
            return result;
        }
       

        public List<EisppImportCrimeQualification> GetCrimeQualificationFromExcel(byte[] excelContent, int fromRow)
        {
            NPoiExcelService excelService = new NPoiExcelService(excelContent, 0);
            int row = fromRow;
            bool haveData = true;
            var structureItems = new List<EisppImportCrimeQualification>();
            int blankRows = 0;
            while (blankRows < 10)
            {
                var item = new EisppImportCrimeQualification();
                item.Row = row;
                excelService.rowIndex = row;
                excelService.colIndex = 0;
                item.GroupName4 = excelService.GetValue();
                item.GroupName3 = excelService.GetValue();
                item.GroupName2 = excelService.GetValue();
                item.Name = excelService.GetValue();
                item.SystemCode = excelService.GetValue();
                item.SystemName = excelService.GetValue();
                item.Active = excelService.GetValue();
                item.DateFrom = excelService.GetValue();
                item.DateTo = excelService.GetValue();
                haveData = !string.IsNullOrEmpty(item.Name) || !string.IsNullOrEmpty(item.SystemCode) ;
                if (haveData)
                {
                    blankRows = 0;
                    structureItems.Add(item);
                }
                else
                {
                    blankRows++;
                }
                row++;
            }
            return structureItems;
        }
        public List<EisppImportCrimeQualification> GetNewCrimeQualificationElement(List<EisppImportCrimeQualification> qList)
        {
            var result = new List<EisppImportCrimeQualification>();
            foreach (var crimeQualification in qList)
            {
                if (!string.IsNullOrEmpty(crimeQualification.Name))
                {
                    var element = repo.AllReadonly<EisppTblElement>()
                                    .Where(x => x.Code == crimeQualification.SystemCode &&
                                                x.EisppTblCode == EISPPConstants.EisppTableCode.EISS_PNE)
                                    .FirstOrDefault();
                    if (element == null)
                        result.Add(crimeQualification);
                }
            }
            return result;
        }
        public void SavewCrimeQualificationElement(List<EisppImportCrimeQualification> qList)
        {
            foreach (var crimeQualification in qList)
            {
                var element = new EisppTblElement();
                element.EisppTblCode = EISPPConstants.EisppTableCode.EISS_PNE;
                element.Code = crimeQualification.SystemCode;
                element.Label = crimeQualification.Name.Trim();
                element.SystemName = crimeQualification.SystemName;
                element.IsActive = (crimeQualification.Active.Trim() == "акт");
                element.DateStart = DateTime.FromOADate(crimeQualification.DateFrom.ToInt());
                element.DateEnd = DateTime.FromOADate(crimeQualification.DateTo.ToInt());
                element.DateWrt = DateTime.Now;
                repo.Add(element);
                repo.SaveChanges();
            }
        }
     
    }
}
