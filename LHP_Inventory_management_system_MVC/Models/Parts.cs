using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class Parts //元件屬性
    {
        public int Id { get; set; }             // 主鍵
        public string Code { get; set; } // 備件編號，如 ASML-001
        public string Name { get; set; }        // 備件名稱
        public string Supplier { get; set; }      // 供應商

        public int MinStock { get; set; }         // 安全庫存量
        public int CurrentStock { get; set; }     // 目前庫存
        public DateTime CreatedAt { get; set; }   // 建立時間
        public DateTime UpdatedAt { get; set; }   // 更新時間

        public decimal CurrentValue { get; set; } //庫存價值

        public decimal AverageValue { get; set; } //

    }

}


