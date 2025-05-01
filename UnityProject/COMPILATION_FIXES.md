# Unity Compilation Errors and Solutions

## Overview

This document explains the compilation errors you encountered and provides solutions to fix them. The main issues are related to missing references to UI components and namespaces, which can be resolved by adding the appropriate using directives and ensuring required packages are installed.

## Common Error Types

### 1. Missing UnityEngine.UI Namespace

**Error:**
```
Assets\Scripts\UI\CardSelectorController.cs(4,19): error CS0234: The type or namespace name 'UI' does not exist in the namespace 'UnityEngine' (are you missing an assembly reference?)
```

**Solution:**
This error occurs because the script is trying to use UI components (like Button, Image, etc.) without the proper namespace reference. Add the following line at the top of your script:

```csharp
using UnityEngine.UI;
```

### 2. Missing TextMeshPro Namespace

**Error:**
```
Assets\Scripts\UI\CardSelectorController.cs(5,7): error CS0246: The type or namespace name 'TMPro' could not be found (are you missing a using directive or an assembly reference?)
```

**Solution:**
Add the TMPro namespace to use TextMeshPro components:

```csharp
using TMPro;
```

### 3. Missing Type References

**Error:**
```
Assets\Scripts\Resources\ZodiacSignil.cs(73,29): error CS0246: The type or namespace name 'ZodiacSign' could not be found (are you missing a using directive or an assembly reference?)
```

**Solution:**
This error occurs because the script is trying to use custom types that are either not defined or not in the correct namespace. Ensure that:

1. The ZodiacSign class exists in your project
2. You're using the correct namespace

```csharp
using CodexBrokenZodiac; // Add your custom namespace
```

### 4. Partial Class Issue

**Error:**
```
Assets\Scripts\Core\PlayerController.cs(9,18): error CS0260: Missing partial modifier on declaration of type 'PlayerController'; another partial declaration of this type exists
```

**Solution:**
Add the `partial` modifier to the class definition:

```csharp
public partial class PlayerController : MonoBehaviour
{
    // Class implementation
}
```

## Unity Package References

To fix most of these errors, ensure you have the necessary Unity packages installed. We've created a `manifest.json` file in the Packages folder with all required dependencies:

- TextMeshPro (for TMPro)
- Unity UI (for UI components)
- Universal Render Pipeline (for post-processing effects)
- Input System (for modern input handling)

## Example Fixed Files

We've created example fixed files for the most common issues:

1. `Example_CardSelectorController.cs` - Shows how to properly reference UI components and TMPro
2. `SanityEffectsManager.cs` - Implements the missing manager referenced in SanitySystem.cs

## Fixing All Files

To fix all the compilation errors, you'll need to:

1. Add the missing using directives to all UI scripts
2. Implement or import the missing classes (ZodiacSign, ProjectileController, etc.)
3. Update any partial class declarations
4. Ensure proper namespace usage throughout the project

## Assembly Definition References

If your project uses Assembly Definition files (.asmdef), make sure they have the correct references to Unity packages:

```json
{
    "name": "YourGameAssembly",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem",
        "Unity.RenderPipelines.Universal",
        "Unity.RenderPipelines.Universal.Runtime",
        "Unity.RenderPipelines.Core.Runtime"
    ]
}
```

## Unity Project Settings

In some cases, you may need to update your project settings to ensure compatibility:

1. Go to Edit > Project Settings > Player
2. Under "Other Settings" make sure "API Compatibility Level" is set to ".NET Standard 2.1"
3. Under "Player" make sure "Scripting Backend" is set to "Mono" or "IL2CPP" based on your needs

## Additional Resources

Further details and solutions can be found in the following files:

- `UNITY_PACKAGE_GUIDE.md` - Comprehensive guide to required Unity packages
- `SETUP_GUIDE.md` - Complete project setup instructions

If you continue to experience issues after implementing these fixes, please review the Unity documentation or ask for additional support.