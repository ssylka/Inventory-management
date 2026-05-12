using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Inventory_Managment.Services
{
    public class ImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        // Upload a file and return its secure URL
        public async Task<string> UploadAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "inventory-managment",
                Transformation = new Transformation()
                    .Width(800).Height(800).Crop("limit")
                    .FetchFormat("auto").Quality("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl.ToString();
        }
    }
}
