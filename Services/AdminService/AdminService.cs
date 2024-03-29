﻿using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;
using newsApi.Services.StoryService;

namespace newsApi.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly DataContext _context;
        private readonly IStoryService _storyService;

        public AdminService(DataContext context, IStoryService storyService)
        {
            _context = context;
            _storyService = storyService;
        }

        public async Task<ServiceResponse<CheckDomainReport>> CheckDomain(string domain)
        {
            var serviceResponse = new ServiceResponse<CheckDomainReport>();
            //var serviceResponseStories = await _storyService.GetStories(domain, 1, 10); // TO DO MAKE SEPARET QUERY FOR IT (UNFILTERED)
            var stories = await _context.Stories
                .Include(s => s.ImageDbs)
                .ToListAsync();

            foreach (var story in stories)
            {
                await CheckStory(domain, story);
            }

            return serviceResponse;
        }

        private async Task CheckStory(string domain, Story story)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(story.HtmlData);

            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");

            if (imgNodes != null)
            {
                var isEdited = false;
                foreach (HtmlNode link in imgNodes)
                {
                    var attribute = link.Attributes["src"];
                    var url = attribute.Value;
                    var uriFull = new Uri(url);
                    var urlHost = uriFull.GetLeftPart(UriPartial.Authority);
                    var urlLocalPath = new Uri(url).LocalPath;
                    var domainTrimmed = domain.Substring(0, domain.Length - 1);
                    Console.WriteLine("urlHost " + urlHost);
                    Console.WriteLine("domainTrimmed: " + domainTrimmed);
                    if (urlHost != domainTrimmed)
                    {
                        var updatedUrl = domainTrimmed + urlLocalPath;
                        attribute.Value = updatedUrl;
                        isEdited = true;
                        Console.WriteLine("edited1");
                    }
                }
                if (isEdited)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    htmlDoc.Save(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    StreamReader streamReader = new StreamReader(memoryStream);

                    story.HtmlData = streamReader.ReadToEnd();
                    _context.Stories.Update(story);
                    Console.WriteLine("edited");
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}