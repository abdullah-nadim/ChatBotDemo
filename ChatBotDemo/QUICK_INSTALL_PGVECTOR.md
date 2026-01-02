# Quick Guide: Install pgvector on Windows (PostgreSQL 17)

## Prerequisites Check

You need:
- ✅ PostgreSQL 17 (you have this)
- ⚠️ Visual Studio with C++ tools OR Build Tools
- ⚠️ Git for Windows

## Step-by-Step Installation

### Step 1: Install Prerequisites (if needed)

**Install Git:**
- Download from: https://git-scm.com/download/win
- Or use: `winget install Git.Git`

**Install Visual Studio Build Tools:**
- Download: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
- During installation, select "Desktop development with C++"

### Step 2: Open Developer Command Prompt

1. Press **Win + R**
2. Type: `cmd` and press Enter
3. Or search for "Developer Command Prompt for VS" in Start Menu

### Step 3: Set Up Environment

Run these commands (adjust paths if needed):

```cmd
REM Set Visual Studio environment (adjust path for your VS version)
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"

REM Set PostgreSQL path (adjust version if different)
set "PGROOT=C:\Program Files\PostgreSQL\17"
set "PG_CONFIG=%PGROOT%\bin\pg_config.exe"
```

### Step 4: Clone and Build pgvector

```cmd
cd %TEMP%
git clone --branch v0.8.1 https://github.com/pgvector/pgvector.git
cd pgvector
nmake /F Makefile.win
```

**If you get errors**, try:
```cmd
nmake /F Makefile.win PG_CONFIG="C:\Program Files\PostgreSQL\17\bin\pg_config.exe"
```

### Step 5: Install (Run as Administrator)

**Important:** Right-click Command Prompt → "Run as Administrator", then:

```cmd
cd %TEMP%\pgvector
nmake /F Makefile.win install
```

This will copy files to:
- `C:\Program Files\PostgreSQL\17\lib\vector.dll`
- `C:\Program Files\PostgreSQL\17\share\extension\vector.*`

### Step 6: Restart PostgreSQL

1. Press **Win + R**
2. Type: `services.msc`
3. Find **postgresql-x64-17** (or similar)
4. Right-click → **Restart**

### Step 7: Enable Extension in pgAdmin

1. Open pgAdmin
2. Connect to your database
3. Right-click **ChatBotDemo** → **Query Tool**
4. Run:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

5. Verify:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

---

## Alternative: If Building Fails

If you encounter build errors, you can:

1. **Use Docker** (easiest alternative):
   ```bash
   docker run -d --name postgres-pgvector -e POSTGRES_PASSWORD=admin -e POSTGRES_DB=ChatBotDemo -p 5432:5432 pgvector/pgvector:pg17
   ```

2. **Ask for pre-built binaries** in pgvector GitHub issues

3. **Use PostgreSQL 16** instead (might have pre-built binaries available)

---

## Troubleshooting

### "nmake is not recognized"
- Make sure you ran `vcvars64.bat` first
- Or use Visual Studio Developer Command Prompt

### "Access Denied" during install
- Run Command Prompt as Administrator
- Or manually copy files to PostgreSQL directories

### "pg_config not found"
- Make sure PostgreSQL is installed
- Check path: `C:\Program Files\PostgreSQL\17\bin\pg_config.exe`

### Still having issues?
Check the pgvector GitHub: https://github.com/pgvector/pgvector



