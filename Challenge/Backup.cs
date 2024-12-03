using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Challenge
{
    /// <summary>
    /// Backup base class
    /// </summary>
    abstract class Backup
    {
        private readonly string FilePath;
        private bool Stop;
        private readonly int MilisecondsDelay;

        /// <summary>
        /// Initializes a new instance of the <see cref="Backup"/> class.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="milisecondsDelay"></param>
        internal Backup(string path, int milisecondsDelay)
        {
            FilePath = path;
            MilisecondsDelay = milisecondsDelay;
        }

        /// <summary>
        /// BackupCreator
        /// </summary>
        /// <returns></returns>
        public async Task CreateBackup()
        {
            do
            {
                await RunBackupAsync();
                await Task.Delay(MilisecondsDelay);

            } while (!Stop);
        }

        /// <summary>
        /// Requests to stop.
        /// </summary>
        public void RequestStop()
        {
            Stop = true;
        }

        /// <summary>
        /// Executes the backup
        /// </summary>
        /// <returns></returns>
        public abstract Task RunBackupAsync();

        public void PersistVehicleAsync(IList<Vehicle> vehicles)
        {
             new CsvCreator(FilePath, vehicles).Run();
        }
    }
}
