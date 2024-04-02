using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;

namespace functions;

public static class ToolFunctions
{
    record ToolParam([property: JsonPropertyName("location")] string Location);

    public static ChatCompletionsFunctionToolDefinition getCityNicknameTool = new()
    {
        Name = "getCityNickname",
        Description = "Gets the nickname of a city, e.g. 'LA' for 'Los Angeles, CA'.",
        Parameters = BinaryData.FromObjectAsJson(
                new
                {
                    Type = "object",
                    Properties = new
                    {
                        Location = new
                        {
                            Type = "string",
                            Description = "The city and state, e.g. San Francisco, CA",
                        },
                        Unit = new
                        {
                            Type = "string",
                            Enum = new[] { "celsius", "fahrenheit" },
                        }
                    },
                    Required = new[] { "location" },
                },
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
    };

    public static ChatCompletionsFunctionToolDefinition getWeatherTool = new()
    {
        Name = "get_current_weather",
        Description = "Get the current weather in a given location",
        Parameters = BinaryData.FromObjectAsJson(
                new
                {
                    Type = "object",
                    Properties = new
                    {
                        Location = new
                        {
                            Type = "string",
                            Description = "The city and state, e.g. San Francisco, CA",
                        },
                        Unit = new
                        {
                            Type = "string",
                            Enum = new[] { "celsius", "fahrenheit" },
                        }
                    },
                    Required = new[] { "location" },
                },
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
    };

    public static string GetCityNickname(string location) => location switch
    {
        "Seattle, WA" => "The Emerald City",
        "New York, NY" => "The Big Apple",
        "San Francisco, CA" => "The Golden City",
        "Los Angeles, CA" => "The City of Angels",
        "Miami, FL" => "The Magic City",
        "Chicago, IL" => "The Windy City",
        "Boston, MA" => "Beantown",
        "Portland, OR" => "The City of Roses",
        "Austin, TX" => "The Live Music Capital of the World",
        "Nashville, TN" => "Music City",
        "New Orleans, LA" => "The Big Easy",
        _ => throw new NotImplementedException(),
    };

    public static int GetWeather(string location) => location switch
    {
        "San Francisco, CA" => Random.Shared.Next(40, 55),
        "Miami, FL" => Random.Shared.Next(70, 85),
        "New York, NY" => Random.Shared.Next(30, 45),
        "Los Angeles, CA" => Random.Shared.Next(60, 75),
        "Seattle, WA" => Random.Shared.Next(35, 50),
        "Chicago, IL" => Random.Shared.Next(25, 40),
        "Boston, MA" => Random.Shared.Next(30, 45),
        "Portland, OR" => Random.Shared.Next(40, 55),
        "Austin, TX" => Random.Shared.Next(50, 65),
        "Nashville, TN" => Random.Shared.Next(45, 60),
        "New Orleans, LA" => Random.Shared.Next(55, 70),
        _ => throw new NotImplementedException(),
    };

    public static ChatRequestToolMessage GetToolCallResponseMessage(ChatCompletionsToolCall toolCall)
    {
        var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;
        if (functionToolCall?.Name == "get_current_weather")
        {
            // Validate and process the JSON arguments for the function call
            string jsonArgumentList = functionToolCall.Arguments;
            var location = JsonSerializer.Deserialize<ToolParam>(jsonArgumentList) ?? new ToolParam(Location: "");
            var temperature = GetWeather(location.Location); // GetYourFunctionResultData(unvalidatedArguments);
            var functionResultData = $"{temperature}F";
            return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
        }
        else if (functionToolCall?.Name == "getCityNickname")
        {
            // Validate and process the JSON arguments for the function call
            string jsonArgumentList = functionToolCall.Arguments;
            var location = JsonSerializer.Deserialize<ToolParam>(jsonArgumentList) ?? new ToolParam(Location: "");
            var functionResultData = GetCityNickname(location.Location); // GetYourFunctionResultData(unvalidatedArguments);
            return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
        }
        else
        {
            // Handle other or unexpected calls
            //throw new NotImplementedException();
            return new ChatRequestToolMessage("Function was not implemented", toolCall.Id);
        }
    }


}