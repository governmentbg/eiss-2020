// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ServiceModel;

namespace IntegrationService.Eispp
{
    [ServiceContract(Namespace = "http://IntegrationService.Eispp")]
    public interface IEisppService
    {
        [OperationContract]
        bool SendMessage(string message);

        [OperationContract]
        string[] ReceiveMessages();
    }
}
