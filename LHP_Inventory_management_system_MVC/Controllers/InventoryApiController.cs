using Google.Protobuf.WellKnownTypes;
using LHP_Inventory_management_system_MVC.Data;
using LHP_Inventory_management_system_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Mysqlx.Crud;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LHP_Inventory_management_system_MVC.Controllers
{
    [Route("[controller]")]
    public class InventoryApiController : Controller
    {
        private readonly OrderRepository _orderRepository;
        private readonly PartRepository _partRepository;
        private readonly ILogger<InventoryApiController> _logger;

        public InventoryApiController(OrderRepository orderRepository, PartRepository partRepository, ILogger<InventoryApiController> logger)
        {
            _orderRepository = orderRepository;
            _partRepository = partRepository;
            _logger = logger;
        }

        [HttpGet("{partId}/trend")]
        public async Task<IActionResult> GetInventoryTrend(int partId)
        {
            try
            {
                // 獲取零件信息
                var part = _partRepository.GetPartById(partId);
                if (part == null)
                {
                    return NotFound(new { success = false, message = "Part not found" });
                }

                // 獲取所有訂單記錄
                var orders = _orderRepository.GetOrders() ?? new List<Orders>();

                // 過濾該零件的訂單
                var partOrders = orders.Where(o => o.PartCode == part.Code)
                    .OrderBy(o => o.OrderDate)
                    .ToList();

                // 計算庫存趨勢
                var inventoryData = CalculateInventoryTrend(partOrders, part.CurrentStock);

                return Ok(new  // 修正這裡
                {
                    success = true,
                    labels = inventoryData.Select(d => d.Date.ToString("MMM dd")).ToArray(),
                    values = inventoryData.Select(d => d.StockLevel).ToArray(),
                    partName = part.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory trend for partId {PartId}", partId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private List<InventoryDataPoint> CalculateInventoryTrend(List<Orders> orders, int currentStock)
        {
            var dataPoints = new List<InventoryDataPoint>();

            // 如果沒有歷史數據, 返回當前庫存
            if (!orders.Any())
            {
                dataPoints.Add(new InventoryDataPoint
                {
                    Date = DateTime.Now,
                    StockLevel = currentStock
                });
                return dataPoints;
            }

            // 從最早的訂單開始計算庫存
            int runningStock = 0;

            foreach (var order in orders)
            {
                if (order.OrderType == "Inbound")
                {
                    runningStock += order.Quantity;
                }
                else if (order.OrderType == "Outbound")
                {
                    runningStock -= order.Quantity;
                }

                dataPoints.Add(new InventoryDataPoint
                {
                    Date = order.OrderDate,
                    StockLevel = runningStock
                });
            }

            return dataPoints; // 修正：移動到 foreach 循環外部
        }
    }

    // 輔助類
    public class InventoryDataPoint
    {
        public DateTime Date { get; set; }
        public int StockLevel { get; set; }
    }
}