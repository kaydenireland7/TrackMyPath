const express = require('express');
const cors = require('cors');
const fetch = require('node-fetch');

const app = express();
const PORT = 3001;

app.use(cors({ origin: 'http://localhost:5173' }));

app.use('/api', async (req, res) => {
    const targetUrl = `http://localhost:5073${req.originalUrl}`;
    console.log(`➡️ Proxying request to: ${targetUrl}`);
    try {
        const response = await fetch(targetUrl);

        // Handle non-JSON responses gracefully
        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
            const text = await response.text();
            return res.status(500).json({ error: 'API did not return JSON', details: text });
        }

        const data = await response.json();
        return res.json(data);
    } catch (err) {
        console.error('❌ Proxy error:', err);
        return res.status(500).json({ error: 'Failed to connect to ASP.NET Core API' });
    }
});

app.listen(PORT, () => {
    console.log(`🚀 Express proxy running on http://localhost:${PORT}`);
});
