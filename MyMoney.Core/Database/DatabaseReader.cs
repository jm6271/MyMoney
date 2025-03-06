using LiteDB;

namespace MyMoney.Core.Database
{
    public interface IDatabaseReader
    {
        public List<T> GetCollection<T>(string collectionName);
        public Dictionary<string, string> GetSettingsDictionary(string collectionName);
    }


    /// <summary>
    /// Read from the applications LiteDB database
    /// </summary>
    public class DatabaseReader : IDatabaseReader
    {
        /// <summary>
        /// Read a collection from the database
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <returns>A List<typeparamref name="T"/> containing the items in the collection</returns>
        public List<T> GetCollection<T>(string collectionName)
        {
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            var collection = db.GetCollection<T>(collectionName);

            // Create list from collection
            List<T> result = [];

            for (int i = 1; i <= collection.Count(); i++)
            {
                result.Add(collection.FindById(i));
            }

            return result;
        }

        public Dictionary<string, string> GetSettingsDictionary(string collectionName)
        {
            Dictionary<string, string> dict = [];

            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            var dbCollection = db.GetCollection<KeyValuePair<string, string>>(collectionName);
            var settingsList = dbCollection.FindAll().ToList();

            foreach (var item in settingsList)
            {
                dict.TryAdd(item.Key, item.Value);
            }

            return dict;
        }
    }
}
