using System.Security.Cryptography;
using System.Text;

namespace LHP_Inventory_management_system_MVC.Data
{
    public static class Hash
    {
        /// <summary>
        /// 将明文密码转为 SHA256 哈希字符串
        /// </summary>
        
        public static string ComputeSha256Hash(string rawPassword)
        {
            /// 檢查是否為空
            if (string.IsNullOrEmpty(rawPassword))
                return string.Empty;

            using (SHA256 sha256Hash = SHA256.Create()) //SHA是一種單向加密算法 sha256Hash 是 SHA256 的實例
            {
                //步驟一: 將輸入字符串轉換為字節數組並計算哈希值
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));

                // 步驟二: 將字節數組轉換為十六進制字符串

                StringBuilder builder = new StringBuilder();

                for (int i =0; i <bytes.Length;i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // 轉換為十六進制格式 
                }

                return builder.ToString();// 返回哈希字符串
            }
        }
    }
}