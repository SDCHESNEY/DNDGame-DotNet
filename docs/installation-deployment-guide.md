# DNDGame Installation & Deployment Guide

Complete guide for installing, configuring, and deploying the DNDGame .NET solution across all platforms (API, Blazor Web, and MAUI Mobile).

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Environment Setup](#development-environment-setup)
3. [Configuration Management](#configuration-management)
4. [Database Setup](#database-setup)
5. [Running Locally](#running-locally)
6. [Production Deployment](#production-deployment)
7. [Docker Deployment](#docker-deployment)
8. [Mobile App Deployment](#mobile-app-deployment)
9. [Monitoring & Logging](#monitoring--logging)
10. [Security Considerations](#security-considerations)
11. [Troubleshooting](#troubleshooting)
12. [Maintenance](#maintenance)

---

## Prerequisites

### Required Software

#### For API and Blazor Web Development

| Software | Version | Purpose | Download |
|----------|---------|---------|----------|
| **.NET SDK** | 9.0+ | Framework runtime | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **Git** | Latest | Version control | [Download](https://git-scm.com/) |
| **IDE** | VS 2022 or VS Code | Development environment | [VS 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) |

#### For MAUI Mobile Development

| Software | Version | Platform | Purpose |
|----------|---------|----------|---------|
| **.NET MAUI Workload** | 9.0+ | All | Cross-platform UI framework |
| **Xcode** | 15.0+ | macOS/iOS | iOS/macOS build tools |
| **Android SDK** | API 24+ | Android | Android build tools |
| **Visual Studio 2022** | 17.8+ | Windows | Recommended IDE for MAUI |

#### Optional Tools

- **dotnet-ef**: Entity Framework Core CLI tools
- **Docker Desktop**: Container runtime for local testing
- **PostgreSQL**: Production database (optional for development)
- **Redis**: Caching and SignalR backplane (production)

### System Requirements

**Development Machine:**
- **OS**: Windows 10/11, macOS 12+, or Linux (Ubuntu 20.04+)
- **RAM**: 8GB minimum, 16GB recommended
- **Disk Space**: 10GB free space
- **CPU**: Dual-core minimum, quad-core recommended

**Production Server:**
- **OS**: Linux (Ubuntu 22.04 LTS recommended) or Windows Server 2022
- **RAM**: 4GB minimum, 8GB+ for production workloads
- **Disk Space**: 20GB+ (depends on database size)
- **CPU**: 2 cores minimum, 4+ recommended

---

## Development Environment Setup

### 1. Install .NET 9.0 SDK

#### Windows
```powershell
# Download installer from Microsoft
# https://dotnet.microsoft.com/download/dotnet/9.0

# Verify installation
dotnet --version
# Expected output: 9.0.x
```

#### macOS
```bash
# Using Homebrew
brew install --cask dotnet-sdk

# Verify installation
dotnet --version
```

#### Linux (Ubuntu)
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Verify installation
dotnet --version
```

### 2. Install Entity Framework Core Tools

```bash
# Global installation
dotnet tool install --global dotnet-ef

# Add to PATH (macOS/Linux)
export PATH="$PATH:$HOME/.dotnet/tools"

# Verify installation
dotnet ef --version
# Expected output: Entity Framework Core .NET Command-line Tools 9.0.x
```

### 3. Install MAUI Workload (For Mobile Development)

```bash
# Install MAUI workload
dotnet workload install maui

# Verify installation
dotnet workload list
# Should show: maui
```

#### Additional iOS Setup (macOS only)

```bash
# Install Xcode from App Store (15.0+)
# Install Xcode Command Line Tools
xcode-select --install

# Accept Xcode license
sudo xcodebuild -license accept

# Install iOS simulators
xcodebuild -downloadPlatform iOS
```

#### Additional Android Setup

```bash
# Install Android SDK via Visual Studio or Android Studio
# Required components:
# - Android SDK Platform 34
# - Android SDK Build-Tools 34.0.0
# - Android Emulator

# Set ANDROID_HOME environment variable (macOS/Linux)
export ANDROID_HOME=$HOME/Library/Android/sdk
export PATH=$PATH:$ANDROID_HOME/platform-tools

# Windows (PowerShell)
$env:ANDROID_HOME = "C:\Users\<username>\AppData\Local\Android\Sdk"
$env:PATH += ";$env:ANDROID_HOME\platform-tools"
```

### 4. Clone Repository

```bash
# Clone from GitHub
git clone https://github.com/SDCHESNEY/DNDGame-DotNet.git
cd DNDGame-DotNet

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Verify build succeeded (0 errors, 0 warnings expected)
```

---

## Configuration Management

### Configuration Files

The solution uses the standard ASP.NET Core configuration system with multiple configuration sources:

```
src/DNDGame.API/
├── appsettings.json                  # Base configuration
├── appsettings.Development.json      # Development overrides
├── appsettings.Production.json       # Production overrides (git-ignored)
└── appsettings.Staging.json          # Staging overrides (optional)
```

### Configuration Hierarchy

Configuration sources are loaded in this order (later sources override earlier):

1. `appsettings.json` - Base settings (committed to git)
2. `appsettings.{Environment}.json` - Environment-specific (Development committed, Production git-ignored)
3. **User Secrets** - Developer-specific secrets (development only)
4. **Environment Variables** - OS-level configuration
5. **Command Line Arguments** - Runtime overrides

### Base Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DndGame;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "LLM": {
    "Provider": "OpenAI",
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-4-turbo-preview",
    "MaxTokens": 500,
    "Temperature": 0.7,
    "StreamResponses": true
  },
  "ContentModeration": {
    "Enabled": true,
    "BlockNsfw": true,
    "BlockHarassment": true,
    "MaxInputLength": 5000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "DNDGame.Infrastructure.Services.OpenAiProvider": "Information",
      "DNDGame.Infrastructure.Services.LlmDmService": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Development Configuration (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dndgame.db"
  },
  "LLM": {
    "ApiKey": "sk-your-development-api-key"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

### User Secrets (Development Only)

**Never commit API keys or sensitive data to version control!**

#### Initialize User Secrets

```bash
cd src/DNDGame.API
dotnet user-secrets init
```

This adds a `UserSecretsId` to your `.csproj`:

```xml
<PropertyGroup>
  <UserSecretsId>aspnet-DNDGame-12345678-abcd-1234-abcd-1234567890ab</UserSecretsId>
</PropertyGroup>
```

#### Set Secrets

```bash
# OpenAI API Key
dotnet user-secrets set "LLM:ApiKey" "sk-your-actual-api-key-here"

# Database connection (if using PostgreSQL locally)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=dndgame;Username=postgres;Password=your-password"

# List all secrets
dotnet user-secrets list
```

User secrets are stored at:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- **macOS/Linux**: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

### Environment Variables

Environment variables use double underscore (`__`) as hierarchy separator:

```bash
# Unix/macOS/Linux
export LLM__ApiKey="sk-your-api-key"
export LLM__Model="gpt-4-turbo-preview"
export LLM__MaxTokens="500"
export ConnectionStrings__DefaultConnection="Host=localhost;Database=dndgame;Username=postgres;Password=secret"

# Windows PowerShell
$env:LLM__ApiKey = "sk-your-api-key"
$env:LLM__Model = "gpt-4-turbo-preview"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=dndgame;Username=postgres;Password=secret"

# Windows Command Prompt
set LLM__ApiKey=sk-your-api-key
set LLM__Model=gpt-4-turbo-preview
```

### Configuration Options Reference

#### Connection Strings

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `DefaultConnection` | string | SQLite path | Database connection string |

**SQLite (Development)**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=dndgame.db"
}
```

**PostgreSQL (Production)**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=postgres.example.com;Port=5432;Database=dndgame;Username=dndgame_user;Password=<strong-password>;SSL Mode=Require"
}
```

**SQL Server**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=sql.example.com;Database=dndgame;User Id=dndgame_user;Password=<strong-password>;TrustServerCertificate=False;Encrypt=True"
}
```

#### LLM Settings

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Provider` | string | `"OpenAI"` | LLM provider (currently only OpenAI) |
| `ApiKey` | string | *required* | OpenAI API key (starts with `sk-`) |
| `Model` | string | `"gpt-4-turbo-preview"` | Model to use (gpt-4-turbo-preview, gpt-3.5-turbo) |
| `MaxTokens` | int | `500` | Maximum tokens per response (100-4000) |
| `Temperature` | float | `0.7` | Response randomness (0.0-2.0) |
| `StreamResponses` | bool | `true` | Enable SSE streaming |

**Cost Optimization**:
- Use `gpt-3.5-turbo` for 20x cost savings
- Reduce `MaxTokens` to 300 for shorter responses
- Set `Temperature` to 0.5 for more deterministic outputs

#### Content Moderation Settings

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `true` | Enable content moderation |
| `BlockNsfw` | bool | `true` | Block NSFW content |
| `BlockHarassment` | bool | `true` | Block harassment keywords |
| `MaxInputLength` | int | `5000` | Maximum input characters |

**⚠️ Never disable moderation in production!**

#### SignalR Settings (Optional)

```json
{
  "SignalR": {
    "MaximumReceiveMessageSize": 102400,
    "StreamBufferCapacity": 10,
    "EnableDetailedErrors": false
  }
}
```

#### Logging Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
      "DNDGame.Infrastructure.Services": "Information"
    }
  }
}
```

**Log Levels**: Trace, Debug, Information, Warning, Error, Critical, None

### Production Configuration

**Create `appsettings.Production.json`** (add to `.gitignore`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-postgres.internal;Database=dndgame;Username=dndgame_app;Password=<use-env-var>;SSL Mode=Require"
  },
  "LLM": {
    "ApiKey": "<use-env-var>",
    "Model": "gpt-4-turbo-preview",
    "MaxTokens": 500,
    "Temperature": 0.7
  },
  "ContentModeration": {
    "Enabled": true,
    "BlockNsfw": true,
    "BlockHarassment": true,
    "MaxInputLength": 5000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "DNDGame": "Information"
    }
  },
  "AllowedHosts": "dndgame.com,www.dndgame.com"
}
```

**Security Best Practices**:
- ✅ Use environment variables or key vaults for secrets
- ✅ Set `AllowedHosts` to specific domains
- ✅ Enable SSL/TLS for database connections
- ✅ Use strong, unique passwords (16+ characters)
- ✅ Rotate API keys regularly (90 days)
- ❌ Never commit production credentials to git
- ❌ Never use default or weak passwords

---

## Database Setup

### Development Database (SQLite)

SQLite is used by default in development for simplicity and portability.

#### Automatic Setup

```bash
cd src/DNDGame.API

# Apply migrations (creates dndgame.db)
dotnet ef database update

# Database file location: src/DNDGame.API/dndgame.db
```

#### Manual Setup

```bash
# Create migration
dotnet ef migrations add InitialCreate --project ../DNDGame.Infrastructure --startup-project .

# Apply migration
dotnet ef database update --project ../DNDGame.Infrastructure --startup-project .

# Verify database created
ls -lh dndgame.db
```

#### SQLite Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dndgame.db"
  }
}
```

### Production Database (PostgreSQL)

PostgreSQL is recommended for production due to better concurrency, performance, and feature support.

#### Install PostgreSQL

**Ubuntu/Debian**:
```bash
sudo apt-get update
sudo apt-get install -y postgresql postgresql-contrib

# Start PostgreSQL
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

**macOS**:
```bash
brew install postgresql@15
brew services start postgresql@15
```

**Docker**:
```bash
docker run --name dndgame-postgres \
  -e POSTGRES_PASSWORD=your-password \
  -e POSTGRES_DB=dndgame \
  -p 5432:5432 \
  -v postgres-data:/var/lib/postgresql/data \
  -d postgres:15-alpine
```

#### Create Database and User

```bash
# Connect to PostgreSQL as superuser
sudo -u postgres psql

# Create database
CREATE DATABASE dndgame;

# Create application user
CREATE USER dndgame_app WITH ENCRYPTED PASSWORD 'your-strong-password';

# Grant privileges
GRANT ALL PRIVILEGES ON DATABASE dndgame TO dndgame_app;

# Exit psql
\q
```

#### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dndgame;Username=dndgame_app;Password=your-strong-password;SSL Mode=Prefer"
  }
}
```

**For SSL/TLS (Production)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres.example.com;Port=5432;Database=dndgame;Username=dndgame_app;Password=your-password;SSL Mode=Require;Trust Server Certificate=false"
  }
}
```

#### Apply Migrations

```bash
# Set environment to Production
export ASPNETCORE_ENVIRONMENT=Production

# Apply migrations
cd src/DNDGame.API
dotnet ef database update --project ../DNDGame.Infrastructure --startup-project .

# Verify tables created
psql -h localhost -U dndgame_app -d dndgame -c "\dt"
```

Expected tables:
- `__EFMigrationsHistory`
- `Characters`
- `Sessions`
- `Messages`
- `DiceRolls`
- `SessionParticipants`
- `Conditions`

### Seed Data (Optional)

Create seed data for testing:

```csharp
// src/DNDGame.Infrastructure/Data/DndGameContextSeed.cs
public static class DndGameContextSeed
{
    public static async Task SeedAsync(DndGameContext context)
    {
        if (await context.Characters.AnyAsync())
            return; // Already seeded

        var character = new Character
        {
            PlayerId = 1,
            Name = "Gandalf the Grey",
            Class = CharacterClass.Wizard,
            Level = 10,
            MaxHitPoints = 60,
            CurrentHitPoints = 60,
            ArmorClass = 14,
            AbilityScores = new AbilityScores(10, 14, 12, 18, 16, 14),
            Skills = new List<string> { "Arcana", "History", "Investigation" },
            Inventory = new List<string> { "Staff of Power", "Spellbook", "Ring of Fire Resistance" }
        };

        context.Characters.Add(character);
        await context.SaveChangesAsync();
    }
}
```

Apply seed in `Program.cs`:

```csharp
// After app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DndGameContext>();
    await DndGameContextSeed.SeedAsync(context);
}
```

### Database Maintenance

#### Backup (PostgreSQL)

```bash
# Backup database
pg_dump -h localhost -U dndgame_app dndgame > dndgame_backup_$(date +%Y%m%d).sql

# Restore database
psql -h localhost -U dndgame_app dndgame < dndgame_backup_20250115.sql
```

#### Backup (SQLite)

```bash
# Backup database (simple copy)
cp dndgame.db dndgame_backup_$(date +%Y%m%d).db

# Restore database
cp dndgame_backup_20250115.db dndgame.db
```

#### Vacuum and Optimize

```sql
-- PostgreSQL
VACUUM ANALYZE;
REINDEX DATABASE dndgame;

-- SQLite
VACUUM;
```

---

## Running Locally

### 1. Run API Server

```bash
cd src/DNDGame.API

# Run with hot reload
dotnet watch run

# Run without hot reload
dotnet run

# API endpoints:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
# - OpenAPI: https://localhost:5001/openapi/v1.json
# - SignalR Hub: wss://localhost:5001/hubs/game-session
```

### 2. Run Blazor Web Application

```bash
cd src/DNDGame.Web

# Run with hot reload
dotnet watch run

# Web UI:
# - HTTP: http://localhost:5002
# - HTTPS: https://localhost:5003
```

**Note**: Ensure API server is running first (Blazor Web calls API endpoints).

### 3. Run MAUI Mobile App

#### Android Emulator

```bash
cd src/DNDGame.MauiApp

# List available devices
dotnet build -t:Run -f net9.0-android -p:AndroidAvd=pixel_5_-_api_34

# Run on Android emulator
dotnet build -t:Run -f net9.0-android
```

#### iOS Simulator (macOS only)

```bash
cd src/DNDGame.MauiApp

# Run on iOS simulator
dotnet build -t:Run -f net9.0-ios
```

#### Windows

```bash
cd src/DNDGame.MauiApp

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

### 4. Run All Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/DNDGame.UnitTests/DNDGame.UnitTests.csproj

# Run with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Expected results:
- **253 tests passing** (185 unit + 10 integration + 29 Blazor component + 29 MAUI)
- **0 failures**
- **Execution time**: <2 seconds

---

## Production Deployment

### Deployment Checklist

- [ ] Environment variables configured
- [ ] Secrets stored securely (Key Vault, Secrets Manager)
- [ ] Database migrations applied
- [ ] SSL/TLS certificates installed
- [ ] CORS policies configured
- [ ] Health checks implemented
- [ ] Logging and monitoring enabled
- [ ] Backup strategy established
- [ ] Load testing completed
- [ ] Security audit passed

### Publishing Applications

#### Publish API

```bash
cd src/DNDGame.API

# Publish for Linux (production)
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish

# Publish for Windows
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish

# Publish for Docker
dotnet publish -c Release -o ./publish
```

Published files location: `src/DNDGame.API/publish/`

#### Publish Blazor Web

```bash
cd src/DNDGame.Web

# Publish Blazor Server
dotnet publish -c Release -o ./publish

# Publish Blazor WebAssembly (future)
dotnet publish -c Release -o ./publish
```

### Linux Server Deployment

#### Install .NET Runtime

```bash
# Add Microsoft repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Install ASP.NET Core runtime
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
```

#### Copy Published Files

```bash
# Create application directory
sudo mkdir -p /var/www/dndgame-api
sudo chown $USER:$USER /var/www/dndgame-api

# Copy files (from your local machine)
scp -r ./publish/* user@server:/var/www/dndgame-api/
```

#### Create Systemd Service

Create `/etc/systemd/system/dndgame-api.service`:

```ini
[Unit]
Description=DNDGame API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/dndgame-api
ExecStart=/usr/bin/dotnet /var/www/dndgame-api/DNDGame.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dndgame-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment="ConnectionStrings__DefaultConnection=Host=localhost;Database=dndgame;Username=dndgame_app;Password=<password>"
Environment="LLM__ApiKey=sk-your-api-key"

[Install]
WantedBy=multi-user.target
```

#### Start Service

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service (start on boot)
sudo systemctl enable dndgame-api

# Start service
sudo systemctl start dndgame-api

# Check status
sudo systemctl status dndgame-api

# View logs
sudo journalctl -u dndgame-api -f
```

#### Configure Nginx Reverse Proxy

Install Nginx:

```bash
sudo apt-get install -y nginx
```

Create `/etc/nginx/sites-available/dndgame`:

```nginx
upstream dndgame_api {
    server localhost:5000;
    keepalive 32;
}

server {
    listen 80;
    listen [::]:80;
    server_name api.dndgame.com;

    location / {
        return 301 https://$server_name$request_uri;
    }
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name api.dndgame.com;

    ssl_certificate /etc/letsencrypt/live/api.dndgame.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.dndgame.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    client_max_body_size 20M;

    location / {
        proxy_pass http://dndgame_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR WebSocket support
    location /hubs/ {
        proxy_pass http://dndgame_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/dndgame /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### SSL Certificate (Let's Encrypt)

```bash
# Install Certbot
sudo apt-get install -y certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d api.dndgame.com

# Auto-renewal (Certbot creates a systemd timer automatically)
sudo systemctl status certbot.timer
```

---

## Docker Deployment

### Dockerfile for API

Create `src/DNDGame.API/Dockerfile`:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/DNDGame.API/DNDGame.API.csproj", "src/DNDGame.API/"]
COPY ["src/DNDGame.Application/DNDGame.Application.csproj", "src/DNDGame.Application/"]
COPY ["src/DNDGame.Core/DNDGame.Core.csproj", "src/DNDGame.Core/"]
COPY ["src/DNDGame.Infrastructure/DNDGame.Infrastructure.csproj", "src/DNDGame.Infrastructure/"]
COPY ["src/DNDGame.Shared/DNDGame.Shared.csproj", "src/DNDGame.Shared/"]
RUN dotnet restore "src/DNDGame.API/DNDGame.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/DNDGame.API"
RUN dotnet build "DNDGame.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DNDGame.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DNDGame.API.dll"]
```

### Docker Compose

Create `docker-compose.yml` in repository root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: dndgame-postgres
    environment:
      POSTGRES_DB: dndgame
      POSTGRES_USER: dndgame_app
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dndgame_app"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - dndgame-network

  redis:
    image: redis:7-alpine
    container_name: dndgame-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - dndgame-network

  api:
    build:
      context: .
      dockerfile: src/DNDGame.API/Dockerfile
    container_name: dndgame-api
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=dndgame;Username=dndgame_app;Password=${DB_PASSWORD}"
      LLM__ApiKey: ${OPENAI_API_KEY}
      LLM__Model: gpt-4-turbo-preview
      LLM__MaxTokens: 500
      LLM__Temperature: 0.7
      ContentModeration__Enabled: "true"
      ContentModeration__BlockNsfw: "true"
      ContentModeration__BlockHarassment: "true"
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - dndgame-network

  nginx:
    image: nginx:alpine
    container_name: dndgame-nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - api
    networks:
      - dndgame-network

volumes:
  postgres-data:
  redis-data:

networks:
  dndgame-network:
    driver: bridge
```

### Environment Variables (.env)

Create `.env` file (add to `.gitignore`):

```bash
DB_PASSWORD=your-strong-database-password
OPENAI_API_KEY=sk-your-openai-api-key
```

### Build and Run

```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Apply Database Migrations

```bash
# Run migrations in container
docker-compose exec api dotnet ef database update --project /src/src/DNDGame.Infrastructure --startup-project /src/src/DNDGame.API

# Or connect to database and run SQL
docker-compose exec postgres psql -U dndgame_app -d dndgame
```

---

## Mobile App Deployment

### iOS App Store

#### Prerequisites

- **Apple Developer Account** ($99/year)
- **Xcode 15+** installed on macOS
- **Provisioning Profiles** and **Certificates**

#### Build for iOS

```bash
cd src/DNDGame.MauiApp

# Archive for App Store
dotnet build -f net9.0-ios -c Release /p:ArchiveOnBuild=true /p:RuntimeIdentifier=ios-arm64

# Output: bin/Release/net9.0-ios/ios-arm64/DNDGame.MauiApp.ipa
```

#### Submit to App Store

1. Open Xcode
2. Go to **Window** → **Organizer**
3. Select the archive
4. Click **Distribute App**
5. Choose **App Store Connect**
6. Follow prompts to upload

#### App Store Configuration

**Info.plist** settings:

```xml
<key>NSCameraUsageDescription</key>
<string>DNDGame needs camera access to take character portraits.</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>DNDGame needs photo library access to save character images.</string>
<key>NSLocalNetworkUsageDescription</key>
<string>DNDGame needs network access to connect to game servers.</string>
```

### Google Play Store

#### Prerequisites

- **Google Play Developer Account** ($25 one-time)
- **Signing Keys** (Keystore file)

#### Generate Signing Key

```bash
keytool -genkey -v -keystore dndgame.keystore -alias dndgame -keyalg RSA -keysize 2048 -validity 10000

# Store keystore file securely - DO NOT COMMIT TO GIT
```

#### Build for Android

```bash
cd src/DNDGame.MauiApp

# Build APK (for testing)
dotnet build -f net9.0-android -c Release

# Build AAB (for Play Store)
dotnet publish -f net9.0-android -c Release /p:AndroidPackageFormat=aab /p:AndroidKeyStore=true /p:AndroidSigningKeyStore=dndgame.keystore /p:AndroidSigningKeyAlias=dndgame /p:AndroidSigningKeyPass=${KEYSTORE_PASSWORD} /p:AndroidSigningStorePass=${KEYSTORE_PASSWORD}

# Output: bin/Release/net9.0-android/publish/com.dndgame.maui-Signed.aab
```

#### Submit to Play Store

1. Go to [Google Play Console](https://play.google.com/console)
2. Create new app
3. Upload AAB file
4. Complete store listing (description, screenshots, etc.)
5. Submit for review

#### Play Store Configuration

**AndroidManifest.xml** settings:

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
```

---

## Monitoring & Logging

### Application Insights (Azure)

#### Setup

```bash
# Install package
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Configure in `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

Add to `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  }
}
```

### Prometheus + Grafana

#### Install Prometheus Metrics

```bash
dotnet add package prometheus-net.AspNetCore
```

Configure in `Program.cs`:

```csharp
app.UseHttpMetrics();
app.MapMetrics();
```

#### Prometheus Configuration

Create `prometheus.yml`:

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'dndgame-api'
    static_configs:
      - targets: ['localhost:8080']
```

Run Prometheus:

```bash
docker run -d \
  --name prometheus \
  -p 9090:9090 \
  -v $(pwd)/prometheus.yml:/etc/prometheus/prometheus.yml \
  prom/prometheus
```

#### Grafana Dashboard

```bash
docker run -d \
  --name=grafana \
  -p 3000:3000 \
  grafana/grafana
```

Access Grafana at `http://localhost:3000` (admin/admin)

### Structured Logging

Configure in `appsettings.json`:

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/dndgame-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

---

## Security Considerations

### SSL/TLS Certificates

- Use **Let's Encrypt** for free SSL certificates
- Enable **HSTS** (HTTP Strict Transport Security)
- Use **TLS 1.2+** only
- Disable weak ciphers

### API Security

- Implement **JWT authentication** (Phase 8)
- Use **API rate limiting**
- Enable **CORS** with specific origins
- Validate all inputs with **FluentValidation**
- Use **Content Moderation** for LLM inputs/outputs

### Database Security

- Use **parameterized queries** (EF Core does this automatically)
- Enable **SSL for database connections** in production
- Use **strong passwords** (16+ characters, mixed case, numbers, symbols)
- Implement **least privilege** for database users
- Regular **backups** with encryption

### Secrets Management

- ✅ Use **User Secrets** in development
- ✅ Use **Azure Key Vault** or **AWS Secrets Manager** in production
- ✅ Use **environment variables** for Docker
- ❌ Never commit secrets to git
- ❌ Never log sensitive data

---

## Troubleshooting

### Common Issues

#### Issue: Database migration fails

**Solution**:
```bash
# Ensure connection string is correct
dotnet user-secrets list

# Drop database and recreate
dotnet ef database drop --force
dotnet ef database update
```

#### Issue: OpenAI API returns 401 Unauthorized

**Solution**:
```bash
# Verify API key is set
dotnet user-secrets list

# Test API key
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer sk-your-api-key"
```

#### Issue: SignalR connection fails

**Solution**:
- Check firewall allows WebSocket connections
- Verify `app.MapHub<GameSessionHub>("/hubs/game-session")` is called
- Enable CORS for SignalR origin

#### Issue: MAUI app won't build

**Solution**:
```bash
# Clean build
dotnet clean
dotnet workload restore
dotnet restore
dotnet build
```

### Logs Location

- **Linux**: `/var/log/dndgame/`
- **Windows**: `C:\ProgramData\DNDGame\logs\`
- **Docker**: `docker-compose logs -f api`
- **Systemd**: `sudo journalctl -u dndgame-api -f`

---

## Maintenance

### Regular Tasks

| Task | Frequency | Command |
|------|-----------|---------|
| Database backup | Daily | `pg_dump dndgame > backup.sql` |
| Log rotation | Daily | Automatic with systemd |
| SSL renewal | Every 60 days | Automatic with Certbot |
| Dependency updates | Monthly | `dotnet list package --outdated` |
| Security patches | As needed | `apt-get update && apt-get upgrade` |
| API key rotation | Every 90 days | Regenerate in OpenAI dashboard |

### Update Application

```bash
# Pull latest code
git pull origin main

# Build and publish
dotnet publish -c Release -o ./publish

# Stop service
sudo systemctl stop dndgame-api

# Copy new files
sudo cp -r ./publish/* /var/www/dndgame-api/

# Restart service
sudo systemctl start dndgame-api

# Verify
sudo systemctl status dndgame-api
```

---

## Additional Resources

- [.NET Deployment Guide](https://learn.microsoft.com/aspnet/core/host-and-deploy/)
- [Entity Framework Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Docker Documentation](https://docs.docker.com/)
- [MAUI Deployment](https://learn.microsoft.com/dotnet/maui/deployment/)
- [Nginx Configuration](https://nginx.org/en/docs/)
- [Let's Encrypt](https://letsencrypt.org/getting-started/)

---

**Need help?** Open an issue on [GitHub](https://github.com/SDCHESNEY/DNDGame-DotNet/issues).
