using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http.Json;
using System.Timers;

namespace MobileApp.Services
{
    class LocationBackgroundService
    {
        private readonly int tripId;
        private readonly HttpClient httpClient;
        private readonly System.Timers.Timer timer;
        private readonly int locationIntervalMs = 10000; // 10 seconds
        private bool isRunning = false;

        public LocationBackgroundService(int tripId)
        {
            this.tripId = tripId;
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://trackmypathapimanagement.azure-api.net/")
            };

            timer = new System.Timers.Timer(locationIntervalMs);
            timer.Elapsed += async (s, e) => await FetchAndPostLocation();
            timer.AutoReset = true;
        }

        public void Start()
        {
            if (!isRunning)
            {
                timer.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
            }
        }

        private async Task FetchAndPostLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                if (location != null)
                {
                    var locPayload = new
                    {
                        id = 0,
                        tripId = tripId,
                        timestamp = DateTime.UtcNow.ToString("o"),
                        latitude = (decimal)location.Latitude,
                        longitude = (decimal)location.Longitude,
                        accuracy = (float)(location.Accuracy ?? 0),
                        speed = (decimal)(location.Speed ?? 0)
                    };

                    var response = await httpClient.PostAsJsonAsync("api/Locations", locPayload);
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Location Background POST: {(int)response.StatusCode} - {body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Background location error: {ex.Message}");
            }
        }
    }
}
