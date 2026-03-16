using Microsoft.AspNetCore.Identity;
namespace LHP_Inventory_management_system_MVC.Service
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }

    public class PasswordService : IPasswordService //繼承街口實現功能
    {
        private readonly PasswordHasher<object> _hasher = new(); // 使用ASP.NET Core的PasswordHasher來處理密碼哈希

        public string HashPassword(string password)
        {
                       return _hasher.HashPassword(null, password); // 哈希密碼
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, providedPassword); // 驗證密碼
            return result == PasswordVerificationResult.Success; // 返回驗證結果
        }



    }





}
