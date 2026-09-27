# Programming Tech Assist Cloud Deployment Guide

This guide details how to deploy the Programming Tech Assist full-stack platform using free cloud services:

1. **Backend**: ASP.NET Core 8 Web API deployed to **Render** (or Railway / Fly.io).
2. **Frontend**: React (Vite) Single-Page Application deployed to **Vercel** (or Netlify / Cloudflare Pages).
3. **Local / Self-Hosted**: Multi-container **Docker Compose** deployment.

---

## 1. Backend Deployment (Render Web Service)

1. Sign in to [Render](https://render.com/).
2. Create a new **Web Service** and link this GitHub repository.
3. Configure the service settings:
   - **Root Directory**: `Backend/Backend`
   - **Runtime**: `Docker` (or Environment: `.NET`)
   - **Docker Command**: Automatically detected from `Dockerfile`
   - **Port**: `8080`
4. Set Environment Variables:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ASPNETCORE_URLS`: `http://+:8080`
   - `Jwt__SecretKey`: `<Generate-a-secure-64-character-random-key>`
   - `Jwt__Issuer`: `ProgrammingTechAssistAPI`
   - `Jwt__Audience`: `ProgrammingTechAssistClient`
   - `Jwt__ExpiresInMinutes`: `1440`
5. Click **Deploy**. Note your service URL (e.g. `https://techroad-api.onrender.com`).

---

## 2. Frontend Deployment (Vercel)

1. Sign in to [Vercel](https://vercel.com/).
2. Select **Add New Project** and import the repository.
3. Configure Project Settings:
   - **Root Directory**: `Frontend/techroad-client`
   - **Framework Preset**: `Vite`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist`
4. Add Environment Variables:
   - `VITE_API_URL`: `https://your-backend-api.onrender.com/api`
5. Click **Deploy**. Vercel will build the SPA and provide a live URL with free global CDN and HTTPS.

---

## 3. Local Full-Stack Deployment with Docker Compose

To run the entire system locally with one command:

```bash
export JWT_SECRET_KEY="replace-with-a-long-random-secret"
docker compose up --build
```

On PowerShell, use `$env:JWT_SECRET_KEY = "replace-with-a-long-random-secret"` before running Compose. The signing key is required in production and is intentionally not committed to the repository.

- Frontend is accessible at: `http://localhost:3000`
- Backend Web API is accessible at: `http://localhost:5000`
- Interactive Swagger UI: `http://localhost:5000/swagger`
