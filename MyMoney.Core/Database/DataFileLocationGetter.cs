namespace MyMoney.Core.Database
{
    public static class DataFileLocationGetter
    {
        public static string GetDataFilePath()
        {
#if DEBUG      
            return "MyMoney-LiteDB-Database-DEBUG.db";
#else
            // make sure all the directories are already created
            System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MyMoney"
                )
            );
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyMoney",
                "MyMoney-LiteDB-DataFile.db"
            );
#endif
        }
    }
}
