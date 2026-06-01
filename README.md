# MWNZ Companies API

A small ASP.NET Core API that proxies the [MWNZ evaluation XML service](https://github.com/MiddlewareNewZealand/evaluation-instructions), transforms company data to JSON, and exposes it according to [openapi-companies.yaml](./openapi-companies.yaml).

## Repository structure

```
├── .github/workflows/        # GitHub Actions (audit, Docker build, tests)
├── Mwnz.slnx                 # Solution file
├── docker-compose.yml        # Runs the mwnz-api container
├── openapi-companies.yaml    # Target API contract (from evaluation repo)
├── Mwnz.Api/                 # ASP.NET Core Web API (Docker container: mwnz-api)
│   ├── Integrations/XmlCompany/   # Integration with the upstream XML API
│   │   ├── Models/           # XmlCompany — external service XML shape
│   │   ├── XmlCompanyClient.cs
│   │   ├── XmlCompanyParser.cs
│   │   └── …                 # IXmlCompanyClient, IXmlCompanyParser, XmlFetchResult
│   ├── Configuration/        # XmlApiOptions (upstream URL, timeout)
│   ├── Endpoints/            # Minimal API routes (companies, OpenAPI spec)
│   ├── Models/               # API domain models (Company, ApiError)
│   ├── Services/             # CompanyService — orchestration and error mapping
│   ├── Dockerfile
│   └── Program.cs
└── Mwnz.Api.Test/            # xUnit + Moq
    ├── Unit/                 # XmlCompanyClient, XmlCompanyParser, CompanyService
    ├── Integration/          # HTTP endpoints via WebApplicationFactory
    └── TestCategories.cs     # Category names for filtered test runs
```

### How it works

1. `GET /v1/companies/{id}` hits **Endpoints** → **CompanyService**.
2. **XmlCompanyClient** (`Integrations/XmlCompany`) fetches `{XmlApi:BaseUrl}/{id}.xml` from GitHub (configurable).
3. **XmlCompanyParser** deserializes the upstream `<Data>` document into `Integrations.XmlCompany.Models.XmlCompany`, then maps to the API `Company` model.
4. JSON is returned matching the OpenAPI `Company` schema, or an `Error` body on failure.

The evaluation OpenAPI document is embedded in the API assembly and served at `GET /openapi/v1.yaml` (`application/yaml`).

| Condition | HTTP status | `error` code |
|-----------|-------------|--------------|
| Company not found (upstream **404**) | **404** | `not_found` |
| Network / upstream HTTP failure | **502** | `upstream_error` |
| Upstream OK but XML invalid | **502** | `invalid_response` |

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
curl http://localhost:5229/openapi/v1.yaml
```

## Run with Docker

From the repository root:

```bash
docker compose up --build
```

The API is available at **http://localhost:8080**.

```bash
curl http://localhost:8080/v1/companies/1
curl http://localhost:8080/v1/companies/2
curl http://localhost:8080/health
curl http://localhost:8080/openapi/v1.yaml
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

The Docker build expects `openapi-companies.yaml` at the repository root (see `Mwnz.Api/Dockerfile`).

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

All tests live in **Mwnz.Api.Test**. Each test class is tagged with a **Category** trait. Upstream HTTP is mocked with [Moq](https://github.com/devlooped/moq) in unit and integration tests.

| Category | What it covers |
|----------|----------------|
| `Unit` | `XmlCompanyClient` (HTTP handler stubs), `XmlCompanyParser`, `CompanyService` |
| `Integration` | Full HTTP pipeline via `WebApplicationFactory` (company routes, health, OpenAPI spec) |

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

## Continuous integration

GitHub Actions workflow [`.github/workflows/ci.yml`](./.github/workflows/ci.yml) (**Build and Publish**) runs on pushes and pull requests to `main`:

1. Check out the code
2. Restore packages and run a .NET vulnerable-package audit
3. Build the Docker image (`docker compose build`)
4. Start the container, run `dotnet test`, then smoke-test the running API (`/health`, `/openapi/v1.yaml`, `/v1/companies/1` and `/2`)
5. Placeholder step for future publish to a Docker registry

## API reference

The contract is defined in [openapi-companies.yaml](./openapi-companies.yaml). The running service exposes:

| Method | Path | Description |
|--------|------|-------------|
| GET | `/v1/companies/{id}` | Company JSON (`id`, `name`, `description`) |
| GET | `/openapi/v1.yaml` | OpenAPI 3.0 specification (YAML) |
| GET | `/health` | Liveness check (`{"status":"healthy"}`) |

## Evaluation source

Built against the [Middleware New Zealand evaluation instructions](https://github.com/MiddlewareNewZealand/evaluation-instructions).
