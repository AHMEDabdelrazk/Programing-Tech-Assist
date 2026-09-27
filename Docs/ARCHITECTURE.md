# Programming Tech Assist System Architecture & Technical Design

## 1. Executive Summary

Programming Tech Assist is an end-to-end full-stack learning roadmap tracking platform built with **ASP.NET Core 8 Web API** and **React 19 (Vite)**. It provides structured career roadmaps across diverse software development tracks, featuring secure JWT-based authentication, per-topic user progress tracking with server-authoritative weighted scoring, thread-safe JSON file persistence, and containerized deployment.

---

## 2. High-Level Architecture Diagram

```
+-------------------------------------------------------------------------+
|                                Client Tier                              |
|                                                                         |
|   +-----------------------------------------------------------------+   |
|   |                         React 19 + Vite                         |   |
|   |  - Auth Pages (Login / Register)                                |   |
|   |  - Protected Dashboard & Dynamic Career Roadmaps                |   |
|   |  - Real-Time Mastery Tracker & Interactive Topic Checklists     |   |
|   |  - Axios HTTP Client with JWT Bearer Interceptor & 401 Handler  |   |
|   +-----------------------------------------------------------------+   |
+------------------------------------+------------------------------------+
                                     |
                         HTTPS / JSON REST APIs
                         (Bearer JWT Header)
                                     v
+-------------------------------------------------------------------------+
|                               API Gateway                               |
|                                                                         |
|   +---------------------+   +---------------------+   +---------------+ |
|   |   CORS Middleware   |-->| JWT Bearer Auth MW  |-->| Routing / DTO | |
|   |   (Cross-Origin)    |   | (Claims Validation) |   | Validation    | |
|   +---------------------+   +---------------------+   +---------------+ |
+------------------------------------+------------------------------------+
                                     v
+-------------------------------------------------------------------------+
|                             Controller Tier                             |
|                                                                         |
|   +----------------------+  +---------------------+  +----------------+ |
|   |   AuthController     |  | ProgressController  |  |RoadmapController| |
|   |  - /api/auth/register|  | - /api/progress     |  | - /api/roadmap | |
|   |  - /api/auth/login   |  | - /api/progress/stat|  | - /category    | |
|   |  - /api/auth/me      |  | - /api/progress/{id}|  | - /tech/{name} | |
|   +----------+-----------+  +----------+----------+  +-------+--------+ |
+--------------|-------------------------|---------------------|----------+
               v                         v                     v
+-------------------------------------------------------------------------+
|                              Service Tier                               |
|                                                                         |
|   +-------------------+  +--------------------+  +--------------------+ |
|   |   IAuthService    |  |  IProgressService  |  |  IRoadmapService   | |
|   |  - User validation|  |  - Topic score formula |  |  - Category filter | |
|   |  - BCrypt hashing |  |  - Aggregation     |  |  - Level traversal | |
|   |  - JWT issuance   |  |  - User isolation  |  |                    | |
|   +---------+---------+  +---------+----------+  +---------+----------+ |
|             |                      |                       |            |
|             +----------------------+-----------------------+            |
|                                    v                                    |
|   +-------------------------------------------------------------------+ |
|   |              IJsonStorageService (JsonStorageService)             | |
|   |  - ReaderWriterLockSlim Concurrency Synchronization               | |
|   |  - Atomic Write Operations (Temp File Swap)                       | |
|   |  - Legacy Plaintext Password Auto-Migration                       | |
|   +--------------------------------+----------------------------------+ |
+------------------------------------+------------------------------------+
                                     v
+-------------------------------------------------------------------------+
|                            Persistence Tier                             |
|                                                                         |
|      +------------------+  +-------------------+  +------------------+  |
|      |    users.json    |  |   progress.json   |  |  roadmaps.json   |  |
|      +------------------+  +-------------------+  +------------------+  |
+-------------------------------------------------------------------------+
```

---

## 3. Core Architectural Decisions

### 3.1 Authentication & Security Model

- **JSON Web Tokens (JWT)**: Standard HMAC-SHA256 bearer tokens containing `sub`, `email`, `unique_name`, `nameid`, and `jti` claims.
- **Salted Password Hashing**: Implemented with **BCrypt.Net-Next** (work factor 11). Plaintext passwords are never stored. Legacy unhashed records are automatically detected and migrated to BCrypt hashes upon first launch.
- **Defense-in-Depth Authorization**: The `[Authorize]` attribute guards all user progress mutations and personal metric endpoints. The server extracts the authenticated username directly from the verified JWT claims, preventing user impersonation.
- **Data Transfer Objects (DTOs)**: Explicit DTOs decouple external HTTP contracts from internal storage entities to prevent over-posting and accidental password hash leakage.

### 3.2 Authoritative Business Logic (Score Calculation)

Progress mastery scores are computed authoritatively on the server inside `ProgressService.cs` rather than trusting client-provided numbers. Each completed topic receives a share of its level weight, then project evidence is added:

$$\text{Score} = \sum_{level}\left(\frac{\text{completed topics in level}}{\text{topics in level}} \times \text{level weight}\right) + \min(\text{Projects} \times 5, 20)$$

The level weights are Basics **20%**, Intermediate **30%**, and Advanced **30%**. The maximum total is **100%**, and the result is clamped on the server.

The persisted `UserProgress.TopicProgress` collection stores the level, topic name, and completion state for every roadmap topic. Legacy section booleans remain readable for older records.

### 3.3 Concurrency & Storage Integrity

- **Thread-Safety**: Read/write concurrency is coordinated using individual `ReaderWriterLockSlim` instances for `users.json`, `progress.json`, and `roadmaps.json`. Multiple simultaneous readers are permitted while write operations execute with exclusive access.
- **Atomic Writes**: Data is serialized and written to a `.tmp` file before an atomic file swap (`File.Replace` / `File.Move`), eliminating file corruption risks during sudden process termination.

### 3.4 Automated Test Suite

- Comprehensive xUnit test suite (`Backend.Tests`) covers:
  - Password hashing and verification algorithms.
  - JWT creation and claim validation.
  - Progress score calculation boundary conditions.
  - User progress metrics and summary calculations.
  - Auth registration, duplicate checks, and credential validation.
