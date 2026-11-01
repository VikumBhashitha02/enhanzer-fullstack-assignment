using System.Text.Json.Serialization;

namespace Enhanzer.Api.Models;

public class ExternalApiLoginRequest
{
    [JsonPropertyName("API_Action")]
    public string ApiAction { get; set; } = "GetLoginData";

    [JsonPropertyName("Device_Id")]
    public string DeviceId { get; set; } = "D001";

    [JsonPropertyName("Sync_Time")]
    public string SyncTime { get; set; } = string.Empty;

    [JsonPropertyName("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [JsonPropertyName("API_Body")]
    public ExternalApiBody ApiBody { get; set; } = new();
}

public class ExternalApiBody
{
    [JsonPropertyName("Username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("Pw")]
    public string Password { get; set; } = string.Empty;
}

public class ExternalApiResponse
{
    [JsonPropertyName("Status_Code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("Sync_Time")]
    public string? SyncTime { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Response_Body")]
    public List<ExternalUserPayload>? ResponseBody { get; set; }
}

public class ExternalUserPayload
{
    [JsonPropertyName("User_Code")]
    public string? UserCode { get; set; }

    [JsonPropertyName("User_Display_Name")]
    public string? UserDisplayName { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("User_Employee_Code")]
    public string? UserEmployeeCode { get; set; }

    [JsonPropertyName("Company_Code")]
    public string? CompanyCode { get; set; }

    [JsonPropertyName("Doc_Msg")]
    public string? DocMsg { get; set; }

    [JsonPropertyName("User_Locations")]
    public List<ExternalLocationItem>? UserLocations { get; set; }
}

public class ExternalLocationItem
{
    [JsonPropertyName("Location_Code")]
    public string LocationCode { get; set; } = string.Empty;

    [JsonPropertyName("Location_Name")]
    public string LocationName { get; set; } = string.Empty;

    [JsonPropertyName("Stock_Handle")]
    public int? StockHandle { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("Status")]
    public int? Status { get; set; }
}

