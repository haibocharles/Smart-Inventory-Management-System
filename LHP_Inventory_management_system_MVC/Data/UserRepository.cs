using LHP_Inventory_management_system_MVC.Models;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Transactions;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace LHP_Inventory_management_system_MVC.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;// 資料庫連接字串

        // 构造函数：注入数据库连接字符串（与 PartRepository 一致）
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
            /// <summary>
            /// 根据用户名获取用户（用于登录验证）
            /// </summary>
            /// 
        }

        /// <summary>
        /// 根据用户名获取用户（用于登录验证）
        /// </summary>
        ///

        public Users GetUserByUsername(string username)
        {
            Users user = null;
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                // 修复 SQL 查询语句：移除多余的逗号
                string query = "SELECT id, login_user, PasswordHash, CreatedAt, Email, FullName, Department, " +
                              "EmployeeId, PhoneNumber, BirthDate, JoinDate, Photo, UpdatedAt " +
                              "FROM Users WHERE login_user = @username";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new Users
                        {
                            //左邊是C#屬性名稱  右邊是資料庫欄位名稱
                            UserID = reader.GetInt32("id"),
                            login_user = reader.GetString("login_user"),
                            PasswordHash = reader.GetString("PasswordHash"),
                            CreatedAt = reader.GetDateTime("CreatedAt"),

                            // 使用 GetOrdinal 获取列索引
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName"),
                            Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString("Department"),
                            EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeId")) ? null : reader.GetString("EmployeeId"),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString("PhoneNumber"),

                            // 处理日期字段
                            BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? (DateTime?)null : reader.GetDateTime("BirthDate"),
                            JoinDate = reader.IsDBNull(reader.GetOrdinal("JoinDate")) ? (DateTime?)null : reader.GetDateTime("JoinDate"),

                            Photo = reader.IsDBNull(reader.GetOrdinal("Photo")) ? null : reader.GetString("Photo"),
                            UpdateAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime("UpdatedAt")
                        };
                    }
                }
            }
            return user;
        }

        /// <summary>
        /// 根据用户名获取用户並進行登入驗證
        /// </summary>
        ///

        public bool ValidateUser(string username, string password)
        {
            var user = GetUserByUsername(username); // 根據用戶名獲取用戶
            if (user == null)
            {
                return false; // 用戶不存在
            }

            // 2. 将输入密码哈希后与数据库中的 PasswordHash 比对
            string inputPasswordHash = Hash.ComputeSha256Hash(password);
            return user.PasswordHash == inputPasswordHash; // 返回比对结果
        }

        /// <summary>
        /// 創建帳戶
        /// </summary>
        ///
        public bool CreateUser(string username, string password)
        {
            // 1. 检查用户名是否已存在
            if (GetUserByUsername(username) != null)
            {
                return false; // 用戶名已存在
            }

            //2.哈希密码
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string sql_query = "INSERT INTO Users (login_user, PasswordHash) VALUES (@username, @passwordHash)";
                MySqlCommand command = new MySqlCommand(sql_query, connection);
                command.Parameters.AddWithValue("@username", username);
                string passwordHash = Hash.ComputeSha256Hash(password);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0; // 返回插入結果
            }
        }

        /// <summary>
        /// 刪除用戶
        /// </summary>
        ///

        public bool DeleteUser(int userId)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string sql_query = "DELETE FROM Users WHERE id = @userId";
                MySqlCommand command = new MySqlCommand(sql_query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0; // 返回刪除結果
            }
        }

        public bool Change_Password(string username, string currentpassword, string newpassword)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // 1. 先驗證當前密碼是否正確
                    string verifyQuery = "SELECT PasswordHash FROM Users WHERE login_user = @username";
                    string storedPasswordHash = "";

                    using (MySqlCommand command = new MySqlCommand(verifyQuery, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        var result = command.ExecuteScalar(); // 執行查詢返回單一結果

                        // 如果用戶不存在
                        if (result == null || result == DBNull.Value)
                        {
                            return false; // 用戶不存在，返回失敗
                        }

                        storedPasswordHash = result.ToString(); // 獲取資料庫中存儲的密碼哈希
                    }

                    // 2. 驗證當前密碼是否正確（使用相同的哈希方法）
                    string inputCurrentPasswordHash = Hash.ComputeSha256Hash(currentpassword);
                    if (storedPasswordHash != inputCurrentPasswordHash)
                    {
                        return false; // 當前密碼不正確，返回失敗
                    }

                    // 3. 哈希新密碼並更新到資料庫
                    string newPasswordHash = Hash.ComputeSha256Hash(newpassword);
                    string updateQuery = "UPDATE Users SET PasswordHash = @newPasswordHash WHERE login_user = @username";

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@newPasswordHash", newPasswordHash);
                        command.Parameters.AddWithValue("@username", username);

                        int result = command.ExecuteNonQuery();
                        return result > 0; // 返回更新結果
                    }
                }
                catch (Exception ex)
                {
                    // 記錄錯誤信息（實際項目中應使用日誌系統）
                    Console.WriteLine($"修改密碼錯誤: {ex.Message}");
                    return false; // 發生異常，返回失敗
                }
            }
        }






        /// <summary>
        /// 更新用戶信息
        /// </summary>
        ///

        public bool UpdateUserProfile(EditProfileViewModel model)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    // 修正 SQL 查詢語句
                    string sql_query = @"
                UPDATE Users 
                SET 
                    Email = @Email,
                    FullName = @FullName,
                    Department = @Department,
                    EmployeeId = @EmployeeId,
                    PhoneNumber = @PhoneNumber,
                    BirthDate = @BirthDate,
                    JoinDate = @JoinDate,   
                    Photo = @Photo, 
                    UpdatedAt = @UpdatedAt
                WHERE id = @UserID";

                    MySqlCommand command = new MySqlCommand(sql_query, connection);

                    // 添加參數 - 添加詳細的調試信息
                    Console.WriteLine($"=== SQL Parameters ===");
                    Console.WriteLine($"UserID: {model.UserID}");
                    Console.WriteLine($"Email: {model.Email ?? "NULL"}");
                    Console.WriteLine($"FullName: {model.FullName ?? "NULL"}");
                    Console.WriteLine($"Department: {model.Department ?? "NULL"}");
                    Console.WriteLine($"EmployeeId: {model.EmployeeId ?? "NULL"}");
                    Console.WriteLine($"PhoneNumber: {model.PhoneNumber ?? "NULL"}");
                    Console.WriteLine($"BirthDate: {model.BirthDate?.ToString("yyyy-MM-dd") ?? "NULL"}");
                    Console.WriteLine($"JoinDate: {model.JoinDate?.ToString("yyyy-MM-dd") ?? "NULL"}");
                    Console.WriteLine($"Photo: {model.Photo ?? "NULL"}");
                    Console.WriteLine($"UpdatedAt: {DateTime.Now}");

                    command.Parameters.AddWithValue("@UserID", model.UserID);
                    command.Parameters.AddWithValue("@Email", (object)model.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FullName", (object)model.FullName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Department", (object)model.Department ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmployeeId", (object)model.EmployeeId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)model.PhoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BirthDate", model.BirthDate.HasValue ? (object)model.BirthDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@JoinDate", model.JoinDate.HasValue ? (object)model.JoinDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Photo", (object)model.Photo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                    connection.Open();
                    Console.WriteLine("Database connection opened successfully");

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Rows affected: {rowsAffected}");

                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== Database Error Details ===");
                    Console.WriteLine($"Error Message: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                    // 重新拋出異常，讓控制器也能捕獲
                    throw;
                }
            }
        }
    }
}

