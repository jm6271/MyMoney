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
        private readonly LiteDatabase _db;

        private readonly string _dataFilePath;

        public DatabaseManager()
        {
            _dataFilePath = DataFileLocationGetter.GetDataFilePath();
            _db = new LiteDatabase(_dataFilePath);
            EnsureIndexes();
        }

        public DatabaseManager(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
            _db = new LiteDatabase(_dataFilePath);
            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            var transactions = _db.GetCollection<Core.Models.Transaction>("Transactions");
            transactions.EnsureIndex(x => x.Date);
            transactions.EnsureIndex(x => x.AccountId);
        }

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
                var collection = _db.GetCollection<T>(collectionName);

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
                var dbCollection = _db.GetCollection<KeyValuePair<string, string>>(collectionName);
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
                _db.BeginTrans();

                var col = _db.GetCollection<T>(collectionName);
                col.DeleteAll();
                col.InsertBulk(collection);

                _db.Commit();
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
                // Get a list version
                var dictList = collection.ToList();

                // Clear the old collection
                if (_db.CollectionExists(collectionName))
                    _db.DropCollection(collectionName);

                var dbCollection = _db.GetCollection<KeyValuePair<string, string>>(collectionName);

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
                var dbCollection = _db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);
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
                // Get a list version
                var dictList = dictionary.ToList();

                // Clear the old collection
                if (_db.CollectionExists(dictName))
                    _db.DropCollection(dictName);

                var dbCollection = _db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);

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
                await action(_db);
            }
            finally
            {
                _databaseLock.Release();
            }
        }
    }
}
