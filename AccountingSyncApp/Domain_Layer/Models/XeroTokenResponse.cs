using System.Text.Json.Serialization;

namespace Domain_Layer.Models
{
    public class XeroTokenResponse
    {
        public int Id { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
//[JsonPropertyName("access_token")]
//public string AccessToken { get; set; } //Why it’s needed
//Xero sends JSON like this:
//{
//    "access_token": "eyJ...",
//  "refresh_token": "eyJ...",
//  "expires_in": 3600,
//  "token_type": "Bearer",
//  "id_token": "eyJ..."
//}
//Notice the keys are snake_case (access_token)
//Your C# properties are PascalCase (AccessToken)
//System.Text.Json is case-sensitive by default and looks for JSON keys that exactly match property names.
//What[JsonPropertyName] does
//[JsonPropertyName("access_token")]
//public string AccessToken { get; set; }
//Maps the JSON key "access_token" → C# property AccessToken
//Now deserialization correctly populates all fields
//This is why without [JsonPropertyName], your AccessToken, RefreshToken, and ExpiresIn were all null/0.