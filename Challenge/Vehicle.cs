using Geotab.Checkmate.ObjectModel;

namespace Challenge
{
    /// <summary>
    /// Relevant info to backup
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Gets or sets the <see cref="Id" /> field.
        /// </summary>
        public Id? ID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Name" /> field.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Timestamp" /> field.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Longitude" /> field.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Latitude" /> field.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VIN" /> field.
        /// </summary>
        public string? VIN { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Licenseplate" /> field.
        /// </summary>
        public string? Licenseplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Odometer" /> field.
        /// </summary>
        public double Odometer { get; set; }

    }
}
