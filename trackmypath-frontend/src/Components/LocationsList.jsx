// src/components/LocationsList.jsx
import React, { useEffect, useState } from 'react';
import { getLocations } from '../services/LocationsService';

const LocationsList = () => {
    const [locations, setLocations] = useState([]);

    useEffect(() => {
        async function fetchLocations() {
            try {
                const data = await getLocations();
                setLocations(data);
            } catch (error) {
                console.error("Error fetching locations:", error);
            }
        }

        fetchLocations();
    }, []);

    return (
        <div>
            <h2>Locations</h2>
            <ul>
                {locations.map(location => (
                    <li key={location.locationId}>{location.name}</li>
                ))}
            </ul>
        </div>
    );
};

export default LocationsList;