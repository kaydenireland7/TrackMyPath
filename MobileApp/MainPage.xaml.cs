
﻿using Microsoft.Maui.Controls.Maps;

using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using Microsoft.Maui.Media;
using System.Net.Http.Headers;
using System.Net.Http.Json;
// using Android.Gms.Common.Apis;   this popped up automatically and causes an error, idk why
using System.Text.Json;


namespace MobileApp
{
    public partial class MainPage : ContentPage
    {
        private Microsoft.Maui.Controls.Maps.Map map;
        private Button startEndTripButton;
        private Button takePhotoButton;
        private Entry tripNameEntry;
        private Button confirmTripButton;
        private StackLayout tripInputLayout;

        private bool tripActive = false;
        private int? currentTripId = null;
        // latestLocationId is a new variable
        private int? latestLocationId = null;
        private System.Timers.Timer locationTimer;
        private const int userId = 1; // Replace with real user logic if needed
        private const int locationIntervalMs = 10000; // every 10 seconds

        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://trackmypathapimanagement.azure-api.net/")
        };

        private BlobService blobService = new BlobService();
        
        public MainPage()
        {
            InitializeComponent();
            _ = InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            // Get location

            Location? userLocation = null;
            try
            {
                userLocation = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Unable to get location. Please enable tracking", "OK");
            }


            // Fallback to a default location at New York if user location comes back null
            userLocation ??= new Location(40.7128, -74.0060); 

            // Define some hard coded locations to start the path
            var location1 = new Location(37.7749, -122.4194); // San Francisco
            var location2 = new Location(34.0522, -118.2437); // Los Angeles
            var location3 = new Location(36.1699, -115.1398); // Las Vegas
            var location4 = userLocation; // User's current location (emulator should default to around San Jose, real devices should track real location if permission granted)

            // Calculate bounds to include all points

            var allLocations = new[] { location1, location2, location3, location4 };
            var minLat = allLocations.Min(loc => loc.Latitude);
            var maxLat = allLocations.Max(loc => loc.Latitude);
            var minLon = allLocations.Min(loc => loc.Longitude);
            var maxLon = allLocations.Max(loc => loc.Longitude);

            var mapBounds = new MapSpan(
                center: new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2),

                latitudeDegrees: maxLat - minLat + 1,
                longitudeDegrees: maxLon - minLon + 1
            );

            map = new Microsoft.Maui.Controls.Maps.Map(mapBounds)

            {
                IsShowingUser = true
            };


            // Add pins

            map.Pins.Add(new Pin { Label = "San Francisco", Location = location1 });
            map.Pins.Add(new Pin { Label = "Los Angeles", Location = location2 });
            map.Pins.Add(new Pin { Label = "Las Vegas", Location = location3 });
            map.Pins.Add(new Pin { Label = "You Are Here", Location = location4 });


