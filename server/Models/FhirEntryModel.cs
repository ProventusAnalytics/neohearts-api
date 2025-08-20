using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NeoHearts_API.Models
{
    public class FhirEntryModel
    {
#pragma warning disable IDE1006 // Naming Styles  :: disables uppercase name warnings
        public string? fullUrl { get; set; }  // Uppercase and Required
        public required dynamic resource { get; set; }  // Uppercase and Required
        public required Request request { get; set; }  // Uppercase and Required
    }

    public class Request
    {
        public required string method { get; set; }  // Uppercase and Required
        public required string url { get; set; }  // Uppercase and Required
    }
}
