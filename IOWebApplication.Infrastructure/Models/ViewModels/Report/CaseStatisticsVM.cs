using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseStatisticsVM
    {
        public int CourtId { get; set; }

        public int CaseTypeId { get; set; }

        public int DocumentTypeId { get; set; }

        public int CaseCodeId { get; set; }

        public int ActComplainResultId { get; set; }

        public int ActTypeId { get; set; }

        public int ActComplainIndexId { get; set; }

        public int ActISPNReasonId { get; set; }

        public int Count { get; set; }

        public int ExcelCol { get; set; }

        public int ExcelRow { get; set; }

        public string LawUnitName
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(LawUnitData) == false)
                {
                    string[] name = LawUnitData.Split(",,");
                    if (name.Length == 2)
                        result = name[1];
                }
                return result;
            }
        }

        public int LawUnitId
        {
            get
            {
                int result = 0;
                if (string.IsNullOrEmpty(LawUnitData) == false)
                {
                    string[] name = LawUnitData.Split(",,");
                    if (name.Length == 2)
                    {
                        if (int.TryParse(name[0], out result) == false)
                            result = 0;
                    }
                }
                return result;
            }
        }

        public string LawUnitData { get; set; }

        public string FromCourtData { get; set; }

        public string FromCourtId
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(FromCourtData) == false)
                {
                    string[] name = FromCourtData.Split(",,");
                    if (name.Length == 3)
                    {
                        result = name[0];
                    }
                }
                return result;
            }
        }

        public string FromCourtName
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(FromCourtData) == false)
                {
                    string[] name = FromCourtData.Split(",,");
                    if (name.Length == 3)
                        result = name[1];
                }
                return result;
            }
        }

        public int FromCourtIsParent
        {
            get
            {
                int result = 0;
                if (string.IsNullOrEmpty(FromCourtData) == false)
                {
                    string[] name = FromCourtData.Split(",,");
                    if (name.Length == 3)
                    {
                        if (int.TryParse(name[2], out result) == false)
                            result = 0;
                    }
                }
                return result;
            }
        }

        public int ProcessPriorityId { get; set; }

        public bool IsDuration { get; set; } = false;

        public TimeSpan Duration { get; set; }
    }


    public class StatisticsExcelReportIndexVM
    {
        public int CourtTypeId { get; set; }

        public string CaseGroupId { get; set; }
        public List<int> CaseGroupCaseTypeIds { get; set; }

        public string CaseTypeIds { get; set; }

        public string ActTypeIds { get; set; }

        public string ActComplainIndexIds { get; set; }

        public List<int> CaseTypes
        {
            get
            {
                return CaseGroupId != null ? CaseGroupCaseTypeIds :
                    CaseTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public List<int> ActTypes
        {
            get
            {
                return ActTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }


        public List<int> ActComplainIndex
        {
            get
            {
                return ActComplainIndexIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }


        public int Col { get; set; }

        public int? SheetIndex { get; set; }
    }

    public class StatisticsExcelReportComplainIndexVM
    {
        public int CourtTypeId { get; set; }

        public int SheetIndex { get; set; }

        public string ActComplainResultIds { get; set; }


        public List<int> ActComplainResult
        {
            get
            {
                return ActComplainResultIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }


        public int Col { get; set; }

        public string SismaIndex { get; set; }
    }

    public class StatisticsExcelReportCaseCodeRowVM
    {
        public string CaseCodeIds { get; set; }

        public int SheetIndex { get; set; }

        public int RowIndex { get; set; }

        public string CaseCodeLabel { get; set; }

        public int CourtTypeId { get; set; }

        public List<int> CaseCode
        {
            get
            {
                return CaseCodeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public string ExcludeColIds { get; set; }

        public List<int> ExcludeCol
        {
            get
            {
                return ExcludeColIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }
    }

    public class StatisticsExcelReportCaseTypeRowVM
    {
        public int SheetIndex { get; set; }

        public int RowIndex { get; set; }

        public int CourtTypeId { get; set; }

        public string CaseTypeIds { get; set; }

        public List<int> CaseType
        {
            get
            {
                return CaseTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }
        public string DocumentTypeIds { get; set; }

        public List<int> DocumentType
        {
            get
            {
                return DocumentTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public string CaseCodeIds { get; set; }

        public List<int> CaseCode
        {
            get
            {
                return CaseCodeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public string ForColumnIds { get; set; }

        public List<int> ForColumns
        {
            get
            {
                return ForColumnIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public bool IsTrue { get; set; }

        public string ProcessPriorityIds { get; set; }

        public List<int> ProcessPriority
        {
            get
            {
                return ProcessPriorityIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }
    }

    public class StatisticsExcelReportCaseTypeColVM
    {
        public int ColIndex { get; set; }

        public int CourtTypeId { get; set; }
        public int ReportTypeId { get; set; }

        public string CaseTypeIds { get; set; }

        public List<int> CaseType
        {
            get
            {
                return CaseTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public string DocumentTypeIds { get; set; }

        public List<int> DocumentType
        {
            get
            {
                return DocumentTypeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public string CaseCodeIds { get; set; }

        public List<int> CaseCode
        {
            get
            {
                return CaseCodeIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }

        public bool IsTrue { get; set; }

        public string SismaIndex { get; set; }

        public string ProcessPriorityIds { get; set; }

        public List<int> ProcessPriority
        {
            get
            {
                return ProcessPriorityIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }
    }

    public class StatisticsExcelReportIspnReasonVM
    {
        public int CourtTypeId { get; set; }

        public int SheetIndex { get; set; }

        public string ActIspnReasonIds { get; set; }


        public List<int> ActIspnReason
        {
            get
            {
                return ActIspnReasonIds.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
        }


        public int Col { get; set; }
    }
    
    public class StatisticsNomDataVM
    {
        public List<StatisticsExcelReportIndexVM> excelReportIndexCols { get; set; }

        public List<StatisticsExcelReportCaseCodeRowVM> excelReportCaseCodeRows { get; set; }

        public List<StatisticsExcelReportComplainIndexVM> excelReportComplainResults { get; set; }

        public List<StatisticsExcelReportCaseTypeRowVM> excelReportCaseTypeRows { get; set; }

        public List<StatisticsExcelReportCaseTypeColVM> excelReportCaseTypeCols { get; set; }

        public List<StatisticsExcelReportIspnReasonVM> excelReportIspnReasons { get; set; }

        public List<SismaIndexRecap> sismaIndexRecaps { get; set; }

        public List<ExcelSismaMapping> excelSismaMappings { get; set; }

        public List<Court> courts { get; set; }
    }
    
    public class SismaCaseStatisticsVM
    {
        public string CourtCode { get; set; }

        public int CaseCodeId { get; set; }

        public int ActComplainResultId { get; set; }

        public string SismaIndex { get; set; }

        public int Count { get; set; }

        public int CountRecap { set; get; }

        public string SubjectCode
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(CodeData) == false)
                {
                    string[] name = CodeData.Split(",,");
                    if (name.Length > 0)
                        result = name[0];
                }
                return result;
            }
        }

        public string SubjectName
        {
            get
            {
                string result = "";
                if (string.IsNullOrEmpty(CodeData) == false)
                {
                    string[] name = CodeData.Split(",,");
                    if (name.Length == 2)
                        result = name[1];
                }
                return result;
            }
        }

        public string CodeData { get; set; }

        public int CaseTypeId { get; set; }

        public int DocumentTypeId { get; set; }

        public int ProcessPriorityId { get; set; }
    }

    public class CaseIspnDuration()
    {
        public int CourtId { get; set; }

        public int? CaseId { get; set; }

        public int CaseSessionActId { get; set; }

        public DateTime ActDeclaredDate { get; set; }
    }

    public class CaseRequest760Duration()
    {
        public int CourtId { get; set; }

        public int CaseId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
