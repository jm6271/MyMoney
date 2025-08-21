using LiteDB;
using System.Collections;

namespace MyMoney.Core.Database
{
    public interface IDatabaseManager
    {
        public void WriteCollection<T>(string collectionName, List<T> collection);
        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection);
        public List<T> GetCollection<T>(string collectionName);
        public Dictionary<string, string> GetSettingsDictionary(string collectionName);
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName) where TKey : notnull;
        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary) where TKey : notnull;
    }

    public class DatabaseManager : IDatabaseManager
    {
        private static readonly Lock _databaseLock = new();

        /// <summary>
        /// Read a collection from the database
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <returns>A List<typeparamref name="T"/> containing the items in the collection</returns>
        public List<T> GetCollection<T>(string collectionName)
        {
            List<T> result = [];

            lock (_databaseLock)
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());
                var collection = db.GetCollection<T>(collectionName);

                // Create list from collection
                for (int i = 1; i <= collection.Count(); i++)
                {
                    result.Add(collection.FindById(i));
                }
            }

            return result;
        }

        public Dictionary<string, string> GetSettingsDictionary(string collectionName)
        {
            Dictionary<string, string> dict = [];
            List<KeyValuePair<string, string>> settingsList;

            lock (_databaseLock)
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

                var dbCollection = db.GetCollection<KeyValuePair<string, string>>(collectionName);
                settingsList = [.. dbCollection.FindAll()];
            }

            foreach (var item in settingsList)
            {
                dict.TryAdd(item.Key, item.Value);
            }

            return dict;
        }

        public void WriteCollection<T>(string collectionName, List<T> collection)
        {
            lock (_databaseLock)
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

            // Invalidate parts of the cache that were affected by this write
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);
        }

        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection)
        {
            lock (_databaseLock)
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

        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName) where TKey: notnull
        {
            Dictionary<TKey, TValue> dict = [];

            lock (_databaseLock)
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

                var dbCollection = db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);
                var settingsList = dbCollection.FindAll().ToList();

                foreach (var item in settingsList)
                {
                    dict.TryAdd(item.Key, item.Value);
                }
            }
            return dict;
        }

        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary) where TKey: notnull
        {
            lock (_databaseLock)
            {
                // Open or create a database file
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

                // Get a list version
                var dictList = dictionary.ToList();

                // Clear the old collection
                if (db.CollectionExists(dictName))
                    db.DropCollection(dictName);

                var dbCollection = db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);

                dbCollection.Insert(dictList);
                dbCollection.EnsureIndex(x => x.Key);
            }
        }
    }
}
