using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOWebApplication.ReportViewer
{
  public partial class ERROR_PAGE : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      if (Session["Error"] != null)
      { lblError.Text = Session["Error"].ToString(); }
    }
  }
}