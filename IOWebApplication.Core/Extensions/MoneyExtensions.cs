using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Extensions
{
    public static class MoneyExtensions
    {
        public enum ROD
        {
            MYGKI,
            GENSKI,
            SREDEN
        };

        private const string a = "и ";
        private const string sa = " " + a;
        private static string[] w1 = new[] { "", "един ", "два ", "три ", "четири ", "пет ", "шест ", "седем ", "осем ", "девет " };
        private static string[] w_Stot = new[] { "", "еднa ", "двe ", "три ", "четири ", "пет ", "шест ", "седем ", "осем ", "девет " };
        private static string[] w1s = new[] { "", "еднa ", "двe " };
        private static string[] w2 = new[] { "", "десет ", "двадесет ", "тридесет ", "четиридесет ", "петдесет ", "шестдесет ", "седемдесет ", "осемдесет ", "деветдесет " };
        private static string[] w21 = new[] { "десет ", "единадесет ", "дванадесет ", "тринадесет ", "четиринадесет ", "петнадесет ", "шестнадесет ", "седемнадесет ", "осемнадесет ", "деветнадесет " };
        private static string[] w3 = new[] { "", "сто ", "двеста ", "триста ", "четиристотин ", "петстотин ", "шестстотин ", "седемстотин ", "осемстотин ", "деветстотин " };


        private static string totalstr_t3(Int64 n, Int64 sec)
        {
            string Result = w3[(n / 100)];

            if (((((n % 100) >= 10) && ((n % 100) <= 19)) || (((n % 100) > 0) && (n % 10 == 0))) && ((n / 100) > 0))
            {
                Result = Result + a;
            }

            if ((n % 100) >= 10 && (n % 100) <= 19)
            {
                Result = Result + w21[(n % 10)];
                return Result;
            }
            else
            {
                Result = Result + w2[((n % 100) / 10)];
                if ((n / 10) > 0 && (n % 10) > 0)
                {
                    Result = Result + a;
                }
            }

            if ((n % 10) > 0)
            {
                if ((sec > 0) && ((n % 10) <= 2))
                {
                    Result = Result + w1s[(n % 10)];
                }
                else
                {
                    Result = Result + w1[(n % 10)];
                }
            }
            return Result;
        }

        public static string NumberToString(decimal money, string cqloSingle,
            string cqloMany, string systavnoSingle, string systavnoMany, ROD rodCqlo, ROD rodSystavno)
        {

            string l1, l2, l3, l4;
            string result;
            string stotinki = String.Format("{0}", Math.Round((Math.Abs(money) - Math.Abs((int)money)) * 100));
            string s = Math.Abs((int)money).ToString();

            if (s.Length > 12)
            {
                return "-- над обхвата --";
            }
            while (s.Length < 12)
                s = "0" + s;
            if (rodCqlo == ROD.SREDEN)
            {
                w1[1] = "едно ";
            }
            else if (rodCqlo == ROD.MYGKI)
            {
                w1[1] = "един ";
            }
            else if (rodCqlo == ROD.GENSKI)
            {
                w1[1] = "една";
            }
            long n4 = int.Parse(s.Substring(0, 3));
            switch (n4)
            {
                case 0:
                    l4 = "";
                    break;
                case 1:
                    l4 = "един милиард ";
                    break;
                default:
                    l4 = totalstr_t3(n4, 0) + "милиарда ";
                    break;
            }

            long n3 = int.Parse(s.Substring(3, 3));
            switch (n3)
            {
                case 0:
                    l3 = "";
                    break;
                case 1:
                    l3 = "един милион ";
                    break;
                default:
                    l3 = totalstr_t3(n3, 0) + "милиона ";
                    break;
            }

            long n2 = int.Parse(s.Substring(6, 3));
            switch (n2)
            {
                case 0:
                    l2 = "";
                    break;
                case 1:
                    l2 = "хиляда ";
                    break;
                default:
                    l2 = totalstr_t3(n2, 1) + "хиляди ";
                    break;
            }

            long n1 = int.Parse(s.Substring(9, 3));
            switch (n1)
            {
                case 0:
                    l1 = "";
                    break;
                case 1:
                    {
                        l1 = (rodCqlo == ROD.MYGKI) ? "един " : ((rodCqlo == ROD.GENSKI) ? "една " : "едно ");

                    }
                    break;
                default:
                    l1 = totalstr_t3(n1, 0) + "";
                    break;
            }

            if (string.IsNullOrEmpty(l4) && string.IsNullOrEmpty(l3) && string.IsNullOrEmpty(l2) && string.IsNullOrEmpty(l1))
            {
                result = "нула ";
            }
            else
            {
                if (!string.IsNullOrEmpty(l1) && (l1.IndexOf(sa) == -1))
                {
                    if (!string.IsNullOrEmpty(l2) || !string.IsNullOrEmpty(l3) || !string.IsNullOrEmpty(l4))
                    {
                        l1 = a + l1;
                    }
                }
                else if (!string.IsNullOrEmpty(l2) && (l2.IndexOf(sa) == -1))
                {
                    if (!string.IsNullOrEmpty(l3) || !string.IsNullOrEmpty(l4))
                    {
                        l2 = a + l2;
                    }
                }
                else if (!string.IsNullOrEmpty(l3) && (l3.IndexOf(sa) == -1))
                {
                    if (!string.IsNullOrEmpty(l4))
                    {
                        l3 = a + l3;
                    }
                }
                result = l4 + l3 + l2 + l1;
                // rezult[1] = chr(ord(Result[1])-32);
            }

            if (string.IsNullOrEmpty(l4) && string.IsNullOrEmpty(l3) && string.IsNullOrEmpty(l2) && (l1 == "един " || l1 == "една " || l1 == "едно "))
            {
                result = result + cqloSingle + SystavnoToStr(int.Parse(stotinki), systavnoSingle, systavnoMany, rodSystavno);
            }
            else
                result = result + cqloMany + SystavnoToStr(int.Parse(stotinki), systavnoSingle, systavnoMany, rodSystavno);
            return result;
        }

        private static string SystavnoToStr(int stotinki, string systavnoSingle, string systavnoMany, ROD rodSystavno)
        {
            if (stotinki == 0)
                return "";

            //Слагаме го стотинките да са само с цифри
            return " и " + stotinki.ToString("00") + " " + (stotinki == 1 ? systavnoSingle : systavnoMany);

            int iDec = 0;
            string sTemp = String.Empty;
            string result;

            if (stotinki > 10)
                iDec = stotinki / 10;
            int iSubDec = stotinki % 10;

            if (stotinki >= 10 && stotinki <= 19)
            {
                sTemp = w21[stotinki - 10];
                iDec = 0;
                iSubDec = 0;
            }

            if (iDec > 0)
                sTemp = w2[iDec];

            if (iSubDec > 0)
            {
                if (iDec > 0)
                    sTemp = sTemp + "и " + w_Stot[iSubDec];
                else
                    sTemp = w_Stot[iSubDec];
            }

            //sTemp  = AnsiUpperCase(Copy(sTemp,1,1))+Copy(sTemp,2,Length(sTemp)-1);
            if (sTemp == "еднa ")
            {
                sTemp = (rodSystavno == ROD.MYGKI) ? "един " : ((rodSystavno == ROD.GENSKI) ? "една " : "едно ");
                result = " и " + sTemp + systavnoSingle;
            }
            else
                result = " и " + sTemp + systavnoMany;

            return result;
        }

        public static string MoneyToString(decimal money)
        {
            return NumberToString(money, "лев", "лева", "стотинка", "стотинки", ROD.MYGKI, ROD.GENSKI);
        }

        public static string MoneyToStringUSD(decimal money)
        {
            return NumberToString(money, "долар", "долара", "цент", "центове", ROD.MYGKI, ROD.MYGKI);
        }

        public static string MoneyToStringEUR(decimal money)
        {
            return NumberToString(money, "евро", "евро", "цент", "цента", ROD.SREDEN, ROD.MYGKI);
        }

        public static string MoneyToStringCHF(decimal money)
        {
            return NumberToString(money, "швейцарски франк", "швейцарски франка", "рапен", "рапена", ROD.MYGKI, ROD.MYGKI);
        }

        public static string MoneyToStringGBP(decimal money)
        {
            return NumberToString(money, "британски паунд", "британски паунда", "пенс", "пенса", ROD.MYGKI, ROD.MYGKI);
        }

        public static string MoneyToStringTRY(decimal money)
        {
            return NumberToString(money, "Турска лира", "Турски лири", "куруш", "куруша", ROD.GENSKI, ROD.MYGKI);
        }

        public static string MoneyToStringRON(decimal money)
        {
            return NumberToString(money, "Румънска лея", "Румънски леи", "бан", "бани", ROD.GENSKI, ROD.MYGKI);
        }

        public static string MoneyToStringRSD(decimal money)
        {
            return NumberToString(money, "Сръбски динар", "Сръбски динари", "пара", "пари", ROD.MYGKI, ROD.GENSKI);
        }

        public static string MoneyToStringN_A(decimal money)
        {
            //За валута - Друго (с код N/A) да не се вижда никакви означения на валути
            return NumberToString(money, "", "", "", "", ROD.MYGKI, ROD.GENSKI);
        }

        public static string MoneyToString(decimal money, int currencyId)
        {
            switch (currencyId)
            {
                case NomenclatureConstants.Currency.BGN: 
                    return MoneyToString(money);
                case NomenclatureConstants.Currency.EUR:
                    return MoneyToStringEUR(money);
                case NomenclatureConstants.Currency.USD:
                    return MoneyToStringUSD(money);
                case NomenclatureConstants.Currency.CHF:
                    return MoneyToStringCHF(money);
                case NomenclatureConstants.Currency.GBP:
                    return MoneyToStringGBP(money);
                case NomenclatureConstants.Currency.TRY:
                    return MoneyToStringTRY(money);
                case NomenclatureConstants.Currency.RON:
                    return MoneyToStringRON(money);
                case NomenclatureConstants.Currency.RSD:
                    return MoneyToStringRSD(money);
                case NomenclatureConstants.Currency.N_A:
                    return MoneyToStringN_A(money);
                default:
                    return MoneyToString(money);
            }
        }

        public static string MoneyToString(decimal money, string currencyCode)
        {
            switch (currencyCode)
            {
                case NomenclatureConstants.CurrencyCode.BGN:
                    return MoneyToString(money);
                case NomenclatureConstants.CurrencyCode.EUR:
                    return MoneyToStringEUR(money);
                case NomenclatureConstants.CurrencyCode.USD:
                    return MoneyToStringUSD(money);
                case NomenclatureConstants.CurrencyCode.CHF:
                    return MoneyToStringCHF(money);
                case NomenclatureConstants.CurrencyCode.GBP:
                    return MoneyToStringGBP(money);
                case NomenclatureConstants.CurrencyCode.TRY:
                    return MoneyToStringTRY(money);
                case NomenclatureConstants.CurrencyCode.RON:
                    return MoneyToStringRON(money);
                case NomenclatureConstants.CurrencyCode.RSD:
                    return MoneyToStringRSD(money);
                case NomenclatureConstants.CurrencyCode.N_A:
                    return MoneyToStringN_A(money);
                default:
                    return MoneyToString(money);
            }
        }
    }
}
