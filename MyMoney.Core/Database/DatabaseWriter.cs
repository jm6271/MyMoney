using LiteDB;

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
    }
}
