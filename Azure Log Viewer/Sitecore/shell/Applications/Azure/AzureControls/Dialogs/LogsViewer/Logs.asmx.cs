using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using ClearPeople.Azure.Utylities.DataSources;
using ClearPeople.Azure.Utylities.Models;

namespace Azure_Log_Viewer.Sitecore.shell.Applications.Azure.AzureControls.Dialogs.LogsViewer
{
    /// <summary>
    /// Summary description for Logs
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Logs : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)]
        public Result<Log> GetPageSitecoreLogs(string storageaccount, DateTime? from, DateTime? to, int level, int pagesize, string instance, int page)
        {
            SitecoreLogsDataSource ds = new SitecoreLogsDataSource(storageaccount);
            return ds.GetPage(from, to, level, pagesize, instance, page);
        }
    }
}
