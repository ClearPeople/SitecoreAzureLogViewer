using System.Collections.Generic;
using Microsoft.WindowsAzure.StorageClient;

namespace ClearPeople.Azure.Utylities.Utilities
{
    public class PaginationCache<T>
    {

        public List<T> InternalData { get; set; }        
        public void AddDataToCache(IEnumerable<T> data)
        {            
            if (InternalData == null)
            {
                InternalData = new List<T>();
            }
            InternalData.AddRange(data);            
        }


        public ResultContinuation ContinuationToken { get; set; }

        public bool HasReachedEnd { get; set; }

        public PaginationCache()
        {
            InternalData = new List<T>();
            HasReachedEnd = false;
        }

    }
}
