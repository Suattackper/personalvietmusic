import { readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

const project = new URL('../MusicWebApp/MusicWebApp/', import.meta.url);
const config = JSON.parse(await readFile(new URL('wwwroot/appsettings.json', project), 'utf8'));
const key = config.YouTube?.ApiKey;
if (!key || key === 'YOUR_YOUTUBE_API_KEY') throw new Error('YouTube API key is not configured.');

const basePath = new URL('wwwroot/data/', project);
const featuredPath = new URL('playlists.json', basePath);
const featuredRaw = await readFile(featuredPath, 'utf8');
const playlists = JSON.parse(featuredRaw);
for (const code of ['vi', 'en', 'ja', 'zh']) {
  playlists.push(JSON.parse(await readFile(new URL(`music/${code}.json`, basePath), 'utf8')));
  playlists.push(JSON.parse(await readFile(new URL(`music/${code}-recent.json`, basePath), 'utf8')));
  playlists.push(JSON.parse(await readFile(new URL(`music/${code}-tiktok.json`, basePath), 'utf8')));
}
const ids = [...new Set(playlists.flatMap((list) => list.tracks.map((track) => track.v)))];
const results = new Map();

for (let i = 0; i < ids.length; i += 50) {
  const batch = ids.slice(i, i + 50);
  const url = new URL('https://www.googleapis.com/youtube/v3/videos');
  url.searchParams.set('part', 'status');
  url.searchParams.set('id', batch.join(','));
  url.searchParams.set('key', key);
  const response = await fetch(url, { headers: { Referer: 'https://suattackper.github.io/personalvietmusic/' } });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(`YouTube API returned ${response.status}: ${body.error?.errors?.[0]?.reason || 'unknown error'}`);
  }
  const data = await response.json();
  for (const item of data.items || []) {
    results.set(item.id, {
      embeddable: item.status?.embeddable ?? null,
      privacyStatus: item.status?.privacyStatus ?? null,
    });
  }
  console.log(`Checked ${Math.min(i + 50, ids.length)} / ${ids.length}`);
}

const blocked = ids.filter((id) => results.get(id)?.embeddable === false);
const unavailable = ids.filter((id) => !results.has(id) || results.get(id)?.privacyStatus !== 'public');
const remove = new Set([...blocked, ...unavailable]);
const report = {
  checkedAt: new Date().toISOString(),
  uniqueVideos: ids.length,
  embeddable: ids.length - remove.size,
  blockedEmbedding: blocked,
  unavailable,
  removed: [...remove],
  note: 'A true embeddable flag does not guarantee playback; platform policies and rights claims can still block playback.',
};
await writeFile(new URL('embed-report.json', import.meta.url), JSON.stringify(report, null, 2) + '\n');

for (const playlist of playlists) {
  const file = playlist.id.startsWith('country-')
    ? new URL(`music/${playlist.id.slice(8)}.json`, basePath)
    : playlist.id.startsWith('recent-')
      ? new URL(`music/${playlist.id.slice(7)}-recent.json`, basePath)
      : playlist.id.startsWith('tiktok-')
        ? new URL(`music/${playlist.id.slice(7)}-tiktok.json`, basePath)
      : null;
  if (file) {
    playlist.tracks = playlist.tracks.filter((track) => !remove.has(track.v));
    await writeFile(file, JSON.stringify(playlist, null, 2) + '\n');
  }
}
if (remove.size > 0) {
  let filtered = featuredRaw.split('\n').filter((line) => {
    const match = line.match(/^\s*\{"v":"([\w-]{11})"/);
    return !match || !remove.has(match[1]);
  }).join('\n').replace(/,\s*\]/g, '\n    ]');
  JSON.parse(filtered);
  await writeFile(featuredPath, filtered);
}

for (const code of ['vi', 'en', 'ja', 'zh']) {
  for (const suffix of ['', '-recent', '-tiktok']) {
    const file = new URL(`${code}${suffix}.json`, import.meta.url);
    const list = JSON.parse(await readFile(file, 'utf8'));
    list.tracks = list.tracks.filter((track) => !remove.has(track.videoId));
    await writeFile(file, JSON.stringify(list, null, 2) + '\n');
  }
}

console.log(`Blocked embedding: ${blocked.length}; unavailable: ${unavailable.length}; removed unique videos: ${remove.size}`);
