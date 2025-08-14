using LiteDB;

namespace MyMoney.Core.Database
{
    public interface IDatabaseManager
    {
        public void WriteCollection<T>(string collectionName, List<T> collection);
        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection);
        public List<T> GetCollection<T>(string collectionName);
        public Dictionary<string, string> GetSettingsDictionary(string collectionName);
    }
    public class DatabaseManager : IDatabaseManager
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

        public void WriteCollection<T>(string collectionName, List<T> collection)
        {
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            var dbCollection = db.GetCollection<T>(collectionName);

            // clear the database collection
            dbCollection.DeleteAll();

            // add the new items to the database
            foreach (var item in collection)
            {
                dbCollection.Insert(item);
            }
        }

        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection)
        {
            // Open or create a database file
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            // Get a list version
            var dictList = collection.ToList();

            // Clear the old collection
            if (db.CollectionExists(collectionName))
                db.DropCollection(collectionName);

            var dbCollection = db.GetCollection<KeyValuePair<string, string>>(collectionName);

            dbCollection.Insert(dictList);
            dbCollection.EnsureIndex(x => x.Key);
        }
    }
}
