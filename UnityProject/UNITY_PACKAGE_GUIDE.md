# Unity Package Installation Guide

## Required Packages

Based on the compilation errors, the following packages need to be installed in the Unity Package Manager:

1. **TextMeshPro** - Required for all UI text components using TextMeshProUGUI
2. **Unity UI (com.unity.ugui)** - Required for UI components like Button, Image, Slider
3. **Universal Render Pipeline (com.unity.render-pipelines.universal)** - Required for advanced rendering features
4. **Input System (com.unity.inputsystem)** - Required for modern input handling

## How to Install the Required Packages

### Using the Unity Editor

1. Open your project in Unity
2. Go to **Window > Package Manager**
3. Click the + button in the top left corner
4. Select "Add package by name" or "Add package from registry"
5. Install each of the packages listed above

### Using the manifest.json File

Alternatively, you can directly edit the `manifest.json` file in your project's Packages folder:

```json
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.ugui": "1.0.0",
    "com.unity.render-pipelines.universal": "14.0.8",
    "com.unity.inputsystem": "1.6.3"
  }
}
```

## Common Compilation Errors and Fixes

### Missing UI Namespace

Error: `The type or namespace name 'UI' does not exist in the namespace 'UnityEngine'`

Fix: Add the following using directives to your script:

```csharp
using UnityEngine.UI;
```

### Missing TextMeshPro Namespace

Error: `The type or namespace name 'TMPro' could not be found`

Fix: Add the following using directives to your script:

```csharp
using TMPro;
```

### Missing Type References

Some of the errors refer to missing custom types like `ZodiacSign`, `SanityEffectsManager`, etc. Make sure all these scripts are properly included in your project and that they're in the correct namespace if applicable.

## Assembly References

If you're using assembly definitions (.asmdef files), ensure they have the correct references:

```csharp
{
    "name": "YourGameAssembly",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem",
        "Unity.RenderPipelines.Universal",
        "Unity.RenderPipelines.Universal.Runtime",
        "Unity.RenderPipelines.Core.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

## Common Issues with Type Resolution

### Partial Class Issues

Error: `Missing partial modifier on declaration of type 'PlayerController'; another partial declaration of this type exists`

Fix: Either add the `partial` modifier to the class declaration or ensure there aren't multiple declarations of the same class across different files.

```csharp
public partial class PlayerController : MonoBehaviour
{
    // Class implementation
}
```

### Missing Custom Types

Many errors refer to missing custom types like `ZodiacSign`, `SanityEffectsManager`, `WeaponData`, etc. Ensure these classes are properly defined in your project and that the namespaces are correct.

If you've created these types in your `CodexBrokenZodiac` namespace, make sure to include the proper using directive:

```csharp
using CodexBrokenZodiac;
```

## Additional Tips

1. **Circular Dependencies**: Check for circular dependencies between your scripts.
2. **Script Execution Order**: Consider setting the script execution order in Unity if scripts are dependent on each other.
3. **Clean and Rebuild**: Sometimes a full project clean and rebuild can resolve phantom reference issues.
4. **Unity Version**: Ensure you're using Unity 2022.3 LTS as specified in the design document to avoid version-specific issues.