import { readFile, mkdir, writeFile } from 'node:fs/promises';
import { join } from 'node:path';

const root = new URL('.', import.meta.url);
const output = new URL('../MusicWebApp/MusicWebApp/wwwroot/data/music/', root);
await mkdir(output, { recursive: true });

const collections = [
  { code: 'vi', name: 'Nhạc Việt', emoji: '🇻🇳', description: 'Nhạc Việt được yêu thích, từ hit mới đến ca khúc quen thuộc.' },
  { code: 'en', name: 'Nhạc Anh', emoji: '🇬🇧', description: 'Những ca khúc tiếng Anh nổi bật qua nhiều năm.' },
  { code: 'ja', name: 'Nhạc Nhật', emoji: '🇯🇵', description: 'J-pop và nhạc Nhật nổi bật.' },
  { code: 'zh', name: 'Nhạc Trung', emoji: '🇨🇳', description: 'Những ca khúc nhạc Hoa được yêu thích.' },
];

for (const collection of collections) {
  const candidates = JSON.parse(await readFile(new URL(`${collection.code}.json`, root), 'utf8'));
  const recent = JSON.parse(await readFile(new URL(`${collection.code}-recent.json`, root), 'utf8'));
  const tiktok = JSON.parse(await readFile(new URL(`${collection.code}-tiktok.json`, root), 'utf8'));
  const toTracks = (items) => items.map((track) => ({
    v: track.videoId,
    t: track.title,
    a: track.artist || artistFromTitle(track.title, collection.code),
  }));
  const tracks = toTracks(candidates.tracks);
  const recentTracks = toTracks(recent.tracks);
  const tiktokTracks = toTracks(tiktok.tracks);
  if (tracks.length === 0 || recentTracks.length !== 30 || tiktokTracks.length !== 100
      || new Set([...tracks, ...recentTracks].map((track) => track.v)).size !== tracks.length + recentTracks.length
      || new Set(tiktokTracks.map((track) => track.v)).size !== 100) {
    throw new Error(`${collection.code} must have unique videos`);
  }
  const playlist = {
    id: `country-${collection.code}`,
    name: collection.name,
    emoji: collection.emoji,
    description: collection.description,
    tracks,
  };
  await writeFile(new URL(`${collection.code}.json`, output), JSON.stringify(playlist, null, 2) + '\n');
  await writeFile(new URL(`${collection.code}-recent.json`, output), JSON.stringify({
    id: `recent-${collection.code}`,
    name: `Nhạc ${collection.code === 'vi' ? 'Việt' : collection.code === 'en' ? 'Anh' : collection.code === 'ja' ? 'Nhật' : 'Trung'} mới 2025–2026`,
    emoji: collection.emoji,
    description: '30 ca khúc nổi bật được đăng trong giai đoạn 2025–2026.',
    tracks: recentTracks,
  }, null, 2) + '\n');
  await writeFile(new URL(`${collection.code}-tiktok.json`, output), JSON.stringify({
    id: `tiktok-${collection.code}`,
    name: `TikTok ${collection.code === 'vi' ? 'Việt' : collection.code === 'en' ? 'Anh' : collection.code === 'ja' ? 'Nhật' : 'Trung'} — 100 bài`,
    emoji: collection.emoji,
    description: '100 ca khúc được tuyển từ xu hướng TikTok, Douyin và playlist viral.',
    tracks: tiktokTracks,
  }, null, 2) + '\n');
}

function artistFromTitle(title, code) {
  if (code === 'zh') {
    if (/周杰倫|Jay Chou/i.test(title)) return 'Jay Chou';
    if (/鄧紫棋|G\.E\.M\./i.test(title)) return 'G.E.M.';
  }
  if (code === 'ja') {
    for (const artist of ['YOASOBI', 'Ado', '米津玄師', 'Mrs. GREEN APPLE', 'RADWIMPS', 'ヨルシカ']) {
      if (title.includes(artist)) return artist;
    }
  }
  const match = title.match(/^(.{2,45}?)\s[-–|]\s/);
  return match?.[1] || (code === 'vi' ? 'Nghệ sĩ Việt' : code === 'en' ? 'International' : code === 'ja' ? 'J-pop' : 'Nhạc Hoa');
}
