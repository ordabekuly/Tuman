# Required video — 2–3 minutes, your own English voice

Record an MP4 file. The following is an outline, not a claim that phone tests have already been performed.

## 0:00–0:25 — Result

Show Home, Courtyards and Settings, then start level one.

“This is Tuman, my mobile village game. For Assignment Eleven I added data-driven balance, three level configurations and persistent JSON saves. The menus show my progress and which courtyards are unlocked.”

## 0:25–0:55 — Live balance

In Unity select PlayerStats, change Move Speed from 4 to 7 during Play mode, demonstrate movement, then restore 4. Show Level_01, Level_02 and Level_03 and their enemy spawns/time limits.

“The player speed comes from a ScriptableObject. I can change this value without editing code. Each LevelData asset contains the goal, spawn positions and time limit. The same scene code loads all three.”

## 0:55–1:35 — Phone restart proof

Use the installed APK on Android. Complete a level, change both volume sliders, then fully close/force-stop the app. Relaunch. Show that level two is unlocked and the slider values remain. Unity Remote alone does not demonstrate an installed app's persistent storage.

“I completed the first courtyard and changed the audio settings. Now I fully close the application and open it again. The unlocked level, best score and settings were restored from the JSON file.”

## 1:35–2:05 — Broken-file proof

Use the Editor save for this test. First back it up, stop Play, open save.json at persistentDataPath, replace its content with `{ broken`, then enter Play. Show the safe default Home and locked later levels. Restore your backup afterward.

“This file is deliberately broken. The loader catches invalid data and uses safe defaults, so the game still starts. I also validate the version, volume ranges and unlocked-level progression.”

## 2:05–2:45 — Script map and decisions

Show the script map in Task11_README, then DECISIONS.md.

“GameSession controls the active level, score and progression. Gameplay scripts read the balance assets. GameUI sends menu and settings actions to the session. SaveService validates and writes the JSON file. I used AI assistance and recorded my decisions. One test exposed an uncaught invalid-data exception, so we corrected recovery and checked that reset really writes a valid save.”

Show Reset Progress → Cancel, then confirm only if you want to erase the demonstration save. Do not narrate a test as passed before actually performing it.
