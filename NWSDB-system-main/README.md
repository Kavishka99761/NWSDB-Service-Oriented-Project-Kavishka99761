# National Water Supply & Drainage Board (NWSDB)
## Service-Oriented Management & Billing System

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 13](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-SQLite-blue)](https://docs.microsoft.com/ef/core/)
[![Tests](https://img.shields.io/badge/xUnit-15%20Passed-brightgreen)](tests/NWSDB.Tests)
[![ClosedXML](https://img.shields.io/badge/Excel-ClosedXML-107c41)](https://github.com/ClosedXML/ClosedXML)

An enterprise-grade, loosely coupled **Service-Oriented Computing (SOC)** solution engineered for the **National Water Supply and Drainage Board (NWSDB), Sri Lanka**. The system connects consumers, operational branch staff, and external commercial financial institutions through standardized services, multi-channel client applications, and real-time transaction auditing.

---

## 🏛 Architecture & Component Structure

The solution adheres strictly to **Service-Oriented Architecture (SOA)** and **Clean Architecture** principles, organized into 6 distinct modular projects:

```
NWSDB_System.sln
├── src/
│   ├── NWSDB.Core/            # Enterprise Domain Entities, DTOs, Enums & Tariff Calculation Engine
│   ├── NWSDB.Data/            # EF Core DbContext, Model Mappings & Seed Data Initializer
│   ├── NWSDB.Services/        # Reusable Service Contracts, Business Logic & ClosedXML Excel Engine
│   ├── NWSDB.WebApi/          # ASP.NET Core REST API & Swagger / OpenAPI 3.0 Documentation
│   ├── NWSDB.WebApp/          # Public Web Portal & Customer Self-Service Portal (Razor Pages)
│   └── NWSDB.DesktopApp/      # Operations & Administrative Console (.NET 9 WinForms)
├── tests/
│   └── NWSDB.Tests/           # 15 Unit & Integration Test Cases (100% Pass Rate)
├── tools/
│   └── NWSDB.ScreenshotGenerator/ # Headless automation utility for verification & reporting
└── screenshots/               # Complete portfolio of 20 high-resolution evidence screenshots
```

---

## 🚀 Key Functional Modules

### 1. Official Sri Lankan Domestic Tariff Engine
* Implements the gazetted progressive tiered domestic tariff scale:
  * **0 – 5 units**: Rs. 60 / unit (Fixed charge: Rs. 300)
  * **6 – 10 units**: Rs. 80 / unit (Fixed charge: Rs. 400)
  * **11 – 15 units**: Rs. 100 / unit (Fixed charge: Rs. 500)
  * **16 – 20 units**: Rs. 130 / unit (Fixed charge: Rs. 600)
  * **21 – 25 units**: Rs. 160 / unit (Fixed charge: Rs. 800)
  * **26 – 30 units**: Rs. 200 / unit (Fixed charge: Rs. 1,000)
  * **> 30 units**: Rs. 250 / unit (Fixed charge: Rs. 1,200)
* Calculates statutory **18% Value Added Tax (VAT)** and total bill balances.

### 2. Operations Desktop Console (WinForms)
* **Live KPI Dashboard**: Real-time counter metrics for Total Customers, Revenue Collected, and Unpaid Bills.
* **ClosedXML Excel Export**: Single-click generation of styled, audited `.xlsx` payment ledgers with currency formulas and headers.
* **Customer Management**: Register new water service connections and look up accounts by NIC or account number.
* **Water Usage Recording**: Record monthly meter readings with automatic consumption validation ($m^3$).
* **Billing Management**: Generate monthly bills and monitor settlement status.
* **Banking Partner Management**: Configure third-party banking credentials and toggle API keys.

### 3. Customer Web Portal & Public Site (ASP.NET Core)
* **Public Gateway**: 24/7 emergency hotline (1939), breakdown reporting, and SOC architectural overview.
* **Instant Online Payment**: Account lookup with immediate breakdown of overdue bills and credit card payment gateway.
* **Customer Portal**: Authenticated consumer self-service for usage tracking, bill downloading, and payment receipt history.

### 4. Third-Party Banking Interoperability (REST API)
* External financial partners (**Commercial Bank of Ceylon**, **Sampath Bank**, and **Dialog eZ Cash**) connect seamlessly via standardized REST endpoints.
* Secured via dedicated **API Keys** (`X-API-KEY` header authentication) to process remote utility bill payments.

### 5. Transaction Audit & Security Governance
* Comprehensive logging of every critical system transaction (logins, bill generations, payments, partner queries) including timestamp, actor, reference code, and IP address.

---

## 🔑 Default Credentials & Test Accounts

### Administrator / Operations Staff:
* **Username**: `admin`
* **Password**: `admin123`

### Consumer Web Portal:
* **Username**: `sunil`
* **Password**: `sunil123` (Linked to Account `ACC10001`)

### Pre-Seeded Sample Accounts:
| Account Number | Customer Name | Area Branch | Meter Serial | Status |
|:---:|:---|:---|:---|:---:|
| `ACC10001` | Sunil Perera | Colombo Central | MTR-COL-901 | Active |
| `ACC10002` | Kusal Mendis | Kandy North | MTR-KDY-442 | Active |
| `ACC10003` | Anoma Silva | Galle Fort | MTR-GAL-118 | Active |
| `ACC10004` | Dilshan Fernando | Gampaha West | MTR-GAM-705 | Active |

---

## 🛠 Running the System in Visual Studio

1. Open `NWSDB_System.sln` in **Visual Studio 2022 or 2026**.
2. To run the **Desktop Console**:
   * Right-click `NWSDB.DesktopApp` in Solution Explorer &rarr; **Set as Startup Project**.
   * Press **F5** (or click ▶ Start).
   * Log in with `admin` / `admin123`.
3. To run the **Web Portal**:
   * Right-click `NWSDB.WebApp` &rarr; **Set as Startup Project**.
   * Press **F5** &rarr; Browser will open at `http://localhost:5100`.
4. To run the **Web API & Swagger**:
   * Right-click `NWSDB.WebApi` &rarr; **Set as Startup Project**.
   * Open `http://localhost:5000/swagger` in your browser.

---

## 🧪 Automated Test Suite (xUnit)

All 15 automated test cases verify core business logic, tariff tiers, and service boundaries:

```bash
dotnet test tests/NWSDB.Tests/NWSDB.Tests.csproj
```

**Results:**
```
Passed! - Failed: 0, Passed: 15, Skipped: 0, Total: 15, Duration: 440 ms
```

---

## 📸 Evidence Screenshots Portfolio

All 20 high-resolution screenshots are located in the [`screenshots/`](screenshots/) directory:

1. `01_Desktop_Login.png` - Staff login dialog with API connectivity monitor.
2. `02_Desktop_Tab1_Dashboard_Monitor.png` - KPI cards and real-time transaction audit stream.
3. `03_Desktop_Tab2_Payments_ClosedXML.png` - Counter payments entry form and ClosedXML Excel export trigger.
4. `04_Desktop_Tab3_Customer_Accounts.png` - Customer directory and account registration.
5. `05_Desktop_Tab4_Water_Usage.png` - Monthly meter reading and water consumption calculations.
6. `06_Desktop_Tab5_Bills_Management.png` - Monthly bill generation applying tiered tariffs.
7. `07_Desktop_Tab6_Banking_Partners.png` - Third-party banking partner API key administration.
8. `08_ClosedXML_Excel_Spreadsheet_View.png` - Visual preview of exported ClosedXML Excel financial report.
9. `09_WebApp_Home.png` - Public landing page with instant bill lookup.
10. `10_WebApp_Features.png` - System features and Service-Oriented Computing architecture blueprint.
11. `11_WebApp_Contact.png` - Regional operations contact info and breakdown emergency hotlines.
12. `12_WebApp_Online_Payment_Lookup.png` - Online bill settlement gateway with live account lookup.
13. `13_WebApp_Customer_Login.png` - Customer and staff portal authentication.
14. `14_WebApp_Customer_Register.png` - Online connection registration portal.
15. `15_WebApp_Portal_Dashboard.png` - Consumer self-service dashboard with outstanding balance.
16. `16_WebApp_Portal_Usage.png` - Multi-month water consumption trend chart.
17. `17_WebApp_Portal_Bills.png` - Itemized monthly bill invoices.
18. `18_WebApp_Portal_Payments.png` - Official customer payment receipt history.
19. `19_WebApi_Swagger_UI.png` - OpenAPI / Swagger REST API endpoints specification.
20. `20_Dotnet_TestSuite_15Passed.png` - 100% test pass verification report.
