# MeetingRoom – Real-Time Meeting Room Reservation Ecosystem

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-Strict-3178C6?logo=typescript)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoft-sql-server)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Overview

**MeetingRoom** is a modern, distributed reservation system for hourly meeting room allocation with enterprise-grade real-time synchronization. The ecosystem provides a granular role-based access control (RBAC) topology, optimistic concurrency conflict resolution, and live slot status propagation across multiple simultaneous users without requiring full-page reloads.

The system enforces strict business hours (07:00–18:00 UTC) slot visibility and prevents data races through Entity Framework Core row-version concurrency tokens, ensuring atomicity and consistency in high-contention booking scenarios.

---

## Architecture Stack

### Backend

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Runtime** | ASP.NET Core (.NET 10) | High-performance, cross-platform HTTP server |
| **Database** | SQL Server LocalDB + Entity Framework Core | Relational data persistence with migrations |
| **Authentication** | JWT Bearer (HS256) | Stateless token-based identity and claims |
| **Authorization** | Policy-based (ASP.NET Core) | Granular role and claim-based access control |
| **Real-time Sync** | SignalR (WebSocket) | Bi-directional pub/sub for live slot updates |
| **API Documentation** | OpenAPI / Swagger | Introspectable REST contract (Development only) |

### Frontend

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **UI Framework** | React 19 (Functional) | Component-driven declarative rendering |
| **Build Tool** | Vite 8 | Blazing-fast dev server and production bundler |
| **Language** | TypeScript 6 (Strict Mode) | Compile-time type safety and IDE autocomplete |
| **State Management** | React Hooks (useState, useEffect, useRef) | Local component state and lifecycle orchestration |
| **Real-time Client** | @microsoft/signalr | WebSocket-based hub connection for push events |
| **Styling** | Inline CSS Objects | Type-safe, co-located style definitions |

### Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (React/Vite)                   │
│  ┌──────────────────┐              ┌──────────────────┐     │
│  │   AuthForm       │              │   RoomGrid       │     │
│  │  (Login/Signup)  │              │  (Slot Matrix)   │     │
│  └────────┬─────────┘              └────────┬─────────┘     │
│           │                                 │                │
│           │ POST /auth/login               │ POST/DELETE    │
│           │ POST /auth/register            │ /bookings      │
│           ▼                                 ▼                │
│  ┌─────────────────────────────────────────────────────┐    │
│  │         HTTP REST API Client (fetch)                │    │
│  └─────────────────────┬───────────────────────────────┘    │
│                        │                                     │
│    ┌───────────────────┴───────────────────┐               │
│    │                                       │               │
│    ▼                                       ▼               │
│  Bearer Token           SignalR WebSocket Connection      │
│  (localStorage)         (Real-time sync)                  │
└─────────────────────────────────────────────────────────────┘
         │                                 │
         ▼                                 ▼
┌─────────────────────────────────────────────────────────────┐
│               Backend (ASP.NET Core .NET 10)                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Controllers (thin HTTP surface)                    │   │
│  │  ├─ AuthController (login, register, logout)       │   │
│  │  ├─ RoomsController (CRUD rooms, list slots)       │   │
│  │  └─ BookingsController (create/delete reservations)│   │
│  └────────────────────────┬────────────────────────────┘   │
│                           │                                │
│  ┌────────────────────────▼────────────────────────────┐   │
│  │  Services (business logic)                          │   │
│  │  ├─ UserService (registration, password hash)      │   │
│  │  ├─ TokenService (JWT issuance & validation)       │   │
│  │  ├─ RoomService (room & slot management)           │   │
│  │  ├─ BookingService (reservation orchestration)     │   │
│  │  └─ SeedService (demo data initialization)         │   │
│  └────────────────────────┬────────────────────────────┘   │
│                           │                                │
│  ┌────────────────────────▼────────────────────────────┐   │
│  │  Data Access Layer (EF Core DbContext)             │   │
│  │  ├─ User entity + Identity integration             │   │
│  │  ├─ Room entity (capacity, metadata)               │   │
│  │  ├─ TimeSlot entity (startTime, endTime, isBooked) │   │
│  │  └─ Booking entity (userId, slotId, timestamp)     │   │
│  └────────────────────────┬────────────────────────────┘   │
│                           │                                │
│  ┌────────────────────────▼────────────────────────────┐   │
│  │  SQL Server LocalDB                                 │   │
│  │  (Persistent storage, ACID transactions)            │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  SignalR Hub (Real-time Broadcasting)               │   │
│  │  ├─ SlotStatusChanged event (broadcast on update)  │   │
│  │  └─ Connected clients receive live slot state      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## Core Feature Engineering

