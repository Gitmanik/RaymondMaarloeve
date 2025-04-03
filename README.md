# Raymond Maarloeve sp. z o.o.

<table>
<tr>
<td>
<img src="Documents/Screenshots/Game.png" alt="Screenshot" width="600"/>
</td>
<td>
<img src="Documents/Screenshots/GameInteraction.png" alt="Screenshot" width="600"/>
</td>
</tr>
</table>

## Introduction

### Description
The project we are working on is a computer game in which artificial intelligence (AI) plays a key role. Its main feature is a dynamic world where non-player characters (NPCs) have their own personalities, daily schedules, and the ability to react to player actions. Thanks to AI, each gameplay session is unique, and NPC behavior influences both the storyline and the player's decisions.

What sets this game apart is the lack of traditional, rigid scripting for events. Instead, the game world develops organically, and interactions between NPCs and the player determine the course of the detective investigation, which is the central element of the gameplay. Procedurally generated environments and complex NPC decision-making systems make each game session a unique experience.

### Technology Stack
- **Game Engine:** Unity 6 (6000.0.38f1) + C#
- **Artificial Intelligence:** Large Language Model (LLM), pathfinding with NavMesh
- **Graphics:** 
  - [Kenney Fantasy UI](https://kenney.nl/assets/fantasy-ui-borders) 
  - [Free Medieval 3D People Low Poly Pack](https://free-game-assets.itch.io/free-medieval-3d-people-low-poly-pack)
  - [CC0 Terrain Textures](https://opengameart.org/content/cc0-terrain-textures)
  - Free assets ([Itch.io Free Assets](https://itch.io/game-assets/free/tag-isometric))
- **Target Platform**: Currently developed for ***Windows***. Linux support is a potential goal for future development stages.

### Team
- **Leader:** Cyprian Zasada
- **Deputy Leader:** Marek Nijakowski
- **Team Members:**
  - Paweł Reich
  - Paweł Dolak
  - Maciej Pitucha
  - Maciej Włudarski
  - Karol Rzepiński
  - Kamil Włodarczyk
  - Łukasz Jastrzębski
  - Michał Eisler
  - Łukasz Czarzasty 

---

## Assumptions

### Gameplay
- **Game Objective:**
  The player takes on the role of a detective investigating a murder case in a small NPC community. The investigation is based on analyzing clues and conversing with AI-controlled NPCs. In the end, the player reconstructs the sequence of events, determining their success or failure.
- **Success Metrics:**
  - The game allows free world exploration and interactions with NPCs.
  - There are at least two sources of clues (e.g., NPC conversations + physical evidence).
  - The player can present their theory in the game's finale through an interactive sequence-building system.

### NPC Characters
- **Behavior:**
  NPCs have unique personalities, daily schedules, and the ability to dynamically react to player actions.
- **Success Metrics:**
  - Minimum number of NPCs: 6, target: 10.
  - NPCs generate responses and make decisions using LLM.
  - NPCs can dynamically change their routes in response to player interactions (eventually, also in response to interactions with other NPCs).

### Map
- **Appearance:**
  Each NPC in the game has their own home, and their placement is procedurally generated.

---

## Execution

### Task Distribution
#### Unity:
- Paweł Reich
- Marek Nijakowski
- Paweł Dolak

#### LLM:
- Maciej Pitucha
- Maciej Włudarski
- Karol Rzepiński
- Kamil Włodarczyk
- Łukasz Jastrzębski
- Michał Eisler
- Łukasz Czarzasty

### Milestones
#### 1st Milestone - Game Prototype (without LLM)
- A game base exists where characters move around.
- NPC actions are taken randomly or follow predefined patterns.

#### 2nd Milestone - LLM Integration with Unity
- A day and night system is introduced.
- The dataset contains 50% of the intended prompts.

#### 3rd Milestone - Final Touches
- Main menu
- Soundtrack
- Credits
- Fine-tuning of the LLM model completed.

### Task Schedule
📌 [Gantt Schedule](https://docs.google.com/spreadsheets/d/1uFGMCmiO6wAubyI_MKR1ynXz4QdD-30tejBS1lcy7w8/edit?usp=sharing)
