// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.MessageQueue;

namespace IOWebApplication.Core.Contracts
{
    public interface IConsoleTaskRecieverService
    {
        void RecieveMessage(MQMessageModel resultModel);
    }
}
