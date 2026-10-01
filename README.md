========================
### PROJECT: Vehicle Management 
### Developer: Vi Vo
========================

A small vehicle register. Each vehicle has an owner, manufacturer, year of manufacture and weight, and is **automatically placed in a weight category** (e.g. Light / Medium / Heavy). Categories, their weight ranges and their icons are managed in the app.

- **API:** 	ASP.NET Core 8 Web API, EF Core 8, SQL Server Express 2025
			API will be running on port 5255 by default. URL: http://localhost:5255/api'
			
- **Web:** 	Angular 22 (standalone components, signals, reactive forms)
			The webapp will start on port 4200. URL: http://localhost:4200/
- **Tests:** MSTest (backend), Vitest (frontend)


Note: This exercise was developed with the assistance of AI. 
AI Tool: Claude

========================
# Project folder structure
========================
CreditWorks/
├── Database/                 		SQL Server data files (CreditWorksDb.mdf / _log.ldf)
├	├── CreditWorksDb.mdf		  	SQLExpress database file. This file is automaticaly generate when execute CreateDatabase.bat
├	└── CreditWorksDb_log.ldf		SQLExpress log file. This file is automaticaly generate when execute CreateDatabase.bat
├
├── Scripts/                  		Helper .bat files (run from anywhere; each explains its usage at the top)
├	├── 0. CheckPrerequisites.bat		Check .NET 8 SDK, dotnet-ef, Node.js, npm, SQL Server Express, database file, web packages, ports
├	├── 1. CreateDatabase.bat		  	To generate SQL Express database into "..\Database" folder and seed data for Categories and Manufactute
├	├── 2. BuildBackend.bat			Build .NET solution  [Debug|Release]
├	├── 3. RunTest.bat					Run backend + frontend tests, reports to Scripts\TestReports\<timestamp>\  [Debug|Release] [nopause]
├	├── 4. StartApi.bat				Start API services on http://localhost:5255/  [Debug|Release]
├	├── 5. Start_Angular_Webapp.bat	Start Angular webapp on http://localhost:4200/  [port]
├	├── 99. UpdateDatabase.bat			Generate migration script to update database. May not needed when running demo
├	└── TestReports/				Created by RunTest.bat, one sub folder per run
├
└── VehicleManagement/
    ├── VehicleManagement.sln
    ├── VehicleManagement.Domain/          Entities, value objects, domain rules
    ├── VehicleManagement.Application/     Use-case services, DTOs, interfaces
    ├── VehicleManagement.Infrastructure/  EF Core DbContext, repositories, migrations
    ├── VehicleManagement.Api/             REST controllers, configuration, startup
    ├── VehicleManagement.Test/            Backend unit/integration tests (MSTest)
    └── VehicleManagement.Web/             Angular front end


========================
### REQUIRED SOFTWARE
========================

Tool                    Version / Requirement
---------------------------------------------------------------
.NET SDK                8.0 (8.0.204)
                        Check: dotnet --version

EF Core CLI             8.0.x
                        Install:
                        dotnet tool install --global dotnet-ef --version 8.*

SQL Server Express      2025
                        Must be installed as the named instance:
                        SQLEXPRESS

Node.js                 24.x
                        Node.js 22.x LTS should also work
                        Check: node --version

npm                     11.x
                        Included with Node.js

Visual Studio 2022      17.8+ (Optional)
                        VS Code or Rider can also be used


========================
### DATABASE REQUIREMENTS
========================

- A local **SQL Server Express** instance named `SQLEXPRESS`, reachable with **Windows authentication** (`Trusted_Connection=True`).

========================
### Checking prerequisites
========================

Run `Scripts\CheckPrerequisites.bat`. It will check all prerequisites software required in this test for referencing only

========================
### Create database
========================

Execute script `Scripts\CreateDatabase.bat` to generate SQLEXPRESS database into `\Database` folder.
Two database files will be created as below
	`Database\CreditWorksDb.mdf`
	`Database\CreditWorksDb_log.ldf`
	
Script `99. UpdateDatabase.bat` is reserved for migration the database if there is any change in future. It is not needed to run this demo.

========================
### Configure, build & run Backend API
========================

All API settings are in `VehicleManagement/VehicleManagement.Api/appsettings.json`. The appsettings.json includes a list of predefine icons required in this test.

Execute below batch file to build the backend API:
    Scripts\3. BuildBackend.bat

Execute below batch file to start the backend API:
    Scripts\4. StartApi.bat

To verify the API: 
	API: http://localhost:5255
	Swagger: http://localhost:5255/swagger


========================
### Running Angular webapp
========================

Angluar config file is '\VehicleManagement\VehicleManagement.Web\src\environments\environment.<env>.ts'. No change required.
To start the Angular webapp, execute below script:
	'Scripts\5. Start_Angular_Webapp.bat'

URL to verify the webapp
	Webapp: http://localhost:4200/


========================
### Running the automated tests
========================

Execute script `Script\3. RunTest.bat` to run the test. It runs the backend tests, then the frontend tests, and writes a report folder for each run to `Scripts\TestReports\<yyyyMMdd-HHmmss>\`:

| File | Contents |
|---|---|
| `backend.html` | Backend results; open in a browser |
| `backend.trx` | Backend results; open in Visual Studio |
| `frontend-junit.xml` | Frontend results (JUnit XML, readable by CI tools) |
| `summary.txt` | PASSED / FAILED for each part |

It exits with code 1 if either part fails, and waits for a key at the end unless you pass `nopause`. Use `Release` while the API is running in Visual Studio, because Visual Studio locks the Debug build files.


========================
## Design notes
========================

### Solution architecture

The backend follows a layered (Clean Architecture-style) design. Dependencies point inwards, towards the domain:

```
Web (Angular)  ──HTTP/JSON──▶  Api  ──▶  Application  ──▶  Domain
                                │              ▲
                                └──▶  Infrastructure (EF Core) ──┘
