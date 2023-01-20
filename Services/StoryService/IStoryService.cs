using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.StoryService
{
    public interface IStoryService
    {
        Task<ServiceResponse<Guid>> CreateStory(StoryCreateDto storyCreateDto, string domainName);

        Task<ServiceResponse<StoryResponsePagedDto>> GetStories(string domainName, int page, int pageSize, List<Category> categories, List<string> tags, string published);

        Task<ServiceResponse<StoryResponsePagedDto>> SearchStories(string searchValue, int page, int pageSize, string domainName);

        Task<ServiceResponse<StoryResponseDto>> GetStory(Guid storyId, string domainName);

        Task<ServiceResponse<StoryResponseDto>> GetStoryByTitleId(string titleId, string domainName);

        Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesByCategoryPaged(string type, Category category, int page, string domainName);

        Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesPaged(string type, int page, string domainName);

        Task<ServiceResponse<Guid>> UpdateStory(StoryUpdateDto storyUpdateDto, string domainName);

        Task<MethodResponse> DeleteStory(Guid storyId);
    }
}