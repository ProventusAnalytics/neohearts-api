using Google.Apis.Auth.OAuth2;

namespace NeoHearts_API.Services
{
    public interface IWorkloadIdentityService
    {
        Task<GoogleCredential> GetCredentialAsync();
    }
}
