using IOWebApplication.ReportViewer.Data.Context;
using IOWebApplication.ReportViewer.Models.Context;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;

namespace IOWebApplication.ReportViewer
{
  
  public class ManageReport
  {
    public ReportRequest GetCurrentRequest(string requestGUID)
    {
      EissDbContext db = new EissDbContext();
      return db.ReportRequests
                          .Where(x => x.Id == requestGUID)
                          .FirstOrDefault();
      
    }

    public ReportRequest ManageRequest(ReportRequest current)
    {
     
      EissDbContext db = new EissDbContext();
      ReportRequest saved = db.ReportRequests.Find(current.Id);
      saved.ValidationGuid = Guid.NewGuid().ToString();
      saved.DateGetReport = DateTime.Now;
      db.SaveChanges();
      return saved;

    }
    public string GetReportUrl(int id)
    {
      
      EissDbContext db = new EissDbContext();
      string url = "/";
      url+=db.Reports.Find(id).ReportPath;
     
     
      return url;

    }
  }
}