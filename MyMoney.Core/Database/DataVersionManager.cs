using Semver;

namespace MyMoney.Core.Database
{
    public class DataVersionManager
    {
        public const string CURRENT_DATA_VERSION = "1.0.0";

        private readonly IDatabaseManager _dbManager;

        public DataVersionManager(IDatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Ensures that the application's data version in the database matches the expected version required by the
        /// application.
        /// </summary>
        /// <remarks>If the data version is missing from the database, this method initializes it to the
        /// current application data version. If the database version is newer than the application's expected version,
        /// the method returns false, indicating that the application may need to be updated to read the data.</remarks>
        /// <returns>true if the database data version is compatible with the application; otherwise, false if the database
        /// version is newer than the application and cannot be read.</returns>
        public bool EnsureDataVersion()
        {
            // Read the settings dictionary from the database to get the version
            Dictionary<string, string> settingsDict = _dbManager.GetSettingsDictionary("ApplicationSettings");
            if (settingsDict.TryGetValue("DataVersion", out string? dbVersion))
            {
                // Compare the versions, to ensure that the database is up to date
                var dbSemVer = SemVersion.Parse(dbVersion);
                var currentSemVer = SemVersion.Parse(CURRENT_DATA_VERSION);

                switch (SemVersion.ComparePrecedence(dbSemVer, currentSemVer))
                {
                    case -1:
                        // Note: Here we need to update the data in the database to the
                        // current version. As of now (v1.0.0), there are no older supported
                        // versions, so we do nothing.
                        break;
                    case 0:
                        // The versions match, do nothing
                        break;
                    case 1:
                        // The version in the database is newer than the application version,
                        // we can't read it. The user needs to update the application.
                        return false;
                    default:
                        break;
                }

                return true;
            }
            else
            {
                // The version is not in the database, so we need to set it
                settingsDict.Add("DataVersion", CURRENT_DATA_VERSION);
                _dbManager.WriteSettingsDictionary("ApplicationSettings", settingsDict);
                return true;
            }
        }
    }
}
