// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplicationService.Infrastructure.Contracts
{
    /// <summary>
    /// Изпращане и получаване на съобщения към и от ядрото на ЕИСПП
    /// </summary>
    public  interface IEisppCommunicationService
    {
        /// <summary>
        /// Изпраща подготвените съобщения 
        /// и обработва отговорите
        /// </summary>
        void SendReceiveMessages();
    }
}