            // Add polyline path ending at current location

            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 5
            };
            polyline.Geopath.Add(location1);
            polyline.Geopath.Add(location2);
            polyline.Geopath.Add(location3);

            polyline.Geopath.Add(location4);
            map.MapElements.Add(polyline);

            // Create buttons
            startEndTripButton = new Button { Text = "Start/End Trip" };
            takePhotoButton = new Button { Text = "Take Photo" };
            startEndTripButton.Clicked += StartEndTripButton_Clicked;
            takePhotoButton.Clicked += TakePhotoButton_Clicked;

            // Create input section (hidden by default)
            tripNameEntry = new Entry { Placeholder = "Enter Trip Name", IsVisible = false };
            confirmTripButton = new Button { Text = "Confirm", IsVisible = false };
            confirmTripButton.Clicked += ConfirmTripButton_Clicked;

            tripInputLayout = new StackLayout
            {
                Children = { tripNameEntry, confirmTripButton },
                Spacing = 5
            };

            // Create the full page layout
            Content = new StackLayout
            {
                Spacing = 10,
                Children =
                {
                    map,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { startEndTripButton, takePhotoButton }
                    },
                    tripInputLayout
                }
            };
        }

        private async void StartEndTripButton_Clicked(object sender, EventArgs e)
        {
            if (!tripActive)
            {
                // Show input
                tripNameEntry.IsVisible = true;
                confirmTripButton.IsVisible = true;
            }
            else
            {
                // End trip
                StopLocationUpdates();

                tripActive = false;
                currentTripId = null;

                startEndTripButton.Text = "Start Trip";
                await DisplayAlert("Trip Ended", "Trip tracking stopped.", "OK");
            }

        }

        private async void ConfirmTripButton_Clicked(object sender, EventArgs e)
        {
            string tripName = tripNameEntry.Text?.Trim();
            if (string.IsNullOrEmpty(tripName)) return;

            var trip = new
            {
                id = 0, // Set to 0 because the server will generate the actual ID
                userId = userId,
                startTime = DateTime.UtcNow.ToString("o"),
                endTime = DateTime.UtcNow.ToString("o"), // You might later update this when ending the trip (could set to null in database)
                tripName = tripName
            };

            var response = await httpClient.PostAsJsonAsync("api/Trips", trip);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Trip POST Status: {(int)response.StatusCode} ({response.StatusCode})");
            Console.WriteLine($"Trip POST Body: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                var createdTrip = await response.Content.ReadFromJsonAsync<Trip>();
                currentTripId = createdTrip.Id;
                tripActive = true;
                startEndTripButton.Text = "End Trip";

                tripNameEntry.Text = "";
                tripNameEntry.IsVisible = false;
                confirmTripButton.IsVisible = false;

                StartLocationUpdates();

                await DisplayAlert("Trip Started", $"Trip ID: {currentTripId}", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Failed to start trip.\nStatus: {(int)response.StatusCode}\n{responseBody}", "OK");
            }
        }


        private void StartLocationUpdates()
        {
            locationTimer = new System.Timers.Timer(locationIntervalMs);
            locationTimer.Elapsed += async (s, e) => await PostCurrentLocation();
            locationTimer.AutoReset = true;
            locationTimer.Start();
        }

        private void StopLocationUpdates()
        {
            if (locationTimer != null)
            {
                locationTimer.Stop();
                locationTimer.Dispose();
                locationTimer = null;
            }
        }

        private async Task PostCurrentLocation()
        {
            if (currentTripId == null) return;

            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                if (location != null)
                {
                    var locPayload = new
                    {
                        id = 0, // New record
                        tripId = currentTripId.Value,
                        timestamp = DateTime.UtcNow.ToString("o"),
                        latitude = (decimal)location.Latitude,
                        longitude = (decimal)location.Longitude,
                        accuracy = (float)(location.Accuracy ?? 0), // Default to 0 if null
                        speed = (decimal)(location.Speed ?? 0)       // Default to 0 if null
                    };

                    var response = await httpClient.PostAsJsonAsync("api/Locations", locPayload);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Location POST Status: {(int)response.StatusCode} ({response.StatusCode})");
                    Console.WriteLine($"Location POST Body: {responseBody}");

                    if (!response.IsSuccessStatusCode)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                            Application.Current?.MainPage?.DisplayAlert("Location Error",
                                $"Failed to send location.\nStatus: {(int)response.StatusCode}\n{responseBody}", "OK")
                        );
                    }
                    else
                    {
                        var createdLocation = await response.Content.ReadFromJsonAsync<Location2>();
                        latestLocationId = createdLocation?.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Location error: {ex.Message}");
                // Optional: Display error to user if needed
            }
        }

        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                FileResult photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    // Save locally (optional but useful for manipulation or displaying)
                    var stream = await photo.OpenReadAsync();
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);

                    using (var fileStream = File.OpenWrite(localPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    // Upload to API
                    await UploadPhotoToApi(localPath);
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Error", "Camera not supported on this device.", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlert("Error", "Camera permissions not granted.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
            }
        }

        private async Task UploadPhotoToApi(string filePath)
        {
            // THIS IS SAMPLE CODE AND DOESN'T ACTUALLY FUNCTION
            if (latestLocationId == null)
            {
                await DisplayAlert("Error", "No location found for the current trip.", "OK");
                return;
            }
            else
            {
                Console.WriteLine($"Latest Location ID: {latestLocationId}");
            }
                using var httpClient = new HttpClient();

            var form = new MultipartFormDataContent();



        }

        public class Trip
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? TripName { get; set; }



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