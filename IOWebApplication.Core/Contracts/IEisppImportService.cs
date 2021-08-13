// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IEisppImportService : IBaseService
    {
        List<EisppImportStructure> GetClosedStructure(List<EisppImportStructure> qList);
        List<EisppImportCrimeQualification> GetCrimeQualificationFromExcel(byte[] excelContent, int fromRow);
        EisppReportFilterVM GetDefaultFilter();
        List<EisppImportEktte> GetEktteFromExcel(byte[] excelContent, int fromRow);
        List<EisppImportCrimeQualification> GetNewCrimeQualificationElement(List<EisppImportCrimeQualification> qList);
        List<EisppImportNomenclature> GetNewNomenclature(List<EisppImportNomenclature> nomList);
        List<EisppImportNomenclature> GetNewNomenclatureElement(List<EisppImportNomenclature> nomList);
        List<EisppImportNomenclature> GetNomenclatureFromExcel(byte[] excelContent, int fromRow);
        List<EisppImportStructure> GetStructureFromExcel(byte[] excelContent, int fromRow);
        bool ImportEktteRajon(List<EisppImportEktte> ektteItems);
        byte[] MakeFridayReport(EisppReportFilterVM filter);
        void SaveClosedStructure(List<EisppImportStructure> qList);
        void SavewCrimeQualificationElement(List<EisppImportCrimeQualification> qList);
    }
}
