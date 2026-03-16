# Smart Inventory Management System

A web-based inventory management system built with **ASP.NET Core MVC**, **C#**, and **SQL Server**.  
This project simulates a practical internal inventory management platform for businesses, supporting user account management, parts management, stock-in / stock-out tracking, and dashboard-based inventory monitoring.

## Project Overview

This project was developed to practice full-stack enterprise web application development with ASP.NET Core MVC and Entity Framework-style layered design.  
The system focuses on common inventory management scenarios in business environments, including:

- user registration and login
- profile management and password update
- parts data management
- inbound / outbound inventory transaction tracking
- dashboard visualization for inventory status

The goal of this project is not only to implement CRUD operations, but also to build a more structured inventory system with practical business flow and modular backend design.

## Core Features

### 1. Account Management
The system includes a basic account module that allows users to:

- register an account
- log in to the system / log out to the system
- edit profile information
- change password securely

### 2. Parts Management
Users can manage part records in the system, including:

- adding new parts
- editing existing parts
- deleting parts
- searching/viewing part information

This module serves as the foundation of the inventory system.

### 3. Inventory Transaction Tracking
The system supports inventory transaction management, allowing users to record:

- inbound transactions
- outbound transactions

This makes stock movement traceable and allows users to review inventory changes through transaction records.

### 4. Dashboard Monitoring
A dashboard view is included to present overall inventory-related information in a more intuitive way, helping users quickly understand current system status.

### 5. Password Security
The project includes password-related handling logic through:

- `Hash.cs`
- `PasswordService.cs`

This improves security awareness in account system development and avoids storing plain text passwords directly.

## Technical Highlights

- Built with **ASP.NET Core MVC**
- Structured with **Controllers / Models / Views**
- Includes **Repository-based data access design**
- Implements **account-related features** beyond simple CRUD
- Tracks **inventory transactions** rather than only static product data
- Applies **password hashing / service layer design**
- Organizes backend logic into reusable components

## Tech Stack

- **Backend:** C#, ASP.NET Core MVC
- **Frontend:** Razor Views, HTML, CSS
- **Database:** MSSQL Server
- **Architecture:** MVC + Repository + Service

## Screenshots

### Main Pagee
![image]((https://github.com/haibocharles/Smart-Inventory-Management-System/blob/main/LHP_Inventory_management_system_MVC/Screenshot/Main_page.png, width="800")



## Project Structure

```text
LHP_Inventory_management_system_MVC
│
├── Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   └── InventoryApiController.cs
│
├── Data
│   ├── DatabaseConnector.cs
│   ├── Hash.cs
│   ├── OrderRepository.cs
│   ├── PartRepository.cs
│   └── UserRepository.cs
│
├── Models
│   ├── Change_Password.cs
│   ├── DashboardViewModel.cs
│   ├── EditProfileViewModel.cs
│   ├── ErrorViewModel.cs
│   ├── Orders.cs
│   ├── Parts.cs
│   ├── PartsTransaction.cs
│   └── Users.cs
│
├── Service
│   └── PasswordService.cs
│
├── Views
│   ├── Account
│   │   ├── Change_Password.cshtml
│   │   ├── Edit_Profile.cshtml
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   │
│   ├── Home
│   │   ├── DashBoard.cshtml
│   │   ├── Inbound_Outbound.cshtml
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   │
│   └── Shared
│       ├── _ViewImports.cshtml
│       └── _ViewStart.cshtml
│
├── appsettings.json
└── Program.cs





