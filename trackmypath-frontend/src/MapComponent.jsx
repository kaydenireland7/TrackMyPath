import React, { useState, useEffect } from 'react';
import { GoogleMap, LoadScript, Marker, Polyline } from '@react-google-maps/api';

const containerStyle = {
    width: '100%',
    height: '400px',
};

const MapComponent = ({ trip, onMarkerClick, onMapClick }) => {
    const [center, setCenter] = useState({ lat: 0, lng: 0 });
    const [path, setPath] = useState([]);
    const [mapLoaded, setMapLoaded] = useState(false);

    useEffect(() => {
        // Reset the path every time the trip changes
        if (trip && trip.locations && trip.locations.length > 0) {
            const tripPath = trip.locations.map((loc) => ({
                lat: parseFloat(loc.latitude),
                lng: parseFloat(loc.longitude),
            }));

            // Clear the previous path before updating
            setPath(tripPath);
            setCenter(tripPath[0]);
        } else {
            // If no locations, clear the path
            setPath([]);
            setCenter({ lat: 0, lng: 0 });
        }
    }, [trip]); // This effect depends on the trip prop

    const createCircleIcon = (color) => {
        if (!window.google) return null;
        return {
            path: window.google.maps.SymbolPath.CIRCLE,
            scale: 8,
            fillColor: color,
            fillOpacity: 1,
            strokeWeight: 1,
        };
    };

    const handleMarkerClick = (location) => {
        if (onMarkerClick) {
            onMarkerClick(location);
        }
    };

    const handleMapClick = () => {
        if (onMapClick) {
            onMapClick();
        }
    };

    return (
        <LoadScript googleMapsApiKey="AIzaSyBoGIKoRKdaOyfJ-zcfFyxVOGFCpXH1mjY">
            <GoogleMap
                mapContainerStyle={containerStyle}
                center={center}
                zoom={12}
                onLoad={() => setMapLoaded(true)}
                onClick={handleMapClick}
                key={trip ? trip.id : "no-trip"}  // This forces the component to re-render on trip change
            >
                {mapLoaded && path.length > 0 && (
                    <Marker
                        position={path[0]}
                        label="Start"
                        icon={createCircleIcon('green')}
                    />
                )}

                {mapLoaded && path.length > 1 && (
                    <Marker
                        position={path[path.length - 1]}
                        label="End"
                        icon={createCircleIcon('red')}
                    />
                )}

                {mapLoaded && path.length > 1 && (
                    <Polyline
                        path={path}
                        options={{
                            strokeColor: '#0000FF',
                            strokeOpacity: 0.8,
                            strokeWeight: 4,
                        }}
                    />
                )}

                {mapLoaded &&
                    trip.locations.map((loc) => (
                        loc.photo && (
                            <Marker
                                key={loc.id}
                                position={{ lat: parseFloat(loc.latitude), lng: parseFloat(loc.longitude) }}
                                onClick={() => handleMarkerClick(loc)}
                            />
                        )
                    ))}
            </GoogleMap>
        </LoadScript>
    );
};

export default MapComponent;



















