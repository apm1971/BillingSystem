# Sale Bill System

A Windows-based sales and billing management desktop application built with .NET 9.0.

## Overview

Comprehensive billing & accounting solution for businesses to manage:
- Parties (customers & suppliers)
- Items (products & services)
- Bill generation and tracking
- Advance payments
- Godown (warehouse) management

## Tech Stack

- **Framework**: .NET 9.0 Windows Forms
- **Database**: SQL Server / Access (OleDb)
- **Libraries**:
  - EPPlus - Excel operations
  - iTextSharp - PDF generation
  - Newtonsoft.Json - JSON handling

## Key Features

- License validation system
- Multi-user support with authentication
- Data export to Excel
- PDF invoice generation
- Database-driven data persistence

## Project Structure

```
├── Data/          # Database services & operations
├── Forms/         # Windows Forms UI components
├── Models/        # Data models (Party, Item, Bill, etc.)
├── Services/      # Business logic services
├── Utils/         # Utility classes
└── Database/      # Database schema & migrations
```

## Build

```bash
dotnet build
```

## Publish

```bash
dotnet publish -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true -o dist
```
