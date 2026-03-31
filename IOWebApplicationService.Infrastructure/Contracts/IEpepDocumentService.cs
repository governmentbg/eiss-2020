// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IEpepDocumentService
    {
        
        /// <summary>
        /// Регистрира документи в MainGroup.TransactionId = 
        /// </summary>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        Task FastProcessRegisterInCourt(int fetchCount);
        Task FetchElectronicDocumentPayments();
        Task FetchElectronicDocuments();
        Task FetchExecProcessNewExecListDeliveryDate();
        Task Test();
        Task TestAssign(int assignCount);
    }
}
