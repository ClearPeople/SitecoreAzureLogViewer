/****************************** Module Header ******************************\
* Module Name:	TableStoragePagingUtility.cs
* Project:		AzureTableStoragePaging
* Copyright (c) Microsoft Corporation.
* 
* This class can be reused for other applications. If you want 
* to reuse the code, what you need to do is to implement custom ICachedDataProvider<T> 
* class to store data required by TableStoragePagingUtility<T>. 
* 
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
* All other rights reserved.
* 
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
* WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.Linq;
using System.Web.Caching;
using ClearPeople.Azure.Utylities.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Monty.Linq;

namespace ClearPeople.Azure.Utylities.Utilities
{
    public abstract class TableStoragePagingUtility<T>
    {
        private CloudStorageAccount _cloudStorageAccount;

        internal TableServiceContext _tableServiceContext;
        public abstract string GetTableName();
        //private const string Currentpagesessionkey = "TableStoragePagingUtilitycurrentpage";
        private readonly CacheItemPriority priority;
        private readonly TimeSpan slidingExpiration;

        public bool HasMorePages(CloudTableQuery<T> query)
        {
            var cached = query.CachedResults(priority, slidingExpiration);
            return !cached.HasReachedEnd;
        }



        public TableStoragePagingUtility(CloudStorageAccount cloudStorageAccount, CacheItemPriority priority,
            TimeSpan slidingExpiration)
        {
            _cloudStorageAccount = cloudStorageAccount;
            this.priority = priority;
            this.slidingExpiration = slidingExpiration;

        }

        public IQueryable<T> GetQuery
        {
            get { return this._tableServiceContext.CreateQuery<T>(this.GetTableName()); }
        }

        public CloudTableQuery<T> GetCloudTableQuery
        {
            get { return this.GetQuery.AsTableServiceQuery(); }
        }



        public Result<T> GetPage(IQueryable<T> query, int pageSize, int page)
        {
            int extrapages =
                Sitecore.Configuration.Settings.GetIntSetting("ClearPeople.Azure.LogsViewer.NumberofExtraPagesToCache",
                    2);
            int pendingrecords = (page + extrapages + 1)*pageSize;
            int maxroqperrequest =
                Sitecore.Configuration.Settings.GetIntSetting("ClearPeople.Azure.LogsViewer.MaxRowsPerRequest", 1000);


            Result<T> res = new Result<T>();
            var cached = query.AsTableServiceQuery().CachedResults(priority, slidingExpiration);
            int pageCount = 0;
            if (cached.InternalData != null)
            {
                pendingrecords -= cached.InternalData.Count;
            }
            if (pendingrecords > 0)
            {
                while (pendingrecords > 0 && !cached.HasReachedEnd)
                {
                    int numberofrecords = Math.Min(pendingrecords, maxroqperrequest);

                    var q = query.Take(numberofrecords).AsTableServiceQuery();
                    IAsyncResult r = q.BeginExecuteSegmented(cached.ContinuationToken, (ar) => { }, q);
                    r.AsyncWaitHandle.WaitOne();
                    ResultSegment<T> result = q.EndExecuteSegmented(r);
                    var results = result.Results;

                    // If there's any entity returns we need to increase pageCount
                    if (results != null && results.Any())
                    {
                        cached.AddDataToCache(results);
                        pendingrecords -= results.Count();
                        cached.ContinuationToken = result.ContinuationToken;
                        cached.HasReachedEnd = results.Count() < numberofrecords;
                    }
                    else
                    {
                        if (result.ContinuationToken == null)
                            cached.HasReachedEnd = true;
                        else
                            cached.ContinuationToken = result.ContinuationToken;
                    }



                }

            }

            res.Data = cached.InternalData.Skip((page + 1)*pageSize).Take(pageSize);

            res.CurrentTotal = cached.InternalData.Count();
            res.HasMorepages = HasMorePages(query.AsTableServiceQuery());
            return res;

        }

    }

}