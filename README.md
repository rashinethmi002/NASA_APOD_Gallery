# 🌌 NASA APOD Gallery

An ASP.NET Core 10.0 MVC Web Application that consumes NASA’s **Astronomy Picture of the Day (APOD)** REST API, stores fetched records in a SQL Server database using raw **ADO.NET (`Microsoft.Data.SqlClient`)**, and displays them in a dark, responsive gallery view.

---

## 📸 Screenshots

*(Add your application screenshots in a `screenshots/` directory at the project root)*

| Home / Fetch Page | Gallery Page |
| :---: | :---: |
| ![Home Fetch Page](./screenshots/home_page.png) | ![Gallery Page](./screenshots/gallery_page.png) |

---

## ✨ Key Features & Requirements Met

- [x] **ASP.NET Core MVC Architecture**: Built with clean separation of concerns using Controllers, Services, Repositories, DTOs, and Razor Views.
- [x] **NASA APOD API Consumption**: Consumes the official NASA APOD endpoint (`/planetary/apod`) using `HttpClient` with automatic fallback to per-day retrieval for high reliability.
- [x] **Database Persistence via ADO.NET**: Uses `SqlConnection`, `SqlCommand`, and `SqlParameter` for high performance without ORM overhead.
- [x] **Duplicate Entry Prevention**: Employs a parameterized `IF NOT EXISTS` check on `dbo.APOD(Date)` and enforces a database-level `UNIQUE` constraint (`UQ_APOD_Date`).
- [x] **Raw SqlClient Retrieval**: Reads database records using `SqlDataReader` to map SQL rows to strong-typed DTO objects.
- [x] **Responsive Gallery UI**: Features a modern dark theme with glassmorphism design, SVG icons, and support for both APOD imagery and embedded YouTube/Vimeo space videos.

---

## 🛠️ Technology Stack

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Database**: Microsoft SQL Server / LocalDB / Express
- **Data Access**: `Microsoft.Data.SqlClient` (ADO.NET)
- **Frontend**: HTML5, CSS3 (Vanilla + Custom Dark Theme), Bootstrap 5, Google Fonts (Space Grotesk & Inter)
- **API**: NASA Open APIs (`api.nasa.gov`)

---

## 📋 Prerequisites

Before running the project, ensure you have the following installed:

1. **.NET 10 SDK** (or .NET 8/9 SDK depending on environment)
2. **SQL Server / SQL Server Express / LocalDB**
3. **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
4. **NASA API Key** (Get a free API key at [api.nasa.gov](https://api.nasa.gov/))

---

## 🚀 Getting Started Guide

### Step 1: Clone the Repository
```bash
git clone https://github.com/YOUR_USERNAME/NASA_APOD_Gallery.git
cd NASA_APOD_Gallery
```

### Step 2: Database Setup
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio.
2. Connect to your local SQL Server instance.
3. Open and run the included SQL creation script located at `Database/ApodDatabase.sql`:

```sql
CREATE DATABASE NASA_APOD_DB;
GO

USE NASA_APOD_DB;
GO

CREATE TABLE APOD
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Date DATE NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    Explanation NVARCHAR(MAX) NULL,
    Url NVARCHAR(2048) NOT NULL,
    MediaType NVARCHAR(50) NULL,
    ServiceVersion NVARCHAR(50) NULL,
    SavedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_APOD_Date UNIQUE (Date)
);
GO
```

*(Note: The application also includes an automatic table initializer (`EnsureTableExistsAsync`) that creates `dbo.APOD` automatically if it does not already exist when the app runs.)*

---

### Step 3: Configure Database Connection String
Open `appsettings.json` and adjust the connection string to match your SQL Server instance name:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=NASA_APOD_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
*Replace `YOUR_SERVER_NAME` with your SQL Server server name (e.g. `localhost`, `DESKTOP-XXXXX\SQLEXPRESS`, or `(localdb)\MSSQLLocalDB`).*

---

### Step 4: Configure NASA API Key

You can configure your NASA API Key using either `appsettings.json` or .NET **User Secrets** (recommended).

#### Option A: Via `appsettings.json` (Quick Setup)
Add your API key directly under `"NasaApiKey"`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=NASA_APOD_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "NasaApiKey": "YOUR_NASA_API_KEY_HERE"
}
```

#### Option B: Via .NET User Secrets (Recommended for Security)
Run the following commands in your terminal inside the project folder:

```bash
dotnet user-secrets init
dotnet user-secrets set "NasaApiKey" "YOUR_NASA_API_KEY_HERE"
```

*(You can also use `DEMO_KEY` for quick testing, though rate limits apply).*

---

### Step 5: Build and Run the Application

Execute the following commands in your terminal:

```bash
dotnet build
dotnet run
```

Once started, open your web browser and navigate to:
```text
https://localhost:7045  (or http://localhost:5000)
```

---

## 💻 How to Use the App

1. **Home / Fetch Page**:
   - Select a **Start Date** and **End Date** using the date pickers.
   - Click **Fetch** to pull images directly from NASA APOD API.
   - The app saves fetched records into the database while skipping duplicates.
2. **Gallery Page**:
   - Click **Gallery** in the navbar to view all saved APOD records stored in your SQL Server database.

---

## 📤 Submission Steps (Git & GitHub)

To push your project to GitHub for final submission:

```bash
# 1. Initialize Git repository (if not already done)
git init

# 2. Add all files
git add .

# 3. Commit your changes
git commit -m "Complete ASP.NET MVC NASA APOD Gallery with ADO.NET SqlClient and duplicate prevention"

# 4. Rename branch to main
git branch -M main

# 5. Link your GitHub remote repository
git remote add origin https://github.com/YOUR_USERNAME/NASA_APOD_Gallery.git

# 6. Push code to GitHub
git push -u origin main
```

---

## 📁 Project Structure

```text
NASA_APOD_Gallery/
├── Controllers/
│   ├── HomeController.cs        # Handles APOD API fetch request & index view
│   └── GalleryController.cs     # Displays all saved database APOD records
├── Database/
│   └── ApodDatabase.sql         # Database & Table creation SQL script
├── Models/
│   └── ApodDto.cs               # APOD API Data Transfer Object & view helpers
├── Repositories/
│   └── ApodRepository.cs        # ADO.NET SqlClient data access layer
├── Services/
│   └── NasaApodService.cs       # NASA API HttpClient service with retry logic
├── Views/
│   ├── Home/
│   │   └── Index.cshtml         # Fetch form & recent results view
│   ├── Gallery/
│   │   └── Index.cshtml         # Database APOD gallery view
│   └── Shared/
│       └── _Layout.cshtml       # Master visual layout
├── wwwroot/
│   └── images/
│       └── nasa-logo.svg        # Official NASA Meatball SVG asset
├── appsettings.json             # Configuration settings & Connection String
├── Program.cs                   # Application entry point & DI configuration
└── README.md                    # Project documentation
```
