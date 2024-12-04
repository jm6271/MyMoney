using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Database
{
    /// <summary>
    /// Read from the applications LiteDB database
    /// </summary>
    public static class DatabaseReader
    {
        /// <summary>
        /// Read a collection from the database
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        /// <param name="CollectionName">The name of the collection</param>
        /// <returns>A List<typeparamref name="T"/> containing the items in the collection</returns>
        public static List<T> GetCollection<T>(string CollectionName)
        {
            using var db = new LiteDatabase(DataFileLocationGetter.GetDataFilePath());

            var collection = db.GetCollection<T>(CollectionName);

            // Create list from collection
            List<T> result = [];

            for (int i = 1; i <= collection.Count(); i++)
            {
                result.Add(collection.FindById(i));
            }

            return result;
        }
    }
}
