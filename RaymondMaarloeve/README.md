# Raymond Maarloeve - Unity

## Project Structure
- **SampleScene** – this scene serves as a sandbox. All new ideas and tests can be introduced here without worrying about affecting the final version of the game.
- **Target scene** – we plan to create a separate scene (or scenes) for the final version of the game. Details and timelines for transferring features will be determined as the project progresses.

## Collaboration Management
1. **Avoid simultaneous editing of the same elements** – if someone is working on a specific element (e.g., prefab, script), try not to modify it at the same time. This helps avoid merge conflicts.
2. **Frequent commits** – regularly push your changes so the rest of the team can see them and avoid duplicating work.
3. **Communication** – inform other team members about what you're working on. Use a task tracking system and/or group communication channel.

## UI Structure
- The `SampleScene` contains an object called `_UI` with a `Canvas` component, which is the main container for all UI elements.
- Each new UI feature should have its **own GameObject** as a child of `_UI`. This makes managing individual elements easier and helps avoid conflicts.
- Remember to use **TextMeshPro** for all UI text – it ensures better visual quality and additional styling options.

## Conventions and Best Practices
- **Naming objects**: Use clear, descriptive names that reflect the object's purpose (e.g., `MenuPanel`, `PlayButton`, `ScoreText`).
- **Prefabs**: If an object might be used in multiple scenes, save it as a prefab. This allows for easier updates and reuse.
- **Scripts**: Each new functionality should, where possible, be placed in a separate script that follows the Single Responsibility Principle.
- **Version control**: Avoid making many radical changes in a single commit. It's better to make several smaller, descriptive commits.

## Getting Started
1. Clone the repository locally.
2. Open the project in **Unity**.
3. In the Unity explorer, find the `SampleScene` and run it to view the current test elements.
4. Add your elements – keeping in mind the collaboration and convention guidelines listed above.

## Development Plan
1. Add new modules and test them in `SampleScene`.
2. Gradually move refined elements to the final scene (or create a new one for the actual game).
3. Optimize, test, and refactor the code before releasing the first official version.
