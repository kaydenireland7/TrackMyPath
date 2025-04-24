
﻿using Microsoft.Maui.Controls.Maps;

using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System;
using System.Net.Http.Json;


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
        private System.Timers.Timer locationTimer;
        private const int userId = 1; // Replace with real user logic if needed
        private const int locationIntervalMs = 10000; // every 10 seconds

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
                UserId = userId,
                StartTime = DateTime.UtcNow,
                TripName = tripName
            };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://your-api-url/api/Trips", trip);

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
                await DisplayAlert("Error", "Failed to start trip.", "OK");
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
                        TripId = currentTripId,
                        Timestamp = DateTime.UtcNow,
                        Latitude = (decimal)location.Latitude,
                        Longitude = (decimal)location.Longitude,
                        Accuracy = (float?)location.Accuracy,
                        Speed = (decimal?)location.Speed
                    };

                    using var client = new HttpClient();
                    await client.PostAsJsonAsync("https://your-api-url/api/Locations", locPayload);
                }
            }
            catch
            {
                // Handle permission or hardware failure, but keep timer running
            }
        }

        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Photo", "Take Photo button clicked.", "OK");
        }
    }

    public class Trip
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? TripName { get; set; }
    }
}