### 1. Granular Role-Based Access Control (RBAC)

The system enforces a two-tier access topology:

#### **Admin**
- Full CRUD capabilities: create, read, update, delete meeting rooms
- Access to global booking audit logs and reservation history
- Ability to cancel any user's booking without restriction
- View all room inventory and slot matrices across the system
- Seed default admin account via `AdminSeed` configuration

#### **Regular User**
- Read-only access: browse available and booked time slots
- Book available slots for themselves
- Cancel only their own active reservations
- No access to administrative functions or audit logs
- Auto-created at first registration

**Implementation**: ASP.NET Core policy-based authorization with JWT role claims. Fallback authorization policy denies all unauthenticated requests. Admin routes decorated with `[AuthorizeAdmin]` attribute. No hand-coded role string literals—all role names stored as constants.

---

### 2. Real-Time Concurrency Synchronization

**Challenge**: Multiple users simultaneously viewing and booking the same slots can lead to stale data, race conditions, and phantom bookings.

**Solution**: Bi-directional SignalR (WebSocket) integration propagates slot state changes to all connected clients instantly.

#### Workflow

1. User A books Slot X in browser tab 1
2. Backend processes booking → records in database
3. SignalR hub broadcasts `SlotStatusChanged(slotId: X, isBooked: true)` event
4. All connected clients (including User B in tab 2) receive event immediately
5. Frontend updates slot button color (green → red) without page reload
6. User B sees slot already booked; cannot attempt duplicate booking

#### Implementation Details

- **Connection**: Established on app load if authenticated; uses Bearer token for authorization
- **Pub/Sub Pattern**: Hub-based; no client-to-client messaging (only server broadcasts)
- **Reconnection**: Automatic with exponential backoff via `.withAutomaticReconnect()`
- **Error Handling**: Failed connections logged to console; graceful degradation (user can still refresh manually)

---

### 3. Data Race Protection via Optimistic Concurrency Control

**Challenge**: Two users attempt to book the same slot in the millisecond window before slot state updates propagate. Without safeguards, both bookings succeed (data corruption).

**Solution**: Entity Framework Core row-version concurrency tokens (`byte[]` RowVersion property) and HTTP 409 Conflict response.

#### Mechanism

- Each `TimeSlot` entity includes a `RowVersion` byte array (auto-managed by SQL Server)
- Row version increments on every update
- When booking a slot, EF Core compares the client-provided row version with current database version
- If versions mismatch → booking rejected with HTTP 409 Conflict
- Frontend catches 409; displays error message; user retries (slot now shows correctly as booked)

#### Benefits

- **Atomicity**: Booking transaction is all-or-nothing; no phantom states
- **High Contention Ready**: Handles burst booking attempts (e.g., popular morning slots)
- **No Pessimistic Locks**: Avoids database locks; better concurrency throughput
- **User-Friendly**: Conflict message guides user to retry; no silent failures

---

### 4. Administrative Overrides & Governance

Admins possess elevated capabilities for system management:

- **Global Audit Log**: View all reservations across all users and rooms
- **Resource Management**: Dynamically create/modify/delete meeting rooms without downtime
- **Conflict Resolution**: Cancel any user's booking (e.g., emergency meeting preemption, dispute resolution)
- **Capacity Planning**: Update room capacity; view historical booking patterns

**UI Boundaries**: Regular users see only their own bookings and available slots in their permitted time windows. Admin view toggles between full audit log and normal slot grid.

---

## Environment Configuration

### Development Setup

