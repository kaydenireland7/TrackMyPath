import React, { useEffect, useState } from 'react'; //Used to manage app state and data fetching
import MapComponent from './MapComponent';
import LocationsList from './components/LocationsList';
import { fetchLocations } from './services/LocationsService';

function App() {
    const [locations, setLocations] = useState([]); //Where fetched data is stored
    const [loading, setLoading] = useState(true); // Loading spinner while waiting for data

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

    //Add method and other classes for all db tables

    useEffect(() => {
        loadLocations();
        //Trip, user, and photo data will also be called here
    }, []);

    return (
        <div className="App">
            <header>
                <h1>Google Maps with React</h1>
            </header>
            {loading ? (
                <p>Loading...</p>
            ) : (
                <>
                    <MapComponent locations={locations} />
                    <LocationsList locations={locations} />
                </>
            )}
        </div>
    );
}

export default App;