using newsApi.Models;

namespace newsApi.Services.TagService
{
    public interface ITag
    {
        Task<ServiceResponse<List<Tag>>> CreateTag(string tag);
    }
}