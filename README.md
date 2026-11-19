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
