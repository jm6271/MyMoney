using System.Collections;
using System.Threading;
using LiteDB;

namespace MyMoney.Core.Database
{
    public interface IDatabaseManager: IDisposable
    {
        public void WriteCollection<T>(string collectionName, IReadOnlyList<T> collection);
        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection);
        public List<T> GetCollection<T>(string collectionName);
        public Dictionary<string, string> GetSettingsDictionary(string collectionName);
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName)
            where TKey : notnull;
        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary)
            where TKey : notnull;
        public void Insert<T>(string collectionName, T entity);
        public bool Update<T>(string collectionName, T entity);
        public bool Delete<T>(string collectionName, BsonValue id);
        public int DeleteMany<T>(string collectionName, System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        public Task QueryAsync<T>(string collectionName, Func<ILiteQueryable<T>, Task> action);
    }

    public class DatabaseManager : IDatabaseManager, IDisposable
    {
        private readonly LiteDatabase _db;

        public DatabaseManager()
        {
            var dataFilePath = DataFileLocationGetter.GetDataFilePath();
            _db = new LiteDatabase(dataFilePath);
            EnsureIndexes();
        }

        public DatabaseManager(string dataFilePath)
        {
            _db = new LiteDatabase(dataFilePath);
            EnsureIndexes();
        }

        public DatabaseManager(Stream stream)
        {
            _db = new(stream);
        }

        private void EnsureIndexes()
        {
            var transactions = _db.GetCollection<Core.Models.Transaction>("Transactions");
            transactions.EnsureIndex(x => x.Date);
            transactions.EnsureIndex(x => x.AccountId);
            transactions.EnsureIndex(x => x.Payee);
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

            var collection = _db.GetCollection<T>(collectionName);

            // Create list from collection
            result = [.. collection.FindAll()];

            return result;
        }

        public Dictionary<string, string> GetSettingsDictionary(string collectionName)
        {
            Dictionary<string, string> dict = [];
            List<KeyValuePair<string, string>> settingsList;

            var dbCollection = _db.GetCollection<KeyValuePair<string, string>>(collectionName);
            settingsList = [.. dbCollection.FindAll()];

            foreach (var item in settingsList)
            {
                dict.TryAdd(item.Key, item.Value);
            }

            return dict;
        }

        public void WriteCollection<T>(string collectionName, IReadOnlyList<T> collection)
        {
            _db.BeginTrans();

            var col = _db.GetCollection<T>(collectionName);
            col.DeleteAll();
            col.InsertBulk(collection);

            _db.Commit();

            // Invalidate parts of the cache that were affected by this write
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);
        }

        public void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection)
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

        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string dictName)
            where TKey : notnull
        {
            Dictionary<TKey, TValue> dict = [];

            var dbCollection = _db.GetCollection<KeyValuePair<TKey, TValue>>(dictName);
            var settingsList = dbCollection.FindAll().ToList();

            foreach (var item in settingsList)
            {
                dict.TryAdd(item.Key, item.Value);
            }
            return dict;
        }

        public void WriteDictionary<TKey, TValue>(string dictName, Dictionary<TKey, TValue> dictionary)
            where TKey : notnull
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

        public bool Delete<T>(string collectionName, BsonValue id)
        {
            var collection = _db.GetCollection<T>(collectionName);
            var result = collection.Delete(id);
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);

            return result;
        }

        public int DeleteMany<T>(string collectionName, System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var collection = _db.GetCollection<T>(collectionName);
            var result = collection.DeleteMany(predicate);
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);

            return result;
        }

        public bool Update<T>(string collectionName, T entity)
        {
            var collection = _db.GetCollection<T>(collectionName);
            var result = collection.Update(entity);
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);

            return result;
        }

        public void Insert<T>(string collectionName, T entity)
        {
            var collection = _db.GetCollection<T>(collectionName);
            collection.Insert(entity);
            ReportsCache cache = new(this);
            cache.InvalidateCacheOnWrite(collectionName);
        }

        public async Task QueryAsync<T>(string collectionName, Func<ILiteQueryable<T>, Task> action)
        {
            var collection = _db.GetCollection<T>(collectionName);
            await action(collection.Query());
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _db?.Dispose();
        }
    }
}
