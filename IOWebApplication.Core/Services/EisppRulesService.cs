using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZXing;
using static IOWebApplication.Infrastructure.Constants.EISPPConstants;

namespace IOWebApplication.Core.Services
{
    public class EisppRulesService : BaseService, IEisppRulesService
    {
        public EisppRulesService(
            ILogger<EisppRulesService> _logger,
            IRepository _repo)
        {
            logger = _logger;
            repo = _repo;
        }

        private string[] EisppRulesValuesToIds(string values)
        {
            var result = new List<string>();
            foreach (var val in values.Split(','))
            {
                if (val.Contains("...", StringComparison.InvariantCultureIgnoreCase))
                {
                    var valArr = val.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase).Split("...");
                    if (valArr.Length == 2)
                    {
                        int fromVal;
                        int toVal;
                        _ = int.TryParse(valArr[0], out fromVal);
                        _ = int.TryParse(valArr[1], out toVal);
                        for (int i = fromVal; i <= toVal; i++)
                        {
                            result.Add(i.ToString());
                        }
                    }
                }
                else
                {
                    _ = int.TryParse(val.Trim(), out int id);
                    if (id != 0)
                        result.Add(id.ToString());
                }
            }
            return result.ToArray();
        }
        public (string[], int) GetEisppRuleIds(int eventType, string propName)
        {
            string values = "";
            int flags = 0;
            var eisppRule = repo.AllReadonly<EisppRules>()
                                .Where(x => x.EventType == eventType && x.ItemName == propName)
                                .FirstOrDefault();
            if (eisppRule != null) {
                values = eisppRule.Values;
                _ = int.TryParse(eisppRule.Flag, out flags);
            } else
            {
                if (propName.StartsWith("NPR.DLO.FZL.NKZ.PBC"))
                {
                    flags = 5;
                }
            }
            var rules = EisppRulesValuesToIds(values);
            return (rules, flags);
        }
        public string GetEisppRuleValue(int eventType, string propName)
        {
            (var rules, var flags) = GetEisppRuleIds(eventType, propName);
            if (rules.Length == 1)
            {
                return rules[0];
            }
            return "";
        }
        public int GetResSidFromRules(int eventType)
        {
            var eisppRule = repo.AllReadonly<EisppRules>()
                                .Where(x => x.EventType == eventType && x.ItemName == ".resSid")
                                .FirstOrDefault();
            if (eisppRule != null)
            {
                string value = eisppRule.Values;
                _ = int.TryParse(value, out int resSid);
                return resSid;
            }
            return 0;
        }
        /// <summary>
        /// Налага правилата от SBE_DESC
        /// </summary>
        /// <param name="message">XML</param>
        /// <param name="rules">Правила от nom_eispp_rules</param>
        /// <returns></returns>
        public async Task<string> ApplyRules(int structureId, string message, int eventType)
        {
            string result = "";
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                Async = true
            };
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Encoding = new UTF8Encoding(false);
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (XmlWriter writer = XmlWriter.Create(outStream, writerSettings))
                    {
                        using (XmlReader reader = XmlReader.Create(stream, settings))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        writer.WriteStartElement(reader.Name, reader.Value);
                                        var isEmptyElement = reader.IsEmptyElement;
                                        CopyAttributes(reader, writer);
                                        if (isEmptyElement)
                                            writer.WriteEndElement();
                                        if (reader.Name == "DATA")
                                            ApplyRulesInData(structureId, "", "DATA", reader, writer, eventType, false);
                                        break;
                                    case XmlNodeType.EndElement:
                                        writer.WriteEndElement();
                                        break;
                                    case XmlNodeType.XmlDeclaration:
                                        writer.WriteStartDocument();
                                        break;
                                    case XmlNodeType.Whitespace:
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        writer.Close();
                    }
                    result = Encoding.UTF8.GetString(outStream.ToArray());
                }
            }
            return result;
        }
        private void CopyAttributes(XmlReader reader, XmlWriter writer)
        {
            for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
            {
                reader.MoveToAttribute(attInd);
                writer.WriteAttributeString(reader.Name, reader.Value);
            }
        }
        private bool CopyAttributesInData(int structureId, string rulesPath, XmlReader reader, XmlWriter writer, int eventType, bool isDeleteEvent)
        {
            // writer.WriteAttributeString("rulesPath", rulesPath);
            bool result = isDeleteEvent;
            for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
            {
                reader.MoveToAttribute(attInd);
                if (reader.Name == "elementType" &&  reader.Value == EventKind.OldEvent)
                {
                    isDeleteEvent = true;
                }
                if (!IsAttribForSkip(rulesPath, reader.Name, reader.Value, eventType, isDeleteEvent))
                {
                    if (!isDeleteEvent)
                      CheckAttrib(structureId, rulesPath, reader.Name, reader.Value, eventType);
                    writer.WriteAttributeString(reader.Name, reader.Value);
                }
            }
            return result;
        }
        private void ApplyRulesInData(int structureId, string rulesPath, string nodeToExit, XmlReader reader, XmlWriter writer, int eventType, bool isDeleteEvent)
        {
            string elementToSkip = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (string.IsNullOrEmpty(elementToSkip))
                        {
                            string currentPath = rulesPath + (string.IsNullOrEmpty(rulesPath) ? "" : ".") + reader.Name;
                            if (IsNodeForSkip(currentPath, eventType))
                            {
                                if (!reader.IsEmptyElement)
                                    elementToSkip = reader.Name;
                                if (reader.Name == nodeToExit)
                                    return;
                            }
                            else
                            {
                                writer.WriteStartElement(reader.Name, reader.Value);
                                var isEmptyElement = reader.IsEmptyElement;
                                bool forExit = (reader.Name == nodeToExit);
                                string nodeName = reader.Name;
                                CopyAttributesInData(structureId, currentPath, reader, writer, eventType, isDeleteEvent);
                                if (isEmptyElement)
                                {
                                    writer.WriteEndElement();
                                    if (forExit)
                                        return;
                                }
                                else
                                {
                                    ApplyRulesInData(structureId, currentPath, nodeName, reader, writer, eventType, isDeleteEvent);
                                }
                            }
                        } 
                        break;
                    case XmlNodeType.EndElement:
                        if (string.IsNullOrEmpty(elementToSkip))
                        {
                            writer.WriteEndElement();
                            if (nodeToExit == reader.Name)
                                return ;
                        } else
                        {
                            if (elementToSkip == reader.Name)
                                elementToSkip = "";
                            if (nodeToExit == reader.Name)
                                return;
                        }
                        break;
                }
            }
        }
        private bool IsNodeForSkip(string rulesPath, int eventType) {
            if (rulesPath == "VHD" || rulesPath == "VHD.SBE" || rulesPath == "KST" || rulesPath == "VHD.SBE.NPR.DLO.PNE.ADR")
                return false;
            rulesPath = rulesPath.Replace("VHD.SBE.", "", StringComparison.InvariantCultureIgnoreCase);

            (var ruleIDs, var flags) = GetEisppRuleIds(eventType, rulesPath);
            if (flags == 0)
            {
                (var ruleIDs2, var flags2) = GetEisppRuleIds(eventType, rulesPath+".*");
                flags = flags2;
            }
            if (flags != 0)
                return false;
            return true;
        }

        private string MakeRulesPathAttr(string rulesPath, string attrName)
        {
            if (rulesPath == "VHD.SBE")
                return attrName;
            if (rulesPath == "KST")
                return  "." + attrName;
            return rulesPath.Replace("VHD.SBE.", "", StringComparison.InvariantCultureIgnoreCase)+ "." + attrName;
        }
        private bool IsAttribForSkip(string rulesPath, string attrName, string attrVal, int eventType, bool isDeleteEvent)
        {
            if (rulesPath == "VHD.SBE" && eventType < 0)
                return (attrName != "elementType" && attrName != "sbesid");
            if (isDeleteEvent)
                return (attrVal == "0" || string.IsNullOrEmpty(attrVal));

            bool autoAdd = (rulesPath == "VHD.SBE" || rulesPath == "KST" );
            bool isPunishment = rulesPath.EndsWith("FZL.NKZ", StringComparison.InvariantCultureIgnoreCase);
            bool isCrimeSubjectStatisticData = rulesPath.EndsWith("NPRFZLPNE.SBC", StringComparison.InvariantCultureIgnoreCase);
            bool autoAddAll = rulesPath.EndsWith("FZL", StringComparison.InvariantCultureIgnoreCase);
            bool isSid = attrName.EndsWith("sid", StringComparison.InvariantCultureIgnoreCase);
            rulesPath = MakeRulesPathAttr(rulesPath, attrName);

            (var ruleIDs, var flags) = GetEisppRuleIds(eventType, rulesPath);
            if (flags == 0)
            {
                string rulesPath2 = rulesPath.Replace("." + attrName, ".*", StringComparison.InvariantCulture);
                (var ruleIDs2, var flags2) = GetEisppRuleIds(eventType, rulesPath2);
                if (flags2 > 0)
                    autoAdd = true;
            }
            if (flags == 0 && !autoAdd)
            {
                string rulesPath2 = rulesPath.Replace("." + attrName, "", StringComparison.InvariantCulture);
                (var ruleIDs2, var flags2) = GetEisppRuleIds(eventType, rulesPath2);
                if (flags2 > 0)
                    autoAdd = true;
            }
            if ((isPunishment && attrName == "nkzrzm") || attrName == "nkzpnerzm")
                return false;
            if (isPunishment && attrVal == "0")
                return true;
            if (isCrimeSubjectStatisticData && attrVal == "0")
                return true;
            if (flags != 0)
                return false;
            if (autoAdd && attrVal != "0")
                return false;
            if (autoAddAll)
                return false;
            if (isSid)
                return false;
            return true;
        }
        private void CheckAttrib(int structureId, string rulesPath, string attrName, string attrVal, int eventType)
        {
            if (attrName == "resSid")
                return;
            rulesPath = MakeRulesPathAttr(rulesPath, attrName);
            (var ruleIDs, var flags) = GetEisppRuleIds(eventType, rulesPath);
            if (flags != 0 && ruleIDs.Length > 0)
            {
                if (ruleIDs.Length == 1 && ruleIDs[0] == "-1")
                {
                    if (structureId.ToString() != attrVal)
                        throw new ArgumentException($"{rulesPath} трябва да е {structureId}");
                } else
                {
                    if (attrVal == "0" && (flags & 2) == 0)
                        return;
                    if (!ruleIDs.Contains(attrVal))
                        throw new ArgumentException($"{rulesPath} трябва да е в {ruleIDs.Join()} a e {attrVal}");
                }
            }
        }
        public void SetIsSelectedAndClear(EisppPackage model)
        {
            foreach (var eisppEvent in model.Data.Events)
            {
                if (eisppEvent.CriminalProceeding.Case.ConnectedCases != null)
                {
                    eisppEvent.CriminalProceeding.Case.ConnectedCases = eisppEvent.CriminalProceeding.Case.ConnectedCases
                                                                                       .Where(x => x.ConnectedCaseId == eisppEvent.CriminalProceeding.Case.ConnectedCaseId)
                                                                                       .ToArray();
                }
                if (eisppEvent.CriminalProceeding.Case.Persons != null)
                {
                    eisppEvent.CriminalProceeding.Case.Persons = eisppEvent.CriminalProceeding.Case.Persons.Where(x => x.IsSelected).ToArray();
                    foreach (var person in eisppEvent.CriminalProceeding.Case.Persons)
                    {
                        if (person.Measures != null)
                            person.Measures = person.Measures.Where(x => x.IsSelected).ToArray();
                        if (person.Punishments != null) 
                        {
                            person.Punishments = person.Punishments
                                                       .Where(x => x.PunishmentKind < 90000 && 
                                                                  x.IsSelected)
                                                       .ToArray();
                            foreach (var punishment in person.Punishments)
                            {
                                ClearPunishmentUnnecessaryField(punishment);
                            }
                        }
                    }
                }
                if (eisppEvent.CriminalProceeding.Case.CPPersonCrimes != null) 
                {
                    eisppEvent.CriminalProceeding.Case.CPPersonCrimes = eisppEvent.CriminalProceeding.Case.CPPersonCrimes.Where(x => x.IsSelected).ToArray();
                    foreach(var cpPersonCrimes in eisppEvent.CriminalProceeding.Case.CPPersonCrimes)
                    {
                        if (cpPersonCrimes.CrimeSanction?.CrimePunishments != null)
                            cpPersonCrimes.CrimeSanction.CrimePunishments = cpPersonCrimes.CrimeSanction.CrimePunishments.Where(x => x.IsSelected && x.PunishmentKind < 90000).ToArray();
                    }
                    foreach(var crime in eisppEvent.CriminalProceeding.Case.Crimes)
                    {
                        crime.IsSelected = eisppEvent.CriminalProceeding.Case.CPPersonCrimes.Any(x => x.CrimeId == crime.CrimeId);
                    }
                    eisppEvent.CriminalProceeding.Case.Crimes = eisppEvent.CriminalProceeding.Case.Crimes.Where(x => x.IsSelected).ToArray();
                }
            }
        }
        public string GetPunishmentKindMode(int punishmentKind)
        {
            return repo.AllReadonly<CodeMapping>()
                       .Where(x => x.Alias == EisppMapping.PunismentKindMap && x.OuterCode == punishmentKind.ToString())
                       .Select(x => x.InnerCode)
                       .FirstOrDefault() ?? "";
        }

        private void ClearPunishmentPeriod(Punishment punishment)
        {
            punishment.PunishmentYears = 0;
            punishment.PunishmentMonths = 0;
            punishment.PunishmentWeeks = 0;
            punishment.PunishmentDays = 0;
        }
        
        private void ClearPunishmentProbationPeriod(Punishment punishment)
        {
            punishment.ProbationYears = 0;
            punishment.ProbationMonths = 0;
            punishment.ProbationWeeks = 0;
            punishment.ProbationDays = 0;
        }

        public void ClearPunishmentUnnecessaryField(Punishment punishment)
        {
            var punishmentKindMode = GetPunishmentKindMode(punishment.PunishmentKind);
            switch (punishmentKindMode)
            {
                case PunishmentVal.efective:
                    ClearPunishmentPeriod(punishment);
                    ClearPunishmentProbationPeriod(punishment);
                    punishment.ProbationMeasure = null;
                    punishment.FineAmount = 0;
                    break;
                case PunishmentVal.effective_period:
                    ClearPunishmentProbationPeriod(punishment);
                    punishment.ProbationMeasure = null;
                    punishment.FineAmount = 0;
                    break;
                case PunishmentVal.probation_period:
                    punishment.ProbationMeasure = null;
                    punishment.FineAmount = 0;
                    punishment.PunishmentRegime = 0;
                    break;
                case PunishmentVal.period:
                    if (punishment.ServingType == ServingType.Efective)
                    {
                        ClearPunishmentProbationPeriod(punishment);
                    }
                    else
                    {
                        punishment.PunishmentRegime = 0;
                        // ClearPunishmentPeriod(punishment);
                    }
                    punishment.ProbationMeasure = null;
                    punishment.FineAmount = 0;
                    break;
                case PunishmentVal.fine:
                    ClearPunishmentPeriod(punishment);
                    ClearPunishmentProbationPeriod(punishment);
                    punishment.ProbationMeasure = null;
                    punishment.PunishmentRegime = 0;
                    break;
                case PunishmentVal.probation:
                    ClearPunishmentProbationPeriod(punishment);
                    punishment.FineAmount = 0;
                    punishment.PunishmentRegime = 0;
                    break;
                default:
                    ClearPunishmentPeriod(punishment);
                    ClearPunishmentProbationPeriod(punishment);
                    punishment.ProbationMeasure = null;
                    punishment.FineAmount = 0;
                    punishment.PunishmentRegime = 0;
                    break;
            }
        }
        /// <summary>
        /// От пробационни мерки прави наказания
        /// </summary>
        /// <param name="eisppEvent">Събитие</param>
        public void CreatePunismentFromProbationMeasuares(EisppPackage model)
        {
            int sid = -20000;
            foreach (var eisppEvent in model.Data.Events)
            {
                if (eisppEvent.CriminalProceeding.Case.Persons != null)
                {
                    foreach (var person in eisppEvent.CriminalProceeding.Case.Persons.Where(x => x.IsSelected))
                    {
                        if (person.Punishments != null)
                        {
                            var punishments = new List<Punishment>();
                            foreach (var punishment in person.Punishments.Where(x => x.IsSelected))
                            {
                                if (punishment.PunishmentKind == PunismentKind.probation && punishment.ProbationMeasures != null)
                                {
                                    bool isSetFirst = false;
                                    foreach (var probationMeasure in punishment.ProbationMeasures.Where(x => x.IsSelected))
                                    {
                                        if (!isSetFirst)
                                        {
                                            punishment.ProbationMeasure = probationMeasure;
                                            isSetFirst = true;
                                        }
                                        else
                                        {
                                            var newPunishment = new Punishment();
                                            newPunishment.ProbationMeasure = probationMeasure;
                                            newPunishment.PunishmentId = sid--;
                                            newPunishment.CasePersonSentencePunishmentId = punishment.CasePersonSentencePunishmentId;
                                            newPunishment.PunishmentType = punishment.PunishmentType;
                                            newPunishment.PunishmentKind = punishment.PunishmentKind;
                                            newPunishment.PunishmentRegime = punishment.PunishmentRegime;
                                            newPunishment.ServingType = punishment.ServingType;
                                            newPunishment.StructureId = punishment.StructureId;
                                            newPunishment.IsSelected = punishment.IsSelected;
                                            newPunishment.PunishmentYears = punishment.PunishmentYears;
                                            newPunishment.PunishmentMonths = punishment.PunishmentMonths;
                                            newPunishment.PunishmentWeeks = punishment.PunishmentWeeks;
                                            newPunishment.PunishmentDays = punishment.PunishmentDays;
                                            newPunishment.ProbationYears = punishment.ProbationYears;
                                            newPunishment.ProbationMonths = punishment.ProbationMonths;
                                            newPunishment.ProbationWeeks = punishment.ProbationWeeks;
                                            newPunishment.ProbationDays = punishment.ProbationDays;
                                            newPunishment.PunishmentActivity = punishment.PunishmentActivity;
                                            newPunishment.PunishmentActivityDate = punishment.PunishmentActivityDate;
                                            punishments.Add(newPunishment);
                                        }
                                        probationMeasure.MeasureId = sid--;
                                    }
                                }
                            }
                            var personPunishments = person.Punishments.ToList();
                            personPunishments.AddRange(punishments);
                            person.Punishments = personPunishments.ToArray();
                        }
                    }
                }
            }
        }

    }
}
