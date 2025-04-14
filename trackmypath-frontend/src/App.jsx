// src/App.jsx
import React from 'react';
import MapComponent from './MapComponent';
import { useEffect, useState } from 'react';
import LocationForm from './Components/LocationForm';
import LocationsList from './Components/LocationsList';

function App() {
  
  const [location, setLocation] = useState(null);

  const handleLocationCreated = () => {
      console.log("Location created");
  };

  const handleLocationUpdated = () => {
      console.log("Location updated");
  };


  return (
    <div className="App">
      <header className="App-header">
        <h1>Google Maps with React</h1>
        <MapComponent />
        


      </header>
      <LocationForm
                location={location}
                onLocationCreated={handleLocationCreated}
                onLocationUpdated={handleLocationUpdated}
            />
            <LocationsList />
    </div>
  );
}

export default App;