using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class BankFilePaymentVM
    {
        public long Id { get; set; }

        public string CourtName { get; set; }

        public string Iban { get; set; }

        public DateTime PaidDate { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        /// <summary>
        /// Начин на плащане - ПОС, Платежно
        /// </summary>
        public string PaymentType { get; set; }

        public string DebtorNames { get; set; }

        public string DebtorIdentifier { get; set; }

        public string SenderName { get; set; }

        public string PaymentInfo { get; set; }

        public string PaymentDescription { get; set; }

        public string Description { get; set; }

        public string PaymentTypeCode { get; set; }

        public string PaymentNumber { get; set; }

        /// <summary>
        /// Референтен номер
        /// </summary>
        public string BankId { get; set; }

        public DateTime? DateExpired { get; set; }

        public string DescriptionExpired { get; set; }

        /// <summary>
        /// Данни за анулиране
        /// </summary>
        public string ExpireData {
            get
            {
                return DateExpired == null ? "" : (DateExpired?.ToString("dd.MM.yyyy") + " - " + DescriptionExpired);
            }
        }
        public BankFileObligationPaymentVM[] ObligationPayments { get; set; }
    }

    public class BankFileObligationPaymentVM
    {
        public string CourtName { get; set; }
        public string ObligationInfo { get; set; }
    }

    public class BankFilePaymentFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Начин на плащане")]
        public int PaymentTypeId { get; set; }

        [Display(Name = "Идентификатор на задължено лице")]
        public string DebtorIdentifier { get; set; }

        [Display(Name = "Име на задължено лице")]
        public string DebtorNames { get; set; }

        [Display(Name = "Име на вносител")]
        public string SenderName { get; set; }

        [Display(Name = "Основание")]
        public string PaymentInfo { get; set; }

        [Display(Name = "Основание 2")]
        public string PaymentDescription { get; set; }

        [Display(Name = "Референтен номер")]
        public string BankId { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }
    }

    public class BankFilePaymentReferenceFilterVM
    {
        [Display(Name = "Референтен номер")]
        public string BankId { get; set; }

        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Начин на плащане")]
        public int PaymentTypeId { get; set; }

        [Display(Name = "Идентификатор на задължено лице")]
        public string DebtorIdentifier { get; set; }

        [Display(Name = "Име на задължено лице")]
        public string DebtorNames { get; set; }

        [Display(Name = "Име на вносител")]
        public string SenderName { get; set; }

        [Display(Name = "Основание")]
        public string PaymentInfo { get; set; }

        [Display(Name = "Основание 2")]
        public string PaymentDescription { get; set; }

        [Display(Name = "Съд")]
        public int CourtId { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Сметка")]
        public string Iban { get; set; }
    }
}
