using System;
using System.Data.OleDb; // Changed from SQLite
using SaleBillSystem.NET.Models;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Data
{
    public static class MockDataGenerator
    {
        private static bool _isDataGenerated = false;

        public static void GenerateMockData()
        {
            // Check if active company exists
            if (Program.ActiveCompany == null)
            {
                System.Windows.Forms.MessageBox.Show(
                    "No active company found. Please create or select a company first.",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            // Check if mock data already exists for this company to avoid duplicates
            if (_isDataGenerated || CheckIfDataExists())
            {
                return;
            }

            try
            {
                GenerateBrokers();
                GenerateParties();
                GenerateItems();
                GenerateSampleBills();
                _isDataGenerated = true;
                
                System.Windows.Forms.MessageBox.Show(
                    $"Mock data has been generated successfully for {Program.ActiveCompany.CompanyName}!\n\n" +
                    "• 5 sample brokers\n" +
                    "• 10 sample parties with credit days and brokers\n" +
                    "• 15 sample items with charges\n" +
                    "• 5 sample bills with broker information",
                    "Mock Data Generated",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error generating mock data: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private static bool CheckIfDataExists()
        {
            try
            {
                int companyID = Program.ActiveCompany.CompanyID;
                var sql = "SELECT COUNT(*) FROM PartyMaster WHERE CompanyID = ?";
                var parameter = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
                var dt = DatabaseManager.ExecuteQuery(sql, parameter);
                if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void GenerateBrokers()
        {
            var brokers = new List<(string name, string phone, string email)>
            {
                ("Rajesh Kumar", "9876543210", "rajesh.kumar@gmail.com"),
                ("Priya Sharma", "9123456789", "priya.sharma@outlook.com"),
                ("Amit Patel", "9456789123", "amit.patel@yahoo.com"),
                ("Sunita Singh", "9789123456", "sunita.singh@gmail.com"),
                ("Vikram Gupta", "9321654987", "vikram.gupta@hotmail.com")
            };

            foreach (var broker in brokers)
            {
                BrokerService.AddBroker(new Broker
                {
                    BrokerName = broker.name,
                    Phone = broker.phone,
                    Email = broker.email,
                    CompanyID = Program.ActiveCompany.CompanyID
                });
            }
        }

        private static void GenerateParties()
        {
            var parties = new List<(string name, string address, string city, string phone, string email, string gst, string pan, double openingBalance)>
            {
                ("ABC Electronics Ltd", "123 Main Street", "Mumbai", "022-12345678", "contact@abcelectronics.com", "27AADCB2230M1ZT", "AADCB2230M", 50000),
                ("XYZ Trading Co", "456 Business Park", "Delhi", "011-87654321", "info@xyztrading.com", "07AABCU9603R1ZP", "AABCU9603R", 75000),
                ("Tech Solutions Pvt Ltd", "789 IT Hub", "Bangalore", "080-11223344", "sales@techsolutions.com", "29AABCU1234R1ZU", "AABCU1234R", 100000),
                ("Global Supplies Inc", "321 Industrial Area", "Chennai", "044-55667788", "orders@globalsupplies.com", "33AADCB4567M1ZT", "AADCB4567M", 80000),
                ("Metro Distributors", "654 Commercial Zone", "Pune", "020-99887766", "contact@metrodist.com", "27AABCU7890R1ZP", "AABCU7890R", 60000),
                ("Prime Enterprises", "987 Trade Center", "Hyderabad", "040-44556677", "info@primeent.com", "36AADCP9876M1ZT", "AADCP9876M", 90000),
                ("Sunrise Industries", "147 Manufacturing Hub", "Ahmedabad", "079-33221100", "sales@sunriseindustries.com", "24AABCS5432R1ZU", "AABCS5432R", 70000),
                ("Crystal Corp", "258 Business District", "Kolkata", "033-77889900", "orders@crystalcorp.com", "19AADCC1122M1ZT", "AADCC1122M", 55000),
                ("Dynamic Systems", "369 Tech Park", "Noida", "0120-1234567", "contact@dynamicsys.com", "09AABCD3344R1ZP", "AABCD3344R", 85000),
                ("United Traders", "741 Market Square", "Jaipur", "0141-9876543", "info@unitedtraders.com", "08AADCU5566M1ZT", "AADCU5566M", 65000)
            };

            // Get brokers for assignment
            var brokers = BrokerService.GetAllBrokers();
            var random = new Random();

            for (int i = 0; i < parties.Count; i++)
            {
                var party = parties[i];
                var newParty = new Party
                {
                    PartyName = party.name,
                    Address = party.address,
                    City = party.city,
                    Phone = party.phone,
                    Email = party.email,
                    // GSTNo = party.gst,
                    // PAN = party.pan,
                    // OpeningBalance = party.openingBalance,
                    // OpeningBalanceDate = DateTime.Today.AddDays(-30),
                    CreditDays = 30 + (i * 5) % 30, // Varied credit days 30-60
                    CompanyID = Program.ActiveCompany.CompanyID
                };

                // Assign broker to some parties (about 60% of them)
                if (brokers.Count > 0 && random.NextDouble() < 0.6)
                {
                    var broker = brokers[random.Next(brokers.Count)];
                    newParty.BrokerID = broker.BrokerID;
                    newParty.BrokerName = broker.BrokerName;
                }

                PartyService.AddParty(newParty);
            }
        }

        private static void GenerateItems()
        {
            var items = new List<(string name, string unit, double rate, double charges, double stock)>
            {
                ("Laptop Computer", "PCS", 45000, 2250, 25),
                ("Wireless Mouse", "PCS", 800, 40, 100),
                ("Keyboard Mechanical", "PCS", 2500, 125, 50),
                ("Monitor 24 inch", "PCS", 15000, 750, 30),
                ("USB Cable Type-C", "PCS", 350, 18, 200),
                ("External Hard Drive", "PCS", 5500, 275, 40),
                ("Wireless Headphones", "PCS", 3200, 160, 60),
                ("Smartphone", "PCS", 25000, 1250, 35),
                ("Tablet 10 inch", "PCS", 18000, 900, 20),
                ("Printer Inkjet", "PCS", 8500, 425, 15),
                ("Router WiFi", "PCS", 2800, 140, 45),
                ("Speaker Bluetooth", "PCS", 1500, 75, 80),
                ("Power Bank", "PCS", 1200, 60, 75),
                ("Camera DSLR", "PCS", 55000, 2750, 10),
                ("Gaming Chair", "PCS", 12000, 600, 25)
            };

            foreach (var item in items)
            {
                ItemService.AddItem(new Item
                {
                    ItemName = item.name,
                    Unit = item.unit,
                    Rate = item.rate,
                    Charges = item.charges,
                    StockQuantity = item.stock,
                    CompanyID = Program.ActiveCompany.CompanyID
                });
            }
        }

        private static void GenerateSampleBills()
        {
            var random = new Random();
            var parties = PartyService.GetAllParties();
            var items = ItemService.GetAllItems();

            if (parties.Count == 0 || items.Count == 0) return;

            for (int i = 0; i < 5; i++)
            {
                var party = parties[random.Next(parties.Count)];
                var billDate = DateTime.Today.AddDays(-random.Next(30));
                var bill = new Bill
                {
                    BillNo = DatabaseManager.GetNextBillNumber(),
                    BillDate = billDate,
                    PartyID = party.PartyID,
                    PartyName = party.PartyName,
                    BrokerID = party.BrokerID,
                    BrokerName = party.BrokerName,
                    CompanyID = Program.ActiveCompany.CompanyID
                };

                // Calculate due date based on party's credit days
                bill.CalculateDueDate(party.CreditDays);

                // Add 2-4 random items to each bill
                int itemCount = random.Next(2, 5);
                for (int j = 0; j < itemCount; j++)
                {
                    var item = items[random.Next(items.Count)];
                    var quantity = random.Next(1, 6);
                    
                    var billItem = new BillItem
                    {
                        ItemID = item.ItemID,
                        ItemName = item.ItemName,
                        Quantity = quantity,
                        Rate = item.Rate,
                        Charges = item.Charges * quantity
                    };
                    billItem.Calculate();
                    
                    bill.BillItems.Add(billItem);
                }

                bill.CalculateTotals();
                int billId;
                BillService.SaveBill(bill, out billId);
            }
        }

        public static void ClearAllData()
        {
            try
            {
                int companyID = Program.ActiveCompany.CompanyID;
                
                // Delete data only for the active company
                DatabaseManager.ExecuteNonQuery("DELETE FROM BillDetails WHERE BillID IN (SELECT BillID FROM BillMaster WHERE CompanyID = ?)", 
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                DatabaseManager.ExecuteNonQuery("DELETE FROM BillMaster WHERE CompanyID = ?",
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                DatabaseManager.ExecuteNonQuery("DELETE FROM ItemMaster WHERE CompanyID = ?",
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                DatabaseManager.ExecuteNonQuery("DELETE FROM PartyMaster WHERE CompanyID = ?",
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                DatabaseManager.ExecuteNonQuery("DELETE FROM BrokerMaster WHERE CompanyID = ?",
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                
                _isDataGenerated = false;
                
                System.Windows.Forms.MessageBox.Show(
                    $"All data for {Program.ActiveCompany.CompanyName} has been cleared successfully!",
                    "Data Cleared",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error clearing data: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
} 