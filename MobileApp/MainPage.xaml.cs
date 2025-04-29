using Microsoft.Maui.Controls.Maps;
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
using MobileApp.Services;

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
   
        private int? latestLocationId = null;

        private System.Timers.Timer locationTimer;
        private const int userId = 1; // Replace with real user logic if needed
        private const int locationIntervalMs = 10000; // every 10 seconds
        
        private BlobService blobservice;

        // new variables
        private List<Location> tripLocations = new();
        private Polyline tripPolyline = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };

        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://trackmypathapimanagement.azure-api.net/")
        };


        // NEW VARIABLE for background locations
        private LocationBackgroundService locationService;

        private List<Location> tripLocations = new();
        private Polyline tripPolyline = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };

        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://trackmypathapimanagement.azure-api.net/")
        };


        public MainPage()
        {
            InitializeComponent();
            _ = InitializeMapAsync();
            blobservice = new BlobService();
        }

        private async Task InitializeMapAsync()
        {
            // Get user location
            Location? userLocation = null;
            try
            {
                userLocation = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            }
            catch
            {
                await DisplayAlert("Error", "Unable to get location. Please enable tracking.", "OK");
            }

            userLocation ??= new Location(40.7128, -74.0060);

            var mapBounds = new MapSpan(
                center: userLocation,
                latitudeDegrees: 0.1,
                longitudeDegrees: 0.1
            );

            // --- Create the map first ---
            map = new Microsoft.Maui.Controls.Maps.Map(mapBounds)
            {
                IsShowingUser = true
            };

            // --- Create the buttons ---
            startEndTripButton = new Button { Text = "Start/End Trip" };
            takePhotoButton = new Button { Text = "Take Photo" };
            startEndTripButton.Clicked += StartEndTripButton_Clicked;
            takePhotoButton.Clicked += TakePhotoButton_Clicked;

            tripNameEntry = new Entry { Placeholder = "Enter Trip Name", IsVisible = false };
            confirmTripButton = new Button { Text = "Confirm", IsVisible = false };
            confirmTripButton.Clicked += ConfirmTripButton_Clicked;

            tripInputLayout = new StackLayout
            {
                Children = { tripNameEntry, confirmTripButton },
                Spacing = 5
            };

            // --- Now build the Grid ---
            var grid = new Grid
            {
                RowDefinitions =
        {
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Map fills available space
            new RowDefinition { Height = GridLength.Auto }, // Buttons
            new RowDefinition { Height = GridLength.Auto }  // Trip input
        }
            };

            grid.Children.Add(map);
            Grid.SetRow(map, 0);

            var buttonsLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                Children = { startEndTripButton, takePhotoButton }
            };
            grid.Children.Add(buttonsLayout);
            Grid.SetRow(buttonsLayout, 1);

            grid.Children.Add(tripInputLayout);
            Grid.SetRow(tripInputLayout, 2);

            // --- Set the page content last ---
            Content = grid;
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
                // CHANGED TO STOP LOCATION SERVICE INSTEAD OF CALLING StopLocationUpdates
                if (locationService != null)
                {
                    locationService?.Stop();
                    locationService = null;
                }


                // New stuff to reset map after ending trip
                tripLocations.Clear();
                map.Pins.Clear();
                map.MapElements.Clear();
                tripPolyline = new Polyline
                {
                    StrokeColor = Colors.Blue,
                    StrokeWidth = 5
                };

                // Recenter map on user
                _ = ResetMapToUserLocation();

                // New stuff to reset map after ending trip
                tripLocations.Clear();
                map.Pins.Clear();
                map.MapElements.Clear();
                tripPolyline = new Polyline
                {
                    StrokeColor = Colors.Blue,
                    StrokeWidth = 5
                };

                // Recenter map on user
                _ = ResetMapToUserLocation();

                tripActive = false;
                currentTripId = null;

                startEndTripButton.Text = "Start Trip";
                await DisplayAlert("Trip Ended", "Trip tracking stopped.", "OK");
            }
        }

        // New helper method to reset the map to only show user location when trip ends
        private async Task ResetMapToUserLocation()
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            if (location != null)
            {
                var mapSpan = new MapSpan(location, 0.05, 0.05);
                map.MoveToRegion(mapSpan);
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

                // CHANGED TO CALL LOCATION SERVICE INSTEAD OF START LOCATION UPDATES
                locationService = new LocationBackgroundService(currentTripId.Value);
                locationService.OnLocationPosted = (Location loc) =>
                {
                    AddLocationToTrip(loc);
                };
                locationService.Start();

                await DisplayAlert("Trip Started", $"Trip ID: {currentTripId}", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Failed to start trip.\nStatus: {(int)response.StatusCode}\n{responseBody}", "OK");
            }
        }

        /*
        // NOW UNUSED, SEE LocationBackgroundService IN THE Services FOLDER FOR REPLACEMENT

        private void StartLocationUpdates()
        {
            locationTimer = new System.Timers.Timer(locationIntervalMs);
            locationTimer.Elapsed += async (s, e) => await PostCurrentLocation();
            locationTimer.AutoReset = true;
            locationTimer.Start();
        }

        // NOW UNUSED, THE LOCATION SERVICE IS ENDED INSTEAD

        private void StopLocationUpdates()
        {
            if (locationTimer != null)
            {
                locationTimer.Stop();
                locationTimer.Dispose();
                locationTimer = null;
            }
        }
        // NOW UNUSED, SEE LocationBackgroundService IN THE Services FOLDER FOR REPLACEMENT

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
                        id = 0,
                        tripId = currentTripId.Value,
                        timestamp = DateTime.UtcNow.ToString("o"),
                        latitude = (decimal)location.Latitude,
                        longitude = (decimal)location.Longitude,
                        accuracy = (float)(location.Accuracy ?? 0),
                        speed = (decimal)(location.Speed ?? 0)
                    };

                    var response = await httpClient.PostAsJsonAsync("api/Locations", locPayload);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Location POST Status: {(int)response.StatusCode} ({response.StatusCode})");
                    Console.WriteLine($"Location POST Body: {responseBody}");

                    if (response.IsSuccessStatusCode)
                    {
                        var createdLocation = await response.Content.ReadFromJsonAsync<Location2>();
                        latestLocationId = createdLocation?.Id;

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AddLocationToTrip(location);
                        });
                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                            Application.Current?.MainPage?.DisplayAlert("Location Error",
                                $"Failed to send location.\nStatus: {(int)response.StatusCode}\n{responseBody}", "OK")
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Location error: {ex.Message}");
            }
        }
        */

        // New method that adds the posted location to the app's map as well
        private void AddLocationToTrip(Location location)
        {
            tripLocations.Add(location);

            // Add a Pin. COMMENT THIS OUT FOR OUR PRESENTATION

            var pin = new Pin
            {
                Label = $"Point {tripLocations.Count}",
                Location = location
            };
            map.Pins.Add(pin);

            // Update Polyline
            tripPolyline.Geopath.Clear();
            foreach (var loc in tripLocations)
            {
                tripPolyline.Geopath.Add(loc);
            }

            if (!map.MapElements.Contains(tripPolyline))
            {
                map.MapElements.Add(tripPolyline);
            }

            // Adjust Map View
            UpdateMapBounds();
        }

        // New helper method that updates the boundaries of the map to neatly fit the entire trip
        private void UpdateMapBounds()
        {
            if (tripLocations.Count == 0) return;

            var minLat = tripLocations.Min(loc => loc.Latitude);
            var maxLat = tripLocations.Max(loc => loc.Latitude);
            var minLon = tripLocations.Min(loc => loc.Longitude);
            var maxLon = tripLocations.Max(loc => loc.Longitude);

            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;

            var mapSpan = new MapSpan(
                new Location(centerLat, centerLon),
                (maxLat - minLat) * 1.5, // Expand view a little
                (maxLon - minLon) * 1.5
            );

            map.MoveToRegion(mapSpan);
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

                    blobservice.SetLocalFilePath(localPath);
                    await blobservice.UploadBlobAsync(fileName);

                    // Upload to API
                    await UploadPhotoToApi(fileName);
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