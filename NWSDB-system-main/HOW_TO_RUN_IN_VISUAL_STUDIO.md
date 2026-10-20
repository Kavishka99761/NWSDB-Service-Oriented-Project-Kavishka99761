# National Water Supply and Drainage Board (NWSDB)
## Service-Oriented Computing (SOC) Enterprise Solution

### Solution Overview
This solution implements the complete practical requirements specified in the Cardiff Met / ICBT **CSE5013 Service Oriented Computing** coursework and the official **NWSDB Service-Oriented System Use Case Diagram**.

---

## 🏗 System Architecture & Projects Structure

```
d:\My Project\NWSDB system\
│
├── NWSDB_System.sln              # Master Visual Studio Solution File
├── NWSDB_System.slnx             # Visual Studio Solution (Modern format)
│
├── src/
│   ├── NWSDB.Core/               # Domain Entities, DTOs, Enums, NwsdbTariffCalculator
│   ├── NWSDB.Data/               # EF Core DbContext, SQLite Database, DbInitializer & Seeder
│   ├── NWSDB.Services/           # Service-Oriented Logic: Billing, Payments, Meter Readings, Partners, ClosedXML Reports
│   ├── NWSDB.WebApi/             # ASP.NET Core REST API Service Host + Swagger UI (Port 5000)
│   ├── NWSDB.WebApp/             # ASP.NET Core Razor Pages Web Portal (Port 5100)
│   └── NWSDB.DesktopApp/         # Windows Forms Operations Console + ClosedXML Excel Generator
│
└── tests/
    └── NWSDB.Tests/              # xUnit Test Suite (Tariff calculations, Payments, Bills, ClosedXML)
```

---

## 🚀 How to Run in Visual Studio (2022 / 2026)

### Option 1: Open the Solution Directly
1. Double-click `NWSDB_System.sln` to open it in **Visual Studio**.
2. Build the solution: Press `Ctrl + Shift + B` (or menu **Build -> Build Solution**).
   - All 7 projects compile cleanly with **0 errors and 0 warnings**.

### Option 2: Running Multiple Projects Simultaneously (Recommended)
To run the Web API, Customer Web Portal, and Administrator Desktop Application together:
1. In Visual Studio, right-click on the **Solution 'NWSDB_System'** in Solution Explorer -> select **Configure Startup Projects...**
2. Choose **Multiple startup projects**:
   - `NWSDB.WebApi` -> **Start**
   - `NWSDB.WebApp` -> **Start**
   - `NWSDB.DesktopApp` -> **Start** (or Start without Debugging)
3. Click **OK**, then press **F5** (or **Ctrl + F5** to start without debugging).

### Option 3: Running Individual Projects
- **Run Web API Service**:
  - Right-click `NWSDB.WebApi` -> **Set as Startup Project** -> Press **F5**.
  - Swagger UI opens automatically at: `http://localhost:5000/swagger`
- **Run Customer Web Portal**:
  - Right-click `NWSDB.WebApp` -> **Set as Startup Project** -> Press **F5**.
  - Opens in browser at: `http://localhost:5100/`
- **Run Administrator Desktop Application**:
  - Right-click `NWSDB.DesktopApp` -> **Set as Startup Project** -> Press **F5**.
  - The WinForms Login Form will appear.

---

## 🔑 Demo Credentials & Test Accounts

### 1. Administrative Access (Desktop Console & Web Portal)
- **Username**: `admin`
- **Password**: `admin123`
- **Role**: Administrator

### 2. Customer Accounts (Web Portal Login & Bill Payments)
| Account Number | Customer Name | NIC | Location | Username | Password |
|---|---|---|---|---|---|
| `ACC10001` | Sunil Perera | 198214502341 | Colombo 03 | `sunil` | `sunil123` |
| `ACC10002` | Kamala Jayasinghe | 197568903412 | Kandy | `kamala` | `kamala123` |
| `ACC10003` | Nimal Fernando | 198923405678 | Galle | `nimal` | `nimal123` |
| `ACC10004` | Fathima Rizna | 199478201944 | Gampaha | `fathima` | `fathima123` |

### 3. Third-Party Banking Partner API Keys (REST Integration)
- **Commercial Bank (ComBank Online)**: `NWSDB-COMBANK-KEY-778899`
- **Sampath Bank (Sampath Vishwa)**: `NWSDB-SAMPATH-KEY-112233`
- **Dialog Axiata (eZ Cash)**: `NWSDB-EZCASH-KEY-445566`

---

## 📊 Features Implemented

### 1. Customer Use Cases (Use Case Diagram & Web Portal)
- **Make Bill Payments**:
  - Validates account, card/bank details, amounts.
  - Settle full or partial outstanding balances.
  - Updates bill status to `Paid`.
  - Generates official `NWSDB-PAY-...` confirmation reference.
- **View Bill Details**:
  - Itemized breakdown of Water Charges, Fixed Charges, and VAT (2.5%).
- **View Current Water Usage**:
  - Monthly meter readings (Previous, Current, Units Consumed).
- **View Payment History**:
  - Verified ledger of all transactions and downloadable payment receipts.
- **Registration & Login**:
  - New customer connection registration and authentication.

### 2. Administrator Use Cases (Desktop Application & Web API)
- **Dashboard & Real-Time Monitoring**:
  - Live KPI metrics: Total Customers, Total Revenue, Pending Bills, Today's Collections.
  - Real-time audit log stream.
- **Manage Customer Accounts**:
  - Add new connection, edit details, search by NIC/Account No.
- **Update Water Usage**:
  - Record field meter readings; automatic calculation of units consumed.
- **Manage Bills**:
  - Multi-tier Domestic Tariff Engine (`NwsdbTariffCalculator`).
  - Batch monthly billing generator.
- **Manage Payment Records**:
  - Counter payment desk, search by reference, view channels.
- **Manage Third-Party Partners**:
  - Register new partner banks, generate API keys, toggle active/inactive status.
- **ClosedXML Excel Export Engine**:
  - One-click export of styled Excel spreadsheets (`.xlsx`) for payment records and billing ledgers.

---

## 🧪 Testing Suite (LO3 Verification)
Run tests in Visual Studio Test Explorer (`Ctrl + E, T`) or via command line:
```powershell
dotnet test tests/NWSDB.Tests/NWSDB.Tests.csproj
```
**Results: 15/15 Tests Passed (100% Success)**
- Tiered tariff calculations (0, 5, 10, 15 units + negative validation)
- Water usage unit calculations and reading validation
- Duplicate bill prevention and billing generation
- Payment processing, bill status updates, and receipt verification
- Third-party partner API key authorization and rejection
- ClosedXML Excel spreadsheet creation and format validation
