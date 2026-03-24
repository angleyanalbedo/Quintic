# Quintic (图洛)

Quintic is a VDI 2143 compliant cam profile generator.

## Architecture

- **Frontend (web-client):** React + TypeScript + Vite application for visualization and interaction.
- **Backend (quintic):** Python + FastAPI application implementing the mathematical kernel.

## Getting Started

### 1. Backend (Python)

Navigate to the `quintic` directory and start the server:

```bash
cd quintic
uv run uvicorn api.main:app --reload
```
The API will be available at `http://localhost:8000`.

### 2. Frontend (Web Client)

Navigate to the `web-client` directory and start the development server:

```bash
cd web-client
npm install
npm run dev
```
Open `http://localhost:5173` in your browser.
