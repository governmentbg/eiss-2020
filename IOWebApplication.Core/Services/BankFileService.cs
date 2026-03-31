using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace IOWebApplication.Core.Services
{
    public class BankFileService : BaseService, IBankFileService
    {
        private readonly IConfiguration config;

        private readonly ICounterService counterService;

        public BankFileService(ILogger<BankFileService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IConfiguration _config,
            ICounterService _counterService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            config = _config;
            counterService = _counterService;
        }

        public async Task ReadBankFiles()
        {
            try
            {
                string certificatePath = config.GetValue<string>("BankSftp:CertificatePath");
                int port = config.GetValue<int>("BankSftp:Port");
                string host = config.GetValue<string>("BankSftp:Host");
                string userName = config.GetValue<string>("BankSftp:UserName");

                var currencies = await repo.AllReadonly<Currency>().ToListAsync();
                var bankAccounts = await repo.AllReadonly<CourtBankAccount>().ToListAsync();
                var banks = await repo.AllReadonly<Bank>().ToListAsync();
                var bankCodePaymentTypes = await repo.AllReadonly<BankCodePaymentType>().ToListAsync();
                var bankFilePaymentTypes = await repo.AllReadonly<BankFilePaymentType>().Select(x => x.Id).ToArrayAsync();
                var dbEuroConfig = DbEuroConfig;

                // Зареждаме private key
                var privateKey = new PrivateKeyFile(certificatePath);

                // Метод за автентикация с ключ
                var authMethod = new PrivateKeyAuthenticationMethod(userName, privateKey);

                // Информация за връзката
                var connectionInfo = new ConnectionInfo(host, port, userName, authMethod);

                // Създаваме SSH клиент
                using var sftp = new SftpClient(connectionInfo);

                sftp.Connect();

                if (sftp.IsConnected)
                {
                    foreach (var bank in banks)
                    {
                        if (string.IsNullOrEmpty(bank.FileDirectory) == true) continue;

                        string remotePath = "/data/" + bank.FileDirectory + "/in";

                        var files = sftp.ListDirectory(remotePath);
                        foreach (var file in files)
                        {
                            if (file.Name == "." || file.Name == ".." || file.IsDirectory == true || file.Name.StartsWith("ioerr_"))
                                continue;                            

                            repo.ClearEntityTracker();
                            bool result = false;
                            BankFile model = null;
                            if (bank.FileStructureType == 1)
                            {
                                (result, model) = SaveStructureFixPosition(sftp, file, bankAccounts, currencies, bank);
                            }
                            else if (bank.FileStructureType == 2)
                            {
                                (result, model) = SaveStructureTabDelimiter(sftp, file, bankAccounts, currencies, bank, dbEuroConfig);
                            }
                            else if (bank.FileStructureType == 3)
                            {
                                (result, model) = SaveStructureMT940(sftp, file, bankAccounts, currencies, bank, bankCodePaymentTypes);
                            }

                            //Ако сме в евро да обърна всички плащания, които са дошли в лева
                            if (dbEuroConfig.IsInEuro == true)
                            {
                                foreach (var payment in model.Payments)
                                {
                                    if (payment.CurrencyId == NomenclatureConstants.Currency.BGN)
                                    {
                                        payment.OldAmount = payment.Amount;
                                        payment.OldCurrencyId = payment.CurrencyId;
                                        payment.CurrencyId = NomenclatureConstants.Currency.EUR;
                                        payment.Amount = Math.Round(payment.Amount / dbEuroConfig.EuroExchangeRate, 2, MidpointRounding.AwayFromZero);
                                    }
                                }
                            }

                            try
                            {
                                //Проверява дали файла го има в директорията done. Ако го има - добавям време отпред. Дори да са качили същият файл аз проверявам записите по банкова референция
                                string newFileName = file.Name;
                                string newFilePath = file.FullName.Replace("/in", "/done");
                                bool doneFileExists = await sftp.ExistsAsync(newFilePath);
                                if (doneFileExists == true)
                                {
                                    newFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{file.Name}";
                                    newFilePath = newFilePath.Replace(file.Name, newFileName);
                                }

                                if (result == true && model.Payments.Count > 0)
                                {
                                    var checkPaymentType = model.Payments.All(x => bankFilePaymentTypes.Contains(x.PaymentTypeId));
                                    if (checkPaymentType == false)
                                        throw new Exception("Невалиден тип на плащане");

                                    await CheckBankReferences(model);
                                    List<BankFileObligationVM> obligations = await ReadObligationForPay();
                                    result = ProcessingBankPayments(model, obligations, dbEuroConfig);

                                    if (result == true)
                                    {
                                        //Може да има смяна на име при повтарящ се файл
                                        model.FileName = newFileName;
                                        await repo.AddAsync(model);
                                        await repo.SaveChangesAsync();
                                    }
                                }

                                if (result == true)
                                {
                                    //Прехвърляне на файла в директория done
                                    sftp.RenameFile(file.FullName, newFilePath);
                                }
                                else
                                {
                                    //Слагам ioerr отпред за да не го обработвам отново
                                    MakeErrorFile(sftp, file);
                                }

                            }
                            catch (Exception ex)
                            {
                                //Слагам ioerr отпред за да не го обработвам отново
                                MakeErrorFile(sftp, file);

                                result = false;
                                logger.LogError(ex, "BankFileService.ReadBankFiles: " + file.Name);
                                continue;
                            }                            
                        }
                    }
                }
                else
                {
                    logger.LogError("Неуспешна връзка.");
                }

                // Затваряме връзката
                sftp.Disconnect();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "BankFileService.ReadBankFiles");
            }
        }

        private (bool result, BankFile model) SaveStructureFixPosition(SftpClient sftp, ISftpFile file, List<CourtBankAccount> bankAccounts, List<Currency> currencies, Bank bank)
        {
            bool result = true;
            BankFile model = null;
            try
            {
                model = new BankFile()
                {
                    FileName = file.Name,
                    Payments = new List<BankFilePayment>()
                };
                using (Stream fileStream = sftp.OpenRead(file.FullName))
                {
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding(bank.FileEncodingName)))
                    {
                        string line;
                        int lineNumber = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;
                            if (line.Substring(30, 1).Trim() == "1") continue; //Това е общия ред

                            string iban = line.Substring(8, 22).Trim();
                            if (model.Iban == null)
                            {
                                model.Iban = iban;
                                var bankAccount = bankAccounts.Where(x => x.Iban == model.Iban).FirstOrDefault();
                                if (bankAccount == null)
                                    throw new Exception("Не е намерена банкова сметка за файл: " + file.Name);

                                model.CourtId = bankAccount.CourtId;
                                model.CourtBankAccountId = bankAccount.Id;
                            }
                            else
                            {
                                if (model.Iban != iban)
                                    throw new Exception("Повече от една сметка за файл: " + file.Name);
                            }

                            var bankFilePayment = new BankFilePayment();

                            bankFilePayment.BankId = line.Substring(107, 16).Trim();
                            bankFilePayment.PaidDate = line.Substring(37, 8).Trim().StrToDateFormat("yyyyMMdd");
                            bankFilePayment.Amount = decimal.Parse(line.Substring(48, 14).Trim()) / 100; //В стотинки е
                            bankFilePayment.CurrencyId = currencies.Where(x => x.Code == line.Substring(45, 3).Trim()).Select(x => x.Id).FirstOrDefault();
                            bankFilePayment.DebtorNames = line.Substring(169, 26).Trim();
                            bankFilePayment.DebtorIdentifier = line.Substring(307, 13).Trim();

                            if (string.IsNullOrEmpty(bankFilePayment.DebtorIdentifier) == true)
                                bankFilePayment.DebtorIdentifier = line.Substring(320, 10).Trim().Replace("0000000000", "");

                            if (string.IsNullOrEmpty(bankFilePayment.DebtorIdentifier) == true)
                                bankFilePayment.DebtorIdentifier = line.Substring(330, 10).Trim().Replace("0000000000", "");

                            bankFilePayment.SenderName = line.Substring(143, 26).Trim();
                            bankFilePayment.PaymentInfo = line.Substring(195, 35).Trim();
                            bankFilePayment.PaymentDescription = line.Substring(230, 35).Trim();
                            bankFilePayment.LineText = line;

                            string paymentTypeCode = line.Substring(62, 1).Trim();

                            switch (paymentTypeCode)
                            {
                                case "K":
                                    {
                                        bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Credit;
                                        break;
                                    }
                                case "D":
                                    {
                                        bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Debit;
                                        break;
                                    }
                                case "L":
                                    {
                                        bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.StornoCredit;
                                        break;
                                    }
                                case "E":
                                    {
                                        bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.StornoDebit;
                                        break;
                                    }
                                default:
                                    throw new Exception("Невалиден тип на плащането за файл: " + file.Name + "; ред: " + lineNumber);
                            }

                            bankFilePayment.PaymentTypeId = int.Parse(line.Substring(360, 1).Trim());
                            model.Payments.Add(bankFilePayment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex, "BankFileService.SaveStructureFixPosition");
            }

            return (result, model);
        }

        private (bool result, BankFile model) SaveStructureTabDelimiter(SftpClient sftp, ISftpFile file, List<CourtBankAccount> bankAccounts, List<Currency> currencies, Bank bank, DbEuroConfigVM dbEuroConfig)
        {
            bool result = true;
            BankFile model = null;
            try
            {
                model = new BankFile()
                {
                    FileName = file.Name,
                    Payments = new List<BankFilePayment>()
                };
                using (Stream fileStream = sftp.OpenRead(file.FullName))
                {
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding(bank.FileEncodingName)))
                    {
                        string line;
                        int lineNumber = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;
                            var fields = line.Split('\t').ToArray();
                            if (fields[0] == "TOTAL") continue; //Това е общия ред

                            string iban = fields[0].Trim();
                            if (model.Iban == null)
                            {
                                model.Iban = iban;
                                var bankAccount = bankAccounts.Where(x => x.Iban == model.Iban).FirstOrDefault();
                                if (bankAccount == null)
                                    throw new Exception("Не е намерена банкова сметка за файл: " + file.Name);

                                model.CourtId = bankAccount.CourtId;
                                model.CourtBankAccountId = bankAccount.Id;
                            }
                            else
                            {
                                if (model.Iban != iban)
                                    throw new Exception("Повече от една сметка за файл: " + file.Name);
                            }

                            var bankFilePayment = new BankFilePayment();

                            bankFilePayment.BankId = fields[5].Trim();
                            bankFilePayment.PaidDate = fields[2].Trim().StrToDateFormat("dd.MM.yyyy");
                            //Не успях да ги убедя да подават валута и не знам след влизане в еврото в какво ще са сумите на плащания преди 01.01, затова няма да се приемат плащания преди 01.01
                            if (dbEuroConfig.IsInEuro && bankFilePayment.PaidDate < dbEuroConfig.InterimPeriodEuroStart)
                                throw new ArgumentException("Плащането е преди датата на влизане в евро");

                            decimal amountCredit = decimal.Parse(fields[7].Trim().Replace(",", "."));
                            decimal amountDebit = decimal.Parse(fields[6].Trim().Replace(",", "."));

                            if (Math.Abs(amountCredit) > 0.0001M)
                            {
                                bankFilePayment.Amount = amountCredit;
                                bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Credit;
                            }
                            else
                            {
                                bankFilePayment.Amount = amountDebit;
                                bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Debit;
                            }

                            //Не успях да ги убедя да подават валута
                            if (dbEuroConfig.IsInEuro == false)
                                bankFilePayment.CurrencyId = NomenclatureConstants.Currency.BGN;
                            else
                                bankFilePayment.CurrencyId = NomenclatureConstants.Currency.EUR;

                            bankFilePayment.DebtorNames = fields[21].Trim();
                            bankFilePayment.DebtorIdentifier = fields[17].Trim();

                            if (string.IsNullOrEmpty(bankFilePayment.DebtorIdentifier) == true)
                                bankFilePayment.DebtorIdentifier = fields[18].Trim();

                            if (string.IsNullOrEmpty(bankFilePayment.DebtorIdentifier) == true)
                                bankFilePayment.DebtorIdentifier = fields[19].Trim();

                            if (string.IsNullOrEmpty(bankFilePayment.DebtorIdentifier) == true)
                                bankFilePayment.DebtorIdentifier = fields[20].Trim();

                            bankFilePayment.SenderName = fields[14].Trim();
                            bankFilePayment.PaymentInfo = fields[9].Trim();

                            //Трябва да е последната колона, но някой подават допълнителни колони и затова взимам последната попълнена
                            string paymentTypeTxt = fields.Where(x => x.Trim() != "").Last();
                            bankFilePayment.PaymentTypeId = int.Parse(paymentTypeTxt.Trim()); //Последната колона е типа
                            bankFilePayment.LineText = line;

                            model.Payments.Add(bankFilePayment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex, "BankFileService.SaveStructureFixPosition");
            }

            return (result, model);
        }

        private (bool result, BankFile model) SaveStructureMT940(SftpClient sftp, ISftpFile file, List<CourtBankAccount> bankAccounts, List<Currency> currencies, Bank bank, List<BankCodePaymentType> bankCodePaymentTypes)
        {
            bool result = true;
            BankFile model = null;
            try
            {
                model = new BankFile()
                {
                    FileName = file.Name,
                    Payments = new List<BankFilePayment>()
                };

                List<string> lines = new List<string>();

                bool isAsetBank = bank.CodeForSearch == "IABG"; //Имат различна структура на поле 86. Нямат време да променят и затова ние
                string delimiter86 = isAsetBank == true ? ";" : "";

                using (Stream fileStream = sftp.OpenRead(file.FullName))
                {
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding(bank.FileEncodingName)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith(":"))
                                lines.Add(line);
                            else
                            {
                                if (lines.Count > 0)
                                {
                                    var lastLine = lines[lines.Count - 1];
                                    if (lastLine.StartsWith(":86:")) //За момента само за поле 86 може да са повече от един ред
                                        lines[lines.Count - 1] += delimiter86 + line;
                                }
                            }
                        }
                    }
                }

                BankFilePayment bankFilePayment = null;
                int currencyId = 0;
                foreach (string line in lines)
                {
                    if (line.StartsWith(":25:"))
                    {
                        string iban = line[4..].Trim();

                        if (model.Iban == null)
                        {
                            model.Iban = iban;

                            var bankAccount = bankAccounts.Where(x => x.Iban == model.Iban).FirstOrDefault();
                            if (bankAccount == null)
                                throw new Exception("Не е намерена банкова сметка за файл: " + file.Name);

                            model.CourtId = bankAccount.CourtId;
                            model.CourtBankAccountId = bankAccount.Id;
                        }
                        else
                        {
                            if (model.Iban != iban)
                                throw new Exception("Повече от една сметка за файл: " + file.Name);
                        }

                    }
                    //Валутата я има или в поле 60F или в поле 34F. Което дойде първо него взима
                    else if (line.StartsWith(":34F:"))
                    {
                        if (currencyId == 0)
                        {
                            string currency = line.Substring(5, 3);
                            currencyId = currencies.Where(x => x.Code == currency).Select(x => x.Id).FirstOrDefault();
                            if (currencyId == 0)
                                throw new Exception("Не е намерена валута за файл: " + file.Name);
                        }
                    }
                    else if (line.StartsWith(":60F:"))
                    {
                        if (currencyId == 0)
                        {
                            string currency = line.Substring(12, 3);
                            currencyId = currencies.Where(x => x.Code == currency).Select(x => x.Id).FirstOrDefault();
                            if (currencyId == 0)
                                throw new Exception("Не е намерена валута за файл: " + file.Name);
                        }
                    }
                    else if (line.StartsWith(":61:"))
                    {
                        bankFilePayment = new BankFilePayment();

                        int indexDelimiter = line.IndexOf("//");

                        bankFilePayment.CurrencyId = currencyId;
                        bankFilePayment.PaidDate = ("20" + line.Substring(4, 6).Trim()).StrToDateFormat("yyyyMMdd");

                        //Всяка банка си подава каквото си иска след сумата до разделителя //
                        string decimalText = "";
                        for (int i = 15; i < indexDelimiter; i++)
                        {
                            if (line[i] == ',' || char.IsDigit(line[i]) == true)
                                decimalText += line[i];
                            else
                                break;
                        }

                        bankFilePayment.Amount = decimal.Parse(decimalText.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));

                        bankFilePayment.BankId = line[(indexDelimiter + 2)..].Trim();

                        var paymentTypeCode = line.Substring(14, 1).Trim();
                        switch (paymentTypeCode)
                        {
                            case "C":
                                {
                                    bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Credit;
                                    break;
                                }
                            case "D":
                                {
                                    bankFilePayment.PaymentTypeCodeId = NomenclatureConstants.BankFilePaymentTypeCodes.Debit;
                                    break;
                                }
                            default:
                                throw new Exception("Невалиден тип на плащането за файл: " + file.Name + "; ред: " + line);
                        }

                        bankFilePayment.LineText = line;

                        model.Payments.Add(bankFilePayment);
                    }
                    else if (line.StartsWith(":86:"))
                    {
                        bankFilePayment.LineText += ";;;" + line;

                        if (isAsetBank == true)
                        {
                            var fields86 = line.Split(delimiter86).ToArray();
                            string paymentType = (fields86[0])[4..];
                            bankFilePayment.PaymentTypeId = bankCodePaymentTypes.Where(x => x.BankId == bank.Id && x.BankCodes.Contains(paymentType)).Select(x => x.PaymentTypeId).FirstOrDefault();
                            if (bankFilePayment.PaymentTypeId == 0)
                                bankFilePayment.PaymentTypeId = NomenclatureConstants.BankFilePaymentTypes.Other;

                            if (fields86.Length > 1)
                                bankFilePayment.PaymentInfo = fields86[1];

                            if (fields86.Length > 3)
                                bankFilePayment.SenderName = fields86[3];
                        }
                        else
                        {

                            string delimiter = line.Substring(7, 1);
                            var fields86 = line.Split(delimiter).ToArray();
                            string paymentType = line.Substring(4, 3);
                            bankFilePayment.PaymentTypeId = bankCodePaymentTypes.Where(x => x.BankId == bank.Id && x.BankCodes.Contains(paymentType)).Select(x => x.PaymentTypeId).FirstOrDefault();
                            if (bankFilePayment.PaymentTypeId == 0)
                                bankFilePayment.PaymentTypeId = NomenclatureConstants.BankFilePaymentTypes.Other;

                            foreach (var itemField in fields86)
                            {
                                if (itemField.StartsWith("32") || itemField.StartsWith("33"))
                                {
                                    string fieldTxt = itemField[2..].Trim();
                                    if (string.IsNullOrEmpty(fieldTxt) == true) continue;

                                    if (string.IsNullOrEmpty(bankFilePayment.SenderName) == false)
                                        bankFilePayment.SenderName += " ";
                                    bankFilePayment.SenderName += fieldTxt;
                                }

                                if (itemField.StartsWith("00") || itemField.StartsWith("2"))
                                {
                                    string fieldTxt = itemField[2..].Trim();
                                    if (string.IsNullOrEmpty(fieldTxt) == true) continue;

                                    if (string.IsNullOrEmpty(bankFilePayment.PaymentInfo) == false)
                                        bankFilePayment.PaymentInfo += "; ";

                                    bankFilePayment.PaymentInfo += fieldTxt;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex, "BankFileService.SaveStructureFixPosition");
            }

            return (result, model);
        }

        private async Task<List<BankFileObligationVM>> ReadObligationForPay()
        {
            List<BankFileObligationVM> result = new List<BankFileObligationVM>();

            DateTime date = DateTime.Now.AddYears(-1);//Тук трябва да се прецени колко назад да се взимат

            int start = 0;
            int take = 1000;
            bool readObligations = true;
            while (readObligations)
            {
                var obligations = await repo.AllReadonly<Obligation>()
                                                .Where(x => x.IsActive == true && x.ObligationDate >= date &&
                                                            x.Uic != null && x.Uic != "" && x.DocumentId != null && x.Document.DocumentRequestTypeId != null)
                                                .Where(x => !x.ObligationPayments.Where(a => a.IsActive == true).Any())
                                                .Select(x => new BankFileObligationVM()
                                                {
                                                    ObligationId = x.Id,
                                                    Uic = x.Uic,
                                                    Amount = x.Amount,
                                                    CaseNumber = x.Case.RegNumber,
                                                    CaseShortNumberYear = x.Case.ShortNumber == null ? null : (x.Case.ShortNumber + "/" + x.Case.RegDate.Year),
                                                    DocumentNumber = x.Document.DocumentNumber,
                                                    EpepNumber = x.Document.ElectronicDocument.ApplyNumber,
                                                    DocumentUic = x.Document.DocumentPersons.Where(d => d.Uic != null && d.Uic != "" && d.Uic != x.Uic).Select(x => x.Uic).ToArray(),
                                                })
                                                .OrderBy(x => x.ObligationId)
                                                .Skip(start)
                                                .Take(take)
                                                .ToListAsync();

                result.AddRange(obligations);
                start = start + take;

                if (obligations.Count < take)
                    readObligations = false;
            }

            return result;
        }

        private bool ProcessingBankPayments(BankFile bankFile, List<BankFileObligationVM> obligations, DbEuroConfigVM dbEuroConfig)
        {
            HashSet<int> obligationIds = new HashSet<int>(); //Задължения, които вече са платени
            foreach (var item in bankFile.Payments)
            {
                //Ако е различно от постъпление в сметката да не прави нищо
                if (item.PaymentTypeCodeId != NomenclatureConstants.BankFilePaymentTypeCodes.Credit) continue;
                if (item.PaymentTypeId != NomenclatureConstants.BankFilePaymentTypes.PaymentOrder) continue;

                //Ако има записана забележка да не прави нищо - Примерно дублирана банкова референция
                if (string.IsNullOrEmpty(item.Description) == false) continue;

                string paymentData = (item.DebtorIdentifier ?? "") + " " + (item.PaymentInfo ?? "") + " " + (item.PaymentDescription ?? "");
                string[] paymentInfo = ((item.PaymentInfo ?? "") + " " + (item.PaymentDescription ?? "")).ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();
                var obligationsRequired = obligations.Where(x => obligationIds.Contains(x.ObligationId) == false && Math.Abs(x.Amount - item.Amount) < 0.001M && paymentData.Contains(x.Uic, StringComparison.OrdinalIgnoreCase)).ToList();

                if (obligationsRequired.Count > 0)
                {
                    List<BankFileObligationVM> obligationsForPay;

                    //Номер на документ от ЕИСС
                    obligationsForPay = obligationsRequired.Where(x => string.IsNullOrEmpty(x.DocumentNumber) == false && paymentInfo.Contains(x.DocumentNumber.ToLower())).ToList();

                    //Ако няма съвпадение по номер на дело
                    if (obligationsForPay.Count == 0)
                        obligationsForPay = obligationsRequired.Where(x => string.IsNullOrEmpty(x.CaseNumber) == false && paymentInfo.Contains(x.CaseNumber.ToLower())).ToList();

                    //Ако няма съвпадение по краткия номер на дело + /година
                    if (obligationsForPay.Count == 0)
                        obligationsForPay = obligationsRequired.Where(x => string.IsNullOrEmpty(x.CaseShortNumberYear) == false && paymentInfo.Contains(x.CaseShortNumberYear.ToLower())).ToList();

                    //Ако няма съвпадение по номер на ЕПЕП
                    if (obligationsForPay.Count == 0)
                        obligationsForPay = obligationsRequired.Where(x => string.IsNullOrEmpty(x.EpepNumber) == false && paymentInfo.Contains(x.EpepNumber.ToLower())).ToList();

                    //Ако няма съвпадение да има някой от идентификаторите в документа
                    if (obligationsForPay.Count == 0)
                        obligationsForPay = obligationsRequired.Where(x => x.DocumentUic.Where(u => paymentInfo.Contains(u.ToLower())).Any()).ToList();

                    //Само ако има едно съвпадение да направя плащане към задължението
                    if (obligationsForPay.Count == 1)
                    {
                        int obligationId = obligationsForPay[0].ObligationId;
                        obligationIds.Add(obligationId);

                        item.Payment = new Payment()
                        {
                            CourtId = bankFile.CourtId,
                            PaymentTypeId = NomenclatureConstants.PaymentType.Bank,
                            IsAvans = false,
                            CourtBankAccountId = bankFile.CourtBankAccountId,
                            Amount = item.Amount,
                            PaidDate = item.PaidDate,
                            SenderName = item.SenderName,
                            PaymentInfo = item.PaymentInfo,
                            PaymentDescription = item.PaymentDescription,
                            IsActive = true,
                            OfflinePos = false,
                            DateWrt = DateTime.Now,
                            IsAutomatic = true,
                        };

                        if (item.CurrencyId == NomenclatureConstants.Currency.EUR)
                        {
                            item.Payment.AmountBGN = DbEuroConfig.GetBGNFromEUR(item.Payment.Amount, item.Payment.PaidDate);
                        }
                        if (counterService.Counter_GetPaymentCounter(item.Payment) == false)
                        {
                            return false;
                        }

                        item.Payment.ObligationPayments.Add(new ObligationPayment()
                        {
                            ObligationId = obligationId,
                            IsActive = true,
                            Amount = item.Payment.Amount,
                            AmountBGN = item.Payment.AmountBGN,
                            DateWrt = DateTime.Now,
                        });
                    }
                }
            }

            return true;
        }

        private async Task CheckBankReferences(BankFile bankFile)
        {

            HashSet<string> bankReferences = bankFile.Payments.Select(x => x.BankId).ToHashSet<string>();
            HashSet<string> existsReferences = new HashSet<string>();

            DateTime date = DateTime.Now.AddYears(-1);//Тук трябва да се прецени колко назад да се взимат

            int recordCount = 1000; //Чета по 1000
            int count = bankReferences.Count / recordCount + 1;
            for (int k = 0; k < count; k++)
            {
                var ids = bankReferences.Skip(k * recordCount).Take(recordCount);
                if (ids.Count() > 0)
                {
                    var list = await repo.AllReadonly<BankFilePayment>()
                               .Where(x => x.PaidDate >= date && x.BankFile.Iban == bankFile.Iban)
                               .Where(x => ids.Contains(x.BankId))
                               .Select(x => x.BankId)
                               .ToListAsync();

                    existsReferences.UnionWith(list);
                }
            }

            foreach (var payment in bankFile.Payments)
            {
                if (existsReferences.Contains(payment.BankId) == true)
                {
                    payment.Description = "Вече е подадено плащане с тази банкова референция";
                }
            }
        }

        private void MakeErrorFile(SftpClient sftp, ISftpFile file)
        {
            var newFileName = $"ioerr_{DateTime.Now:yyyyMMdd_HHmmss}_{file.Name}";
            var newFilePath = file.FullName.Replace(file.Name, newFileName);

            //Слагам ioerr отпред за да не го обработвам отново
            sftp.RenameFile(file.FullName, newFilePath);
        }
    }
}

