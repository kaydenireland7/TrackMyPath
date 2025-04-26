import axios from "axios"; //Makes http requests :)

export const fetchLocations = async () => {
    const response = await axios.get("http://localhost:3001/api/Locations");
    const rawLocations = response.data?.$values || [];
    return rawLocations.map(loc => ({
        id: loc.id,
        name: loc.name,
        latitude: loc.latitude,
        longitude: loc.longitude,
        timeStamp: loc.timestamp,
        tripId: loc.tripId,
        // If you want accuracy and speed as well add them here!
    }));
};

