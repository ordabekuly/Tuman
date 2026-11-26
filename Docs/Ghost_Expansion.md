# Local village expansion — 19 November 2026

This update is local only. The user explicitly asked not to upload it to GitHub yet.

## Enemy types

- **Werewolf:** existing melee patrol/chase/attack enemy, 3 HP.
- **Ghost:** floating ranged enemy, 3 HP. Sees the player with the same wall-blocked Raycast. Winds up before firing a slow cyan projectile. Retreats when the player is closer than the data-defined retreat distance. Bolts hit walls and can be dodged; ghosts respect solid walls.

`Assets/Data/GhostStats.asset` holds ghost HP/damage, detection/attack/retreat distances, movement, attack timing and projectile speed/range/radius. `EnemyAI` retains the same explicit Patrol/Chase/Attack FSM and selects melee or ranged attack from data. `GhostMotion` adds procedural floating, casting tint and fade-out death around the generated ghost art.

## Map and camera

VillageSource art remains. Tuman11 expands from **24×16 to 32×18** cells (1.5× floor area), split into western/eastern courtyards by a stone divider with a three-cell gate passage. Cover is placed in both courtyards. FollowCamera tracks the player with smoothing and clamps the visible area to map bounds, keeping characters readable instead of shrinking the whole expanded map onto one screen.

## Level composition

| Level | Werewolves | Ghosts | Time |
|---|---:|---:|---:|
| 1 | 2 | 0 | 90 s |
| 2 | 2 | 1 | 120 s |
| 3 | 2 | 2 | 150 s |

LevelData now has separate werewolf and ghost spawn arrays and stat references. Total enemy count, HUD goals and victory progression include both types. Existing three-level saves remain compatible.

The pre-expansion scene/data backup is in `Submission_Backup/Before_Ghost_Expansion`. The previously delivered Task11 APK and ZIP are kept separately from the new expansion build.

## Mobile controls

The user's supplied red joystick references were recolored into separate gold base and white-star knob PNGs. The flaming bullet FIRE icon was recreated with real alpha transparency, removing the checkerboard painted into the supplied JPG. GameUI uses these three sprites, preserves their aspect ratios, rejects touches on transparent FIRE corners and supports hold-to-fire. A rendered Editor preview is stored in `Submission_Expanded/Expanded_Ghost_Controls.png`; it is not physical-device proof.
