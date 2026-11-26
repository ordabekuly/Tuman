# Decision log

## Week 12 — 26 November 2026
- Asked Codex to source free online audio and implement the sound pass. Chose CC0 Kenney effects and two CC0 OpenGameArt loops; each imported file and its author/source is listed in README. The wolf warning is a stylized wooden creak; the ghost uses a rising electronic cue.
- Replaced the earlier two-tone prototype and direct source-volume settings with Master/Music/Tracks and SFX routing, two persistent music sources and pooled SFX voices. The initial editor mixer setup assumed an existing group view and failed; added an explicit All groups view before saving the mixer.
- Music/SFX faders use Log10 × 20 from Start onwards. Kept pause attenuation and lowpass on the separate Tracks child, because exposed MusicVol is controlled by code and cannot also be controlled by snapshots.
- Kept wind-up cues at the beginning of EnemyAI's existing wind-up, rather than at damage time. Added pitch variation and alternate clips. Same-track requests keep playback on Retry; unscaled time makes crossfades and pause transitions work at timeScale zero.
- Twenty-two Play-mode checks passed, including real wolf warning-before-damage, saved faders, crossfade overlap, Retry continuity, source routing and scene reload. Automated results do not establish sound quality on a phone; the student's listening/restart test and own English video are still required.

## Week 11 — 19 November 2026
- Asked Codex to continue the existing Tuman project: keep VillageSource and the Task 10 courtyard, add player/enemy animations and implement Assignment 11. Kept the original Courtyard scene and made Tuman11 as the new entry scene.
- Chose full-body Idle, Walk, IdleShoot and WalkShoot states in four directions, so firing does not stop movement. Reused existing Idle/Walk art and generated shooting sheets; rejected the clipped last werewolf column and kept six complete frames per action.
- Moved player, enemy and item balance into ScriptableObject assets. Three LevelData assets configure the same scene with different enemy spawns, atmosphere and time limits; enemy health remains three one-damage hits.
- The first corruption test exposed an uncaught InvalidDataException, and a reset check could pass by loading defaults after a failed write. Added explicit recovery, a file-replace fallback and assertions that reset actually writes a valid readable save.
- Chose an asymmetric home/journal layout, separate level cards and focused settings/pause panels, informed by the freely available Kenney UI Pack (CC0). AI assistance is disclosed; physical-phone restart evidence and the student's English voice video must be recorded by the student, not fabricated.
