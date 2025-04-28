import React from 'react';

function TripsList({ trips }) {
    return (
        <div>
            <h2>Trips </h2>
            <ul>
                {trips.map((loc) => (
                    <li key={loc.id}>
                        Trip {loc.id} — {loc.startTime} - {loc.endTime} - {loc.tripName}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default TripsList;