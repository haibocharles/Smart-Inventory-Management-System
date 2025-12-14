using LHP_Inventory_management_system_MVC.Data;
using LHP_Inventory_management_system_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LHP_Inventory_management_system_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PartRepository _partRepository;
        private readonly OrderRepository _orderRepository;

        /// <summary>
        /// 通过依赖注入添加 PartRepository
        /// </summary>
        public HomeController(ILogger<HomeController> logger, PartRepository partRepository, OrderRepository orderRepository)
        {
            _logger = logger;
            _partRepository = partRepository;
            _orderRepository = orderRepository;
        }

        // ================================
        // 首页显示备件列表
        // ================================
        public IActionResult Index(string searchString)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;//保留搜索字符串以便在視圖使用

                List<Parts> partslist;

                if (!string.IsNullOrEmpty(searchString)) // 如果有搜索字符串，进行过滤
                {
                    partslist = _partRepository.SearchParts(searchString);//調用搜索方法
                }
                else
                {
                    partslist = _partRepository.GetParts() ?? new List<Parts>();//沒有搜索字符串，顯示所有備件
                }

                return View(partslist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取备件列表时发生错误");
                TempData["ErrorMessage"] = "获取备件列表时发生错误，请稍后再试。";
                return View(new List<Parts>());
            }
        }

        // ================================
        // 添加设备 - GET 请求（显示表单）
        // ================================
        [HttpGet]
        public IActionResult Create_Parts_Form()
        {
            return View(new Parts());
        }

        // ================================
        // 添加设备 - POST 请求（处理表单提交）
        // ================================

        [HttpPost]
        [ValidateAntiForgeryToken] // 防止跨站请求伪造（CSRF）攻击
        public IActionResult Create_Parts(Parts newPart)
        {
            if (!ModelState.IsValid)
            {
                return View("Create_Parts_Form", newPart);
            }

            try
            {

                // 設置初始值
                newPart.CurrentStock = 0;  // 新零件庫存為0
                newPart.CurrentValue = 0;  // 新零件價值為0
                newPart.AverageValue = 0; // 新零件平均價值為0
                // 检查备件代码是否已存在
                if (_partRepository.CheckPartCodeExists(newPart.Code))
                {
                    ModelState.AddModelError("Code", "备件代码已存在，请使用其他代码。");
                    return View("Create_Parts_Form", newPart);
                }

                // 设置创建和更新时间
                newPart.CreatedAt = DateTime.Now;
                newPart.UpdatedAt = DateTime.Now;

                // 添加备件到数据库
                _partRepository.AddParts(newPart);

                TempData["SuccessMessage"] = "备件添加成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加备件时发生错误");
                ModelState.AddModelError(string.Empty, $"添加备件时发生错误: {ex.Message}");
                return View("Create_Parts_Form", newPart);
            }
        }

        
        // ================================
        // 编辑设备 - GET 请求（显示编辑表单）
        // ================================
        [HttpGet]

        public IActionResult Edits_Parts_Form(int id)
        {
            try
            {
                var parts = _partRepository.GetParts();//顯示所有備件
                var part = parts.Find(p => p.Id == id);//根據ID查找備件

                if (part == null)
                {
                    TempData["ErrorMessage"] = "未找到指定的备件";
                    return RedirectToAction(nameof(Index));
                }
                return View(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取設備發生錯誤");
                TempData["ErrorMessage"] = "獲取設備資訊發生錯誤，請稍後再试。";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]

        // ================================
        // 编辑设备 - POST 请求
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edits_Parts(Parts part)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // 获取原始记录
                var originalPart = _partRepository.GetPartById(part.Id);
                if (originalPart == null)
                {
                    TempData["ErrorMessage"] = "Part not found";
                    return RedirectToAction(nameof(Index));
                }

                // 检查Code是否已存在（排除当前记录）
                if (part.Code != originalPart.Code &&
                    _partRepository.CheckPartCodeExists(part.Code))
                {
                    TempData["ErrorMessage"] = "Part code already exists, please use a different code";
                    return RedirectToAction(nameof(Index));
                }

                // 保留原始的价值字段和创建时间
                part.CurrentValue = originalPart.CurrentValue;  // 保留当前价值
                part.AverageValue = originalPart.AverageValue;  // 保留平均价值
                part.CurrentStock = originalPart.CurrentStock;  // 保留当前库存
                part.CreatedAt = originalPart.CreatedAt;        // 保留创建时间
                part.UpdatedAt = DateTime.Now;                  // 更新修改时间

                _partRepository.UpdateParts(part);
                TempData["SuccessMessage"] = "Part updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part");
                TempData["ErrorMessage"] = $"Error updating part: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ================================
        // 获取备件数据 - 用于JS请求
        // ================================
        [HttpGet]
        public IActionResult GetPartData(int id)
        {
            try
            {
                var part = _partRepository.GetPartById(id);
                if (part == null) return NotFound();

                return Json(new
                {
                    id = part.Id,
                    code = part.Code,
                    name = part.Name,
                    supplier = part.Supplier,
                    minStock = part.MinStock,
                    currentStock = part.CurrentStock
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part data");
                return StatusCode(500, "Error getting part data");
            }
        }


        // ================================
        // 刪除设备 - POST 请求
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_Parts(int id)
        {
            try
            {
                var part = _partRepository.GetPartById(id);
                if (part == null)
                {
                    TempData["ErrorMessage"] = "Part not found";
                    return RedirectToAction(nameof(Index));
                }
                _partRepository.deleteParts(id);
                TempData["SuccessMessage"] = "Sucessfully deleted the equipment！";
                return RedirectToAction(nameof(Index));
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part");
                TempData["ErrorMessage"] = $"Error deleting part: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ================================
        // 搜索设备请求
        private List<Parts> SearchParts(string searchString) // 搜索設備
        {
            try
            {
                var partslist = _partRepository.GetParts() ?? new List<Parts>();
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    return partslist;
                }
                searchString = searchString.ToLower();
                var filteredParts = partslist.FindAll(p => // 根據Code, Name, Supplier進行搜索Findall表示返回所有符合條件的
                    (!string.IsNullOrEmpty(p.Code) && p.Code.ToLower().Contains(searchString)) || 
                    (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(p.Supplier) && p.Supplier.ToLower().Contains(searchString))
                );
                return filteredParts;//返回过滤后的列表
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching parts");
                return new List<Parts>();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 跳轉到Inbound_Outbound.cshtml
        public IActionResult Inbound_Outbound()
        {
            try
            {
                // 確保永遠返回非 null 集合
                var parts = _partRepository.GetParts() ?? new List<Parts>();
               
                //獲取訂單連結
                var orders = _orderRepository.GetOrders() ?? new List<Orders>();
                ViewBag.Orders = orders;//用來

                _logger.LogInformation($"Loaded {parts.Count} parts and {orders.Count} orders");


                return View(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts for Inbound_Outbound");
                TempData["ErrorMessage"] = "Error loading inventory data";
                return View(new List<Parts>());
            }
        }

        // ================================
        // 創建入庫記錄
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateInbound(
         [FromForm] List<Orders> orders,
        [FromForm] List<Parts> newProducts)
        {
            try
            {
                _logger.LogInformation("=== CREATE INBOUND STARTED ===");//開始入庫

                // 檢查接收到的數據
                _logger.LogInformation($"Received orders count: {orders?.Count ?? 0}");
                _logger.LogInformation($"Received new products count: {newProducts?.Count ?? 0}");


                //空值或空集合檢查
                if (orders == null || !orders.Any())
                {
                    _logger.LogWarning("No orders received or orders list is empty");
                    TempData["ErrorMessage"] = "沒有提供任何產品信息";
                    return RedirectToAction(nameof(Inbound_Outbound));
                }

                // 從表單獲取共享的訂單信息
                //HttpContext.Request.Form用於讀取表單數據
                var orderNumber = HttpContext.Request.Form["OrderNumber"];
                var orderType = HttpContext.Request.Form["OrderType"];
                var operatorName = HttpContext.Request.Form["OperatorName"];
                var supplierOrCustomer = HttpContext.Request.Form["SupplierOrCustomer"];
                var remarks = HttpContext.Request.Form["Remarks"];

                _logger.LogInformation($"Shared order info - Number: {orderNumber}, Type: {orderType}, Operator: {operatorName}, Supplier: {supplierOrCustomer}");

                // 為每個產品設置共享的訂單信息和生成唯一訂單號 // ?? 空值合併運算符：如果 orderNumber 為空，則使用後面的默認值
                string baseOrderNumber = string.IsNullOrEmpty(orderNumber) ? $"IN-{DateTime.Now:yyyyMMddHHmmss}" : orderNumber;

                //遍歷所有訂單
                for (int i = 0; i < orders.Count; i++)
                {
                    //空值或空集合檢查
                    if (orders[i] != null && !string.IsNullOrEmpty(orders[i].PartCode))
                    {
                        // 為每個產品行生成唯一的訂單號
                        orders[i].OrderNumber = $"{baseOrderNumber}-{i + 1:000}";
                        orders[i].OrderType = orderType;
                        orders[i].OperatorName = operatorName;
                        orders[i].SupplierOrCustomer = supplierOrCustomer;
                        orders[i].Remarks = remarks;

                        // 設置訂單日期和計算總金額
                        orders[i].OrderDate = DateTime.Now;
                        // 如果總金額為0，自動計算（數量 × 單價）
                        if (orders[i].TotalAmount == 0 && orders[i].Quantity > 0 && orders[i].UnitPrice > 0)
                        {
                            orders[i].TotalAmount = orders[i].Quantity * orders[i].UnitPrice;
                        }

                        _logger.LogInformation($"Order {i}: OrderNumber={orders[i].OrderNumber}, PartCode={orders[i].PartCode}, Quantity={orders[i].Quantity}");
                    }
                }

                // 處理新產品 - 必須在保存訂單之前創建！
                if (newProducts != null && newProducts.Any())
                {
                    _logger.LogInformation($"Processing {newProducts.Count} new product(s)");
                    // LINQ 的 Where 方法：過濾出 Code 不為空的新產品
                    // np => !string.IsNullOrEmpty(np.Code) 是 Lambda 表達式
                    foreach (var newProduct in newProducts.Where(np => !string.IsNullOrEmpty(np.Code)))
                    {
                        _logger.LogInformation($"New product: {newProduct.Code}, {newProduct.Name}");

                        if (_partRepository.CheckPartCodeExists(newProduct.Code))
                        {
                            _logger.LogWarning($"Product code {newProduct.Code} already exists");
                            TempData["ErrorMessage"] = $"產品代碼 {newProduct.Code} 已存在";
                            return RedirectToAction(nameof(Inbound_Outbound));
                        }

                        newProduct.CreatedAt = DateTime.Now;
                        newProduct.UpdatedAt = DateTime.Now;

                        _logger.LogInformation($"Adding new product to database: {newProduct.Code}");
                        _partRepository.AddParts(newProduct);

                        // 等待一下確保新產品已創建
                        Thread.Sleep(100);
                    }

                    _logger.LogInformation("All new products created successfully");
                }

                // 處理訂單和庫存更新
                foreach (var order in orders.Where(o => !string.IsNullOrEmpty(o.PartCode)))
                {
                    // 检查是否是新产品
                    bool isNewProduct = newProducts?.Any(p => !string.IsNullOrEmpty(p.Code) && p.Code == order.PartCode) ?? false;

                    if (!isNewProduct)
                    {
                        // 使用专门的库存更新方法
                        bool stockUpdated = _partRepository.UpdateStock(order.PartCode, order.Quantity, order.UnitPrice);
                        if (stockUpdated)
                        {
                            _logger.LogInformation($"Stock updated: {order.PartCode} increased by {order.Quantity}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to update stock for: {order.PartCode}");
                            // 如果现有产品找不到，创建它
                            var newPart = new Parts
                            {
                                Code = order.PartCode,
                                Name = order.PartName ?? order.PartCode,
                                Supplier = order.SupplierOrCustomer,
                                CurrentStock = order.Quantity, // 设置初始库存
                                MinStock = 0,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CurrentValue = order.Quantity*order.UnitPrice,
                                AverageValue = order.UnitPrice
                            };
                            _partRepository.AddParts(newPart);
                            _logger.LogInformation($"Created missing product: {order.PartCode}");
                        }
                    }
                    else
                    {
                        // 对于新创建的产品，也需要更新库存
                        bool stockUpdated = _partRepository.UpdateStock(order.PartCode, order.Quantity,order.UnitPrice);
                        if (stockUpdated)
                        {
                            _logger.LogInformation($"Stock updated for new product: {order.PartCode} now has initial stock");
                        }
                    }

                    // 保存訂單
                    _logger.LogInformation($"Saving order: {order.OrderNumber} - {order.PartCode}");
                    bool orderSaved = _orderRepository.AddOrder(order);
                    _logger.LogInformation($"Order save result: {orderSaved}");
                }

                _logger.LogInformation("=== CREATE INBOUND COMPLETED SUCCESSFULLY ===");
                var updatedParts = _partRepository.GetParts() ?? new List<Parts>();
                var updatedOrders = _orderRepository.GetOrders() ?? new List<Orders>();

                ViewBag.Orders = updatedOrders;


                TempData["SuccessMessage"] = $"入庫操作成功完成！處理了 {orders.Count} 個產品";
                // 直接返回視圖，而不是重定向
                return View("Inbound_Outbound", updatedParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== CREATE INBOUND FAILED ===");
                TempData["ErrorMessage"] = $"創建入庫失敗: {ex.Message}";
                return RedirectToAction(nameof(Inbound_Outbound));
            }
        }

        // ================================
        // 創建出庫記錄
        // ================================
        public IActionResult CreateOutbound([FromForm] List<Orders> orders )
        {
            try
            {
                _logger.LogInformation("=== CREATE OUTBOUND STARTED ===");

                // 檢查接收到的數據
                _logger.LogInformation($"Received outbound orders count:{orders?.Count ?? 0}");

                if(orders==null ||!orders.Any())
                {
                    _logger.LogWarning("No outbound orders received or orders list is empty");
                    TempData["ErrorMessage"] = "沒有提供任何出庫產品信息";
                    return RedirectToAction(nameof(Inbound_Outbound));
                }

                // 從表單獲取共享的訂單信息

                var orderNumber = HttpContext.Request.Form["OutboundNumber"];
                var operatorName = HttpContext.Request.Form["OperatorName"];
                var customer = HttpContext.Request.Form["Customer"];
                var remarks = HttpContext.Request.Form["Remarks"];

                _logger.LogInformation($"Shared outbound info - Number: {orderNumber}, Operator: {operatorName}");

                // 為每個產品設置共享的訂單信息和生成唯一訂單號
                string baseOrderNumber = string.IsNullOrEmpty(orderNumber) ? $"OUT-{DateTime.Now:yyyyMMddHHmmss}" : orderNumber;

                //遍歷所有訂單

                for(int i = 0; i < orders.Count;i++)
                {
                    if (orders[i]!=null && !string.IsNullOrEmpty(orders[i].PartCode))
                        {
                        // 為每個產品行生成唯一的訂單號
                        orders[i].OrderNumber = $"{baseOrderNumber}-{i + 1:000}";
                        orders[i].OrderType = "Outbound";
                        orders[i].OperatorName = operatorName;
                        orders[i].SupplierOrCustomer = customer;
                        orders[i].Remarks = remarks;
                        // 設置訂單日期和計算總金額
                        orders[i].OrderDate = DateTime.Now;
                        // 使用填寫的單價計算總金額
                        if (orders[i].TotalAmount == 0 && orders[i].Quantity > 0 && orders[i].UnitPrice > 0)
                        {
                            orders[i].TotalAmount = orders[i].Quantity * orders[i].UnitPrice;
                        }

                        _logger.LogInformation($"Outbound Order {i}: OrderNumber={orders[i].OrderNumber}, PartCode={orders[i].PartCode}, Quantity={orders[i].Quantity}, UnitPrice={orders[i].UnitPrice}");

                    }
                }

                // 處理出庫和庫存更新
                foreach (var order in orders.Where(o => !string.IsNullOrEmpty(o.PartCode)))
                {
                    //檢查庫存是否足夠
                    var part = _partRepository.GetPartByCode(order.PartCode);
                    if(part == null)
                    {
                        _logger.LogWarning($"Product not found: {order.PartCode}");
                        TempData["ErrorMessage"] = $"產品{order.PartCode}不存在";
                        return RedirectToAction(nameof(Inbound_Outbound));
                    }


                    if(part.CurrentStock<order.Quantity)
                    {

                        _logger.LogWarning($"Insufficient stock: {order.PartCode}. Current: {part.CurrentStock}, Required: {order.Quantity}");
                        TempData["ErrorMessage"] = $"產品 {order.PartCode} 庫存不足。當前庫存: {part.CurrentStock}, 需求數量: {order.Quantity}";
                        return RedirectToAction(nameof(Inbound_Outbound));
                    }

                    //調用庫存更新方法出庫數量為負數
                    // 注意：出庫時使用填寫的單價

                    bool stockUpdated =_partRepository.UpdateStock(order.PartCode, -order.Quantity, order.UnitPrice);

                    if (stockUpdated)
                    {
                        _logger.LogInformation($"Stock updated: {order.PartCode} decreased by {order.Quantity} at unit price {order.UnitPrice}");

                    }
                    else
                    {
                        _logger.LogWarning($"Failed to update stock for: {order.PartCode}");
                        TempData["ErrorMessage"] = $"更新產品 {order.PartCode} 庫存時發生錯誤";
                        return RedirectToAction(nameof(Inbound_Outbound));
                    }

                    // 保存訂單

                    _logger.LogInformation($"Saving outbound order: {order.OrderNumber} - {order.PartCode}");
                    bool orderSaved =_orderRepository.AddOrder(order);
                    _logger.LogInformation($"Outbound order save result: {orderSaved}");
                }
                _logger.LogInformation("=== CREATE OUTBOUND COMPLETED SUCCESSFULLY ===");

                //重新加載最新數據
                var updatedParts = _partRepository.GetParts() ?? new List<Parts>();   
                var updatedOrders = _orderRepository.GetOrders() ?? new List<Orders>();

                ViewBag.Orders = updatedOrders;
                TempData["SuccessMessage"] = $"出庫操作成功完成！處理了 {orders.Count} 個產品";

                // 直接返回視圖
                return View("Inbound_Outbound", updatedParts);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "=== CREATE OUTBOUND FAILED ===");
                TempData["Error Message"] = $"創建出庫失敗{ex.Message}";
              
                return RedirectToAction(nameof(Inbound_Outbound));
            }

        }


        // ================================
        // Dashborad 儀錶板功能
        // ================================

        public IActionResult Dashboard()
        {
            try
            {
                var parts = _partRepository.GetParts() ?? new List<Parts>();
                var orders = _orderRepository.GetOrders() ?? new List<Orders>();

                var dashboardStats = CreateDashboardViewModel(parts, orders);
                ViewBag.Orders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToList();
                return View(dashboardStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Error loading dashboard data";
                return View(new DashboardViewModel());
            }
        }

        private DashboardViewModel CreateDashboardViewModel(List<Parts> parts, List<Orders> orders)
        {
            var outboundOrders = orders.Where(o => o.OrderType == "Outbound").ToList();
            var inboundOrders = orders.Where(o => o.OrderType == "Inbound").ToList();

            // 获取最近交易（按日期降序排列的前10条）
            var recentTransactions = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new Orders
                {
                    OrderNumber = o.OrderNumber,
                    OrderType = o.OrderType,
                    OrderDate = o.OrderDate,
                    PartCode = o.PartCode,
                    PartName = o.PartName,
                    Quantity = o.Quantity,
                    OperatorName = o.OperatorName,
                    TotalAmount = o.TotalAmount
                })
                .ToList();

            return new DashboardViewModel
            {
                TotalProducts = parts.Count,
                LastUpdated = parts.Any() ? parts.Max(p => p.UpdatedAt) : DateTime.Now,
                ProductCategories = parts.Select(p => p.Supplier).Distinct().Count(),
                TotalSold = outboundOrders.Sum(o => o.Quantity),
                LowStockItems = parts.Count(p => p.CurrentStock <= p.MinStock && p.CurrentStock > 0),
                OutOfStockItems = parts.Count(p => p.CurrentStock == 0),
                TotalInventoryValue = parts.Sum(p => p.CurrentValue),
                Parts = parts,
                RecentTransactions = recentTransactions // 添加最近交易数据
            };
        }
        }
    }







 