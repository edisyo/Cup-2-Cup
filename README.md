A homework project in Unity for a Latvian Mobile Game Developer company.
This was my first serious project in Unity. 


# Cup 2 Cup
<img width="826" height="1235" alt="Cup2Cup" src="https://github.com/user-attachments/assets/2e8c9ab4-e7b5-4b99-afab-c2b362f9f465" />

A 3D swipe-based stacking puzzle for Android, built in Unity.

**Unity 2019.2.14f1 · C# · Portrait mobile · Android (min SDK 16)**

## The game

Cups sit on a grid of platforms. Swipe a platform in any of four directions and its cup — or its whole stack — flips over onto the neighbouring platform, arcing through the air and rotating 180° as it goes.

The catch is orientation. A cup is either standing upright or resting upside-down, and every flip inverts that state. Two stacks will only merge if their orientations match; mismatch, and the stack arcs over, fails to land, and drops back where it came from. Winning means routing every cup into a single stack, which means planning the parity of each move before you make it.

## What's implemented

**Input** — Four-direction swipe detection with a configurable threshold, distinguishing horizontal from vertical by dominant axis. Screen-to-world raycasting picks out the platform under the finger; a layer mask and a UI check keep taps on menus from reaching the board. Works with both touch and mouse for editor testing.

**Board logic** — Each platform raycasts to its four neighbours to find out whether one holds cups and how tall that stack is. That height feeds back into the animation, so a cup landing on a tall stack arcs higher and further.

**Movement** — Custom parabolic interpolation for the flight path, running alongside an independent rotation lerp, so the cup tumbles as it travels. Landing height is computed from the combined stack heights of source and destination.

**Stacking** — Cups form a parent hierarchy under a root cup. On a successful merge, every cup in the moving stack is reparented to the destination root, stack counts are transferred, and the children are repositioned into an even column. Failed merges rewind the animation and restore the original orientation.

**Progression and shop** — Coins are awarded on a win and spend on cup skins. Buying a skin swaps mesh and materials across every cup in the scene at runtime. Skins already owned can be re-equipped for free.

**Presentation** — Animated camera intro, staged UI (logo, controls hint, level name, retry, win screen with confetti and stars), and six sound effects wired to specific gameplay events: jump, land, failed stack, drop, victory, confetti.

**Reset** — Every cup stores its original position, rotation, orientation state and parent at startup, so retry restores the whole board without reloading the scene.

## Assets

Cup and platform models are custom-made and imported as FBX/OBJ. Three cup variants ship as purchasable skins. ProBuilder was used for greyboxing, TextMesh Pro for UI text.

## Status

Prototype — one playable level in a single scene. Coins and skin ownership live in memory only and reset when the app closes.

## What I'd do differently

Revisiting this project some years later, a few things stand out:

- **`cupMovement.cs` is roughly 800 lines, and about two-thirds of that is the same routine written out four times** — once per direction. Directions should be a single parameterised method taking an axis and a sign.
- **Movement state is four separate public booleans** (`moveRight`, `moveLeft`, `moveUp`, `moveDown`) where an enum and an explicit state machine would prevent whole classes of bug.
- **Debug logging was left in `Update()`.** On mobile, per-frame string concatenation and logging is a real cost, not a cosmetic one.
- **Several things poll when they should react** — the coin counter rewrites its UI text every frame instead of on change, and components are fetched with `GetComponent` inside update loops rather than cached.
- **No persistence.** Coins and purchased skins should be saved.
