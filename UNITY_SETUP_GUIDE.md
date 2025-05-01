# Unity Setup Guide for "Codex of the Broken Zodiac"

## Introduction

This guide will help you set up the Unity project for "Codex of the Broken Zodiac" after the conversion from Godot. Follow these steps carefully to ensure all systems work correctly.

## Requirements

- Unity 2022.3 LTS or newer (recommended: 2022.3.20f1)
- .NET SDK 6.0 or newer
- Visual Studio 2022 or JetBrains Rider for C# development

## Installation Steps

### 1. Install Unity Hub

1. Download and install [Unity Hub](https://unity.com/download)
2. Launch Unity Hub and sign in with your Unity account

### 2. Install Unity 2022.3 LTS

1. In Unity Hub, go to the "Installs" tab
2. Click "Add" and select Unity 2022.3 LTS
3. Make sure to include the following modules during installation:
   - Microsoft Visual Studio Community (or your preferred IDE)
   - Universal RP
   - Input System
   - WebGL Build Support (if needed for web deployment)

### 3. Import the Project

1. Clone or download this repository
2. In Unity Hub, click "Open" and select the "UnityProject" folder from this repository
3. Unity will import and set up the project

### 4. Install Required Packages

Once the project is open, you need to install the required packages if they aren't already included:

1. Go to Window > Package Manager
2. Click the "+" button > "Add package by name..."
3. Add these packages one by one:
   - `com.unity.textmeshpro` (version 3.0.6 or newer)
   - `com.unity.ugui` (version 1.0.0 or newer)
   - `com.unity.render-pipelines.universal` (version 14.0.8 or newer)
   - `com.unity.inputsystem` (version 1.6.3 or newer)

Alternatively, we've included a `manifest.json` file in the "UnityProject/Packages" folder that should automatically install these dependencies.

## Project Structure

The project is organized as follows:

- `/UnityProject/Assets/Scripts/Core/` - Core game systems (PlayerController, SanitySystem, etc.)
- `/UnityProject/Assets/Scripts/UI/` - UI components and controllers
- `/UnityProject/Assets/Scripts/Resources/` - Scriptable objects and resource definitions
- `/UnityProject/Assets/Scripts/Environment/` - Room generation and environment scripts
- `/UnityProject/Assets/Scripts/AI/` - Enemy AI and behavior

## Resolving Common Issues

### Missing References to UI Components

If you encounter errors like "The type or namespace name 'UI' does not exist in the namespace 'UnityEngine'":

1. Add `using UnityEngine.UI;` to the top of your script
2. Ensure the Unity UI package is installed

### Missing TextMeshPro References

If you see errors about missing TMPro or TextMeshProUGUI:

1. Add `using TMPro;` to the top of your script
2. Make sure TextMeshPro package is installed

### Partial Class Errors

For errors about missing partial modifiers, add the `partial` keyword to the class declaration that shows the error.

## Key Features Implemented

1. **Sanity System**: The core mechanic affecting player perception and abilities (SanitySystem.cs)
2. **Zodiac Transformations**: Sign-specific transformations based on sanity levels (ZodiacSanityTransformation.cs)
3. **Procedural Horror Events**: Dynamic events triggered by sanity thresholds (HorrorEventGenerator.cs)
4. **Tarot Card System**: Card selection and effects system (CardSelectorController.cs)
5. **Destructible Environments**: Secret walls and hidden passages
6. **Spell Crafting System**: Element combination mechanics with zodiac resonance

## Running the Game

1. Open the project in Unity
2. Navigate to Assets > Scenes
3. Open the "MainMenu" scene
4. Click the Play button at the top of the Unity Editor

## Need Help?

If you encounter any issues not covered in this guide:

1. Check the troubleshooting section in COMPILATION_FIXES.md
2. Refer to the UNITY_PACKAGE_GUIDE.md for package-specific issues
3. Check for missing namespaces as described in the code comments

## Credits

Codex of the Broken Zodiac - An advanced isometric roguelike shooter with innovative player transformation mechanics through a unique zodiac-driven sanity system.