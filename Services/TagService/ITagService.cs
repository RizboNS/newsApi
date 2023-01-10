using newsApi.Models;

namespace newsApi.Services.TagService
{
    public interface ITagService
    {
        Task<ServiceResponse<List<Tag>>> CreateTags(List<Tag> tags);

        Task<ServiceResponse<List<Tag>>> GetTags();

        Task<ServiceResponse<List<Tag>>> DeleteTags(List<Tag> tags);

        Task<ServiceResponse<List<Tag>>> ModifyTags(List<Tag> tags);
    }
}