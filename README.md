# UBB-SE-2026-Jobs

## Prerequisites

- Visual Studio 2022 (with .NET 10 / Windows App SDK workloads)
- SQL Server Express (or any local SQL Server instance) + SSMS

---

## 1. Set up the database

1. Open **SSMS** and connect to your local SQL Server instance.
2. Open the file `ISS_Merged_Setup.sql` (at the root of the repo).
3. Press **F5** (Execute) — this creates the `ISS_Merged` database and seeds all tables for both PussyCats and TestsAndInterviews.

---

## 2. Update connection strings

The default connection strings point to `ASUS\SQLEXPRESS`. Replace that with **your own server name** in the following files:

| File | Key |
|------|-----|
| `PussyCats.Api/appsettings.json` | `ConnectionStrings.PussyCatsDb` |
| `PussyCats.Api/appsettings.Development.json` | `ConnectionStrings.PussyCatsDb` |
| `TestsAndInterviews/Tests_and_Interviews.API/.../appsettings.json` | `ConnectionStrings.Default` — also change `Initial Catalog` to `ISS_Merged` |

Example (replace `YOUR_SERVER` with your instance, e.g. `localhost\SQLEXPRESS` or just `localhost`):

```
Data Source=YOUR_SERVER;Initial Catalog=ISS_Merged;Integrated Security=True;Encrypt=True;Trust Server Certificate=True
```

---

## 3. Configure Multiple Startup Projects

1. Right-click the **solution** in Solution Explorer → **Set Startup Projects…**
2. Select **Multiple startup projects** and set the following to **Start**:

| Project | Action |
|---------|--------|
| `PussyCats.Api` | Start |
| `PussyCats.App` | Start |
| `PussyCats.Web` | Start |
| `Tests_and_Interviews_API` | Start |
| `Tests_and_Interviews` | Start |
| `Tests_and_Interviews.Web` | Start |

3. Click **OK**.

---

## 4. Run the app

Press **F5** (or the Start button). The APIs must be up before the desktop apps try to connect — Visual Studio starts all selected projects simultaneously, so this is handled automatically.

**Default ports:**

| Service | URL |
|---------|-----|
| PussyCats API | http://localhost:5041 |
| PussyCats Web | http://localhost:5077 |
| TestsAndInterviews API | http://localhost:5179 |
| TestsAndInterviews Web | http://localhost:5238 |
