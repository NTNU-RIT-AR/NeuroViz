# NeuroViz

NeuroViz is an neuroscience visualization platform for the XREAL Air2 Ultra. The system enables researchers, educators, and students to explore a brain model in an immersive environment.

## Project Structure

```
NeuroViz/
├── api-spec/            # API specifications using TypeSpec
│   ├── main.tsp         # Source code for the API specification
├── desktop/             # Desktop application based on Tauri
│   ├── neuroviz/        # Core application logic
│   ├── python/          # Python integration
│   └── tauri/           # Tauri frontend integration
├── mobile/              # Mobile AR application
│   └── Assets/          # Application assets
└── models/              # 3D models including Brain.blend
```

## Components

### Desktop Application

The desktop application is built using Tauri, combining Rust for backend performance with web technologies for the frontend. It also provides a Python interface for programatically interacting with the mobile AR application.

### Mobile AR Application

The mobile component is built with Unity.

### API Specification

The API specification is written in TypeSpec and generates OpenAPI documentation, defining the interfaces for communication  between the Desktop Application and the Mobile Application.

## Technology Stack

- **Backend**: Tauri with Rust
- **Frontend**: React
- **Mobile**: Unity
- **API Documentation**: TypeSpec, OpenAPI, Swagger UI
- **3D Modeling**: Blender

## Development

### Prerequisites

- Rust and Cargo (https://www.rust-lang.org/tools/install)
- Node.js and npm (https://nodejs.org/)
- Python 3.x with pip (https://www.python.org/downloads/)
- Unity 2022.3 or later (for mobile development)
- Blender 3.x or later (for model editing)
- Nix package manager (optional, for development environment)

#### Desktop Application

1. Clone the repository:
```
git clone https://github.com/NTNU-RIT-AR/NeuroViz.git
cd NeuroViz
```

2. Install dependencies and build the desktop application:
```
cd desktop/tauri
npm install
npm run tauri build
```

3. Run the desktop application in development mode:
```
npm run tauri dev
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
