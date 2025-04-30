const express = require('express');
const cors = require('cors');
const fetch = require('node-fetch');

const app = express();
const PORT = 3001;

app.use(cors({ origin: 'http://localhost:5173' }));

// 🔐 Insert your actual SAS token here (omit the leading '?')
const BLOB_SAS_TOKEN = 'sv=2024-11-04&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-05-04T03:34:04Z&st=2025-04-29T19:34:04Z&sip=0.0.0.0-255.255.255.255&spr=https&sig=qwrwWxwR6Dp4%2BG2uPEFMgyDPB5f1GldvjnCmBBgAwJw%3D';

// API Proxy Route
app.use('/api', async (req, res) => {
    const targetUrl = `http://localhost:5073${req.originalUrl}`;
    console.log(`➡️ Proxying request to: ${targetUrl}`);
    try {
        const response = await fetch(targetUrl);

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

// Handles requests like: /blob/images/:filename
app.get('/blob/images/:filename', async (req, res) => {
    const filename = req.params.filename;

    // Make sure to include your actual SAS token here
    const BLOB_SAS_TOKEN = 'sv=2024-11-04&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-05-04T03:34:04Z&st=2025-04-29T19:34:04Z&sip=0.0.0.0-255.255.255.255&spr=https&sig=qwrwWxwR6Dp4%2BG2uPEFMgyDPB5f1GldvjnCmBBgAwJw%3D';;

    if (!BLOB_SAS_TOKEN) {
        return res.status(500).json({ error: 'Missing SAS token' });
    }

    const blobUrl = `https://trackmypath.blob.core.windows.net/images/${filename}?${BLOB_SAS_TOKEN}`;

    console.log(`➡️ Proxying image from: ${blobUrl}`);

    try {
        const imageResponse = await fetch(blobUrl);

        if (!imageResponse.ok) {
            return res.status(imageResponse.status).json({ error: 'Failed to fetch image from blob storage' });
        }

        res.setHeader('Content-Type', imageResponse.headers.get('content-type'));
        imageResponse.body.pipe(res);
    } catch (err) {
        console.error('❌ Image proxy error:', err);
        res.status(500).json({ error: 'Image proxy failed' });
    }
});

app.listen(PORT, () => {
    console.log(`🚀 Express proxy running on http://localhost:${PORT}`);
});



