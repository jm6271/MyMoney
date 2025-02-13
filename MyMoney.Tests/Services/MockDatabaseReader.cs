using MyMoney.Core.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Tests.Services
{
    public class MockDatabaseReader : IDatabaseReader
    {
        public List<T> GetCollection<T>(string CollectionName)
        {
            return [];
        }

        public Dictionary<string, string> GetSettingsDictionary(string CollectionName)
        {
            throw new NotImplementedException();
        }
    }
}
