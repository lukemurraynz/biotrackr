using System.Text.Json.Serialization;

namespace Biotrackr.Food.Api.Models.FitbitEntities;

public class Goals
{
    [JsonPropertyName("calories")]
    public int Calories { get; set; }
}
