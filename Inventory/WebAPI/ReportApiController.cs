using Syncfusion.EJ.ReportViewer;
using Syncfusion.Reports.EJ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Micraft.BBraun.WebAPI
{
    public class ReportApiController : ApiController, IReportController
    {
        // GET api/<controller>
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            

            return ReportHelper.ProcessReport(jsonResult, this);

        }

        [System.Web.Http.ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            //reportOption.ReportModel.ReportServerCredential = new System.Net.NetworkCredential("username", "password");
            //reportOption.ReportModel.DataSourceCredentials.Add(new DataSourceCredentials("DataSource1 ", "sa", "shinde1976", "Data Source=MICRAFT;Initial Catalog=BBraun_HR;User Id=sa;password=shinde1976; Encrypt=True;", false));  

            DataSourceCredentials credn = new DataSourceCredentials();
            credn.Name = "dsInventory";
            credn.UserId = "sa";
            credn.Password = "shinde1976";
            reportOption.ReportModel.DataSourceCredentials.Add(credn);
            
        }

        public void OnReportLoaded(ReportViewerOptions reportOption)
        {

        }
    }
}
