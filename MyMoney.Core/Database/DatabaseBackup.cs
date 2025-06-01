using System.IO;

namespace MyMoney.Core.Database
{
    public static class DatabaseBackup
    {
        public static void WriteDatabaseBackup(string filePath)
        {
            // Get the database location
            string location = DataFileLocationGetter.GetDataFilePath();

            // Copy the datafile to the new file path, overwriting the file if it already exists
            File.Copy(location, filePath, true);
        }

        public static void RestoreDatabaseBackup(string backupPath)
        {
            // Get the location we want to put the backup
            string location = DataFileLocationGetter.GetDataFilePath();

            File.Copy(backupPath, location, true);
        }
    }
}
