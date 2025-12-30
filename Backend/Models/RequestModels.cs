namespace petpal.API.Models
{
    // ===============================
    // AuthController 请求模型
    // ===============================

    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public UserRole Role { get; set; } = UserRole.User; // 默认宠物主人
    }

    public class LoginRequest
    {
        public string Account { get; set; } = ""; // 手机号或邮箱
        public string Password { get; set; } = "";
    }


    public class ResetPasswordRequest
    {
        public string Phone { get; set; } = "";
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    // ===============================
    // AdminController 请求模型
    // ===============================

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    public class ChangeRoleRequest
    {
        public string MemberId { get; set; } = "";
        public UserRole NewRole { get; set; }
    }

    public class RemoveMemberRequest
    {
        public string MemberId { get; set; } = "";
    }

    public class ReviewRequest
    {
        public string RequestId { get; set; } = "";
    }

    public class RejectReviewRequest : ReviewRequest
    {
        public string Reason { get; set; } = "";
    }

    // ===============================
    // UserController 请求模型
    // ===============================

    public class UpdateUserProfileRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    public class DeleteAccountRequest
    {
        public string Password { get; set; } = ""; // 确认密码
        public string Reason { get; set; } = ""; // 注销原因
    }

    public class UpdateLocationRequest
    {
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }

    // ===============================
    // SitterController 请求模型
    // ===============================

    public class SubmitAuditMaterialRequest
    {
        public string MaterialType { get; set; } = "";
        public string MaterialName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "";
    }

    public class UpdateSitterProfileRequest
    {
        public string? CareIntroduction { get; set; }
        public string? ServiceTypes { get; set; }
        public string? QualificationDocuments { get; set; }
    }
}
