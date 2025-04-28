import React from 'react';

function PhotosList({ photos }) {
    return (
        <div>
            <h2>Photos </h2>
            <ul>
                {photos.map((loc) => (
                    <li key={loc.id}>
                        id: {loc.id} loc id {loc.locationId}  url {loc.fileUrl}) capt {loc.caption}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default PhotosList;