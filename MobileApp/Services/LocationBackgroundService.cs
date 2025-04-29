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
        public int currentLocationID;
        private bool isRunning = false;

        public Action<Location>? OnLocationPosted;
        public Action<Location, int>? OnLocationPostedWithId;

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

        public int getCurrentLocationID()
        {
            return currentLocationID;
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

                    if (response.IsSuccessStatusCode)
                    {
                        var createdLocation = await response.Content.ReadFromJsonAsync<Location2>();
                        currentLocationID = createdLocation.Id;
                        Console.WriteLine(response.Content.ReadAsStringAsync().Result.ToString());
                        // Notify UI
                        if (OnLocationPosted != null)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                OnLocationPosted(location);
                                OnLocationPostedWithId?.Invoke(location, createdLocation.Id);

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Background location error: {ex.Message}");
            }
        
        }
        public class Location2
        {
            public int Id { get; set; }
            public int TripId { get; set; }
            public DateTime Timestamp { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public float? Accuracy { get; set; }
            public decimal? Speed { get; set; }
        }
    }

}
