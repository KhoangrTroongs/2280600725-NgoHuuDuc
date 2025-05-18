using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.DTOs
{
    public class ExternalLoginDTO
    {
        [Required]
        public string Provider { get; set; } = "";
        
        [Required]
        public string ProviderKey { get; set; } = "";
        
        public string? Email { get; set; }
        
        public string? Name { get; set; }
        
        public string? PhotoUrl { get; set; }
    }
}
