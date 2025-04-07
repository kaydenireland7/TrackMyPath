// src/MapComponent.jsx
import React, { useState, useEffect } from 'react';
import { GoogleMap, LoadScript, Marker } from '@react-google-maps/api';

const containerStyle = {
  width: '100%',
  height: '400px'
};

const center = {
  lat: -3.745,
  lng: -38.523
};

const MapComponent = () => {
  const [position, setPosition] = useState(center);

  useEffect(() => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition((position) => {
        setPosition({
          lat: position.coords.latitude,
          lng: position.coords.longitude
        });
      }, () => {
        console.error('Error getting location');
      });
    }
  }, []);

  return (
    <LoadScript
      googleMapsApiKey="AIzaSyBoGIKoRKdaOyfJ-zcfFyxVOGFCpXH1mjY"
    >
      <GoogleMap
        mapContainerStyle={containerStyle}
        center={position}
        zoom={15}
      >
        <Marker position={position} />
      </GoogleMap>
    </LoadScript>
  );
};

export default MapComponent;