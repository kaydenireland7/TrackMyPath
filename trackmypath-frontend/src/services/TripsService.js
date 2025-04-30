import axios from "axios";

export const fetchTrips = async () => {
    const response = await axios.get("http://localhost:3001/api/Trips");
    const rawTrips = response.data?.$values || [];
    return rawTrips.map(loc => ({
        id: loc.id,
        userId: loc.userId,
        startTime: loc.startTime,
        endTime: loc.endTime,
        tripName: loc.tripName
    }));
};