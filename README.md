[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/toCiXtCp)
[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-2e0aaae1b6195c2367325f4f02e2d04e9abb55f0b24a779b69b11b9e10269abc.svg)](https://classroom.github.com/online_ide?assignment_repo_id=23364553&assignment_repo_type=AssignmentRepo)
# Assignment 02 (Individual) – GAMES DEVELOPMENT

**SE4031 – Games Development**  
**Title – 2D Interactive Game**  
**Assignment Created By:** Mr. Aruna Ishara Gamage, Mr. Nushkan Nismi  
**Weight:** 20%

---

## Objective

The objective of this assignment is to enable students to design and develop a 2D interactive game using  
Unity, applying the core game development concepts covered in the module, while integrating required  
gameplay mechanics and creative enhancement.

---

## Core Requirements

### 1. Add a health system for the player.

Your health system should

- Be displayed as a bar or number on the UI
- Decrease their health value upon collision with an obstacle  
  - If player hits an obstacle, they should then pass through the obstacle and their health should  
    be decremented.
- Stop the player only when their health value is 0
- Have a max health value
- Correctly reset to its max health value when the game is restarted

---

### 2. Add a score.

Your score should:

- Be displayed as a number on the UI
- Increment only when the player successfully goes through a hole in an obstacle
- Reset to 0 when the game is restarted

---

### 3. Add health packs.

Your health packs should:

- Be a collidable object
- Be destroyed upon collision with the player
- Be destroyed shortly after if the player goes past them  
  - The implementation for this is up to you. This can be based on a fixed time interval, after  
    the player passes the current tunnel, etc.
- Increase the health value upon collision with the player, up to a max health value

---

### 4. Add player projectile attacks.

Attacks should:

- Create a projectile on left mouse click that shoot
- The projectile is destroyed upon collision with anything and after a short duration
- The projectile is created at the player/mouse position

---

### 5. Add enemies.

Enemies should:

- Be a collidable object
- Be destroyed on collision with projectiles or the player
- Increase the score on destruction by a projectile
- Decrease the health value on collision with the player
- Be destroyed shortly after if the player goes past them  
  - Similar to the health pack, this can be implemented in a variety of ways.
- The visual representation of the enemy is up to you…it can be simple.  
  - Please make the enemy visually distinguishable from the health pack! This includes  
    different colors and shapes.
- However, many enemies spawned can be up to you, as long as it’s reasonable for the player  
  to pass them.  
  - For this assignment, it’s okay if the enemies spawn on top of the health packs (although  
    issues like these should be addressed in the final project)

---

### 6. Increase the player speed over time.

The player speed increase should:

- Get faster over time, either directly based on time, the player score, etc.  
  - It’s up to you if you want the speed to increase forever, or cap at some threshold.
- Increase at a rate that is noticeable but does not ramp up the difficulty too fast

---

### 7. Add one creative modification that is unique to your game.

Some ideas:

- Health bar decreases along a gradient of colors  
  - The gradient should constantly update according to the player’s current health (not just set  
    the color based on certain thresholds)
- Add screen shake on collision with barriers and enemies
- Color of the walls change depending on player health
- Add textures to the health packs and enemies

---

## Possible Visual Modifications

Some potential ideas for some visual modifications could be:

- Health bar decreases along a gradient of colors
- Screen flashes red when the player collides with a wall
- Some indicators that the health pack was obtained
- Color of the walls updates according to the player’s health

---

## Submission Instructions

### Windows .exe Build

- Include the .exe file and Data folder
- Must be playable on a Windows system without additional configuration

### Zipped Unity Project Folder

- Submit the full Unity project folder zipped
- Must follow a clean folder structure
- All necessary scripts, assets, and scenes should be included
- Upload to a shared Google Drive folder and share the view-only link

### Gameplay Demo Video

- Duration: Up to 6 minutes
- Demonstrate all required features (health, score, enemies, power-ups, creative feature, etc.)
- Screen recording with narration (optional but preferred)
- Use screen recording tools like OBS, Game Bar, or QuickTime
- If screen recording isn’t possible, a steady phone video may be submitted as a last resort

---

## Game Documentation (5 Marks)

Submit a PDF file including:

- Game Title
- Student Name and IT Number
- Summary of the Game Concept
- Description of how core features are implemented
- Screenshots of gameplay
- Control guide (keyboard/mouse input)
- Description of the creative feature
- Credits for any external assets/tools used
- Add the video link to a text file and include it in the Document

---

## Reference

- https://docs.unity3d.com/Manual/Quickstart2DCreate.html

---

## Assessment Rubric

| Criteria | Excellent | Good | Satisfactory | Poor | Marks |
|---------|-----------|------|--------------|------|-------|
| Health System (10 Marks) | Fully functional health UI; decreases on enemy hit; resets on restart | Mostly functional; minor UI or logic issues | Some features work; UI or reset issues present | Not functional or missing | 10 |
| Score System (10 Marks) | Score increases correctly; clear UI; resets on restart | Score increases but has minor bugs or unclear UI | Basic scoring works but lacks polish or reset | Scoring not implemented or ineffective | 10 |
| Health Packs (10 Marks) | Interactive health packs; restore health; disappear after use; visually distinct | Functional with minor visual or timing issues | Basic interaction present; no disappearance or unclear effect | Not functioning or not included | 10 |
| Projectile Attack (10 Marks) | Projectiles launch on input; hit enemies; destroy on contact; origin logic is correct | Mostly works; some bugs in direction or destruction | Basic projectile working but with major bugs | Feature not working or missing | 10 |
| Enemy Mechanics (10 Marks) | Enemies interact logically; damage player; destroyed on hit; award score; distinct appearance | Most features functional; some inconsistencies | Enemy presence is minimal or buggy | No enemies or no interaction logic | 10 |
| Speed Increment Feature (10 Marks) | Player speed scales with score/time; well-balanced and smooth | Speed changes but lacks refinement | Speed varies minimally or inconsistently | No speed variation | 10 |
| Creative Feature (10 Marks) | Innovative feature (e.g., screen shake, spell visual, particle FX); adds meaningful gameplay or visual impact | Creative element is present but basic | Attempt at creative feature made but poorly implemented | No creative addition | 10 |
| Visual Enhancements (10 Marks) | Game has polished visuals: animations, particle effects, UI design, lighting | Visuals are decent with some enhancements | Basic visuals only; minimal polish | No visual effort; raw or default assets | 10 |
| Code Quality & Stability (10 Marks) | Code is clean, well-structured, bug-free; game is stable | Some minor code inefficiencies; game mostly stable | Code is cluttered or repetitive; occasional crashes | Code is messy; unstable or unplayable | 10 |
| Demonstration & Submission (10 Marks) | Clear, complete demo video showing all features; correct format; good presentation | Demo video provided and mostly clear | Demo lacks clarity or misses some features | Missing demo or incomplete submission | 10 |
| **Total** |  |  |  |  | **100** |
