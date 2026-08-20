using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Lvy.Trip.Weixin.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }
    }

    public class UserAdminViewModel
    {
        public string UserName { get; set; }
        public string UserCnName { get; set; }
        public string Status { get; set; }
        public IList<EditUserViewModel> UserList { get; set; }
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "电子邮件")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "姓名")]
        public string NikeName { get; set; }
        [StringLength(100, ErrorMessage = "设置 {0} 字符长度不少于 {2} 个字符.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Display(Name = "手机号码")]
        public string MobileNumber { get; set; }
        [Display(Name = "淘宝子账号")]
        public string TaobaoSubUser { get; set; }
        public string IsOnline { get; set; }
        public DateTime? OnlineTime { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> RolesList { get; set; }
    }
}
