using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Helper
{
    public static class Utils
    {
        public static class Validation
        {
            /// <summary>
            /// Check value is EGN
            /// </summary>
            /// <param name="EGN">string</param>
            /// <returns>string</returns>
            public static bool IsEGN(string EGN, bool InitiallyValidation = false)
            {
                if (EGN == null) return false;
                if (EGN.Length != 10) return false;
                if (EGN == "0000000000") return false;

                // само първична валидация
                if (InitiallyValidation)
                {
                    decimal egn = 0;
                    if (!decimal.TryParse(EGN, out egn)) return false;
                    return true;
                }

                // пълна валидация
                int a = 0;
                int valEgn = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (!int.TryParse(EGN.Substring(i, 1), out a)) return false;
                    switch (i)
                    {
                        case 0:
                            valEgn += 2 * a;
                            continue;
                        case 1:
                            valEgn += 4 * a;
                            continue;
                        case 2:
                            valEgn += 8 * a;
                            continue;
                        case 3:
                            valEgn += 5 * a;
                            continue;
                        case 4:
                            valEgn += 10 * a;
                            continue;
                        case 5:
                            valEgn += 9 * a;
                            continue;
                        case 6:
                            valEgn += 7 * a;
                            continue;
                        case 7:
                            valEgn += 3 * a;
                            continue;
                        case 8:
                            valEgn += 6 * a;
                            continue;
                    }
                }
                long chkSum = valEgn % 11;
                if (chkSum == 10)
                    chkSum = 0;
                if (chkSum != Convert.ToInt64(EGN.Substring(9, 1))) return false;
                if ((int.Parse(EGN.Substring(8, 1)) / 2) == 0)
                {
                    // girl person
                    return true;
                }
                // guy person
                return true;
            }

            /// <summary>
            /// EIK validation
            /// </summary>
            /// <param name="EIK">string</param>
            /// <returns>bool</returns>
            public static bool IsEIK(string EIK)
            {
                if (EIK == null) return false;
                if ((EIK.Length != 9) && (EIK.Length != 13)) return false;

                int sum = 0, a = 0, chkSum = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (!int.TryParse(EIK.Substring(i, 1), out a)) return false;
                    sum += a * (i + 1);
                }
                chkSum = sum % 11;
                if (chkSum == 10)
                {

                    sum = 0;
                    a = 0;
                    chkSum = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!int.TryParse(EIK.Substring(i, 1), out a)) return false;
                        sum += a * (i + 3);
                    }
                    chkSum = sum % 11;
                    if (chkSum == 10) chkSum = 0;
                }

                if (chkSum.ToString() == EIK.Substring(8, 1))
                {
                    if (EIK.Length == 9)
                    {
                        return true;
                    }
                    else
                    {
                        for (int i = 8; i < 12; i++)
                        {
                            if (!int.TryParse(EIK.Substring(i, 1), out a)) return false;
                            switch (i)
                            {
                                case 8:
                                    sum = a * 2;
                                    continue;
                                case 9:
                                    sum += a * 7;
                                    continue;
                                case 10:
                                    sum += a * 3;
                                    continue;
                                case 11:
                                    sum += a * 5;
                                    continue;
                            }
                        }
                        chkSum = sum % 11;
                        if (chkSum == 10)
                        {
                            for (int i = 8; i < 12; i++)
                            {
                                if (!int.TryParse(EIK.Substring(i, 1), out a)) return false;
                                switch (i)
                                {
                                    case 8:
                                        sum = a * 4;
                                        continue;
                                    case 9:
                                        sum += a * 9;
                                        continue;
                                    case 10:
                                        sum += a * 5;
                                        continue;
                                    case 11:
                                        sum += a * 7;
                                        continue;
                                }
                            }
                            chkSum = sum % 11;
                            if (chkSum == 10) chkSum = 0;
                            if (chkSum.ToString() == EIK.Substring(12, 1))
                                return true;
                            else
                                return false;
                        }
                    }
                }
                else
                    return false;

                return true;
            }

            public static DateTime? GetBirthDayFromEgn(string EGN)
            {
                DateTime? result = null;
                if (IsEGN(EGN) == true)
                {
                    var year = int.Parse(EGN.Substring(0, 2));
                    var month = int.Parse(EGN.Substring(2, 2));
                    var day = int.Parse(EGN.Substring(4, 2));
                    if (month >= 1 && month <= 12)
                    {
                        year += 1900;
                    }
                    else if (month >= 21 && month <= 32)
                    {
                        month -= 20;
                        year += 1800;
                    }
                    else if (month >= 41 && month <= 52)
                    {
                        month -= 40;
                        year += 2000;
                    }

                    try
                    {
                        result = new DateTime(year, month, day);
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                }
                return result;
            }

            public static bool? IsMaleFromEGN(string egn)
            {
                bool? result = null;
                if (string.IsNullOrEmpty(egn))
                {
                    return result;
                }
                try
                {
                    var sexDigit = int.Parse(egn.Substring(8, 1));
                    if (sexDigit % 2 == 1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }
                catch (Exception ex) { }

                return result;
            }


        }
        /// <summary>
        /// Safe convert string to int
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <param name="nullIntValue">Default value if null or empty</param>
        /// <returns></returns>
        public static int ToInt(this string value, int nullIntValue = 0)
        {
            if (string.IsNullOrEmpty(value))
            {
                return nullIntValue;
            }

            try
            {
                return int.Parse(value);
            }
            catch { return nullIntValue; }
        }

        public static string ToShortCaseNumber(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (value.Length < 5)
            {
                return value.PadLeft(5, '0');
            }
            else
            {
                return value;
            }
        }

        public static string ToCasePaternSearch(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return "%";
            }
            return "%" + model.ToShortCaseNumber();
        }

        public static string DayDiggitName(this DateTime value)
        {
            switch (value.Day)
            {

                case 1: return "Първи";
                case 2: return "Втори";
                case 3: return "Трети";
                case 4: return "Четвърти";
                case 5: return "Пети";
                case 6: return "Шести";
                case 7: return "Седми";
                case 8: return "Осми";
                case 9: return "Девети";
                case 10: return "Десети";
                case 11: return "Единадесети";
                case 12: return "Дванадесети";
                case 13: return "Тринадесети";
                case 14: return "Четиринадесети";
                case 15: return "Петнадесети";
                case 16: return "Шестнадесети";
                case 17: return "Седемнадесети";
                case 18: return "Осемнадесети";
                case 19: return "Деветнадесети";
                case 20: return "Двадесети";
                case 21: return "Двадесет и първи";
                case 22: return "Двадесет и втори";
                case 23: return "Двадесет и трети";
                case 24: return "Двадесет и четвърти";
                case 25: return "Двадесет и пети";
                case 26: return "Двадесет и шести";
                case 27: return "Двадесет и седми";
                case 28: return "Двадесет и осми";
                case 29: return "Двадесет и девети";
                case 30: return "Тридесети";
                case 31: return "Тридесет и първи";
                default:
                    return value.Day.ToString();
            }

        }
        public static string MonthDiggitName(this DateTime value)
        {
            switch (value.Month)
            {

                case 1: return "Януари";
                case 2: return "февруари";
                case 3: return "Март";
                case 4: return "Април";
                case 5: return "Май";
                case 6: return "Юни";
                case 7: return "Юли";
                case 8: return "Август";
                case 9: return "Септември";
                case 10: return "Октомври";
                case 11: return "Ноември";
                case 12: return "Декември";
                default:
                    return value.Day.ToString();
            }
        }

        private static string yearMinorDiggitName(int minorYear)
        {
            switch (minorYear)
            {
                case 1: return "и първа";
                case 2: return "и втора";
                case 3: return "и трета";
                case 4: return "и четвърта";
                case 5: return "и пета";
                case 6: return "и шеста";
                case 7: return "и седма";
                case 8: return "и осма";
                case 9: return "и девета";
                case 10: return "и десета";
                case 11: return "и единадесета";
                case 12: return "и дванадесета";
                case 13: return "и тринадесета";
                case 14: return "и четиринадесета";
                case 15: return "и петнадесета";
                case 16: return "и шестнадесета";
                case 17: return "и седемнадесета";
                case 18: return "и осемнадесета";
                case 19: return "и деветнадесета";
                case 20: return "и двадесета";
                case 30: return "и тридесета";
                case 40: return "и четиридесета";
                case 50: return "и петдесета";
                case 60: return "и шестдесета";
                case 70: return "и седемдесета";
                case 80: return "и осемдесета";
                case 90: return "и деветдесета";
                default:
                    return string.Empty;
            }
        }

        public static string YearDiggitName(this DateTime value)
        {
            int dateYear = value.Year;
            switch (dateYear)
            {
                case 1900:
                    return "хиляда и девестотната";
                case 2000:
                    return "две хилядната";
                case 2100:
                    return "две хиляди и стотната";
            }

            int majorYear = int.Parse(value.Year.ToString("D2").Substring(0, 2)) * 100;
            int minorYearDecade = int.Parse(value.Year.ToString("D2").Substring(2, 2));
            int minorYearLast = int.Parse(value.Year.ToString("D2").Substring(3, 1));
            string result = "";
            switch (majorYear)
            {
                case 1900:
                    result = "хиляда деветстотин";
                    break;
                case 2000:
                    result = "две хиляди";
                    break;
                default:
                    break;
            }
            if (minorYearDecade <= 19 || minorYearLast == 0)
            {
                result += " " + yearMinorDiggitName(minorYearDecade);
            }
            else
            {
                result += " " + yearMinorDiggitName(minorYearDecade - minorYearLast).TrimEnd('а').TrimStart('и').Trim();
                result += " " + yearMinorDiggitName(minorYearLast);
            }

            return result;
        }

        public static string FullDateDiggitName(this DateTime value)
        {
            return $"{DayDiggitName(value)} {MonthDiggitName(value)}, през {YearDiggitName(value)} година".ToLower();

        }
    }
}
