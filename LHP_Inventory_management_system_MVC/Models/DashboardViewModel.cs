namespace LHP_Inventory_management_system_MVC.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ProductCategories { get; set; }
        public int TotalSold { get; set; }
        public decimal MonthlyIncome { get; set; }

        public List<Parts> Parts { get; set; } = new List<Parts>();

        public List<Orders> RecentTransactions { get; set; } = new List<Orders>();

        // 可擴充儀表板属性
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal TotalInventoryValue { get; set; }


    }
}
