using Microsoft.EntityFrameworkCore;
using newsApi.Data;
using newsApi.Models;

namespace newsApi.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly DataContext _context;

        public TagService(DataContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Tag>>> CreateTag(Tag tag)
        {
            var serviceResponse = new ServiceResponse<List<Tag>>();
            var tags = await GetAllTags();
            var tagExist = await CheckIfTagExist(tag.TagName);

            if (tagExist)
            {
                serviceResponse.Data = tags;
                serviceResponse.Success = false;
                serviceResponse.Message = "Tag already exist";
            }

            try
            {
                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();
                tags.Add(tag);
                serviceResponse.Data = tags;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<Tag>>> GetTags()
        {
            var serviceResponse = new ServiceResponse<List<Tag>>();
            try
            {
                var tags = await GetAllTags();
                serviceResponse.Data = tags;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        private async Task<bool> CheckIfTagExist(string tagName)
        {
            var tags = await _context.Tags.ToListAsync();
            if (tags.Any(t => t.TagName == tagName))
            {
                return true;
            }
            return false;
        }

        private async Task<List<Tag>> GetAllTags()
        {
            var tags = await _context.Tags.ToListAsync();
            return tags;
        }
    }
}