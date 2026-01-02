# How to Enable pgvector Extension in PostgreSQL

## Option 1: Using psql Command Line (Recommended)

1. Open Command Prompt or PowerShell
2. Connect to your PostgreSQL database:

```bash
psql -U postgres -d ChatBotDemo
```

Or if you need to specify the host and port:

```bash
psql -h localhost -p 5432 -U postgres -d ChatBotDemo
```

3. Enter your password when prompted (your password is `admin` based on your connection string)
4. Run the command:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

5. Verify it's installed:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

6. Exit psql:

```sql
\q
```

## Option 2: Using pgAdmin (GUI Tool)

1. Open **pgAdmin** (if you have it installed)
2. Connect to your PostgreSQL server
3. Expand: **Servers** → **PostgreSQL** → **Databases** → **ChatBotDemo**
4. Right-click on **ChatBotDemo** → **Query Tool**
5. Paste and run:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

6. Click the **Execute** button (or press F5)

## Option 3: Using SQL Script File

1. Create a file named `enable_pgvector.sql` with this content:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

2. Run it from command line:

```bash
psql -U postgres -d ChatBotDemo -f enable_pgvector.sql
```

## Option 4: Using .NET EF Core Migration (Alternative)

If you prefer, you can also add this to your migration. But the SQL method above is simpler.

## Verify Installation

After running the command, verify it worked:

```sql
-- Check if extension exists
SELECT * FROM pg_extension WHERE extname = 'vector';

-- Should return a row with extname = 'vector'
```

## Troubleshooting

### Error: "could not open extension control file"
- pgvector extension is not installed on your PostgreSQL server
- You need to install pgvector first (see VECTOR_SETUP.md)

### Error: "permission denied"
- Make sure you're connected as a user with CREATE privileges (usually `postgres` superuser)
- Or grant permission: `GRANT CREATE ON DATABASE ChatBotDemo TO your_user;`

### Error: "extension vector already exists"
- This is fine! The extension is already installed
- You can proceed with the migration



