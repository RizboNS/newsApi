using newsApi.Models;

namespace newsApi.Services.AdminService
{
    public interface IAdminService
    {
        public Task<ServiceResponse<CheckDomainReport>> CheckDomain(string domain);
    }
}