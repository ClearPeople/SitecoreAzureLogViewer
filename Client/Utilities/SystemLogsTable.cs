using System;
using System.Web.Caching;
using ClearPeople.Azure.Utylities.Models;
using Microsoft.WindowsAzure;

namespace ClearPeople.Azure.Utylities.Utilities
{
    class SystemLogsTable:TableStoragePagingUtility<Log>
    {
        public SystemLogsTable(CloudStorageAccount cloudStorageAccount, CacheItemPriority priority, TimeSpan slidingExpiration)
            : base(cloudStorageAccount, priority, slidingExpiration)
        {
            this._tableServiceContext = new LogDataContext(cloudStorageAccount.TableEndpoint.AbsoluteUri, cloudStorageAccount.Credentials);
        }

        public override string GetTableName()
        {
            return Sitecore.Configuration.Settings.GetSetting("ClearPeople.Azure.LogsViewer.WindowsEventsTableName");            
        }
    }
}
