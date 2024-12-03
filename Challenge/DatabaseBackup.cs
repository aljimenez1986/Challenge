
namespace Challenge
{
    /// <summary>
    /// Backup for a database
    /// </summary>
    internal class DatabaseBackup : Backup
    {
        readonly VehicleProcessor Processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBackup" /> class.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="server"></param>
        /// <param name="milisecondsDelay"></param>
        public DatabaseBackup(string filePath, string user, string password, string database, string server, int milisecondsDelay) : base(filePath, milisecondsDelay)
        {
            Processor = new VehicleProcessor(user, password, database, server);
        }

        /// <summary>
        /// Execute backup actions
        /// </summary>
        /// <returns></returns>
        public override async Task RunBackupAsync()
        {
            PersistVehicleAsync(await Processor.GetVehiclesAsync());
        }
    }
}
