using Hl7.Fhir.Model;
using NeoHearts_API.Models;

namespace NeoHearts_API.Services
{
    public interface IFhirDataMappingService
    {
        NewbornModel MapFhirToNewbornModel(Bundle bundle);
    }
}
