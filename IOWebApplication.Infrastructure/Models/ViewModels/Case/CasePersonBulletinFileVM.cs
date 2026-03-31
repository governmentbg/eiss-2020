// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonBulletinFileVM
    {
        public int Id { get; set; }
        public DateTime DateWrt { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? DateRegisteredInCais { get; set; }
        public DateTime? DateSigned { get; set; }

        public string CaisStatus
        {
            get
            {
                if (DateRegisteredInCais != null)
                {
                    return $"получен в СС: {DateRegisteredInCais.Value:dd.MM.yyyy}";
                }
                if (DateSigned != null)
                {
                    return $"подписан на: {DateSigned.Value:dd.MM.yyyy}";
                }
                return "за подпис";
            }
        }
    }
}
