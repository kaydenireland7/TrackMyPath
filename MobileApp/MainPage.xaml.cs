using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Linq;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace MobileApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            _ = InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            // Try to get user's current location
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
                latitudeDegrees: maxLat - minLat + 1, // +1 adds padding
                longitudeDegrees: maxLon - minLon + 1
            );

            // Create map
            var map = new Map(mapBounds)
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
            polyline.Geopath.Add(location4); // current location

            map.MapElements.Add(polyline);

            // Show map
            Content = map;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
