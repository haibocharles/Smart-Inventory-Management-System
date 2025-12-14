using LHP_Inventory_management_system_MVC.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LHP_Inventory_management_system_MVC.Data
{
    public class PartRepository
    {
        /// <summary>
        /// 備件（Parts）資料表的資料存取層（Repository）
        /// 負責對 Parts 表進行 CRUD（增刪查改）操作
        /// </summary>
        private readonly string _connectionString;// 資料庫連接字串 readonly 表示只能在建構函式中設定

        /// <summary>
        /// 建構函式：初始化資料庫連線字串
        /// </summary>
        /// <param name="connectionString">MySQL 資料庫連線字串</param>

        public PartRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 查詢操作（Read Operations）

        /// <summary>
        /// 取得所有備件資料（Read - 查詢全部）
        /// </summary>
        /// <returns>包含所有備件的 List<Part> 集合</returns>
        public List<Parts> GetParts()
        {
            var parts = new List<Parts>(); //創建空的備件列表

            try
            {
                using (var connection = new MySqlConnection(_connectionString)) //使用 MySQL連接
                {
                    connection.Open(); //打開連接
                    var read_sequence = "SELECT * FROM Parts ORDER BY Id DESC"; //SQL 查詢語句

                    using (var command = new MySqlCommand(read_sequence, connection)) //是 MySQL 資料庫操作中用來執行 SQL 指令的物件
                    using (var reader = command.ExecuteReader()) // 用來讀取資料庫查詢結果的物件
                    {
                        while (reader.Read()) //逐行讀取查詢結果
                        {
                            //將資料庫讀取到的資料映射到 Part 物件
                            var part = MapReaderToPart(reader);
                            parts.Add(part);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception (記錄異常)
                throw new Exception($"取得備件列表時發生錯誤: {ex.Message}", ex);
            }
            return parts; //返回備件列表
        }

        /// <summary>
        /// 取得單一備件資料（Read - 查詢全部）
        /// </summary>
        /// <returns>單一備件</returns>
        /// 
        public Parts? GetPartByCode(string Code)
        {
            Parts? part = null;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var sql = "SELECT * FROM Parts WHERE Code = @Code";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Code", Code);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                part = MapReaderToPart(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"根據 Code 取得備件時發生錯誤: {ex.Message}", ex);
            }

            return part;
        }







        /// <summary>
        /// 新增備件資料
        /// </summary>
        /// <param name="part">要新增的 Parts 物件</param>
        /// <returns>新增成功的備件 ID</returns>

        public int AddParts(Parts parts)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open(); //打開連接
                    var insert_sequence = @"
                        INSERT INTO Parts (Code, Name, Supplier, MinStock, CreatedAt, UpdatedAt, CurrentValue, AverageValue)
                        VALUES (@Code, @Name, @Supplier, @MinStock, @CreatedAt, @UpdatedAt, @CurrentValue, @AverageValue);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(insert_sequence, connection))
                    {
                        SetPartParameters(command, parts);

                        var newId = Convert.ToInt32(command.ExecuteScalar());
                        return newId;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"新增備件時發生錯誤: {ex.Message}", ex);

            }
        }


        /// <summary>
        /// 更新備件資料
        /// </summary>
        /// <param name="part">要更新的 Parts 物件</param>
        /// <returns>更新成功的備件 ID</returns>

        public int UpdateParts(Parts parts)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open(); //打開連接
                    var update_sequence = @"
                        UPDATE Parts SET
                                Code = @Code,
                                Name = @Name,
                                Supplier = @Supplier,
                                MinStock = @MinStock,
                                CurrentValue =  @CurrentValue,
                                AverageValue = @AverageValue


                         WHERE Id = @Id;";


                    var command = new MySqlCommand(update_sequence, connection);
                    SetPartParameters(command, parts);
                    command.Parameters.AddWithValue("@Id", parts.Id);

                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新備件時發生錯誤: {ex.Message}", ex);
            }
            ;
        }


        /// <summary>
        /// 刪除備件資料
        /// </summary>
        /// <param name="part">要刪除的 Parts 物件</param>
        /// <returns>刪除成功的備件 ID</returns>
        /// 
        public int deleteParts(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var delete_sequence = "DELETE FROM Parts WHERE Id = @Id;";

                    using (var command = new MySqlCommand(delete_sequence, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"刪除備件時發生錯誤: {ex.Message}", ex);
            }
        }

        public Parts? GetPartById(int id)
        {
            Parts? part = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var sql = "SELECT * FROM Parts WHERE Id = @Id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                part = MapReaderToPart(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"根據 Id 取得備件時發生錯誤: {ex.Message}", ex);
            }

            return part;
        }

        /// <summary>
        /// 搜索備件資料
        /// </summary>
        /// <param name="part">要搜索的 Parts 物件</param>
        /// <returns>搜索成功的備件 ID</returns>
        /// 

        /// <summary>
        /// 搜索备件
        /// </summary>
        /// <param name="searchString">搜索关键词</param>
        /// <returns>符合条件的备件列表</returns>
        public List<Parts> SearchParts(string searchString)
        {
            var parts = new List<Parts>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var sql = @"
                SELECT * FROM Parts 
                WHERE Code LIKE @SearchString 
                   OR Name LIKE @SearchString 
                   OR Supplier LIKE @SearchString 
                ORDER BY Id DESC";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SearchString", $"%{searchString}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var part = MapReaderToPart(reader);
                                parts.Add(part);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"搜索备件时发生错误: {ex.Message}", ex);
            }
            return parts;
        }

        #endregion

        /// <summary>
        /// 將 SqlDataReader 的資料映射到 Parts 物件
        /// </summary>
        /// <param name="reader">SqlDataReader 物件</param>
        /// <returns>映射完成的 Parts 物件</returns>
        private Parts MapReaderToPart(MySqlDataReader reader)
        {
            return new Parts
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),           // 備件ID（主鍵）
                Code = reader.GetString(reader.GetOrdinal("Code")),              // 備件編號
                Name = reader.GetString(reader.GetOrdinal("Name")),              // 備件名稱
                Supplier = reader.GetString(reader.GetOrdinal("Supplier")),      // 供應商
                MinStock = reader.GetInt32(reader.GetOrdinal("MinStock")),       // 安全庫存量
                CurrentStock = reader.GetInt32(reader.GetOrdinal("CurrentStock")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),  // 建立時間
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),   // 更新時間
                CurrentValue = reader.GetDecimal("CurrentValue"),// 目前庫存價值
                AverageValue = reader.GetDecimal("AverageValue"),// 平均價值
                //•	GetOrdinal("欄位名") 會根據欄位名稱取得該欄位在查詢結果中的索引（位置），確保後續讀取資料時能正確對應欄位。
            };
        }


        /// <summary>
        /// 設定 Parts 物件的 SQL 命令參數
        /// </summary>
        /// <param name="command">MySqlCommand 物件</param>
        /// <param name="part">Parts 物件</param>
        private void SetPartParameters(MySqlCommand command, Parts part)
        {
            var now = DateTime.Now;

            command.Parameters.AddWithValue("@Code", part.Code);
            command.Parameters.AddWithValue("@Name", part.Name);
            command.Parameters.AddWithValue("@Supplier", (object)part.Supplier ?? DBNull.Value);// 允許 Supplier 為 null
            command.Parameters.AddWithValue("@MinStock", part.MinStock);
            command.Parameters.AddWithValue("@CreatedAt", part.CreatedAt == default ? now : part.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", now);
            command.Parameters.AddWithValue("@CurrentValue", part.CurrentValue);
            command.Parameters.AddWithValue("@AverageValue", part.AverageValue);
        }

        /// <summary>
        /// 取得庫存不足的備件列表（當前庫存 < 安全庫存）
        /// </summary>
        /// <returns>庫存不足的備件列表</returns>

        public List<Parts> GetLowStockParts()
        {
            var lowStockParts = new List<Parts>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var sql = "SELECT * FROM Parts WHERE CurrentStock < MinStock ORDER BY Id DESC";
                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())// 用來讀取資料庫查詢結果的物件
                    {
                        while (reader.Read())//逐行讀取查詢結果
                        {
                            var part = MapReaderToPart(reader);
                            lowStockParts.Add(part);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得庫存不足的備建", ex.Message);

            }
            return lowStockParts;
        }






        //#endregion
        /// <summary>
        /// 檢查備件編號是否已存在
        /// </summary>
        /// <param name="code">備件編號</param>
        /// <param name="excludePartId">要排除的備件 ID（用於編輯時）</param>
        /// <returns>true 表示編號已存在</returns>
        /// 
        public bool CheckPartCodeExists(string code, int? excludePartId = null)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    // 打開連接
                    connection.Open();

                    // 基礎 SQL：計算符合條件的記錄數量
                    var sql = "SELECT COUNT(*) FROM Parts WHERE Code = @Code";

                    // 如果提供了 excludePartId，則在查詢中排除該 ID
                    //追加條件
                    if (excludePartId.HasValue)
                    {
                        sql += " AND Id != @ExcludeId";
                    }

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Code", code);// 備件編號參數
                        if (excludePartId.HasValue)
                        {
                            command.Parameters.AddWithValue("@ExcludeId", excludePartId.Value);// 排除的備件 ID 參數
                        }
                        // 執行查詢並取得結果
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0; // 如果計數大於 0，表示編號已存在
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"檢查重複備件編號是否存在時發生錯誤: {ex.Message}", ex);
            }
        }




        /// <summary>
        /// 更新零件库存（专用于入库出库操作）
        /// </summary>
        /// <param name="partCode">零件代码</param>
        /// <param name="quantityChange">库存变化量（正数表示入库，负数表示出库）</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateStock(string partCode, int quantityChange, decimal unitPrice = 0)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // 获取当前库存和价值信息
                    var getCurrentSql = "SELECT CurrentStock, CurrentValue, AverageValue FROM Parts WHERE Code = @PartCode";
                    decimal currentStock = 0;
                    decimal currentValue = 0;
                    decimal averageValue = 0;


                    using (var getCommand = new MySqlCommand(getCurrentSql, connection))
                    {
                        getCommand.Parameters.AddWithValue("@PartCode", partCode);
                        using (var reader = getCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentStock = reader.GetInt32("currentStock");
                                currentValue = reader.GetDecimal("CurrentValue");
                                averageValue = reader.GetDecimal("AverageValue");
                            }
                            else
                            {
                                // 零件不存在
                                return false;
                            }

                        }
                    }

                    // 计算新的库存和价值
                    decimal newStock = currentStock + quantityChange;
                    // 确保库存不为负
                    if (newStock < 0)
                    {
                        throw new Exception($"库存不足：零件 {partCode} 当前库存 {currentStock}，无法出库 {Math.Abs(quantityChange)}");
                    }

                    decimal newCurrentValue = currentValue;
                    decimal newAverageValue = averageValue;

                    if (quantityChange > 0) // 入库操作
                    {
                        //入庫重新計算價值和平均成本
                        decimal incomingValue = quantityChange * unitPrice;  //入庫市值
                        newCurrentValue = currentValue + incomingValue;

                        //計算平均成本
                        if (newStock > 0)
                        {
                            newAverageValue = newCurrentValue / newStock;
                        }
                        else
                        {
                            newAverageValue = 0;
                        }
                    }
                    else if (quantityChange < 0)//出庫操作
                    {
                        decimal outgoingValue = Math.Abs(quantityChange) * unitPrice;
                        newCurrentValue = currentValue - outgoingValue;
                        // 平均成本在出库时保持不变（移动加权平均法）
                        newAverageValue = averageValue;
                    }

                    // 更新库存和价值

                    var updateSql = @"
                          UPDATE Parts 
                                SET CurrentStock = @NewStock,
                                 CurrentValue = @NewCurrentValue,
                                    AverageValue = @NewAverageValue,
                                     UpdatedAt = @UpdatedAt
                          WHERE Code = @PartCode";

                    using (var updateCommand = new MySqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@PartCode", partCode);
                        updateCommand.Parameters.AddWithValue("@NewStock", newStock);
                        updateCommand.Parameters.AddWithValue("@NewCurrentValue", newCurrentValue);
                        updateCommand.Parameters.AddWithValue("@NewAverageValue", newAverageValue);
                        updateCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                        var rowsAffected = updateCommand.ExecuteNonQuery();
                        return rowsAffected > 0;

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"更新库存和价值时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根據所有訂單記錄重新計算零件的完整價值（包含入庫和出庫）
        /// </summary>
        public void RecalculateAllPartValuesComplete()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // 獲取所有零件
                    var parts = GetParts();

                    foreach (var part in parts)
                    {
                        Console.WriteLine($"正在處理零件: {part.Code} - {part.Name}");

                        // 1. 獲取該零件的所有入庫記錄
                        var getInboundSql = @"
                    SELECT Quantity, UnitPrice
                    FROM Orders 
                    WHERE PartCode = @PartCode AND OrderType = 'Inbound'
                    ORDER BY OrderDate";

                        decimal totalInboundValue = 0;
                        int totalInboundQuantity = 0;
                        List<(int quantity, decimal unitPrice)> inboundTransactions = new List<(int, decimal)>();

                        using (var command = new MySqlCommand(getInboundSql, connection))
                        {
                            command.Parameters.AddWithValue("@PartCode", part.Code);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int quantity = reader.GetInt32("Quantity");
                                    decimal unitPrice = reader.GetDecimal("UnitPrice");

                                    inboundTransactions.Add((quantity, unitPrice));
                                    totalInboundValue += quantity * unitPrice;
                                    totalInboundQuantity += quantity;
                                }
                            }
                        }

                        // 2. 獲取出庫記錄來驗證當前庫存
                        var getOutboundSql = @"
                    SELECT SUM(Quantity) as TotalOutboundQuantity
                    FROM Orders 
                    WHERE PartCode = @PartCode AND OrderType = 'Outbound'";

                        int totalOutboundQuantity = 0;

                        using (var command = new MySqlCommand(getOutboundSql, connection))
                        {
                            command.Parameters.AddWithValue("@PartCode", part.Code);

                            var result = command.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                totalOutboundQuantity = Convert.ToInt32(result);
                            }
                        }

                        // 3. 計算理論庫存和實際庫存
                        int calculatedStock = totalInboundQuantity - totalOutboundQuantity;
                        int actualStock = part.CurrentStock;

                        Console.WriteLine($"  入庫總數量: {totalInboundQuantity}, 出庫總數量: {totalOutboundQuantity}");
                        Console.WriteLine($"  理論庫存: {calculatedStock}, 實際庫存: {actualStock}");

                        // 4. 計算平均價值（只基於入庫記錄）
                        decimal averageValue = 0;
                        if (totalInboundQuantity > 0)
                        {
                            averageValue = totalInboundValue / totalInboundQuantity;
                        }

                        // 5. 計算當前價值
                        decimal currentValue = actualStock * averageValue;

                        Console.WriteLine($"  入庫總價值: {totalInboundValue:C}");
                        Console.WriteLine($"  平均成本: {averageValue:C}");
                        Console.WriteLine($"  當前價值: {currentValue:C}");

                        // 6. 更新零件價值
                        var updateSql = @"
                    UPDATE Parts 
                    SET AverageValue = @AverageValue, 
                        CurrentValue = @CurrentValue,
                        UpdatedAt = @UpdatedAt
                    WHERE Code = @PartCode";

                        using (var command = new MySqlCommand(updateSql, connection))
                        {
                            command.Parameters.AddWithValue("@PartCode", part.Code);
                            command.Parameters.AddWithValue("@AverageValue", averageValue);
                            command.Parameters.AddWithValue("@CurrentValue", currentValue);
                            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine($"  ✅ 價值更新成功");
                            }
                            else
                            {
                                Console.WriteLine($"  ❌ 價值更新失敗");
                            }
                        }

                        Console.WriteLine();
                    }

                    Console.WriteLine("🎉 所有零件價值重新計算完成！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 重新計算所有零件價值時發生錯誤: {ex.Message}");
                Console.WriteLine($"堆棧跟踪: {ex.StackTrace}");
                throw;
            }
        }




    }
}
