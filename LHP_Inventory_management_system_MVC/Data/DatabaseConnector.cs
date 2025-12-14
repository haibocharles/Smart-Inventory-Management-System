using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LHP_Inventory_management_system_MVC.Models
{
    public class DatabaseConnector  // 注意：建議類別名稱用正確大小寫，如 Connector 而非 Connecter
    {
        // 建議將連線字串設為常數或從設定檔讀取
        private static string connectionString =
            "Server=localhost;Database=InventoryManagementSystemDB;Uid=root;Pwd=Aa0925129251;";

        // 方法1：取得一個開啟的 MySqlConnection（建議用 using 包住）
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(connectionString);// 建立連線物件
            try
            {
                conn.Open(); // 嘗試開啟連線
                Console.WriteLine("My SQL database successfully connected");
                return conn; // 回傳開啟的連線
            }
            catch (Exception ex)
            {
                Console.WriteLine("My SQL database connection failed: " + ex.Message);
                throw; // 可以選擇拋出例外，讓呼叫者處理
            }
        }

    }
        
}