import React from 'react';

function PhotosList({ photos }) {
    return (
        <div>
            <h2>Photos</h2>
            <ul>
                {photos.map((photo) => {
                    console.log('Photo:', photo); // ← ADD THIS LINE

                    return (
                        <li key={photo.id}>
                            <p>id: {photo.id}</p>
                            <p>location id: {photo.locationId}</p>
                            <p>caption: {photo.caption}</p>
                            <p>fileUrl: {photo.fileUrl}</p>
                            <img
                                src={`http://localhost:3001/blob/images/${photo.fileUrl}`}
                                alt={photo.caption || 'photo'}
                                onError={() => console.error('❌ Failed to load image:', photo.fileUrl)}
                                style={{ maxWidth: '300px' }}
                            />

                        </li>
                    );
                })}
            </ul>
        </div>
    );
}

export default PhotosList;

