using System;
using System.Web.Caching;
using ClearPeople.Azure.Utylities.Models;
using ClearPeople.Azure.Utylities.Utilities;

namespace ClearPeople.Azure.Utylities.DataSources
{
    public class SystemLogsDataSource:LogsDataSource
    {
        public SystemLogsDataSource(string storageaccount)
            : base(storageaccount)
        {
        }

        public  override TableStoragePagingUtility<Log> Table
        {
            get { return this._table ?? new SystemLogsTable(_storageaccount, CacheItemPriority.Default, TimeSpan.FromMinutes(20)); }
        }
    }
}
