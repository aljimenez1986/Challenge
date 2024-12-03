using Geotab.Checkmate.ObjectModel;
using System.Globalization;

namespace Challenge
{
    /// <summary>
    /// Creates a separate CSV file for each Vehicle in csv format
    /// </summary>
    public class CsvCreator
    {
        readonly string CsvPath;

        private const string SeparatorCharacter = ";";

        readonly IList<Vehicle> Vehicles;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvCreator" /> class.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="vehicles"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CsvCreator(string filePath, IList<Vehicle> vehicles)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            CsvPath = filePath;

            if (!Directory.Exists(CsvPath))
            {
                Directory.CreateDirectory(CsvPath);
            }

            Vehicles = vehicles ?? new List<Vehicle>();
        }

        /// <summary>
        /// Runs the instance
        /// </summary>
        public void Run()
        {
            try
            {
                bool includeHeader = false;

                foreach (Vehicle vehicle in Vehicles)
                {
                    string fileName = Path.Combine(CsvPath, vehicle.Name + ".csv");

                    if (!File.Exists(fileName))
                    {
                        includeHeader = true;
                    }

                    using (StreamWriter fwriter = new StreamWriter(Path.Combine(CsvPath, vehicle.Name + ".csv"), true)) //true opens the file in append mode 
                    {
                        if (includeHeader)
                        {
                            fwriter.WriteLine(SetHeader());
                            includeHeader = false;
                        }
                        fwriter.WriteLine(SetLine(vehicle));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is IOException)
                {
                    // Possiable system out of memory exception or file lock. Log then sleep for a minute and continue.
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
        }

        /// <summary>
        /// Sets a line of the csv
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private string SetLine(Vehicle vehicle)
        {
            return string.Join(SeparatorCharacter,
                vehicle.ID,
                vehicle.Timestamp.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss"),
                vehicle.Name == null ? "": vehicle.Name.Replace(SeparatorCharacter, " "),
                vehicle.Latitude,
                vehicle.Longitude,
                vehicle.VIN == null ? "" : vehicle.VIN.Replace(SeparatorCharacter, " "),
                vehicle.Licenseplate,
                Math.Round(RegionInfo.CurrentRegion.IsMetric ? vehicle.Odometer : Distance.ToImperial(vehicle.Odometer / 1000), 0));
        }

        /// <summary>
        /// Sets the header of the csv
        /// </summary>
        /// <returns></returns>
        private string SetHeader()
        {
            return string.Join(SeparatorCharacter,
                "ID",
                "TimeStamp",
                "Name",
                "Latitude",
                "Longitude",
                "VIN",
                "Licenseplate",
                "Odometer");
        }
    }
}
