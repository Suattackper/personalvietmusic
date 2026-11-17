# AnhTanMusic experience checklist

1. [x] Unified dark stage design, responsive VI/EN UI.
2. [x] Aurora/banner layers and artwork-aware palette with CORS-safe fallback.
3. [x] Track/genre hover, focus and press, fine-pointer 3D tilt, stacked playlist artwork. No separate album entity exists.
4. [x] Artwork fade and mini-player to Now Playing cover animation, route fade. Playlist detail retains route transition because it has no album cover model.
5. [x] Mini player controls, seek preview, volume and loading/playing/paused/ended/error states.
6. [x] Canvas ring/bars, explicitly labeled simulated for YouTube iframe. Real audio analysis is unavailable from this source.
7. [x] Vinyl mode with rotation tied to playback and simpler light/off motion.
8. [x] Now Playing, swipe close, Escape/focus restoration and safe area layout.
9. [x] Optional static, timed-line and timed-word lyrics with manual scroll override; empty data gives unavailable state.
10. [x] Queue drag/buttons, short FLIP movement, removal undo without audio reload.
11. [x] Stage mode with user-initiated fullscreen, in-page fallback, idle controls and fullscreen exit handling.
12. [x] Neon City, Rain Night and Vinyl Room themes with distinct color/motion, persisted.
13. [x] Sliding nav marker, one-shot favorite burst, queue toast/undo, search/route transitions and existing loading/empty/error states.
14. [x] Home uses real recent tracks and curated playlists; media frame behavior unchanged.
15. [x] CSS/JavaScript/Canvas modules; no new library or lockfile change.
16. [x] Full effects fixed, hidden-tab drawing stopped, DPR capped, focus/mobile support; scrollbars hidden without disabling scrolling.
17. [x] Debug build, Release publish, desktop and 390px browser checks, playback, seek, volume, favorite, theme, vinyl, visualizer, queue buttons/undo and console checked. Pages base-href workflow inspected. Automated browser coverage for touch swipe, native drag, actual fullscreen and network failure was not available in this run.

Limits: the YouTube iframe does not expose audio samples to this page. Lyrics need licensed/owned data; none is present. The shared cover transition is limited to mini player and Now Playing because this project has no album cover model.

Source constraint: `youtubePlayer.js`, iframe markup/visibility behavior and `RecentlyPlayedService.cs` remain unchanged.
