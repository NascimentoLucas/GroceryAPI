# GroceryAPI

GroceryAPI is a .NET 8 Web API that extracts a structured recipe (food + ingredients) from free text using an external Extraction service (LLM), and persists it in PostgreSQL. It also provides CRUD endpoints for foods, ingredients, and their relationships, plus a helper endpoint to perform the extraction.

## Features
- ASP.NET Core Web API (net8.0)
- PostgreSQL + Entity Framework Core (Npgsql)
- Foods, Ingredients, and FoodIngredients (join with quantity)
- Recipe extraction endpoint that calls an external API with a prompt
- Swagger UI in Development
- Simple file-based secrets loader mapped from Docker (`/etc/secrets`)

## Quick Start (Docker)

Prerequisites: Docker and Docker Compose.

1) Copy sample secrets and fill values:
   - `secrets.example` → `secrets`
   - Edit the files in `secrets/`:
     - `ConnectionStrings__Default.txt` → PostgreSQL connection string
     - `Services__Extraction__ApiKey.txt` → API key for the extraction provider

2) Configure environment variables for Postgres/pgAdmin:
   - Copy root `.env.example` to `.env` and adjust as needed.

3) Start everything:
   - `docker-compose up -d`

Services exposed:
- API: `http://localhost:5000`
- pgAdmin: `http://localhost:5050`

The compose file mounts `./secrets` to `/etc/secrets` in the API container. The application loads these files into configuration at startup.

## Secrets & Configuration

The API loads configuration from:
- Environment variables
- Files under `/etc/secrets` (if mounted)

File secrets mapping:
- Each file name becomes a configuration key; file contents are the value
- Double underscores map to nested keys: `A__B__C` → `A:B:C`

Important keys:
- `ConnectionStrings:Default` (e.g., `Host=...;Port=5432;Database=...;Username=...;Password=...;Pooling=true`)
- `Services:Extraction:ApiUrl` (default set in `GroceryAPI/appsettings.json`)
- `Services:Extraction:ApiKey` (set via secret or environment variable)

Sample secrets are provided under `secrets.example/`. Do not commit the `secrets/` directory — it is ignored by `.gitignore`.

## Local Development (without Docker)

Option A — environment variables:
- Set `ConnectionStrings__Default` to your Postgres connection string
- Set `Services__Extraction__ApiKey`
- Optionally set `Services__Extraction__ApiUrl` (defaults to OpenAI Responses API)

Run:
- `dotnet run --project GroceryAPI`
- API will listen on the default Kestrel ports (development). Swagger is enabled in Development at `/swagger`.

## Database & Migrations

Generate migrations:
- `dotnet ef migrations add <Name> --project GroceryAPI`

Apply migrations:
- `dotnet ef database update --project GroceryAPI`

When using Docker Compose, the API connects to the `postgres` service using the connection string you provide.

## Endpoints (Overview)

Base path: `/api`

- `POST /api/grocery/extract?save=true` — Body: plain text string. Calls the extraction service; returns the parsed recipe. If `save=true` (default), upserts Food, upserts Ingredients by name, and creates FoodIngredient rows for each ingredient with its quantity.

Foods
- `GET /api/foods?query=&page=&pageSize=` — Paged list with optional case-insensitive search
- `GET /api/foods/{id}` — Get by id
- `POST /api/foods` — Create (body: `{ "name": "..." }`)
- `PUT /api/foods/{id}` — Update name
- `DELETE /api/foods/{id}` — Delete

Ingredients
- `GET /api/ingredients?query=&page=&pageSize=` — Paged list with optional search
- `GET /api/ingredients/{id}` — Get by id
- `POST /api/ingredients` — Create (unique name)
- `PUT /api/ingredients/{id}` — Update name
- `DELETE /api/ingredients/{id}` — Delete (fails if referenced by foods)

FoodIngredients
- `GET /api/foodingredients/{id}` — Get by id
- `POST /api/foodingredients` — Link ingredient to food with quantity
- `PUT /api/foodingredients/{id}` — Update quantity
- `DELETE /api/foodingredients/{id}` — Remove link
- `POST /api/foodingredients/foods/{foodId}/add` — Add ingredient to a food (body includes `ingredientId` and `quantity`)
- `PUT /api/foodingredients/foods/{foodId}/ingredients/{ingredientId}` — Update quantity for a specific food + ingredient

Swagger (Development): `GET /swagger`

## Example: Extract Recipe

Request:
```
POST http://localhost:5000/api/grocery/extract?save=true
Content-Type: application/json

"I cooked pasta with tomato sauce using 200g pasta, 3 tomatoes and 2 tbsp olive oil."
```

Response (shape):
```
{
  "food": "Pasta with tomato sauce",
  "ingredients": [
    { "name": "pasta", "quantity": "200g" },
    { "name": "tomato", "quantity": "3 units" },
    { "name": "olive oil", "quantity": "2 tbsp" }
  ]
}
```

## Development Notes
- Swagger/CORS are enabled only in Development.
- The API uses Bearer auth when calling the extraction service; ensure `Services:Extraction:ApiKey` is set.
- PostgreSQL schema uses unique constraints and case-insensitive text (CITEXT) for ingredient names.

## Security
- Do not commit real secrets. The `secrets/` folder and `.env` are git-ignored.
- Prefer environment variables or mounted secret files for sensitive values.
- Consider adding a secret-scanning tool (e.g., gitleaks) to CI.

## License
MIT — see `LICENSE` for details.
