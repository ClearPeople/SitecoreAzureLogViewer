using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using ClearPeople.Azure.Utylities.DataSources;
using ClearPeople.Azure.Utylities.Models;
using Sitecore.Shell.Applications.ContentEditor;
using SitecoreS = global::Sitecore;
using Sitecore.Azure.Deployments;
using Sitecore.Azure.Deployments.StorageProjects;
using Sitecore.Azure.Managers;
using Sitecore.Diagnostics;
using Sitecore.Web;
using Log = ClearPeople.Azure.Utylities.Models.Log;
using System.Collections.Generic;
using DateTime = System.DateTime;

namespace ClearPeople.AzureLogViewer
{
    public partial class SitecoreLogsViewer : System.Web.UI.Page
    {

        protected AzureDeployment Deployment
        {
            get
            {
                AzureDeployment deployment = AzureDeploymentManager.Current.GetDeployment(SitecoreS.Data.ID.Parse(UrlHandle.Get()["selDpl"]));
                Assert.IsNotNull((object)deployment, "deployment");
                return deployment;
            }
        }
        protected string UiThemeUrl
        {
            get
            {
                return
                    global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.JQueryUIThemeUrl");
            }
        }
        protected int LogType
        {
            get
            {
                return int.Parse(UrlHandle.Get()["logtype"]);
            }
        }
        protected string DateFormat
        {
            get
            {
                return global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.DateFormat");
                
            }
        }
        protected string DatePickerDateFormat
        {
            get
            {
                return global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.DatePickerDateFormat");
            }
        }
        protected string DatePickerTimeFormat
        {
            get
            {
                return global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.DatePickerTimeFormat");
            }
        }
        protected string NoRecordsFoundText
        {
            get
            {
                return global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.NoRecordsFoundText");
            }
        }
        protected string LoadingText
        {
            get
            {
                return global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.LoadingText");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Errors.Visible = false;
                ltlerrors.Text = "";
                Literal cssFile = new Literal() { Text = @"<link href=""" + UiThemeUrl + @""" type=""text/css"" rel=""stylesheet"" />" };
                Page.Header.Controls.Add(cssFile);

                if (!IsPostBack)
                {
                    WireUp();
                    if (global::Sitecore.Configuration.Settings.GetBoolSetting("ClearPeople.Azure.LogsViewer.BindGridOnLoad", false))
                        BindDataGrid(0);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            
        }
        private void WireUp()
        {
            try
            {
                int hours = global::Sitecore.Configuration.Settings.GetIntSetting("ClearPeople.Azure.LogsViewer.DefaultHoursToGet", 2) * -1;
                this.fromdatetime.Text = DateTime.Now.AddHours(-2).ToString(DateFormat, CultureInfo.InvariantCulture);
                this.todatetime.Text = DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);

                int i = 0;
                foreach (var wr in Deployment.ServiceDefinition.WebRole)
                {
                    Instances.Items.Add(string.Format("{0}_IN_{1}", new ListItem(wr.name), i.ToString()));
                }
                SetDateTimeFormat();

                Literal text = UpdateProgress1.FindControl("ltlloading") as Literal;
                    if (text != null)
                    {
                        text.Text = LoadingText;
                    }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            
            
        }
        private void LogError(Exception ex)
        {
            string html =
                "<p><span class=\"ui-icon ui-icon-alert\" style=\"margin-right: 0.3em; float: left;\"></span>{0}</p>";
            ltlerrors.Text += string.Format(html, ex.Message);
            Errors.Visible = true;
            global::Sitecore.Diagnostics.Log.Error("ClearPeople  Log Viewer Error", ex, this);
        }

        private void BindDataGrid(int page)
        {
            try
            {
                AzureStorage st = Deployment.Storages.AzureStorages.FirstOrDefault();
                //throw new Exception();
                bool nextvisible = false;
                LogsDataSource lds;
                switch (LogType)
                {
                    case 1:
                        lds = new SitecoreLogsDataSource(string.Format(global::Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.StoageConnectionString"), st.Protocol.Replace("://", ""), st.AccountName, st.PrimaryAccessKey));
                        break;

                    case 2:
                        lds = new SystemLogsDataSource(string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", st.Protocol.Replace("://", ""), st.AccountName, st.PrimaryAccessKey));
                        break;

                    default:
                        lds = new SitecoreLogsDataSource(string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}", st.Protocol.Replace("://", ""), st.AccountName, st.PrimaryAccessKey));
                        break;
                }
                //LogGridView.DataSource = lds;
                //LogGridView.SelectMethod = "GetPage";
                //LogGridView.VirtualItemCount
                Result<Log> logs = lds.GetPage(DateTime.ParseExact(fromdatetime.Text, DateFormat, CultureInfo.InvariantCulture),
                    DateTime.ParseExact(todatetime.Text, DateFormat, CultureInfo.InvariantCulture), int.Parse(Level.SelectedValue), int.Parse(PageSize.SelectedValue),
                    Instances.SelectedValue, page);
                LogGridView.TotalRecords = logs.CurrentTotal;
                if (logs.Data.Any())
                {
                    LogGridView.DataSource = logs.Data.ToList();
                    LogGridView.PageSize = int.Parse(PageSize.SelectedValue);
                    LogGridView.CustomPageIndex = page;                    

                }
                else
                {
                    LogGridView.DataSource = null;
                }
                LogGridView.DataBind();

                if (LogGridView.Rows.Count > 0)
                {
                    LogGridView.UseAccessibleHeader = true;
                    LogGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
                }
                if (LogGridView.FooterRow != null)
                {
                    Label tb = (Label)LogGridView.FooterRow.FindControl("FooterText");
                    if (tb != null)
                    {
                        tb.Text =
                            string.Format(
                                logs.HasMorepages
                                    ? global::Sitecore.Configuration.Settings.GetSetting(
                                        "ClearPeople.Azure.LogsViewer.CachingRowsPendingTableSummaryText")
                                    : global::Sitecore.Configuration.Settings.GetSetting(
                                        "ClearPeople.Azure.LogsViewer.FullyCachedTableSummaryText"),
                                ((LogGridView.CustomPageIndex * LogGridView.PageSize) + 1).ToString(),
                                ((LogGridView.CustomPageIndex + 1) * LogGridView.PageSize).ToString(),
                                LogGridView.Rows.Count.ToString(),
                                LogGridView.TotalRecords);

                    }
                    LogGridView.FooterRow.TableSection = TableRowSection.TableFooter;
                    if (LogGridView.TopPagerRow != null)
                    {
                        LogGridView.TopPagerRow.TableSection = TableRowSection.TableHeader;
                    }
                    if (LogGridView.BottomPagerRow != null)
                    {
                        LogGridView.BottomPagerRow.TableSection = TableRowSection.TableFooter;
                    }

                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            
        }      
  
        private void SetDateTimeFormat()
        {
            if (!ClientScript.IsClientScriptBlockRegistered("DateTime"))
            {
                string script = "$(function() {" +                    
                    string.Format("$(\'#fromdatetime\').datetimepicker({{ dateFormat: \"{0}\", timeFormat: \"{1}\" }});", DatePickerDateFormat, DatePickerTimeFormat) +
                    string.Format("$('#todatetime').datetimepicker({{ dateFormat: \"{0}\", timeFormat: \"{1}\" }});", DatePickerDateFormat, DatePickerTimeFormat) +
                    "});";
                ClientScript.RegisterClientScriptBlock(this.GetType(),
                    "DateTime", script,
                    true);
            }
        }

        protected void update_Click(object sender, EventArgs e)
        {
            BindDataGrid(0);
        }

        protected void LogGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            BindDataGrid(e.NewPageIndex);            
        }
    }
}