import React from 'react';

function UsersList({ users }) {
    return (
        <div>
            <h2> Users </h2>
            <ul>
                {users.map((loc) => (
                    <li key={loc.id}>
                        User {loc.id} — {loc.email} - {loc.passwordHash}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default UsersList;