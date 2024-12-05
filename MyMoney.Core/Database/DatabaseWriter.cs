using LiteDB;
using System.Collections.Concurrent;

namespace MyMoney.Core.Database
{
    public static class DatabaseWriter
    {
        public static void WriteCollection<T>(string CollectionName, List<T> Collection)
        {
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            var dbCollection = db.GetCollection<T>(CollectionName);

            // clear the database collection
            dbCollection.DeleteAll();

            // add the new items to the database
            foreach (var item in Collection)
            {
                dbCollection.Insert(item);
            }
        }

        public static void WriteSettingsDictionary(string CollectionName, Dictionary<string, string> Collection)
        {
            // Open or create a database file
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            // Get a list version
            var dictList = Collection.ToList();

            // Clear the old collection
            if (db.CollectionExists(CollectionName))
                db.DropCollection(CollectionName);

            var dbCollection = db.GetCollection<KeyValuePair<string, string>>(CollectionName);

            dbCollection.Insert(dictList);
            dbCollection.EnsureIndex(x => x.Key);
        }
    }
}
