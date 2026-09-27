# Programming Tech Assist — Learning Roadmap & Skill Tracker

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-8-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![JWT](https://img.shields.io/badge/JWT-Bearer_Auth-black?logo=jsonwebtokens)](https://jwt.io/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

**Programming Tech Assist** is a full-stack learning platform engineered to help developers navigate career paths, master software technologies, and track real-time milestone progress with automated score evaluation.

Built with an **ASP.NET Core 8 Web API** backend and a modern **React 19 (Vite)** single-page application, the project features industry-standard **JWT Bearer authentication**, **salted BCrypt password hashing**, **thread-safe JSON persistence**, and a comprehensive **automated xUnit test suite**.

---

## 🌟 Key Features

- 🔐 **Secure JWT Authentication**: Stateless HMAC-SHA256 authentication tokens with claims validation, protected routes, and Axios interceptors.
- 🛡️ **Industry-Standard Security**: Salted **BCrypt** password hashing, DTO boundary isolation, and user-scoped data access.
- 📊 **Server-Authoritative Topic Scoring**: Every roadmap topic is persisted per user and scored by level: Basics contributes 20%, Intermediate 30%, Advanced 30%, distributed evenly across the topics in each level. Projects contribute 5% each up to 20%.
- 📈 **Real-Time Dynamic Metrics**: Dynamic KPIs displaying tracked technologies, average score, mastered roadmaps, and completed project counters.
- 🧵 **Thread-Safe Storage**: JSON persistence stores a user/technology progress record with every topic completion, using `ReaderWriterLockSlim` for concurrent readers and atomic write file swaps.
- 🧪 **Automated Testing**: Comprehensive **xUnit** test suite with **Moq** covering authentication, boundary condition calculations, and metrics aggregation.
- 🐳 **Containerized & Deployment Ready**: Multi-stage Dockerfiles for backend and frontend orchestrated via `docker-compose.yml`.

---

## 🛠️ Tech Stack & Architecture

| Layer             | Technology                              | Purpose                                                                            |
| :---------------- | :-------------------------------------- | :--------------------------------------------------------------------------------- |
| **Backend API**   | ASP.NET Core 8 Web API, C#              | High-performance RESTful API endpoints and business logic                          |
| **Security**      | `JwtBearer`, `BCrypt.Net-Next`          | Authentication, token generation, claims verification, and salted password hashing |
| **Storage**       | System.Text.Json + ReaderWriterLockSlim | Concurrency-safe, atomic file-based persistence                                    |
| **Documentation** | Swagger / OpenAPI 3.0                   | Interactive API documentation with Bearer Auth support                             |
| **Testing**       | xUnit, Moq                              | Unit tests and regression verification                                             |
| **Frontend SPA**  | React 19, Vite, React Router 7          | Responsive, reactive single-page user experience                                   |
| **HTTP Client**   | Axios                                   | Request/Response interceptors for JWT injection and 401 handling                   |
| **DevOps & CI**   | Docker, Docker Compose, GitHub Actions  | Containerization and automated continuous integration pipeline                     |

### System Architecture Diagram

```
[ React 18 SPA (Vite) ]
          │  ▲
   HTTPS  │  │  Axios (Bearer Token Interceptor)
   REST   ▼  │
[ ASP.NET Core 8 Web API ]
    ├─► Middleware: CORS -> JWT Bearer Authentication -> Authorization
    ├─► Controllers: AuthController, ProgressController, RoadmapController
    ├─► Services: AuthService, ProgressService, RoadmapService, JwtTokenService, PasswordHasherService
    └─► Storage: Thread-Safe JsonStorageService (Atomic File I/O)
            │
            ▼
     [ Data/users.json | Data/progress.json | Data/roadmaps.json ]
```

---

## 🚀 Quick Start (Local Setup)

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Node.js 18+](https://nodejs.org/) and npm
- [Docker](https://www.docker.com/) (optional, for containerized run)

### Method 1: Running with Docker Compose (Recommended)

Run both the frontend and backend with a single command:

```bash
docker compose up --build
```

Set a signing key before starting the stack; secrets are intentionally not committed:

```powershell
$env:JWT_SECRET_KEY = "replace-with-a-long-random-secret"
docker compose up --build
```

- **Frontend Client**: [http://localhost:3000](http://localhost:3000)
- **Backend API**: [http://localhost:5000](http://localhost:5000)
- **Interactive Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

### Method 2: Manual Development Setup

#### 1. Backend API

```bash
# Navigate to the backend directory
cd Backend/Backend

# Run the API
dotnet run
```

The API starts on `http://localhost:5000` (or `https://localhost:5001`). View API docs at `/swagger`.

#### 2. Frontend Client

```bash
# In a new terminal, navigate to the frontend directory
cd Frontend/techroad-client

# Install dependencies
npm install

# Start the Vite development server
npm run dev
```

The client will launch on `http://localhost:5173`.

---

## 🧪 Running Automated Tests

Run the backend unit test suite:

```bash
dotnet test Backend/Backend.sln
```

The automated tests cover password hashing, JWT token validation, progress boundary and topic-weighting calculations, statistics aggregation, and authentication workflows.

---

## 📚 API Reference Summary

| Method | Endpoint                       | Auth Required | Description                                                          |
| :----- | :----------------------------- | :-----------: | :------------------------------------------------------------------- |
| `POST` | `/api/auth/register`           |      No       | Register a new user account and receive JWT token                    |
| `POST` | `/api/auth/login`              |      No       | Authenticate user credentials and receive JWT token                  |
| `GET`  | `/api/auth/me`                 | Bearer Token  | Retrieve profile information and statistics for the current user     |
| `GET`  | `/api/roadmap`                 |      No       | Fetch all roadmap categories and curriculum trees                    |
| `GET`  | `/api/roadmap/{category}`      |      No       | Fetch details for a specific roadmap category                        |
| `GET`  | `/api/roadmap/tech/{techName}` |      No       | Fetch levels and topics for a specific technology                    |
| `GET`  | `/api/progress`                | Bearer Token  | Get all tracked technology progress for the authenticated user       |
| `GET`  | `/api/progress/stats`          | Bearer Token  | Get aggregated statistics (mastery average, completed roadmaps)      |
| `GET`  | `/api/progress/{technology}`   | Bearer Token  | Get progress details for a specific technology                       |
| `POST` | `/api/progress`                | Bearer Token  | Save or update progress (authoritative score calculated server-side) |

For complete documentation, see [`Docs/API_DOCUMENTATION.md`](Docs/API_DOCUMENTATION.md).

---

## 📁 Project Structure

```
TechRoadMap/
├── .github/workflows/ci.yml       # GitHub Actions CI pipeline
├── docker-compose.yml             # Orchestration for Backend and Frontend
├── README.md                      # Primary project documentation
├── Docs/                          # In-depth architectural and deployment docs
│   ├── ARCHITECTURE.md            # Architectural decisions and data flows
│   ├── API_DOCUMENTATION.md       # Full REST API endpoints and schemas
│   └── DEPLOYMENT_GUIDE.md        # Cloud deployment guide (Render, Vercel)
├── Backend/
│   ├── Backend.sln                # Visual Studio solution file
│   ├── Backend/                   # ASP.NET Core 8 Web API project
│   │   ├── Controllers/           # REST API Controllers (Auth, Progress, Roadmap)
│   │   ├── Services/              # Business logic (Auth, Progress, Token, Hash, Storage)
│   │   ├── Models/                # Data entities and DTOs
│   │   ├── Data/                  # JSON storage files (users, progress, roadmaps)
│   │   ├── Dockerfile             # Multi-stage production container build
│   │   └── Program.cs             # Application configuration & middleware pipeline
│   └── Backend.Tests/             # xUnit automated test project (27 tests)
└── Frontend/
    └── techroad-client/           # Programming Tech Assist React + Vite SPA
        ├── src/
        │   ├── components/        # Reusable UI components (Navbar, ProgressCard, Stats)
        │   ├── pages/             # Route pages (Login, Register, Dashboard)
        │   ├── services/          # Axios HTTP client with JWT interceptor
        │   └── styles/            # Responsive CSS layouts and themes
        ├── Dockerfile             # Multi-stage Vite + NGINX container build
        └── package.json           # Dependencies and build scripts
```

---

## 📄 License

This project is licensed under the MIT License.
