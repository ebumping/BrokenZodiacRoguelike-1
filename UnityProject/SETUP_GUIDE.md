# CODEX OF THE BROKEN ZODIAC - SETUP GUIDE

## System Requirements

- **Operating System**: Windows 10/11, macOS 12+, or Linux with compatible graphics drivers
- **CPU**: Intel Core i5-6600 / AMD Ryzen 5 1600 or better
- **RAM**: 8GB minimum, 16GB recommended
- **GPU**: NVIDIA GeForce GTX 1060 / AMD Radeon RX 580 or better
- **Storage**: 2GB available space
- **Software**: Unity 2022.3 LTS or newer

## Setting Up The Project

### 1. Install Unity

1. Download and install Unity Hub from the [official Unity website](https://unity.com/download)
2. Install Unity 2022.3 LTS through Unity Hub
3. Make sure to include the following modules during installation:
   - Microsoft Visual Studio Community (or your preferred code editor)
   - Universal Render Pipeline components
   - 2D feature set

### 2. Clone The Repository

```bash
git clone [repository-url]
cd codex-of-the-broken-zodiac
```

### 3. Open The Project

1. Open Unity Hub
2. Click "Add" and browse to the cloned UnityProject directory
3. Select the project and open it with Unity 2022.3 LTS
4. Wait for Unity to import and process all assets (this may take a few minutes on first load)

### 4. Play The Game

1. In the Unity Editor, open the Scene "MainMenu" located in Assets/Scenes/
2. Click the Play button at the top center of the Unity Editor
3. The game should start running in the editor

### 5. Building The Game (Optional)

1. Go to File > Build Settings
2. Ensure all necessary scenes are added to the build
3. Select your target platform (Windows/Mac/Linux)
4. Click "Build" and choose a destination folder
5. Once built, you can run the executable directly

## Troubleshooting

### Common Issues

1. **Missing packages or compilation errors**:
   - Go to Window > Package Manager
   - Ensure all required packages are installed (URP, Input System, 2D packages)
   - Click "Resolve" if any dependency issues appear

2. **Shader compilation issues**:
   - Go to Edit > Project Settings > Graphics
   - Make sure Universal Render Pipeline is selected as the Scriptable Render Pipeline

3. **Performance issues**:
   - Lower the quality settings in Edit > Project Settings > Quality
   - Disable post-processing effects for better performance

### Contact Support

If you encounter any issues not covered in this guide, please contact:

- GitHub Issues: [repository-issues-url]
- Email: [support-email]

## Development Guidelines

If you plan to contribute to the project, please refer to the CONTRIBUTING.md file for coding standards and workflow procedures.

## License

This project is licensed under the terms specified in the LICENSE file included in the repository.