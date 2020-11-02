// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IEmailSender
    {
        Task SendMail(string address, string subject, string body);
        Task SendMail(string[] addressList, string subject, string body);
    }
}
