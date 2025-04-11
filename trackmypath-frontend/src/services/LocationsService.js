// src/services/locationsService.js
const API_URL = "https://localhost:5073/api/locations";

export async function getLocations() {
    const response = await fetch(API_URL);
    if (!response.ok) {
        throw new Error("Failed to fetch locations");
    }
    return await response.json();
}

export async function getLocation(id) {
    const response = await fetch(`${API_URL}/${id}`);
    if (!response.ok) {
        throw new Error("Failed to fetch location");
    }
    return await response.json();
}

export async function createLocation(location) {
    const response = await fetch(API_URL, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(location),
    });
    if (!response.ok) {
        throw new Error("Failed to create location");
    }
    return await response.json();
}

export async function updateLocation(id, location) {
    const response = await fetch(`${API_URL}/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(location),
    });
    if (!response.ok) {
        throw new Error("Failed to update location");
    }
    return await response.json();
}

export async function deleteLocation(id) {
    const response = await fetch(`${API_URL}/${id}`, {
        method: "DELETE",
    });
    if (!response.ok) {
        throw new Error("Failed to delete location");
    }
}