1. **Backend Configuration** (`appsettings.Development.json` — gitignored)

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MeetingRoomDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "Jwt": {
       "Key": "<32+ byte signing key (base64 or hex)>",
       "Issuer": "MeetingRoom",
       "Audience": "MeetingRoom.Users",
       "ExpiryMinutes": 1440
     },
     "AdminSeed": {
       "Username": "admin",
       "Email": "admin@meetingroom.local",
       "Password": "<secure development password>"
     }
   }
   ```

> ⚠️ **SEEDED CREDENTIALS FOR MENTORS/REVIEWERS EVALUATION**
>
> The development environment includes pre-configured admin credentials for immediate evaluation:
>
> | Field | Value |
> |-------|-------|
> | **Username** | `admin` |
> | **Password** | `dev-only-admin-password` |
> | **Email** | `admin@meetingroom.local` |
>
> These credentials are automatically seeded into the database on first startup via the `AdminSeed` configuration block in `appsettings.Development.json`. Use them to immediately access admin features (room management, audit logs) without creating a new account.
>
> ⚠️ **NEVER use these credentials in production.** They are intentionally simple for development ease only.

2. **Frontend Configuration** (environment variables optional; `.env` file supported by Vite)

   ```bash
   VITE_API_URL=http://localhost:5080/api
   VITE_SIGNALR_HUB_URL=http://localhost:5080/api/hubs/bookings
   ```

   Both URLs must match backend host:port to avoid CORS/WebSocket failures.

### Production Deployment

- Use environment variables or cloud secret management (Azure Key Vault, AWS Secrets Manager)
- Never commit sensitive data (passwords, keys) to version control
- Rotate `Jwt:Key` on each deployment cycle
- Enable HTTPS; update CORS origin to deployed frontend domain
- Use managed SQL Server (Azure SQL Database, AWS RDS); retire LocalDB

---

## Building & Running

### Quick Start

**Terminal 1 (Backend)**
```bash
dotnet restore backend/MeetingRoom.Api.csproj
dotnet build backend/MeetingRoom.Api.csproj --no-restore
dotnet ef database update --project backend/MeetingRoom.Api.csproj
dotnet run --project backend/MeetingRoom.Api.csproj --launch-profile http
```

**Terminal 2 (Frontend)**
```bash
cd frontend
npm install
npm run build      # Type check + optimize
npm run dev        # Start dev server
```

Both running → navigate to `http://localhost:5173`

### Full Command Reference

See [CLAUDE.md](./CLAUDE.md) for exhaustive build, run, database, and test commands.

---

## Project Structure

