# NeuroViz

NeuroViz is an neuroscience visualization platform for the XREAL Air2 Ultra. The system enables researchers, educators, and students to explore a brain model in an immersive environment.

## Installation

Visit the [Releases](https://github.com/NTNU-RIT-AR/NeuroViz/releases/) page to download the latest version of NeuroViz.

- For the XREAL Air2 Ultra glasses, the `.apk` can be downloaded and installed directly on the device.
- For the controller application, download the correct installer for your operating system (`.dmg` for macOS, `.exe` for Windows, or `.AppImage` for Linux) and follow the installation instructions.

## Project Structure

```
NeuroViz/
├── desktop/             # Desktop application based on Tauri
│   ├── core/            # Core application logic
│   ├── tauri/           # Tauri frontend integration
│   └── python/          # Python integration
├── mobile/              # Mobile AR application
│   └── Assets/          # Application assets
├── api-spec/            # API specifications using TypeSpec
│   └── main.tsp         # Source code for the API specification
└── models/              # 3D models including Brain.blend
```

### Components

#### Desktop Application

The desktop application is built using Tauri, combining Rust for backend performance with web technologies for the frontend. It also provides a Python interface for programatically interacting with the mobile AR application.

#### Mobile AR Application

The mobile component is built with Unity.

#### API Specification

The API specification is written in TypeSpec and generates OpenAPI documentation, defining the interfaces for communication between the Desktop Application and the Mobile Application.

## Development

### Prerequisites

- Rust and Cargo (https://www.rust-lang.org/tools/install)
- Node.js and npm (https://nodejs.org/)
- Python 3.x with pip (https://www.python.org/downloads/)
- Unity Hub for mobile development (https://unity.com/download)

#### Desktop Application

1. Clone the repository:

```bash
git clone https://github.com/NTNU-RIT-AR/NeuroViz.git
cd NeuroViz
```

2. Install dependencies and run the application in development mode:

```bash
cd desktop/tauri
npm install
npm run tauri dev
```

3. Build the application for release:

```bash
npm run tauri build
```

#### Mobile AR Application

1. Open the mobile project in Unity:

   - Launch Unity Hub
   - Add the `NeuroViz/mobile` directory as a project
   - Open the project

2. Build for your target platform:
   - Select File > Build Settings
   - Choose your target platform (iOS or Android)
   - Click Build and Run
