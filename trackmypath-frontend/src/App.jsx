// src/App.jsx
import React, { useEffect, useState } from 'react';
import MapComponent from './MapComponent';
import { fetchLocations } from './services/LocationsService';
import { fetchTrips } from './services/TripsService';
import { fetchUsers } from './services/UsersService';
import { fetchPhotos } from './services/PhotosService';
import Login from './components/Login';

function App() {
    const [locations, setLocations] = useState([]);
    const [trips, setTrips] = useState([]);
    const [users, setUsers] = useState([]);
    const [photos, setPhotos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [formattedUsers, setFormattedUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [selectedTrip, setSelectedTrip] = useState(null);

    useEffect(() => {
        const loadData = async () => {
            try {
                const [locationsData, tripsData, usersData, photosData] = await Promise.all([
                    fetchLocations(),
                    fetchTrips(),
                    fetchUsers(),
                    fetchPhotos(),
                ]);
                setLocations(locationsData);
                setTrips(tripsData);
                setUsers(usersData);
                setPhotos(photosData);
            } catch (err) {
                console.error('Error fetching data:', err);
            } finally {
                setLoading(false);
            }
        };
        loadData();
    }, []);

    useEffect(() => {
        if (!loading && users.length && trips.length && locations.length && photos.length) {
            const formatted = formatData(users, trips, locations, photos);
            setFormattedUsers(formatted);
        }
    }, [users, trips, locations, photos, loading]);

    const handleLogin = (user) => {
        setSelectedUser(user);
    };

    function formatData(users, trips, locations, photos) {
        const locationsWithPhotos = locations.map(loc => ({
            ...loc,
            photo: photos.find(photo => photo.locationId === loc.id) || null,
        }));

        const tripsWithLocations = trips.map(trip => ({
            ...trip,
            locations: locationsWithPhotos.filter(loc => loc.tripId === trip.id),
        }));

        const usersWithTrips = users.map(user => ({
            ...user,
            trips: tripsWithLocations.filter(trip => trip.userId === user.id),
        }));

        return usersWithTrips;
    }

    if (loading) {
        return <p style={{ textAlign: 'center', marginTop: '100px' }}>Loading...</p>;
    }

    if (!selectedUser) {
        if (!formattedUsers.length) {
            return <p style={{ textAlign: 'center', marginTop: '100px' }}>Loading users...</p>;
        }
        return <Login users={formattedUsers} onLogin={handleLogin} />;
    }

    return (
        <div className="App" style={{ fontFamily: 'Arial, sans-serif', backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
            <header style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '10px 30px',
                backgroundColor: '#1976d2',
                color: 'white',
                boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)'
            }}>
                <h1 style={{ margin: 0 }}>Track My Path</h1>
                <button
                    onClick={() => {
                        setSelectedUser(null);
                        setSelectedTrip(null);
                    }}
                    style={{
                        padding: '8px 16px',
                        backgroundColor: '#f44336',
                        color: 'white',
                        border: 'none',
                        borderRadius: '5px',
                        cursor: 'pointer',
                        fontSize: '16px'
                    }}
                >
                    Logout
                </button>
            </header>

            <div style={{ padding: '40px', maxWidth: '800px', margin: '0 auto' }}>
                <h2 style={{ color: '#333' }}>Welcome, {selectedUser.email}!</h2>

                <label htmlFor="tripSelect" style={{ display: 'block', marginTop: '30px', marginBottom: '8px', fontSize: '18px' }}>
                    Select a Trip:
                </label>
                <select
                    id="tripSelect"
                    value={selectedTrip ? selectedTrip.id : ""}
                    onChange={(e) => {
                        const tripId = e.target.value;
                        const trip = selectedUser.trips.find(t => t.id === parseInt(tripId));
                        setSelectedTrip(trip || null);
                    }}
                    style={{
                        width: '100%',
                        maxWidth: '400px',
                        padding: '10px',
                        borderRadius: '5px',
                        border: '1px solid #ccc',
                        fontSize: '16px'
                    }}
                >
                    <option value="">-- Choose a Trip --</option>
                    {selectedUser.trips.map(trip => (
                        <option key={trip.id} value={trip.id}>
                            {trip.tripName}
                        </option>
                    ))}
                </select>

                {selectedTrip && (
                    <div style={{ marginTop: '40px', backgroundColor: 'white', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}>
                        <MapComponent trip={selectedTrip} />
                        <div style={{ marginTop: '20px' }}>
                            <h3 style={{ borderBottom: '2px solid #1976d2', paddingBottom: '6px' }}>Trip Details</h3>
                            <p><strong>Trip Name:</strong> {selectedTrip.tripName?.trim() || 'No Trip Name'}</p>
                            <p><strong>Start Date:</strong> {selectedTrip.startTime ? new Date(selectedTrip.startTime).toLocaleString() : 'N/A'}</p>
                            <p><strong>End Date:</strong> {selectedTrip.endTime ? new Date(selectedTrip.endTime).toLocaleString() : 'N/A'}</p>
                        </div>
                    </div>
                )}

            </div>
        </div>
    );
}

export default App;
