using Microsoft.WindowsAzure.StorageClient;

namespace ClearPeople.Azure.Utylities.Models
{
    public class Log:TableServiceEntity
    {
        private string _message;
        //public string DeploymentId { get; set; }
        public string Role { get; set; }
        public string RoleInstance { get; set; }
        public int Level { get; set; }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public string Description
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
