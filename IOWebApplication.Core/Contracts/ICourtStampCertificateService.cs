// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtStampCertificateService
    {
        Task<SaveResultVM> AddCertificate(int courtId, byte[] certificateContent, string password, bool noCheck);
        Task<CourtStampInfoVM> GetCertificate(int courtId = 0);
        IQueryable<CourtStampCertificateListVM> Select(CourtStampCertificateFilterVM filter);
        Task<CourtStampResponseVM> Stamp(CourtStampRequestVM request);
    }
}
