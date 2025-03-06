using LiteDB;

namespace MyMoney.Core.Database
{
    public static class DatabaseWriter
    {
        public static void WriteCollection<T>(string collectionName, List<T> collection)
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

        public static void WriteSettingsDictionary(string collectionName, Dictionary<string, string> collection)
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
