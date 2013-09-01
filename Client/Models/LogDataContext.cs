using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace ClearPeople.Azure.Utylities.Models
{
    public class LogDataContext:TableServiceContext
    {
        public LogDataContext(string baseAddress, StorageCredentials credentials) :
            base(baseAddress, credentials)
        {
           
        }        
    }
}
