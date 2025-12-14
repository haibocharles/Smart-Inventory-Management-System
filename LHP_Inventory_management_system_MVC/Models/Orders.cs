using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class Orders
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        [StringLength(20)]  // 修正為 20，與資料庫一致
        public string OrderType { get; set; } // Inbound, Outbound, Transfer (修正屬性名稱)

        public DateTime OrderDate { get; set; } = DateTime.Now; // 移除 [StringLength]

        [StringLength(100)]  // 移除 [Required]
        public string OperatorName { get; set; }

        [StringLength(200)]  
        public string SupplierOrCustomer { get; set; } // 修正屬性名稱

        [StringLength(100)]
        public string FromLocation { get; set; }

        [StringLength(100)]
        public string ToLocation { get; set; }

        [Required]
        [StringLength(100)]
        public string PartCode { get; set; } // 使用 PartCode 而不是 PartID

        [StringLength(200)]  // 移除 [Required]
        public string PartName { get; set; }

        public int Quantity { get; set; } // 

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // 移除 [Required]

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }
    }
}