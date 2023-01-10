﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<ServiceResponse<List<Tag>>> CreateTags(List<Tag> newTags)
        {
            var serviceResponse = new ServiceResponse<List<Tag>>();
            var tagsFromDb = await GetAllTags();
            newTags = newTags
                        .Select(x => { x.TagName = x.TagName.ToLower(); return x; })
                        .Where(x => !tagsFromDb.Any(y => y.TagName == x.TagName))
                        .ToList();
            try
            {
                if (newTags.Count > 0)
                {
                    await _context.AddRangeAsync(newTags);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = await GetAllTags();
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<Tag>>> DeleteTags(List<Tag> tags)
        {
            var serviceResponse = new ServiceResponse<List<Tag>>();
            try
            {
                _context.Tags.RemoveRange(tags);
                await _context.SaveChangesAsync();
                serviceResponse.Data = await GetAllTags();
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

        private async Task<List<Tag>> GetAllTags()
        {
            var tags = await _context.Tags.ToListAsync();
            return tags;
        }
    }
}