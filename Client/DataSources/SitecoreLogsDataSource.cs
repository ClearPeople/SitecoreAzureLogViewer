using System;
using System.Web.Caching;
using ClearPeople.Azure.Utylities.Models;
using ClearPeople.Azure.Utylities.Utilities;

namespace ClearPeople.Azure.Utylities.DataSources
{
    public class SitecoreLogsDataSource:LogsDataSource
    {
        public SitecoreLogsDataSource(string storageaccount) : base(storageaccount)
        {
        }

        public  override TableStoragePagingUtility<Log> Table
        {
            get { return this._table ?? new SitecoreLogsTable(_storageaccount, CacheItemPriority.Default, TimeSpan.FromMinutes(20)); }
        }
    }
}
