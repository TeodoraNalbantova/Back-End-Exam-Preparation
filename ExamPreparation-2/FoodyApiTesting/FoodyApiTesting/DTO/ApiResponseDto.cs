using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoodyApiTesting.DTO
{
    public class ApiResponseDto
    {
        [JsonPropertyName("msg")]
        public string Message  { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }
    }
}
