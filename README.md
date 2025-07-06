# Sale Bill System

A .NET-based sales and billing management system that helps track parties, items, and bills.

## Features

- Party management
- Item management
- Bill generation and tracking
- Database integration

## Prerequisites

- .NET Framework 4.7.2 or higher
- SQL Server/SQL Express (for database functionality)
- Visual Studio 2019 or higher (recommended for development)

## Setup Instructions

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/your-username/sale_project.git
   cd sale_project
   ```

2. Open the solution file `SaleBillSystem.NET.sln` in Visual Studio.

3. Restore NuGet packages:
   - Right-click on the solution in Solution Explorer
   - Select "Restore NuGet Packages"

4. Build the solution:
   - Press Ctrl+Shift+B or select Build > Build Solution

### Database Configuration

1. Update the connection string in `DatabaseManager.cs` to point to your SQL Server instance.
2. Run the application once to initialize the database (if using Entity Framework Code First).

## Usage

1. Launch the application from Visual Studio or run the executable from the build directory.
2. Use the main form to navigate through different modules:
   - Party Master: Manage customer and supplier information
   - Item Master: Manage product information
   - Bill Generation: Create and manage bills

## Project Structure

- **Data/**: Contains services for database operations
  - BillService.cs: Handles bill-related operations
  - ItemService.cs: Handles item-related operations
  - PartyService.cs: Handles party-related operations
  - DatabaseManager.cs: Manages database connections

- **Models/**: Contains data models
  - Bill.cs: Bill data structure
  - BillItem.cs: Bill item data structure
  - Item.cs: Item data structure
  - Party.cs: Party data structure

- **Forms/**: Contains UI components
  - MainForm.cs: Main application interface
  - PartyMasterForm.cs: Party management interface

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 