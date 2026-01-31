using System.Collections;
using System.Threading;
using LiteDB;

namespace MyMoney.Core.Database
{
    public interface IDatabaseManager
    {
        public void WriteCollection<T>(string collectionName, IReadOnlyList<T> collection);
        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection);
        public List<T> GetCollection<T>(string collectionName);
        public Dictionary<string, string> GetSettingsDictionary(string collectionName);
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName)
            where TKey : notnull;
        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary)
            where TKey : notnull;

        public Task ExecuteAsync(Func<LiteDatabase, Task> action);
    }

    public class DatabaseManager : IDatabaseManager
    {
        private static readonly SemaphoreSlim _databaseLock = new(1);

        /// <summary>
        /// Read a collection from the database
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        /// <param name="collectionName">The name of the collection</param>
        /// <returns>A List<typeparamref name="T"/> containing the items in the collection</returns>
        public List<T> GetCollection<T>(string collectionName)
        {
            List<T> result = [];

            _databaseLock.Wait();
            try
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());
                var collection = db.GetCollection<T>(collectionName);

                // Create list from collection
                result = [.. collection.FindAll()];
            }
            finally
            {
                _databaseLock.Release();
            }

            return result;
        }

        public Dictionary<string, string> GetSettingsDictionary(string collectionName)
        {
            Dictionary<string, string> dict = [];
            List<KeyValuePair<string, string>> settingsList;

            _databaseLock.Wait();
            try
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

                var dbCollection = db.GetCollection<KeyValuePair<string, string>>(collectionName);
                settingsList = [.. dbCollection.FindAll()];
            }
            finally
            {
                _databaseLock.Release();
            }

            foreach (var item in settingsList)
            {
                dict.TryAdd(item.Key, item.Value);
            }

            return dict;
        }

        public void WriteCollection<T>(string collectionName, IReadOnlyList<T> collection)
        {
            _databaseLock.Wait();
            try
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());
                db.BeginTrans();

                var col = db.GetCollection<T>(collectionName);
                col.DeleteAll();
                col.InsertBulk(collection);

                db.Commit();
            }
            finally
            {
                _databaseLock.Release();
            }

            // Invalidate parts of the cache that were affected by this write
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);
        }

        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection)
        {
            _databaseLock.Wait();
            try
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
            finally
            {
                _databaseLock.Release();
            }
        }

        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName)
            where TKey : notnull
        {
            Dictionary<TKey, TValue> dict = [];

            _databaseLock.Wait();
            try
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

                var dbCollection = db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);
                var settingsList = dbCollection.FindAll().ToList();

                foreach (var item in settingsList)
                {
                    dict.TryAdd(item.Key, item.Value);
                }
            }
            finally
            {
                _databaseLock.Release();
            }
            return dict;
        }

        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary)
            where TKey : notnull
        {
            _databaseLock.Wait();
            try
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
            finally
            {
                _databaseLock.Release();
            }
        }

        public async Task ExecuteAsync(Func<LiteDatabase, Task> action)
        {
            await _databaseLock.WaitAsync();
            try
            {
                using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());
                await action(db);

            }
            finally
            {
                _databaseLock.Release();
            }
        }
    }
}
