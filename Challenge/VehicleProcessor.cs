using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Challenge
{
    /// <summary>
    /// A class that queries Geotab's servers for vehicle data.
    /// </summary>
    public class VehicleProcessor
    {
        readonly API Api;

        bool Initial = true;

        IDictionary<Id, Vehicle> VehicleCache;

        IList<Vehicle> VehiclesToReturn;

        Vehicle CurrentVehicle;

        readonly int DegreeOfParallelism = 10;//simultaneous elements to be processed

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleProcessor"/> class.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="server"></param>
        public VehicleProcessor(string user, string password, string database, string server)
        {
            Api = new API(user, password, null, database, server);
        }


        /// <summary>
        /// Requests initial and new data, populates vehicles info and cache
        /// </summary>
        /// <returns></returns>
        public async Task<IList<Vehicle>> GetVehiclesAsync()
        {
            VehiclesToReturn = new List<Vehicle>();

            try
            {
                if (Initial)
                {
                    VehicleCache = new Dictionary<Id, Vehicle>();

                    await GetAllVehiclesAsync();

                    Initial = false;
                }
                else
                {
                    await GetLastVehicleChangesAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e is HttpRequestException)
                {
                    await Task.Delay(5000);
                }
                if (e is DbUnavailableException)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }

            return VehiclesToReturn;
        }


        /// <summary>
        /// Get a list of all devices and process them
        /// </summary>
        /// <returns></returns>
        private async Task GetAllVehiclesAsync()
        {
            var returnedDevices = await Api.CallAsync<IList<Device>>("Get", typeof(Device));

            if (returnedDevices != null)
            {
                ProcessDevices(returnedDevices);
            }
        }

        /// <summary>
        /// Populates vehicle info
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private async Task SetCurrentVehicle(Device device)
        {
            var deviceId = device.Id;

            if (deviceId == null)
            {
                return;
            }

            CurrentVehicle = new Vehicle
            {
                ID = deviceId,
                Name = device.Name,
                Timestamp = DateTime.UtcNow,
                Licenseplate = ((XDevice)device).LicensePlate
            };

            SetVin(device);

            await SetDeviceStatusProperties(deviceId);

            await SetOdometer(deviceId);

            VehiclesToReturn.Add(CurrentVehicle);

            VehicleCache.Add(deviceId, CurrentVehicle);
        }

        /// <summary>
        /// Populates VIN field
        /// </summary>
        /// <param name="device"></param>
        private void SetVin(Device device)
        {
            var goDevice = device as GoDevice;

            CurrentVehicle.VIN = (goDevice == null ? "" : goDevice.VehicleIdentificationNumber ?? "");
        }

        /// <summary>
        /// Populates longitude and latitude fields
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task SetDeviceStatusProperties(Id? id)
        {
            // Get the Device Status Info which contains the current latitude and longitude for this device
            var returnedDeviceStatusInfoList = await Api.CallAsync<List<DeviceStatusInfo>>("Get", typeof(DeviceStatusInfo), new
            {
                search = new DeviceStatusInfoSearch
                {
                    DeviceSearch = new DeviceSearch
                    {
                        Id = id
                    }
                }
            });

            if (returnedDeviceStatusInfoList.Count > 0)
            {
                CurrentVehicle.Longitude = returnedDeviceStatusInfoList[0].Longitude;
                CurrentVehicle.Latitude = returnedDeviceStatusInfoList[0].Latitude; ;
            }
        }

        /// <summary>
        /// Populates odometer field
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task SetOdometer(Id? id)
        {
            var statusData = await Api.CallAsync<IList<StatusData>>("Get", typeof(StatusData), new
            {
                search = new StatusDataSearch
                {
                    DeviceSearch = new DeviceSearch(id),
                    DiagnosticSearch = new DiagnosticSearch(KnownId.DiagnosticOdometerAdjustmentId),
                    FromDate = DateTime.MaxValue
                }
            });

            if (statusData.Count > 0)
            {
                CurrentVehicle.Odometer = statusData[0].Data ?? 0;
            }

        }

        /// <summary>
        /// Get last changes from vehicles
        /// </summary>
        /// <returns></returns>
        private async Task GetLastVehicleChangesAsync()
        {
            //Check if there is a new vehicle
            await GetLastDeviceChangesAsync();

            //Check if there is any changes in latitude or longitude
            await GetLastDeviceStatusInfoChangesAsync();

            //I can't compare if there is any change to the odometer value because it can be retrieved by GetFeed
        }

        /// <summary>
        /// Check if there are new vehicles and populates them if found
        /// </summary>
        /// <returns></returns>
        private async Task GetLastDeviceChangesAsync()
        {
            var deviceFeed = await MakeFeedCallAsync<Device>();

            if (deviceFeed.Data.Count > 0)
            {
                await ProcessDevices(deviceFeed.Data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        private async Task ProcessDevices(IList<Device> devices)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = DegreeOfParallelism };

            await Parallel.ForEachAsync(devices, parallelOptions, async (device, _) =>
            {
                await SetCurrentVehicle(device);
            });
        }

        /// <summary>
        /// Check for new data in DeviceStatusInfo if found, checks if it is different from last info
        /// </summary>
        /// <returns></returns>
        private async Task GetLastDeviceStatusInfoChangesAsync()
        {
            var deviceStatusInfoFeed = await MakeFeedCallAsync<DeviceStatusInfo>();

            if (deviceStatusInfoFeed.Data.Count > 0)
            {
                foreach (var deviceStatusInfo in deviceStatusInfoFeed.Data)
                {
                    var deviceId = deviceStatusInfo.Device.Id;

                    if (deviceId == null)
                    {
                        continue;
                    }

                    //get the vehicule from cache and compare
                    if (VehicleCache.TryGetValue(deviceId, out CurrentVehicle))
                    {
                        if (CurrentVehicle.Longitude != deviceStatusInfo.Longitude ||
                            CurrentVehicle.Latitude != deviceStatusInfo.Latitude)
                        {
                            CurrentVehicle.Longitude = deviceStatusInfo.Longitude;
                            CurrentVehicle.Latitude = deviceStatusInfo.Latitude;

                            //update cache && set to the return list
                            VehicleCache[deviceId] = CurrentVehicle;
                            VehiclesToReturn.Add(CurrentVehicle);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets last feed of entity requested
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        async Task<FeedResult<T>> MakeFeedCallAsync<T>()
         where T : Entity
        {
            return await Api.CallAsync<FeedResult<T>>("GetFeed", typeof(T));
        }
    }
}

