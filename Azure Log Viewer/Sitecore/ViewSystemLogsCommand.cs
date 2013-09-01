using Sitecore.Azure.Deployments;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.Azure.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace ClearPeople.AzureLogViewer.Sitecore
{
    public class ViewSystemLogs : BaseCommand
    {
        protected override CommandState QueryStateWorker(ManagerCommandContext managerCommandContext)
        {
            Assert.IsNotNull((object)managerCommandContext, "context");
            switch (managerCommandContext.DeploymentStatus.ExternalStatus)
            {
                case ExternalStatus.Deleting:
                case ExternalStatus.Deploying:
                case ExternalStatus.NoDeployment:
                case ExternalStatus.Unknown:
                    return CommandState.Disabled;
                default:
                    return CommandState.Enabled;
            }
        }

        protected override void ExecuteWorker(ManagerCommandContext managerCommandContext)
        {
            Assert.ArgumentNotNull((object)managerCommandContext, "managerCommandContext");
            UrlHandle urlHandle = new UrlHandle();
            urlHandle["selDpl"] = managerCommandContext.Deployment.ID.ToString();
            urlHandle["logtype"] = "2";
            UrlString urlString = new UrlString("/sitecore/shell/Applications/Azure/AzureControls/Dialogs/LogsViewer/SitecoreLogsViewer.aspx");
            urlHandle.Add(urlString);
            string width = global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.DialogWidth");
            string height = global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.DialogHeight");
            SheerResponse.ShowModalDialog(urlString.ToString(), width, height, "Sitecore Logs Viewer", false);
        }
    }
}
