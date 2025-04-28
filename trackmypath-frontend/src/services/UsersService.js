import axios from "axios";

export const fetchUsers = async () => {
    const response = await axios.get("http://localhost:3001/api/Users");
    const rawUsers = response.data?.$values || [];
    return rawUsers.map(loc => ({
        id: loc.id,
        email: loc.email,
        passwordHash: loc.passwordHash
    }));
};