using System;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Data
{
    public static class MockDataGenerator
    {
        private static bool _isDataGenerated = false;

        public static void GenerateMockData()
        {
            // Check if mock data already exists to avoid duplicates
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
                    "Mock data has been generated successfully!\n\n" +
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
                var dt = DatabaseManager.ExecuteQuery("SELECT COUNT(*) FROM PartyMaster");
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
                    Email = broker.email
                });
            }
        }

        private static void GenerateParties()
        {
            var parties = new List<(string name, string address, string city, string phone, string email, double creditLimit, int creditDays, double outstanding)>
            {
                ("ABC Electronics Ltd", "123 Main Street", "Mumbai", "022-12345678", "contact@abcelectronics.com", 50000, 30, 15000),
                ("XYZ Trading Co", "456 Business Park", "Delhi", "011-87654321", "info@xyztrading.com", 75000, 45, 22000),
                ("Tech Solutions Pvt Ltd", "789 IT Hub", "Bangalore", "080-11223344", "sales@techsolutions.com", 100000, 60, 35000),
                ("Global Supplies Inc", "321 Industrial Area", "Chennai", "044-55667788", "orders@globalsupplies.com", 80000, 30, 18000),
                ("Metro Distributors", "654 Commercial Zone", "Pune", "020-99887766", "contact@metrodist.com", 60000, 15, 8000),
                ("Prime Enterprises", "987 Trade Center", "Hyderabad", "040-44556677", "info@primeent.com", 90000, 45, 28000),
                ("Sunrise Industries", "147 Manufacturing Hub", "Ahmedabad", "079-33221100", "sales@sunriseindustries.com", 70000, 30, 12000),
                ("Crystal Corp", "258 Business District", "Kolkata", "033-77889900", "orders@crystalcorp.com", 55000, 60, 31000),
                ("Dynamic Systems", "369 Tech Park", "Noida", "0120-1234567", "contact@dynamicsys.com", 85000, 45, 19500),
                ("United Traders", "741 Market Square", "Jaipur", "0141-9876543", "info@unitedtraders.com", 65000, 30, 14500)
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
                    CreditLimit = party.creditLimit,
                    CreditDays = party.creditDays,
                    OutstandingAmount = party.outstanding
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
            var items = new List<(string code, string name, string unit, double rate, double charges, double stock)>
            {
                ("ITEM001", "Laptop Computer", "PCS", 45000, 2250, 25),
                ("ITEM002", "Wireless Mouse", "PCS", 800, 40, 100),
                ("ITEM003", "Keyboard Mechanical", "PCS", 2500, 125, 50),
                ("ITEM004", "Monitor 24 inch", "PCS", 15000, 750, 30),
                ("ITEM005", "USB Cable Type-C", "PCS", 350, 18, 200),
                ("ITEM006", "External Hard Drive", "PCS", 5500, 275, 40),
                ("ITEM007", "Wireless Headphones", "PCS", 3200, 160, 60),
                ("ITEM008", "Smartphone", "PCS", 25000, 1250, 35),
                ("ITEM009", "Tablet 10 inch", "PCS", 18000, 900, 20),
                ("ITEM010", "Printer Inkjet", "PCS", 8500, 425, 15),
                ("ITEM011", "Router WiFi", "PCS", 2800, 140, 45),
                ("ITEM012", "Speaker Bluetooth", "PCS", 1500, 75, 80),
                ("ITEM013", "Power Bank", "PCS", 1200, 60, 75),
                ("ITEM014", "Camera DSLR", "PCS", 55000, 2750, 10),
                ("ITEM015", "Gaming Chair", "PCS", 12000, 600, 25)
            };

            foreach (var item in items)
            {
                ItemService.AddItem(new Item
                {
                    ItemCode = item.code,
                    ItemName = item.name,
                    Unit = item.unit,
                    Rate = item.rate,
                    Charges = item.charges,
                    StockQuantity = item.stock
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
                    BrokerName = party.BrokerName
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
                BillService.SaveBill(bill);
            }
        }

        public static void ClearAllData()
        {
            try
            {
                DatabaseManager.ExecuteNonQuery("DELETE FROM BillDetails");
                DatabaseManager.ExecuteNonQuery("DELETE FROM BillMaster");
                DatabaseManager.ExecuteNonQuery("DELETE FROM ItemMaster");
                DatabaseManager.ExecuteNonQuery("DELETE FROM PartyMaster");
                DatabaseManager.ExecuteNonQuery("DELETE FROM BrokerMaster");
                
                // Reset auto-increment counters
                DatabaseManager.ExecuteNonQuery("DELETE FROM sqlite_sequence");
                
                _isDataGenerated = false;
                
                System.Windows.Forms.MessageBox.Show(
                    "All data has been cleared successfully!",
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