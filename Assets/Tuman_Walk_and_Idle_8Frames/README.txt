TUMAN - WALK + IDLE ANIMATION PACKAGE (v2)

Idle animations newly added:
 Idle/Idle_Down_8frames_64px.png  - towards the player/front
 Idle/Idle_Up_8frames_64px.png    - facing away/back
 Idle/Idle_Left_8frames_64px.png  - facing screen left
 Idle/Idle_Right_8frames_64px.png - facing screen right

Each idle spritesheet is exactly 512 x 64 pixels: EIGHT 64 x 64 frames in ONE horizontal row.
Each frame has a transparent background and is a slight breathing/sway pose.
Feet stay fixed in place; the character does NOT walk during idle.
Individual idle PNGs are in Idle/Frames/ and preview loops in Idle/*Preview.gif.

Walk/ contains the original walk sheets, unchanged from the previous archive.

UNITY IMPORT SETTINGS
 Texture Type: Sprite (2D and UI)
 Sprite Mode: Multiple
 Pixels Per Unit: 32
 Filter Mode: Point (no filter)
 Compression: None
 Sprite Editor: Slice -> Grid by Cell Size -> 64 x 64, offset 0, padding 0.
 Animation Samples: around 6-8 frames/second for idle; enable Loop Time.

Recommended pivot: consistent Bottom Center or a custom feet pivot
(for both Walk and Idle strips); the actual pixel content has been kept stable.

NOTE
The animation is synthesized from the first pose of each previously generated
walk strip for exact visual continuity. This is a simple working idle loop,
not a hand-painted frame-by-frame professional sprite animation. Review in Unity.
