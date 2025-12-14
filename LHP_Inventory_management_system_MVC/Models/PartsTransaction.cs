using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class PartsTransaction //元件交易屬性
    {
        public int? Id { get; set; }
        public int? PartId { get; set; }
        public string TransactionType { get; set; }
        public int? Quantity { get; set; }
        public int? OperatorId { get; set; }
        public string DeviceType { get; set; }
        public string Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

