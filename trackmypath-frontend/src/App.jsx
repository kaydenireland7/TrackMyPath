import React, { useEffect, useState } from 'react'; //Used to manage app state and data fetching
import MapComponent from './MapComponent';
import LocationsList from './components/LocationsList';
import TripsList from './Components/TripsList';
import UsersList from './Components/UsersList';
import PhotosList from './Components/PhotosList';
import { fetchLocations } from './services/LocationsService';
import { fetchTrips } from './services/TripsService';
import { fetchUsers } from './services/UsersService';
import { fetchPhotos } from './services/PhotosService';

function App() {
    const [locations, setLocations] = useState([]); //Where fetched data is stored
    const [trips, setTrips] = useState([]);
    const [users, setUsers] = useState([]);
    const [photos, setPhotos] = useState([]);
    const [loading, setLoading] = useState(true); // Loading indicator
    const [formattedUsers, setFormattedUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null); // Picked user (The user is going to be user 1 until login is implemented)
    const [selectedTrip, setSelectedTrip] = useState(null); // Picked trip 

    const loadLocations = async () => {
        try {
            const data = await fetchLocations();
            setLocations(data);
        } catch (err) {
            console.error("Error fetching locations:", err);
        } finally {
            setLoading(false);
        }
    };

    const loadTrips = async () => {
        try {
            const data = await fetchTrips();
            setTrips(data);
        } catch (err) {
            console.error("Error fetching Trips:", err);
        } finally {
            setLoading(false);
        }
    };

    const loadUsers = async () => {
        try {
            const data = await fetchUsers();
            setUsers(data);
        } catch (err) {
            console.error("Error fetching Users:", err);
        } finally {
            setLoading(false);
        }
    };

    const loadPhotos = async () => {
        try {
            const data = await fetchPhotos();
            setPhotos(data);
        } catch (err) {
            console.error("Error fetching Photos:", err);
        } finally {
            setLoading(false);
        }
    };

    // Load all of the data
    useEffect(() => {
        Promise.all([loadLocations(), loadTrips(), loadUsers(), loadPhotos()])
            .then(() => setLoading(false));
    }, []);

    useEffect(() => {
        if (!loading && users.length && trips.length && locations.length && photos.length) {
            const formatted = formatData(users, trips, locations, photos);
            setFormattedUsers(formatted);
            console.log("Formatted Users Data:", formatted);
        }
    }, [users, trips, locations, photos, loading]);

    //Log in user one for now! Fix later 
    useEffect(() => {
        if (!loading && users.length && trips.length && locations.length && photos.length) {
            const formatted = formatData(users, trips, locations, photos);
            setFormattedUsers(formatted);
            console.log("Formatted Users Data:", formatted);

            // Auto-select first user for now
            setSelectedUser(formatted[0]);
        }
    }, [users, trips, locations, photos, loading]);


    function formatData(users, trips, locations, photos) {
        // Attach photos to locations
        const locationsWithPhotos = locations.map(loc => ({
            ...loc,
            photo: photos.find(photo => photo.locationId === loc.id) || null
        }));

        // Attach locations to trips
        const tripsWithLocations = trips.map(trip => ({
            ...trip,
            locations: locationsWithPhotos.filter(loc => loc.tripId === trip.id)
        }));

        // Attach trips to users
        const usersWithTrips = users.map(user => ({
            ...user,
            trips: tripsWithLocations.filter(trip => trip.userId === user.id)
        }));

        return usersWithTrips;
    }


    return (
        <div className="App">
            <header>
                <h1>Google Maps with React</h1>
            </header>

            {loading ? (
                <p>Loading...</p>
            ) : (
                <>
                    {/* Welcome Message and Trip Dropdown */}
                    {selectedUser && (
                        <div>
                            <h2>Welcome, {selectedUser.email}!</h2>

                            <label htmlFor="tripSelect">Select a Trip:</label>
                            <select
                                id="tripSelect"
                                onChange={(e) => {
                                    const tripId = e.target.value;
                                    const trip = selectedUser.trips.find(t => t.id === parseInt(tripId));
                                    setSelectedTrip(trip);
                                }}
                            >
                                <option value="">-- Choose a Trip --</option>
                                {selectedUser.trips.map(trip => (
                                    <option key={trip.id} value={trip.id}>
                                        {trip.tripName}
                                    </option>
                                ))}
                            </select>
                        </div>
                    )}

                    {/* Map for selected trip */}
                    {selectedTrip && (
                        <div>
                            <MapComponent trip={selectedTrip} />
                        </div>
                    )}

                    {/* Print trip details */}
                    {selectedTrip && (
                        <div style={{ marginTop: '20px', padding: '10px', border: '1px solid #ccc', borderRadius: '8px' }}>
                            <h2>Trip Details</h2>
                            <p><strong>Trip Name:</strong> {selectedTrip.tripName?.trim() || 'No Trip Name'}</p>
                            <p><strong>Start Date:</strong> {selectedTrip.startTime ? new Date(selectedTrip.startTime).toLocaleString() : 'N/A'}</p>
                            <p><strong>End Date:</strong> {selectedTrip.endTime ? new Date(selectedTrip.endTime).toLocaleString() : 'N/A'}</p>

                            <ul>
                                {selectedTrip.locations.map(location => (
                                    <li key={location.id}>
                                        
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                </>
            )}
        </div>
    );


}

export default App;