// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
    }
}
