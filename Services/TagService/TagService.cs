using newsApi.Models;

namespace newsApi.Services.TagService
{
    public class TagService : ITagService
    {
        public Task<ServiceResponse<List<TagService>>> CreateTag(string tag)
        {
            throw new NotImplementedException();
        }
    }
}