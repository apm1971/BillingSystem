╔════════════════════════════════════════════════════════════════════════════╗
║              LICENSE GENERATOR TOOL - INSTRUCTIONS                         ║
║                      FOR DEVELOPER USE ONLY                                ║
╚════════════════════════════════════════════════════════════════════════════╝

⚠️  WARNING: DO NOT DISTRIBUTE THIS FOLDER TO CUSTOMERS!
    Keep this tool only on YOUR computer.

═══════════════════════════════════════════════════════════════════════════════
HOW TO RUN THIS TOOL:
═══════════════════════════════════════════════════════════════════════════════

STEP 1: Open Command Prompt or PowerShell

STEP 2: Navigate to this folder:
        cd C:\billingsystem\BillingSystem\LicenseGeneratorTool

STEP 3: Build and run:
        dotnet run

═══════════════════════════════════════════════════════════════════════════════
HOW TO GENERATE A LICENSE:
═══════════════════════════════════════════════════════════════════════════════

1. Customer installs your software
2. Customer sees their Hardware ID (e.g., "A1B2-C3D4-E5F6-G7H8")
3. Customer sends you: Hardware ID + Payment
4. You run this tool
5. Enter: Hardware ID and Customer Name
6. Tool generates: License Key
7. Send the License Key to customer
8. Customer enters Name + License Key in software → Activated!

═══════════════════════════════════════════════════════════════════════════════
IMPORTANT NOTES:
═══════════════════════════════════════════════════════════════════════════════

• The SECRET_KEY in this tool MUST match the one in:
  Utils/LicenseManager.cs (in your main application)

• If you change the SECRET_KEY, all previously generated licenses will stop
  working! Only change it BEFORE distributing your software.

• Each license is tied to a specific Hardware ID. If customer changes their
  computer, they will need a new license key.

═══════════════════════════════════════════════════════════════════════════════

