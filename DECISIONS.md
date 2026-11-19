# Decision log

## Week 11 — 19 November 2026
- Asked Codex to continue the existing Tuman project: keep VillageSource and the Task 10 courtyard, add player/enemy animations and implement Assignment 11. Kept the original Courtyard scene and made Tuman11 as the new entry scene.
- Chose full-body Idle, Walk, IdleShoot and WalkShoot states in four directions, so firing does not stop movement. Reused existing Idle/Walk art and generated shooting sheets; rejected the clipped last werewolf column and kept six complete frames per action.
- Moved player, enemy and item balance into ScriptableObject assets. Three LevelData assets configure the same scene with different enemy spawns, atmosphere and time limits; enemy health remains three one-damage hits.
- The first corruption test exposed an uncaught InvalidDataException, and a reset check could pass by loading defaults after a failed write. Added explicit recovery, a file-replace fallback and assertions that reset actually writes a valid readable save.
- Chose an asymmetric home/journal layout, separate level cards and focused settings/pause panels, informed by the freely available Kenney UI Pack (CC0). AI assistance is disclosed; physical-phone restart evidence and the student's English voice video must be recorded by the student, not fabricated.
