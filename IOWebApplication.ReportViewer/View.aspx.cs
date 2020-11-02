using IOWebApplication.ReportViewer.Data.Context;
using IOWebApplication.ReportViewer.Models.Context;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOWebApplication.ReportViewer
{
  public partial class View : System.Web.UI.Page
  {

    protected void Page_Init(object sender, EventArgs e)
    {
      if (Request.QueryString["guid"] is null)
      { Session["Error"] = "Ресурсът не е открит";
        Response.Redirect("ERROR_PAGE.aspx");
      }
        
      string sGUID = Request.QueryString["guid"];

      ManageReport manageReport = new ManageReport();
      var curentRequest = new ReportRequest();
      
      ReportViewer1.ServerReport.ReportServerCredentials =
       new MyReportServerCredentials();


      
        curentRequest = manageReport.GetCurrentRequest(sGUID);
      if (curentRequest is null)
      {
        Session["Error"] = "Ресурсът не е открит";
        Response.Redirect("ERROR_PAGE.aspx");
      }
      if (!Page.IsPostBack)
      {
        if ((curentRequest.DateGetReport??DateTime.Now.AddDays(1))>DateTime.Now)
        {
           curentRequest = manageReport.ManageRequest(curentRequest);
          Session["ValidationGuid"] = curentRequest.ValidationGuid;

        }
      }

      if (Session["ValidationGuid"] is null)
      {
        Session["Error"] = "Невалидна Сесия";
        Response.Redirect("ERROR_PAGE.aspx");
      }
      string ss = Session["ValidationGuid"].ToString();
      string sss = curentRequest.ValidationGuid;
      if (Session["ValidationGuid"].ToString() == curentRequest.ValidationGuid)
      {
        // Set the processing mode for the ReportViewer to Remote  
        ReportViewer1.ProcessingMode = ProcessingMode.Remote;

        ServerReport serverReport = ReportViewer1.ServerReport;
        //====================================================================
        //Set the report server URL and report path
        string uri_str = manageReport.GetReportUrl(curentRequest.ReportId);

        serverReport.ReportServerUrl = new Uri(ConfigurationManager.AppSettings["ReportServer"]);
        serverReport.ReportPath = ConfigurationManager.AppSettings["ReportsPrefix"] +manageReport.GetReportUrl(curentRequest.ReportId);
        ReportParameter ID = new ReportParameter();
        ID.Name = "court_id";
        string value_string = curentRequest.CourtList;
        ID.Values.Add(value_string);
        ReportViewer1.ServerReport.SetParameters(new ReportParameter[] { ID });
     
      }
      else
      {
        Session["Error"] = "Невалидна Сесия";
        Response.Redirect("ERROR_PAGE.aspx");
      }


    }
    protected void Page_Load(object sender, EventArgs e)
    {
      ReportViewer1.AsyncRendering = false;
      ReportViewer1.SizeToReportContent = true;
      ReportViewer1.ZoomMode = ZoomMode.FullPage;
    }


  }
}