using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class ModelController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ModelController> _logger;

        public ModelController(IWebHostEnvironment webHostEnvironment, ILogger<ModelController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        [Route("model/{*filePath}")]
        public IActionResult GetModel(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            // Sanitize the file path to prevent directory traversal attacks
            filePath = filePath.Replace("..", "").Replace("\\", "/").TrimStart('/');

            // Only allow access to files in the models directory
            if (!filePath.StartsWith("products/"))
            {
                _logger.LogWarning("Attempted to access file outside of models/products directory: {FilePath}", filePath);
                return NotFound();
            }

            // Construct the full path to the file
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "models", filePath);

            // Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Model file not found: {FilePath}", fullPath);
                return NotFound();
            }

            // Determine the content type based on the file extension
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".glb" => "model/gltf-binary",
                ".gltf" => "model/gltf+json",
                ".obj" => "text/plain",
                _ => "application/octet-stream"
            };

            // Return the file
            return PhysicalFile(fullPath, contentType);
        }
    }
}