```
MeetingRoom/
├── backend/
│   ├── MeetingRoom.Api.csproj
│   ├── Program.cs                    # Composition root, DI, middleware
│   ├── Authorization/
│   │   ├── AuthPolicies.cs           # Policy names, constants
│   │   └── AuthorizeAdminAttribute.cs# Reusable admin check
│   ├── Configuration/
│   │   ├── JwtOptions.cs
│   │   └── AdminSeedOptions.cs
│   ├── Controllers/
│   │   ├── AuthController.cs         # Login, register, logout
│   │   ├── RoomsController.cs        # Room CRUD, list slots
│   │   └── BookingsController.cs     # Create/delete reservations
│   ├── Models/
│   │   ├── User.cs                   # Identity user + custom claims
│   │   ├── Room.cs                   # Entity: name, capacity
│   │   ├── TimeSlot.cs               # Entity: startTime, endTime, isBooked, RowVersion
│   │   ├── Booking.cs                # Entity: userId, slotId, timestamp
│   │   ├── ClaimNames.cs             # Constant claim names
│   │   └── RoleNames.cs              # 'admin', 'user' constants
│   ├── Services/
│   │   ├── IUserRepository.cs        # User persistence interface
│   │   ├── ITokenService.cs          # JWT creation interface
│   │   ├── IBookingService.cs        # Reservation logic interface
│   │   ├── UserRepository.cs         # EF-based implementation
│   │   ├── TokenService.cs           # JWT encoding/validation
│   │   ├── BookingService.cs         # Concurrency, slot logic
│   │   └── SeedService.cs            # Demo data initialization
│   ├── Data/
│   │   └── AppDbContext.cs           # EF DbContext, migrations
│   ├── Hubs/
│   │   └── BookingsHub.cs            # SignalR hub, real-time events
│   ├── appsettings.json              # Public config (no secrets)
│   └── appsettings.Development.json  # Gitignored (secrets, JWT key)
│
├── frontend/
│   ├── package.json
│   ├── tsconfig.json                 # TypeScript strict: true
│   ├── vite.config.ts
│   ├── src/
│   │   ├── App.tsx                   # Root orchestration component
│   │   ├── styles.ts                 # Centralized style constants
│   │   └── components/
│   │       ├── AuthForm.tsx          # Login/register (owns auth logic)
│   │       ├── Header.tsx            # Navigation, user info, logout
│   │       ├── AdminPanel.tsx        # Room management (owns CRUD)
│   │       ├── AdminCreateRoom.tsx   # Create form UI
│   │       ├── AdminEditRoom.tsx     # Edit form UI
│   │       ├── RoomGrid.tsx          # Slot matrix display
│   │       ├── SlotButton.tsx        # Individual slot (owns book/cancel)
│   │       ├── AuditLog.tsx          # Admin audit log table
│   │       ├── MessageNotification.tsx# Toast notifications
│   │       └── index.ts              # Barrel exports
│   └── public/
│       └── index.html                # Entry point
│
├── CLAUDE.md                         # Build & run commands
├── README.md                         # This file
└── .gitignore
```

---

## Key Design Patterns

### Backend

1. **Dependency Injection**: All services injected into controllers/services; no service locator
2. **Policy-Based Authorization**: Named policies; reusable across controllers
3. **Options Pattern**: Configuration classes (JwtOptions, AdminSeedOptions) validated at startup
4. **Entity Framework Core**: Migrations-driven schema evolution; RowVersion for concurrency
5. **Async-All-The-Way**: All I/O operations async; proper CancellationToken propagation

### Frontend

1. **Component Encapsulation**: Each component owns its domain logic (AuthForm: auth, SlotButton: booking, AdminPanel: room CRUD)
2. **Props-Over-State-Drilling**: Minimal prop tunneling; callbacks for parent notification
3. **useRef for Imperative Coordination**: AdminPanel exposes ref methods for RoomGrid to trigger edit mode
4. **Centralized Styles**: All inline styles in `styles.ts` constant object; no scattered inline styles
5. **Functional Components + Hooks**: No class components; useState, useEffect, useRef for lifecycle

---

## Known Limitations & Future Work

- **No Refresh Token Rotation**: JWT token never rotated; single issuance per login
- **No Account Lockout**: Brute-force protection absent; future: implement exponential backoff
- **No Rate Limiting**: API endpoints unthrottled; future: add middleware-based request limiting
- **No Audit Trail**: Booking history viewable by admins; detailed change logs (who/when/why) not captured
- **No Timezone Handling**: All times assumed UTC; future: client-side timezone conversion + storage in UTC
- **Test Project Absent**: No automated integration or unit tests; manual QA only
- **HTTPS Disabled in Dev**: LocalDB + HTTP sufficient for development; HTTPS required in production

---

## Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure managed SQL Server (Azure SQL, AWS RDS)
- [ ] Generate strong `Jwt:Key` (≥32 bytes, random)
- [ ] Store secrets in secure vault (not in code)
- [ ] Update CORS allowed origin to production domain
- [ ] Enable HTTPS; configure certificate
- [ ] Set frontend `VITE_API_URL` to production backend domain
- [ ] Run `dotnet ef database update` on production database
- [ ] Execute seed admin user setup
- [ ] Test end-to-end booking flow in staging
- [ ] Monitor logs and SignalR connections post-launch

---

## Support & Contribution

For bug reports, feature requests, or contribution guidelines, refer to the project's issue tracker or contact the maintainers directly.

---

## License

MIT License – See LICENSE file for details.

---

**Last Updated**: September 2026  
**Status**: Active Development
