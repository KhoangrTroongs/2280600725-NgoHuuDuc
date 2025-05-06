namespace NgoHuuDuc_2280600725.DTOs
{
    public class AuthResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
