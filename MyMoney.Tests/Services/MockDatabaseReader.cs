using MyMoney.Core.Database;

namespace MyMoney.Tests.Services
{
    public class MockDatabaseReader : IDatabaseReader
    {
        public List<T> GetCollection<T>(string collectionName)
        {
            return [];
        }

        public Dictionary<string, string> GetSettingsDictionary(string collectionName)
        {
            throw new NotImplementedException();
        }
    }
}
