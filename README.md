# 💊 Medical Store Billing System

A modern **Medical Store Billing and Management System** developed using **ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server**.

The system is designed to simplify day-to-day pharmacy operations such as medicine management, inventory tracking, customer management, billing, invoice generation, and payment processing.

---

## 📌 Project Overview

The **Medical Store Billing System** is a web-based application developed to digitally manage the major operations of a medical store.

It provides a centralized platform for managing:

* 👤 Customers
* 💊 Medicines
* 📦 Medicine Inventory
* 🧾 Billing
* 🧮 Invoice Generation
* 💰 Payments
* 📊 Sales and billing information
* 🔐 User authentication and authorization

The project follows a clean and maintainable architecture using **ASP.NET Core MVC with Entity Framework Core and SQL Server**.

---

## ✨ Features

### 🔐 Authentication & Authorization

* User registration and login
* Secure authentication
* Role-based access
* Authorization for protected modules
* ASP.NET Core Identity integration

### 👤 Customer Management

* Add new customers
* View customer details
* Update customer information
* Delete customers
* Search and manage customer records

### 💊 Medicine Management

* Add medicines
* Edit medicine information
* Delete medicines
* View medicine details
* Manage medicine categories
* Maintain medicine pricing and stock information

### 📦 Inventory Management

* Track available medicine stock
* Update stock quantities
* Monitor medicine availability
* Manage inventory records
* Reduce stock automatically during billing

### 🧾 Billing & Invoice Management

* Create new bills
* Select customer and medicines
* Calculate item totals
* Calculate invoice totals
* Generate invoices
* Maintain invoice records
* Manage invoice items

### 💰 Payment Management

* Record customer payments
* Manage payment information
* Support different payment modes
* Track payment status
* Maintain payment records

### 📊 Dashboard & Management

* Centralized management dashboard
* Quick access to important modules
* Organized navigation
* User-friendly interface

---

## 🛠️ Technology Stack

| Technology                | Purpose                        |
| ------------------------- | ------------------------------ |
| **C#**                    | Programming Language           |
| **ASP.NET Core MVC**      | Web Application Framework      |
| **.NET**                  | Application Platform           |
| **Entity Framework Core** | ORM / Database Access          |
| **SQL Server**            | Relational Database            |
| **ASP.NET Core Identity** | Authentication & Authorization |
| **AutoMapper**            | Object Mapping                 |
| **Razor Views**           | UI Development                 |
| **HTML5**                 | Frontend Structure             |
| **CSS3**                  | Styling                        |
| **JavaScript**            | Client-side Functionality      |
| **Bootstrap**             | Responsive UI                  |
| **Visual Studio 2022**    | Development Environment        |
| **Git & GitHub**          | Version Control                |

---

## 🏗️ Architecture

The project follows a structured application architecture that separates responsibilities between different parts of the system.

```text
Medical_Store_Billing_System
│
├── Domain
│   ├── Entities
│   └── Enums
│
├── Application
│   ├── Services
│   ├── ViewModels
│   └── Mapping
│
├── Infrastructure
│   ├── Data
│   ├── DbContext
│   └── Database Configuration
│
└── Web
    ├── Controllers
    ├── Views
    ├── wwwroot
    └── Program.cs
```

### Request Flow

```text
User
  ↓
Razor View
  ↓
MVC Controller
  ↓
Application Service
  ↓
Entity Framework Core
  ↓
SQL Server
  ↓
Database
```

This separation helps keep the application **maintainable, scalable, and easier to debug**.

---

## 🗄️ Main Modules

### 1. Customer Module

Responsible for maintaining customer information.

```text
Customer
├── Customer Name
├── Contact Information
├── Address
└── Other Customer Details
```

### 2. Medicine Module

Responsible for medicine records and related information.

```text
Medicine
├── Medicine Name
├── Category
├── Manufacturer
├── Purchase Price
├── Selling Price
├── Quantity
└── Expiry Information
```

### 3. Billing Module

The billing module allows users to create invoices by selecting customers and medicines.

```text
Customer
   ↓
Invoice
   ↓
Invoice Items
   ↓
Medicine
   ↓
Payment
```

### 4. Payment Module

Manages payment transactions associated with invoices.

```text
Invoice
   ↓
Payment
   ├── Payment Amount
   ├── Payment Mode
   └── Payment Date
```

---

## 🗃️ Database

The application uses **Microsoft SQL Server** as the database.

Entity Framework Core is used to communicate with SQL Server and manage database operations.

### Database Technologies

* SQL Server
* Entity Framework Core
* Code First approach
* Migrations
* Foreign Key Relationships
* Primary Keys
* Data Validation

