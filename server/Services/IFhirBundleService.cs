using NeoHearts_API.Models;

namespace NeoHearts_API.Services
{
    public interface IFhirBundleService
    {
        object CreateFhirBundle(NewbornModel newborn);
    }
}
