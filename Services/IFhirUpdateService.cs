using Hl7.Fhir.Model;
using NeoHearts_API.Models;

namespace NeoHearts_API.Services
{
    public interface IFhirUpdateService
    {
        Bundle UpdateFhirBundleFromNewbornModel(Bundle bundle, NewbornModel newborn);
    }
}
