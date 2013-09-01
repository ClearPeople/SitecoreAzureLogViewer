using System;
using System.ComponentModel;
using System.Linq;
using ClearPeople.Azure.Utylities.Models;
using ClearPeople.Azure.Utylities.Utilities;
using Microsoft.WindowsAzure;

namespace ClearPeople.Azure.Utylities.DataSources
{
    [DataObject]
    public abstract class LogsDataSource
    {
        internal CloudStorageAccount _storageaccount;
        internal TableStoragePagingUtility<Log> _table;
        public abstract TableStoragePagingUtility<Log> Table { get; }

        public LogsDataSource(string storageaccount)
        {
            _storageaccount = CloudStorageAccount.Parse(storageaccount);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public Result<Log> GetPage(DateTime? from, DateTime? to, int level, int pagesize, string instance, int page)
        {
            var sclogs = Table;
            
            IQueryable<Log> query = sclogs.GetQuery;

            if (from != null)
            {
                from = from.Value.AddSeconds(-from.Value.Second);
                from = from.Value.ToUniversalTime();
                var partitionKey = "0" + from.Value.Ticks.ToString();
                query = query.Where(l => l.PartitionKey.CompareTo(partitionKey)>=0 && l.Timestamp >= from);
                //query = query.Where(l => l.Timestamp >= from);
            }
            if (to != null)
            {
                to = to.Value.AddSeconds(-to.Value.Second);
                to = to.Value.ToUniversalTime();
                var partitionKey = "0" + to.Value.Ticks.ToString();
                query = query.Where(l => l.PartitionKey.CompareTo(partitionKey) <= 0 && l.Timestamp <= to);
                //query = query.Where(l => l.Timestamp <= to);
            }
            if (!string.IsNullOrEmpty(instance))
                query = query.Where(l => l.RoleInstance == instance);
            if (level < 6)
                query = query.Where(l => l.Level <= level);
            return sclogs.GetPage(query, pagesize, page);            
        }
        
    }

}

