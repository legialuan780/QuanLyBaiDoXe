using System;
using System.Collections.Generic;

namespace QuanLyBaiDoXe.Models.Entities;

public partial class PasswordResetToken
{
    public int Id { get; set; }

    public int MaTaiKhoan { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}