```

Domain
  - Entities, value objects and business rules
  - Category resolution and validation

Application
  - Use cases, DTOs and interfaces
  - Vehicle, Category and Manufacturer services

Infrastructure
  - EF Core, SQL Server, repositories and migrations

API
  - REST controllers, error handling, Swagger, CORS and rate limiting

Web
  - Angular application organised by feature
  
  
**REST endpoints**
	Vehicles
	--------
	GET     /api/vehicles
			List vehicles with manufacturer and derived category

	GET     /api/vehicles/{id}
			Get a vehicle (404 if not found)

	POST    /api/vehicles
			Create a vehicle (201 / 400)

	PUT     /api/vehicles/{id}
			Update a vehicle (200 / 400 / 404)

	DELETE  /api/vehicles/{id}
			Delete a vehicle (204 / 404)


	Manufacturers
	-------------
	GET     /api/manufacturers
			List manufacturers for the vehicle form


	Categories
	----------
	GET     /api/categories
			List categories ordered by minimum weight

	GET     /api/categories/{id}
			Get a category (404 if not found)

	GET     /api/categories/icons
			List configured category icons

	POST    /api/categories
			Create a category (201 / 400)

	PUT     /api/categories/{id}
			Update a category (200 / 400 / 404)

	DELETE  /api/categories/{id}


### Database Design

The database contains 3 tables:

Table 1: Manufacturers
  - Seed data only; no admin screen
  - Id
  - Name (nvarchar(100))
  - Name is unique

Table 2: Vehicles
  - Seeded with 1 record for quick testing
  - Has admin screen
  - Id
  - OwnerName (nvarchar(200))
  - ManufacturerId
  - YearOfManufacture (int)
  - Weight (decimal(10,2))
  - ManufacturerId → Manufacturers.Id (Restrict delete)
  - Index on ManufacturerId

Table 3: VehicleCategories
  - Id
  - Name (nvarchar(100))
  - MinWeight (decimal(10,2))
  - MaxWeight (decimal(10,2), NULL)
  - Icon (nvarchar(100))
  - Name is unique
  - MaxWeight = NULL means no upper limit


### How the category is worked out

A vehicle's category is **calculated whenever a vehicle is read, never stored**:

1. `VehicleService` loads the vehicles and all categories.
2. `VehicleCategoryResolver.Resolve(weight, categories)` returns the single category whose range contains the weight.
3. If no category matches (the weight is in a gap), the vehicle is **Uncategorised**: `categoryName` and `categoryIcon` are `null`. More than one match can't happen, because clashing ranges are rejected, so the resolver treats it as corrupt data and throws.

Because nothing is stored, **changing a category's limits immediately re-categorises every affected vehicle**, and a vehicle's category can never be out of date. The vehicle form also shows the category live as the weight is typed, using the same rule in the browser (`categories/models/category-rules.ts`).


### Category Boundary Rules

Category boundary rules are validated on both the frontend and backend.

Administrators are allowed to configure category ranges with gaps. Any gaps are
clearly highlighted on the category listing page so they can be easily identified
and addressed by the administrator.

Category ranges cannot overlap, and boundary values are handled consistently to
ensure that a vehicle belongs to at most one category.

### Category icons

There is no admin screen for category icon in this test. 
But, these icons are configurable in the API appsetting.json file.

### Important assumptions

- Weights are in **kilograms with at most 2 decimal places**, and must be > 0.
- The year of manufacture only has to be a positive whole number (no upper or lower bound).
- Manufacturers are reference data, changed by developers through seed data and migrations; there's no manufacturer admin screen.
- One person at a time manages categories (see the concurrency limitation below).
- The app runs on a trusted local network for now: **there is no authentication**.

### Some design decisions

**The category is calculated, not stored.** 
	- There's no foreign key that can go out of date when categories change, and no background job to re-categorise vehicles. 
	- The cost is a small calculation on every read, which is fine at this scale only
**Gaps allowed, rather than requiring full coverage from 0 kg.** 
	An earlier version required the categories to cover every weight with no gaps, which meant an add, edit or delete also had to adjust neighbouring categories automatically. That was replaced by a simpler rule: a clash is rejected, and nothing else changes. To move a limit into a neighbour's range, change the neighbour first. The trade-off is that Uncategorised vehicles are possible, and the UI shows them clearly.
**Icons come from configuration.** 
**The browser repeats the API's rules (`category-rules.ts`) for instant feedback,** but the API makes the final decision.
**Sorting happens in the browser,** 

### Known limitations
- **No authentication or authorisation.** Anyone who can reach the API can create, change and delete data.
- **CORS allows any `localhost` origin.** That's convenient for development, but production should list its exact origins. `Program.cs` still has an unused `Cors:AllowedOrigins` policy for this.
- **Concurrency:** last write wins; there are no row versions. Two people saving categories at the same moment could each pass the clash check and together create a clash, because the check isn't serialised in a transaction.
- **No paging or server-side filtering.** All vehicles are loaded and sorted in the browser.
