# MWNZ Companies API

A small ASP.NET Core API that proxies the [MWNZ evaluation XML service](https://github.com/MiddlewareNewZealand/evaluation-instructions), transforms company data to JSON, and exposes it according to [openapi-companies.yaml](./openapi-companies.yaml).

## Repository structure

```
carl-mwnz/
├── Mwnz.slnx                 # Solution file
├── docker-compose.yml        # Runs the mwnz-api container
├── openapi-companies.yaml    # Target API contract (from evaluation repo)
├── Mwnz.Api/                  # ASP.NET Core Web API (Docker container: mwnz-api)
│   ├── Configuration/        # App settings binding (XML upstream URL, timeout)
│   ├── Endpoints/            # Minimal API route definitions
│   ├── Models/               # Company, ApiError, XML DTOs
│   ├── Services/             # HTTP client, XML parser, orchestration
│   ├── Dockerfile
│   └── Program.cs
└── Mwnz.Api.Test/            # xUnit test project
    ├── Unit/                 # Parser and service tests (no HTTP server)
    ├── Integration/          # Full HTTP pipeline via WebApplicationFactory
    └── TestCategories.cs     # Category names for filtered test runs
```

### How it works

1. `GET /v1/companies/{id}` receives a request.
2. `XmlCompanyClient` fetches `{XmlApi:BaseUrl}/{id}.xml` from GitHub (configurable).
3. `XmlCompanyParser` deserializes the `<Data>` XML document.
4. JSON is returned matching the OpenAPI `Company` schema, or an `Error` body on failure.

Upstream errors (network failure, non-success HTTP status, invalid XML) return **502** with an `error` / `error_description` payload. A missing company (XML service **404**) returns **404**.

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for container runs) | Recent |

## Run locally (without Docker)

```bash
dotnet restore
dotnet run --project Mwnz.Api
```

The API listens on the URLs shown in the console (typically `http://localhost:5229` in Development).

Example requests:

```bash
curl http://localhost:5229/v1/companies/1
curl http://localhost:5229/v1/companies/2
curl http://localhost:5229/health
```

## Run with Docker

From the repository root:

```bash
docker compose up --build
```

The API is available at **http://localhost:8080**.

```bash
curl http://localhost:8080/v1/companies/1
```

To stop:

```bash
docker compose down
```

### Build the image only

```bash
docker build -f Mwnz.Api/Dockerfile -t mwnz-api .
docker run --rm -p 8080:8080 mwnz-api
```

## Configuration

Settings in `Mwnz.Api/appsettings.json`:

| Key | Description | Default |
|-----|-------------|---------|
| `XmlApi:BaseUrl` | Base URL for upstream XML files | GitHub `evaluation-instructions` `xml-api` path |
| `XmlApi:TimeoutSeconds` | HTTP timeout for upstream calls | `30` |

Override via environment variables (Docker example):

```bash
XmlApi__BaseUrl=https://example.com/xml-api
XmlApi__TimeoutSeconds=15
```

## Tests

All tests live in **Mwnz.Api.Test**. Each test class is tagged with a **Category** trait so unit and integration suites can run independently.

| Category | What it covers |
|----------|----------------|
| `Unit` | XML parsing and `CompanyService` with a fake upstream client |
| `Integration` | HTTP endpoints via `WebApplicationFactory` and a fake XML client |

### Run all tests

```bash
dotnet test
```

### Run unit tests only

```bash
dotnet test --filter "Category=Unit"
```

### Run integration tests only

```bash
dotnet test --filter "Category=Integration"
```

## API reference

See [openapi-companies.yaml](./openapi-companies.yaml). Primary endpoint:

- **GET** `/v1/companies/{id}` — company JSON (`id`, `name`, `description`)

## Evaluation source

Built against the [Middleware New Zealand evaluation instructions](https://github.com/MiddlewareNewZealand/evaluation-instructions).
