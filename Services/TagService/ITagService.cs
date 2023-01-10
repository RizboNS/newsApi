using newsApi.Models;

namespace newsApi.Services.TagService
{
    public interface ITagService
    {
        Task<ServiceResponse<List<Tag>>> CreateTag(Tag tag);

        Task<ServiceResponse<List<Tag>>> GetTags();
    }
}