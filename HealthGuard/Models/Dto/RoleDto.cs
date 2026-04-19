namespace HealthGuard.Models.Dto
{
    public class RoleDto
    {
        // ID của quyền (Primary Key trong Database)
        public long Id { get; set; }

        // Tên quyền (Ví dụ: ROLE_ADMIN, ROLE_USER)
        // Trong Service đã có logic tự động viết hoa và thêm tiền tố ROLE_
        public string RoleName { get; set; }
    }
}