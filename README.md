# Tuman
A mobile 2D action shooter set in a haunted Kazakh village.

### Local expansion (not uploaded yet)

The working project now includes a ranged ghost, a 32×18 two-courtyard map and a bounded follow camera. Level composition is 2 wolves / 2 wolves + 1 ghost / 2 wolves + 2 ghosts. See [local expansion notes](Docs/Ghost_Expansion.md). This update remains uncommitted and unpushed at the user's request; the earlier Task 11 hand-in remains in Submission_Task11, and new local deliverables are in Submission_Expanded.

## Assignment 11 — Data and Saves

Open the project with **Unity 6000.4.8f1**, then open **Assets/Scenes/Tuman11.unity** and press Play.

This branch adds three data-driven courtyards, ScriptableObject balance, safe JSON progression/settings, level unlocks, reset confirmation, redesigned mobile menus, touch controls and directional player animations. The original VillageSource map and Task 10 scene remain in the project.

- [Implementation and script map](Docs/Task11_README.md)
- [Student decision log](DECISIONS.md)
- [English video outline](Docs/Task11_Video_Plan.md)
- [Қазақша тапсыру нұсқаулығы](Docs/Task11_HandIn_KZ.md)
- [UI references and provenance](Docs/UI_REFERENCES.md)
- [Game design document](Tuman_GDD.md)

Controls: WASD/arrows or left joystick to move; Space or hold FIRE to shoot. A normal werewolf takes three one-damage hits. Clear each courtyard before the timer expires to unlock the next.

The filesystem and Unity Play-mode checks are documented under Verification. The physical Android force-stop/relaunch demonstration and a 2–3 minute MP4 narrated by the student are still personal submission steps; automated tests are not presented as that evidence.
# Week 12 — Sound Pass

Entry: `Assets/Scenes/TumanVillageDraft.unity`. Mixer: `Assets/Audio/TumanMix.mixer`. Clip library: `Assets/Resources/TumanSound.asset`.

## Audio sources and licenses

All audio below is **CC0 1.0**, downloaded 26 November 2026. License: https://creativecommons.org/publicdomain/zero/1.0/. Original Kenney license texts are included in `Assets/Audio/Licenses`.

| Files in Assets/Audio | Source and author | Use |
| --- | --- | --- |
| SFX/footstep00.ogg, footstep01.ogg, footstep02.ogg | Kenney, [RPG Audio](https://kenney.nl/assets/rpg-audio), CC0 | Footsteps |
| SFX/chop.ogg | Kenney, RPG Audio, CC0 | Accepted damage |
| SFX/handleCoins.ogg, handleCoins2.ogg | Kenney, RPG Audio, CC0 | Pickups |
| SFX/creak1.ogg, creak2.ogg | Kenney, RPG Audio, CC0 | Werewolf wind-up, stylized creak |
| SFX/knifeSlice.ogg, knifeSlice2.ogg | Kenney, RPG Audio, CC0 | Enemy strike/release |
| SFX/laser1.ogg, laser2.ogg | Kenney, [Digital Audio](https://kenney.nl/assets/digital-audio), CC0 | Player light shots |
| SFX/phaserUp1.ogg | Kenney, Digital Audio, CC0 | Ghost wind-up |
| SFX/lowDown.ogg | Kenney, Digital Audio, CC0 | Death |
| SFX/click_001.ogg, click_002.ogg | Kenney, [Interface Sounds](https://kenney.nl/assets/interface-sounds), CC0 | UI clicks |
| SFX/confirmation_002.ogg | Kenney, Interface Sounds, CC0 | Victory |
| SFX/error_004.ogg | Kenney, Interface Sounds, CC0 | Defeat |
| Music/Menu.ogg | epb9000, [Creepy Ambient Loop v2](https://opengameart.org/content/creepy-ambient-loop), CC0 | Menu music, unchanged |
| Music/Level.ogg | wipics, [Spooky Loop](https://opengameart.org/content/spooky-loop), CC0 | Gameplay music; original Spook_0.mp3 trimmed to 3.962–19.700 seconds, 8ms edge ramps, converted to OGG |

## Architecture

GameSession requests one persistent GameAudio. Two looping sources crossfade over one unscaled second through Master/Music/Tracks. Sixteen reusable 2D voices route effects through Master/SFX; pitch varies from 0.92 to 1.08 and clip variants avoid immediate repetition. EnemyAI sends its warning at wind-up entry, before damage/projectile release. Health, shooting, pickups and buttons send their own events. Footsteps follow actual movement distance.

Music and SFX sliders convert with `20 * Log10(Max(0.0001, value))` and use the Week 11 JSON save. Mixer faders apply from Start onwards. Normal/Paused snapshots control the unexposed Tracks volume and lowpass (0dB/22000Hz versus -6dB/800Hz), preserving the user's Music fader. Same-track requests preserve playback on Retry/stage changes; the manager survives scene loads.

Short effects: mono, Decompress On Load, ADPCM. Music: Streaming, Vorbis. Rebuild assets through Tuman > Setup Sound Pass. See `Docs/Task12_Video_Plan.md` for required student recordings. Automated checks do not replace listening on a phone.

