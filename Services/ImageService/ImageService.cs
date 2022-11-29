﻿using ImageMagick;
using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.ImageService
{
    public class ImageService : IImageService
    {
        public async Task<ServiceResponse<ImageSavedDto>> SaveImage(string imageAsBase64, string imageFileType)
        {
            var serviceResponse = new ServiceResponse<ImageSavedDto>();
            var imageSaveDto = new ImageSavedDto();

            imageSaveDto.Id = Guid.NewGuid();
            imageSaveDto.Location = $"./Images/{imageSaveDto.Id}.{imageFileType}";

            byte[] bytes = Convert.FromBase64String(imageAsBase64);

            var image = new MagickImage(bytes);

            switch (imageFileType)
            {
                case "jpeg":
                    image.Format = MagickFormat.Jpeg;
                    break;

                case "png":
                    image.Format = MagickFormat.Png;
                    break;

                case "gif":
                    image.Format = MagickFormat.Gif;
                    break;

                default:
                    // Exit
                    break;
            }
            try
            {
                await image.WriteAsync(imageSaveDto.Location);
                serviceResponse.Data = imageSaveDto;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}