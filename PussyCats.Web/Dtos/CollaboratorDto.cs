using System.Text.Json.Serialization;

namespace PussyCats.Web.Dtos
{
    public class CollaboratorDto
    {
        public int CompanyId { get; set; }

        [JsonPropertyName("Name")]
        public string CompanyName { get; set; } = string.Empty;
    }
}
