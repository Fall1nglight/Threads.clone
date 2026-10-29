<div align="center">
  <h1>Threads Clone</h1>
  <p><b>A social networking application built with ASP.NET Core and Vue, featuring text posts, personal feeds, and private profiles.</b></p>

  ![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
  ![Vue](https://img.shields.io/badge/Vue-3-4FC08D?logo=vuedotjs&logoColor=white)
  ![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)
  ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)
  ![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)
  ![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-2088FF?logo=githubactions&logoColor=white)
</div>

---

## Overview

Threads Clone lets users share posts, join conversations, and follow other accounts. It is a personal portfolio project inspired by Threads, covering backend and frontend development, database design, automated testing, and deployment workflows.

The repository contains the following applications and supporting projects:

| Project | Responsibility |
|---|---|
| [Backend](./backend/src/Threads.Api/) | REST API, authentication, database models, and migrations |
| [Frontend](./frontend/src/) | Vue application with desktop and mobile layouts |
| [Tests](./backend/tests/) | Backend unit and integration tests |
| [Workflows](./.github/workflows/) | Build checks, tests, and Azure deployment |

---

## Screenshots

<!-- Add screenshots here once the image files are included in the repository. -->

---

## Key Features

- Account registration, sign-in, password changes, and sign-out across sessions
- Text posts and comments with editing and deletion
- Post likes and activity notifications
- Global and following feeds
- User search, profiles, and editable bios
- Public and private accounts with follow request management
- User blocking and content visibility rules
- Backend email verification through Resend

---

## Deployment Workflows

GitHub Actions builds the frontend and runs backend builds and tests on pull requests. Deployment workflows target Azure Container Apps for the API and Azure Static Web Apps for the frontend after relevant changes reach `main`.

The backend includes a Dockerfile for the API and a Docker Compose configuration for local PostgreSQL.

---

## Architecture and Design Patterns

Both applications group code by feature, including authentication, posts, users, and notifications.

- **Modular endpoints:** Minimal API endpoint classes define routes, request validation, and handlers.
- **Focused startup:** `Program.cs` delegates service registration and HTTP pipeline setup to extension methods.
- **Token authentication:** JWT access tokens and rotating refresh tokens support session management and token reuse detection.
- **Content access rules:** database queries account for private profiles, accepted follows, and blocked users.
- **Cursor pagination:** feeds and lists load records in batches using a date and ID cursor.

---

## Tech Stack

| Area | Technology |
|---|---|
| Backend | C#, .NET 10, ASP.NET Core Minimal APIs |
| Frontend | Vue 3, TypeScript, Pinia, Vue Router, Vite |
| Database | PostgreSQL, Entity Framework Core |
| Authentication | ASP.NET Core Identity, JWT |
| Validation | FluentValidation, Joi |
| Logging and email | Serilog, Resend |
| Testing | xUnit, FluentAssertions, Testcontainers |
| Deployment | Docker, GitHub Actions, Azure Container Apps, Azure Static Web Apps |

---

## Developer Workflows

Build and test the backend from the repository root. Integration tests require Docker for their PostgreSQL containers.

```powershell
dotnet build backend/Threads.sln
dotnet test backend/Threads.sln --verbosity minimal
```

Install frontend dependencies and run the development server:

```powershell
cd frontend
npm ci
npm run dev
```

Run `npm run build` from `frontend/` to type-check and build the production bundle. The development server requires a configured, running backend for API features.
