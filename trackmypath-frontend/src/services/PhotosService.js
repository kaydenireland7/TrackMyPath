import axios from "axios";

export const fetchPhotos = async () => {
    const response = await axios.get("http://localhost:3001/api/Photos");
    const rawPhotos = response.data?.$values || [];
    return rawPhotos.map(loc => ({
        id: loc.id,
        locationId: loc.locationId,
        fileUrl: loc.fileUrl,
        caption: loc.caption
    }));
};