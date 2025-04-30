import React from 'react';

function LocationsList({ locations }) {
    return (
        <div>
            <h2>Locations</h2>
            <ul>
                {locations.map((loc) => {
                    console.log("Location object:", loc);
                    return (
                        <li key={loc.id}>
                            Trip {loc.tripId} — ({loc.latitude}, {loc.longitude}) at {new Date(loc.timeStamp).toLocaleString()}
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}

export default LocationsList;

