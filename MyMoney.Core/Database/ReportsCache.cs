using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Database
{
    public class ReportsCache
    {
        private const string DICT_NAME = "ReportsCache";

        private readonly Dictionary<string, object> _cacheObjects;

        private readonly IDatabaseManager _dbManager;

        public ReportsCache(IDatabaseManager databaseManager)
        {
            _dbManager = databaseManager;

            // Load cache
            _cacheObjects = _dbManager.ReadDictionary<string, object>(DICT_NAME);
        }

        public bool DoesKeyExist(string key)
        {
            return _cacheObjects.ContainsKey(key);
        }

        public void CacheObject(string key, object value)
        {
            _cacheObjects[key] = value;

            // Save cache
            _dbManager.WriteDictionary(DICT_NAME, _cacheObjects);
        }

        public bool RetrieveCachedObject(string key, out object? value)
        {
            var success = _cacheObjects.TryGetValue(key, out value);
            return success;
        }

        public void UncacheObject(string key)
        {
            _cacheObjects.Remove(key);
        }

        public static string GenerateKeyForBudgetReportCache(DateTime budgetDate)
        {
            return "Budget-Report-" + budgetDate.ToString("MMMM-yyyy");
        }

        public void InvalidateCacheOnWrite(string collectionName)
        {
            if (collectionName == "Budgets" || collectionName == "Accounts")
            {
                // Invalidate all the budget reports
                var keysToRemove = _cacheObjects.Keys.Where(key => key.StartsWith("Budget-Report-")).ToList();
                foreach (var key in keysToRemove)
                {
                    _cacheObjects.Remove(key);
                }
            }

            _dbManager.WriteDictionary(DICT_NAME, _cacheObjects);
        }
    }
}
