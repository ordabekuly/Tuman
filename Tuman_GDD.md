# Tuman - Game Design Document

Maksat Ordabekuly | Assignment 8-9 | Design proposal

This document describes planned work, not an already completed game.

## 1. Overview - title and pitch

Tuman is a mobile top-down action shooter set in a fog-covered Kazakh village. The player returns home, fights supernatural creatures and collects three amulet fragments to lift the village's curse. The game uses room-to-room combat inspired by The Binding of Isaac, with an original setting, characters and artwork.

## 2. Genre and platform

Single-player 2D action-adventure shooter for Android, played offline in landscape orientation. A complete run should take about 5-10 minutes. Combat is stylized and uses no graphic gore.

## 3. Presentation and main character

The world is viewed from above, with one courtyard visible at a time. The student-supplied pixel character represents the player: dark hair, a black-and-white jacket and a red shirt. Additional front, back and side movement frames are planned. Houses, fences, garden paths and a small cemetery establish the rural setting.

## 4. Game structure and core loop

Explore a courtyard > dodge and fight > collect rewards > unlock the next gate. Gates remain closed while enemies are alive. The orchard, barn and cemetery each award one amulet fragment when cleared. After collecting all three fragments and clearing the final courtyard, the player activates a shrine and reaches the win screen. Health reaching zero restarts the current courtyard.

## 5. Control and interface

The left joystick moves the character. The right joystick aims and fires light projectiles from an amulet while displaced; releasing it stops firing. Nearby pickups are collected automatically. The Main Menu contains the title, Play and Quit. The HUD shows three hearts, score, fragment count, both joysticks and Pause. Pause stops gameplay time and input and provides Resume, Restart and Menu. Restart resets the current courtyard; returning to Menu preserves the latest unlocked courtyard.

## 6. Game elements and levels

Five fixed courtyards: Home Yard, Abandoned Orchard, Old Barn, Cemetery Edge and Curse Shrine. The first three introduce enemy behaviours; Cemetery Edge contains all three enemy types together. Pickups include healing herbs, score tokens and three amulet fragments. There is one player weapon and no separate boss in the MVP.

| Enemy | Behaviour and states |
|---|---|
| Jeztyrnak | Wait > stalk > wind-up > claw attack > recover. A close-range ambusher. |
| Werewolf | Patrol > chase > wind-up > charge > recover. A fast, committed dash. |
| Ghost Archer | Watch > aim > shoot > cooldown. A ranged enemy whose sight is blocked by walls. |

## 7. Art, sound and asset sources

Low-resolution pixel art, dark blue fog, muted earth colours and warm lantern light. Walls and fences remain readable; the HUD has a solid backing. Planned sources: the student-supplied hero sprite, newly drawn village/enemy sprites, and original recorded or synthesized sound effects. The supplied hero's creator and reuse rights must be confirmed before release. Any external asset source and licence will be recorded in README. Sounds cover firing, pickup, damage, enemy warnings, win and loss, with quiet ambient music and a mute option.

## 8. MVP - week 15 build

A playable Android build with all five courtyards, three enemy types, one weapon, damage and short invulnerability feedback, healing, three fragments, gate transitions, restart and victory. Save the latest unlocked courtyard and sound preference locally. Each enemy has a state machine with at least three states, visible or audible attack preparation of 0.3-0.8 seconds, a cooldown and no off-screen attacks. At least one enemy checks distance, viewing angle and a wall-blocked Raycast; 2D movement uses waypoints. Speed, vision, wind-up and damage are Inspector fields.

## 9. Cut list

No procedural maps, multiplayer, crafting, dialogue trees, multiple playable characters, weapon inventories, complex item combinations, purchases or separate boss system. The five courtyards and three enemy types define the initial scope.

## 10. Development plan and roles

This is a solo project through week 13. The student is responsible for design, implementation, UI, asset attribution, playtesting and documentation. Any team scope changes after week 14 will be agreed with the instructor.

| Week | Deliverable |
|---|---|
| 9 | Finalize the GDD and scope; create the Unity GitHub repository, Unity .gitignore and README. |
| 10 | Build Menu, HUD and Pause. Check 16:9, notch phone and 4:3 tablet. Make 3+ meaningful commits and merge one feature branch through a pull request; submit A8-9 evidence. |
| 11 | Add all three enemy state machines, sight checks and fair attacks. Have two new players test on a phone; count attempts, target 2-4 tries, and record at least two tuning changes with old/new values. Update DECISIONS.md and submit A10 evidence. |
| 12 | Complete five courtyards, gates, fragments, healing and local progress saving. |
| 13 | Add sound and effects, tune difficulty, fix layouts and test the Android build; prepare the final-project handoff. |

UI: 1920 x 1080, Match = 1, anchors, SafeArea, 120 x 120 px minimum touch targets, 36 px minimum key text, three Device Simulator aspect ratios.
