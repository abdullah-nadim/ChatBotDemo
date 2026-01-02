# Installing pgvector on Windows (PostgreSQL 17)

## Method 1: Download Pre-compiled Binaries (Easiest)

### Step 1: Download pgvector for PostgreSQL 17

1. Go to the pgvector releases page: https://github.com/pgvector/pgvector/releases
2. Look for a Windows build or download the source code
3. **OR** use this direct link if available:
   - For PostgreSQL 17: https://github.com/pgvector/pgvector/releases/latest
   - Download the Windows build (usually named something like `pgvector-v0.x.x-windows-x64.zip`)

### Step 2: Extract Files

1. Extract the downloaded ZIP file
2. You should see files like:
   - `vector.dll`
   - `vector.control`
   - `vector--*.sql` files

### Step 3: Copy Files to PostgreSQL Directory

Copy the files to your PostgreSQL extension directory:
```
C:\Program Files\PostgreSQL\17\share\extension\
```

**Files to copy:**
- `vector.control`
- `vector--*.sql` (all SQL files)

**For the DLL file:**
Copy `vector.dll` to:
```
C:\Program Files\PostgreSQL\17\lib\
```

### Step 4: Restart PostgreSQL Service

1. Open **Services** (Win + R, type `services.msc`)
2. Find **postgresql-x64-17** (or similar)
3. Right-click → **Restart**

### Step 5: Enable Extension

Now go back to pgAdmin and run:
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

---

## Method 2: Build from Source (If pre-built not available)

### Prerequisites:
- Visual Studio 2019 or later with C++ tools
- Git
- PostgreSQL development headers (usually included with PostgreSQL installation)

### Steps:

1. **Open PowerShell as Administrator**

2. **Install Git** (if not installed):
   ```powershell
   winget install Git.Git
   ```

3. **Clone pgvector repository**:
   ```powershell
   cd C:\
   git clone --branch v0.5.1 https://github.com/pgvector/pgvector.git
   cd pgvector
   ```

4. **Build pgvector**:
   ```powershell
   # Set PostgreSQL path
   $env:PG_CONFIG = "C:\Program Files\PostgreSQL\17\bin\pg_config.exe"
   
   # Build (you may need to adjust based on your Visual Studio installation)
   nmake /f Makefile.win
   ```

5. **Install**:
   ```powershell
   nmake /f Makefile.win install
   ```

6. **Restart PostgreSQL service**

---

## Method 3: Use StackBuilder (Easiest for some setups)

1. Open **StackBuilder** (usually installed with PostgreSQL)
2. Select your PostgreSQL installation
3. Look for **pgvector** in the available extensions
4. Install it through StackBuilder

---

## Method 4: Use Docker (Alternative - if you can switch)

If installing pgvector is too complex, you could use a PostgreSQL Docker image with pgvector pre-installed:

```bash
docker run -d \
  --name postgres-pgvector \
  -e POSTGRES_PASSWORD=admin \
  -e POSTGRES_DB=ChatBotDemo \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

Then update your connection string to point to `localhost:5432`.

---

## Verification

After installation, verify in pgAdmin:

```sql
-- Check if extension files exist
SELECT * FROM pg_available_extensions WHERE name = 'vector';

-- If it shows up, create it
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify it's installed
SELECT * FROM pg_extension WHERE extname = 'vector';
```

---

## Troubleshooting

### "Access Denied" when copying files
- Right-click on the PostgreSQL folder → Properties → Security
- Give your user "Full Control" or run PowerShell as Administrator

### "vector.dll not found"
- Make sure `vector.dll` is in `C:\Program Files\PostgreSQL\17\lib\`
- Check that the DLL architecture matches your PostgreSQL (usually x64)

### Still not working?
- Check PostgreSQL logs: `C:\Program Files\PostgreSQL\17\data\log\`
- Make sure you restarted the PostgreSQL service
- Verify PostgreSQL version: `SELECT version();`



