// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class DbEuroConfigVM
    {
        public decimal EuroExchangeRate { get; set; }
        public DateTime InterimPeriodEuroStart { get; set; }
        public DateTime InterimPeriodEuroEnd { get; set; }

        public bool IsInEuro
        {
            get
            {
                return InterimPeriodEuroStart < DateTime.Now;
            }
        }

        public decimal GetBGNFromEUR(decimal amount, DateTime? dtNow = null)
        {
            //Ако е в евро след влизане
            if ((dtNow ?? DateTime.Now) > InterimPeriodEuroStart)
            {
                return Math.Round(amount * EuroExchangeRate, 2, MidpointRounding.AwayFromZero);
            }
            return amount;
        }

        public decimal GetEURFromBGN(decimal amount, DateTime? dtNow = null)
        {
            //Ако е в евро след влизане
            if ((dtNow ?? DateTime.Now) > InterimPeriodEuroStart)
            {
                return Math.Round(amount / EuroExchangeRate, 2, MidpointRounding.AwayFromZero);
            }
            return amount;
        }
    }
}
