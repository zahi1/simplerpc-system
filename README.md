# simplerpc-system: Gas Pressure RPC Simulation

A .NET 8 SimpleRPC-based distributed simulation of gas pressure in a container with three components:
- Server: container state and business logic (pressure, mass, temperature, safety limits)
- Input clients: add gas mass when pressure is below the lower limit
- Output clients: remove gas mass when pressure is above the upper limit

## Project goals
- Demonstrate contract-first remote interactions using SimpleRPC
- Keep contracts RPC-agnostic and separate from business logic
- Support many concurrent clients safely without races or deadlocks

## Architecture
- GasContract (class library)
  - `IGasContainerService` defines the remote service contract.
- gasPressure (server)
  - Implements the service and hosts SimpleRPC.
  - Every ~2 seconds updates temperature with a random delta and recomputes pressure.
  - Resets simulation on implosion/explosion.
- inputClient (client)
  - If pressure < lower limit, generates positive random mass and calls `AddMass`.
- outputClient (client)
  - If pressure > upper limit, generates positive random mass and calls `RemoveMass`.

### Communication flow
1. Clients obtain a proxy for `IGasContainerService` using SimpleRPC and the shared GasContract DLL.
2. Clients call short, non-blocking methods (get state, add mass, remove mass).
3. Server validates constraints and applies updates atomically, logs each step.
4. Server’s timer adjusts temperature; clients react next cycle.

### Lab requirements mapping (summary)
- One server, unlimited clients: yes
- Interfaces for contracts: `IGasContainerService`
- Contract/logic separation: contract lib vs. server/clients
- No sleeping in service ops: yes (timer runs outside RPC calls)
- Proxy-based RPC: SimpleRPC proxies
- No races/deadlocks: serialized updates in server, short calls
- Continuous cycles: server timer + client loops
- Separate processes: server and each client run separately
- Logging: to stdout/files

## How to run
Prerequisites: .NET SDK 8+

Build all

```bash
dotnet build
```

Run server (terminal 1)

```bash
dotnet run --project gasPressure
```

Run one or more input clients (terminal 2+)

```bash
dotnet run --project inputClient
```

Run one or more output clients (terminal 3+)

```bash
dotnet run --project outputClient
```

Configuration such as limits/endpoints/logging is in `gasPressure/appsettings.json` (and Development override).

## License
Default: MIT (see LICENSE). You can switch to Apache-2.0 or GPL-3.0 if you prefer.
