# 🏢 ERMS Global | Enterprise Resource Management System

![Status](https://img.shields.io/badge/Status-Secure_Node-0f172a?style=for-the-badge)
![Framework](https://img.shields.io/badge/ASP.NET_Core_8.0-512bd4?style=for-the-badge&logo=dotnet)
![Database](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)

ERMS Global is a high-performance, administrative dashboard designed for corporate personnel management. Built with **ASP.NET Core MVC** and a custom **Industrial-Luxury UI**, it provides a secure environment for managing human capital with audit-ready protocols.

---

## 💎 Premium Design Philosophy
The system utilizes a **"Classy Node"** aesthetic, featuring:
* **Glassmorphism Filters**: Overlapping depth effects for a modern dashboard feel.
* **Industrial Palette**: Navy (#0f172a) and Gold (#c5a059) contrast for executive appeal.
* **Adaptive Layout**: Fluid grid system optimized for high-density data viewing.
* **Micro-Animations**: Utilizing `Animate.css` for professional view transitions.

---

## 🛡️ Security Infrastructure
* **BaseController Gatekeeper**: Centralized session-based authentication prevents unauthorized URL access.
* **Secure OTP Protocol**: 6-digit One-Time Password system for administrative recovery.
* **SQL Transaction Logic**: All critical employee updates are wrapped in ACID-compliant transactions to ensure data integrity.
* **Session Shield**: Administrative identities are logged and monitored via encrypted system nodes.

---

## 🛠️ Tech Stack
* **Backend**: C# | ASP.NET Core MVC 8.0
* **Frontend**: Razor Pages, Bootstrap 5.3, Inter Font Family
* **Icons**: FontAwesome 6.4 (Pro Aesthetic)
* **Database**: Microsoft SQL Server (ADO.NET via Secure DbHelper)
* **Mailing**: SMTP Protocol integration (Gmail/Mailtrap ready)

---

## 📁 Project Architecture
```text
EmployeePortal/
├── Controllers/
│   ├── BaseController.cs    <-- Security Layer
│   ├── AccountController.cs <-- Auth Logic
│   └── EmployeeController.cs <-- CRUD Logic
├── Data/
│   └── DbHelper.cs          <-- DB Connection handling
├── Models/
│   ├── Domain/              <-- Database Entities
│   └── ViewModels/          <-- Data Transfer Objects
└── Views/
    ├── Shared/_Layout.cshtml <-- Premium Frame
    └── Employee/Index.cshtml <-- Dashboard Node
