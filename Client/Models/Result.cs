using System.Collections.Generic;

namespace ClearPeople.Azure.Utylities.Models
{
    public class Result<T>
    {
        public IEnumerable<T> Data;
        public int CurrentTotal;
        public bool HasMorepages;
        public Result()
        {
            Data = new List<T>();
        }
    }
}
