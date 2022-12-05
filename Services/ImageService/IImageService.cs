﻿using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.ImageService
{
    public interface IImageService
    {
        Task<ServiceResponse<ImageSavedDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, Category storyCategory);

        Task<ServiceResponse<List<ImageDb>>> CreateImages(List<ImageSavedDto> images, Guid storyId);

        Task<ServiceResponse<string>> DownloadImageToBase64(string url);

        string GetFileExtension(string url);
    }
}