---

## 🔗 Entity Relationships

A simplified relationship structure is:

```text
Customer
   │
   │ 1
   │
   ▼
Invoice
   │
   ├───────────────┐
   │               │
   │ 1             │ 1
   ▼               ▼
InvoiceItem      Payment
   │
   │ *
   ▼
Medicine
```

---

## 📂 Project Structure

```text
Medical_Store
│
├── Medical_Store.slnx
│
├── Medical_Store_Billing_System
│   │
│   ├── Application
│   │   ├── Services
│   │   └── ViewModels
│   │
│   ├── Domain
│   │   ├── Entities
│   │   └── Enums
│   │
│   ├── Infrastructure
│   │   └── Data
│   │
│   ├── Controllers
│   │
│   ├── Views
│   │
│   ├── wwwroot
│   │   ├── css
│   │   ├── js
│   │   └── images
│   │
│   ├── Program.cs
│   └── appsettings.json
│
├── .gitignore
└── README.md
```

---

# 🚀 Getting Started

Follow these steps to run the project locally.

## ✅ Prerequisites

Before running the application, install:

* Visual Studio 2022
* .NET SDK
* SQL Server
* SQL Server Management Studio (SSMS)
* Git

Make sure the required .NET SDK version matches the project's target framework.

---

## 📥 Clone the Repository

Open Command Prompt or PowerShell and run:

```bash
git clone https://github.com/akshaychavan4510/Medicale_Store.git
```

Navigate to the project:

```bash
cd Medicale_Store
```

---

## 🗄️ Configure SQL Server

1. Open **SQL Server Management Studio**.
2. Create a database for the application.
3. Open the project's `appsettings.json`.
4. Update the SQL Server connection string according to your local SQL Server configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MedicalStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Replace `YOUR_SERVER` with your SQL Server instance name.

---

## 🔄 Apply Entity Framework Migrations

Open **Package Manager Console** in Visual Studio and run:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

This creates/updates the required database tables.

---

## ▶️ Run the Application

Open the solution:

```text
Medical_Store.slnx
```

in **Visual Studio 2022**.

Then:

1. Build the solution.
2. Set the web project as the startup project.
3. Press **F5** or **Ctrl + F5**.
4. The application will open in your browser.

---

# 🧪 Development Workflow

A typical workflow for adding a new module is:

```text
Entity
   ↓
ViewModel
   ↓
AutoMapper Profile
   ↓
Service
   ↓
Controller
   ↓
Razor View
   ↓
SQL Server
```

This approach keeps business logic separated from presentation logic.

---

# 🔒 Security

The application uses ASP.NET Core security features such as:

* Authentication
* Authorization
* ASP.NET Core Identity
* Role-based access control
* Model validation
* Entity Framework Core parameterized queries

Sensitive configuration such as database passwords should **not** be committed to GitHub.

---

# 🎯 Project Objectives

The main objectives of this project are:

* Reduce manual billing work.
* Maintain accurate medicine records.
* Simplify customer management.
* Improve inventory management.
* Generate invoices efficiently.
* Track payments.
* Reduce data-entry errors.
* Provide a centralized medical store management system.
* Create a scalable web-based application.

---

# 🔮 Future Enhancements

Possible future improvements include:

* 📊 Advanced sales dashboard
* 📈 Sales and profit reports
* 📦 Low-stock notifications
* ⏰ Medicine expiry alerts
* 🧾 PDF invoice generation
* 📧 Email invoice functionality
* 📱 Mobile-friendly improvements
* 🔍 Advanced medicine search
* 📊 Monthly and yearly sales reports
* 💳 Online payment integration
* ☁️ Cloud deployment
* 🔔 Real-time notifications

---

# 🐛 Troubleshooting

### Database Connection Error

Check:

```text
SQL Server is running
        ↓
Database exists
        ↓
Connection string is correct
        ↓
Application has database access
```

### Migration Error

Try:

```powershell
Add-Migration InitialCreate
Update-Database
```

If migrations already exist, use:

```powershell
Update-Database
```

### Build Error

Clean and rebuild the project:

```text
Build → Clean Solution
Build → Rebuild Solution
```

---



# 👨‍💻 Author

**Akshay Chavan**

MCA Graduate | Junior Software Developer

### Skills

* C#
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* React
* Angular
* HTML
* CSS
* JavaScript
* Bootstrap

---

# 🔗 Repository

**GitHub:**
https://github.com/akshaychavan4510/Medicale_Store

---


**Thank you for visiting the Medical Store Billing System repository!** 💊🚀
