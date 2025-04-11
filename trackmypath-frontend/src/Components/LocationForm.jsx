// src/components/LocationForm.jsx
import React, { useState } from 'react';
import { createLocation, updateLocation } from '../services/LocationsService';

const LocationForm = ({ location, onLocationCreated, onLocationUpdated }) => {
    const [formData, setFormData] = useState(location || {});

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (location) {
            await updateLocation(location.locationId, formData);
            onLocationUpdated();
        } else {
            await createLocation(formData);
            onLocationCreated();
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <label>
                Name:
                <input type="text" name="name" value={formData.name || ''} onChange={handleChange} />
            </label>
            <button type="submit">{location ? 'Update' : 'Create'}</button>
        </form>
    );
};

export default LocationForm;