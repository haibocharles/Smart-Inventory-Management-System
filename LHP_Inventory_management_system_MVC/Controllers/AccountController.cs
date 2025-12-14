using LHP_Inventory_management_system_MVC.Data;
using LHP_Inventory_management_system_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;// 用於管理HTTP請求和回應
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Security.Claims;

namespace LHP_Inventory_management_system_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository userRepository;

        public AccountController(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        // ================================
        // 登入頁面 - GET
        // ================================
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ================================
        // 登入處理 - POST
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // 1. 验证输入
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "用户名和密码不能为空");
                return View();
            }

            // 2. 验证用户凭据
            if (userRepository.ValidateUser(username, password))
            {
                var user = userRepository.GetUserByUsername(username);
                if (user != null)
                {
                    //創建身分聲明
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.login_user),
                        new Claim(ClaimTypes.NameIdentifier,user.UserID.ToString())

                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // 设置身份验证属性
                    var authProperties = new AuthenticationProperties
                    {
                        // 设置"记住我"选项
                        IsPersistent = rememberMe,
                        // 设置Cookie有效期
                        ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null //30天有效期
                    };

                    // 登录用户
                    await HttpContext.SignInAsync( //await 是用於等待異步操作完成的關鍵字 HttpContext.SignInAsync 方法是用於在ASP.NET Core中進行用戶登錄的異步方法
                        CookieAuthenticationDefaults.AuthenticationScheme,//使用Cookie身份验证方案
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    //安全重定向
                    return SafeRedirect(returnUrl);
                }
            }

            // 登入施敗
            ModelState.AddModelError(string.Empty, "Invalid username pr password");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            // 1. 验证输入
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "username or password can not be empty");
                return View();
            }
            // 2.如果密碼與確認密碼不一致
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "密码和确认密码不匹配");
                return View();
            }

            //3.如果密碼小於6位
            if (password.Length < 6)
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 6 characters long");
                return View();
            }

            //3.創建用戶
            if (userRepository.CreateUser(username, password))
            {
                TempData["SuccessMessage"] = "Successfully register, please login";
                return RedirectToAction("Login", "Account"); // 成功後跳轉到登入頁面
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Username already exists, Please choose another one");
            }
            return View();

        }


        [HttpGet]
        public IActionResult Register()
        {
            // 如果用户已登录，重定向到首页
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        // ================================
        // 安全重定向方法
        // ================================
        private IActionResult SafeRedirect(string returnUrl)
        {
            // 检查是否为本地URL
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // 默认重定向到首页
            return RedirectToAction("Index", "Home");
        }

        // ================================
        // 登出處理
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // 清除会话
            HttpContext.Session.Clear();

            // 清除"记住我"Cookie
            Response.Cookies.Delete("RememberMeToken");

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit_Profile()
        {
            // 確保用戶已登錄
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            // 獲取當前登錄用戶的用戶名
            var username = User.Identity.Name;
            var user = userRepository.GetUserByUsername(username);//獲取使用者名字

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // 轉換為視圖模型 - 確保使用 EditProfileViewModel
            var viewModel = new EditProfileViewModel
            {
                UserID = user.UserID,
                Username = user.login_user,
                Email = user.Email,
                FullName = user.FullName,
                Department = user.Department,
                EmployeeId = user.EmployeeId,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                JoinDate = user.JoinDate,
                Photo = user.Photo
            };

            // 準備部門下拉列表
            ViewBag.Departments = GetDepartmentList();

            return View(viewModel); // 確保返回 viewModel，不是 user
        }





        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Edit_Profile(EditProfileViewModel model)
        {
            Console.WriteLine("=== 開始處理 Edit_Profile POST 請求 ===");

            // 詳細記錄模型狀態
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"字段 '{state.Key}' 錯誤: {error.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "請修正表單中的錯誤";
                ViewBag.Departments = GetDepartmentList();
                return View(model);
            }

            try
            {
                var username = User.Identity.Name;
                Console.WriteLine($"當前登入用戶: {username}");

                var user = userRepository.GetUserByUsername(username);
                Console.WriteLine($"從數據庫獲取的用戶: {user != null}");

                if (user == null)
                {
                    Console.WriteLine("用戶不存在");
                    TempData["ErrorMessage"] = "User not found";
                    ViewBag.Departments = GetDepartmentList();
                    return View(model);
                }

                // 處理文件上傳
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    Console.WriteLine($"檢測到文件上傳: {model.PhotoFile.FileName}, 大小: {model.PhotoFile.Length}");
                    model.Photo = await SaveProfileImage(model.PhotoFile);
                    Console.WriteLine($"文件保存路徑: {model.Photo}");
                }
                else
                {
                    model.Photo = user.Photo;
                    Console.WriteLine($"未上傳新文件，使用原有照片: {model.Photo}");
                }

                model.UserID = user.UserID;
                Console.WriteLine($"準備更新用戶 ID: {model.UserID}");

                // 記錄要更新的數據
                Console.WriteLine($"更新數據 - Email: {model.Email}");
                Console.WriteLine($"更新數據 - FullName: {model.FullName}");
                Console.WriteLine($"更新數據 - Department: {model.Department}");
                Console.WriteLine($"更新數據 - EmployeeId: {model.EmployeeId}");
                Console.WriteLine($"更新數據 - PhoneNumber: {model.PhoneNumber}");
                Console.WriteLine($"更新數據 - BirthDate: {model.BirthDate}");
                Console.WriteLine($"更新數據 - JoinDate: {model.JoinDate}");

                // 執行更新
                bool updateResult = userRepository.UpdateUserProfile(model);
                Console.WriteLine($"數據庫更新結果: {updateResult}");

                if (updateResult)
                {
                    Console.WriteLine("更新成功");
                    TempData["SuccessMessage"] = "個人資料更新成功！";
                }
                else
                {
                    Console.WriteLine("更新失敗");
                    TempData["ErrorMessage"] = "更新個人資料失敗，請重試";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生異常: {ex.Message}");
                Console.WriteLine($"堆棧跟踪: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"更新時發生錯誤: {ex.Message}";
            }

            ViewBag.Departments = GetDepartmentList();
            return View(model);
        }

        /// <summary>
        /// 保存头像文件到服务器
        /// </summary>


        private async Task<string> SaveProfileImage(IFormFile photoFile)
        {
            if (photoFile == null || photoFile.Length == 0)
            {
                return null;
            }

            //确保上传的是图片文件
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photoFile.FileName).ToLower(); ; //取得文件副檔名並轉成小寫

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Invalid file type. Only image files are allowed.");
            }

            //建立存放上傳檔案的資料夾路徑

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile_images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名

            var uniqueFileName = $"avatar_{User.Identity.Name}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}"; //表示使用者名稱與時間戳記
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 保存文件到服务器

            using (var stream = new FileStream(filePath, FileMode.Create))  //FileStream 用於讀取和寫入檔案 參數是檔案路徑和檔案模式
            {
                await photoFile.CopyToAsync(stream);
            }

            // 返回相对路径（用于在网页中显示）
            return $"/uploads/profile_images/{uniqueFileName}";
        }

        /// <summary>
        /// 更改密碼
        /// </summary>
        [HttpGet]
        public  IActionResult Change_Password()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Change_Password(Change_Password model) //async 用於表示方法是異步的 並且可以包含await 關鍵字來等待異步操作完成 Task<IActionResult> 表示這個方法會返回一個實現了 IActionResult 介面的對象，並且是異步操作
        {
            if (!ModelState.IsValid)
            {
                return View(model);// 返回视图并显示验证错误
            }

            var username = User.Identity?.Name; //獲取登入名
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "無法識別當前用戶，請重新登入";
                return RedirectToAction("Login", "Account");
            }


            bool result = userRepository.Change_Password(
                username,
                model.CurrentPassword,
                model.NewPassword
            );

            if (result)
            {
                TempData["SuccessMessage"] = "Successfully update the password";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "failed to update the password");
                return View(model);
            }
        }


        /// <summary>
        /// 获取部门列表
        /// </summary>
        private List<SelectListItem> GetDepartmentList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "IT", Text = "IT Department" },
                new SelectListItem { Value = "HR", Text = "Human Resources" },
                new SelectListItem { Value = "Finance", Text = "Finance Department" },
                new SelectListItem { Value = "Operations", Text = "Operations" },
                new SelectListItem { Value = "Sales", Text = "Sales Department" },
                new SelectListItem { Value = "Marketing", Text = "Marketing Department" },
                new SelectListItem { Value = "Engineering", Text = "Engineering" },
                new SelectListItem { Value = "Design", Text = "Design Team" }
            };
        }





    }






}
