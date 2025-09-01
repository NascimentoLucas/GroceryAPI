Sample secrets for local/dev usage

How to use:

1) Copy this folder to `secrets` at the repo root (which is git-ignored):
   - `secrets/ConnectionStrings__Default.txt`
   - `secrets/Services__Extraction__ApiKey.txt`

2) Replace placeholder values:
   - In `ConnectionStrings__Default.txt`, set a valid Postgres connection string.
   - In `Services__Extraction__ApiKey.txt`, set your API key.

3) Run with Docker Compose:
   - `docker-compose.yml` mounts `./secrets` to `/etc/secrets`, and the app loads keys from there.

Notes:
- Do not commit the `secrets/` folder. It is already ignored by `.gitignore`.
- File names map `__` to nested keys, e.g., `ConnectionStrings__Default` => `ConnectionStrings:Default`.
