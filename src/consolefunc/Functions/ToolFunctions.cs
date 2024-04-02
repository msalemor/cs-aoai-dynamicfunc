using System.Text.Json;
using Azure.AI.OpenAI;

namespace functions;

public static class ToolFunctions
{
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
        _ => throw new NotImplementedException(),
    };

    public static int GetWeather(string location) => location switch
    {
        "San Francisco, CA" => Random.Shared.Next(40, 55),
        _ => throw new NotImplementedException(),
    };

    public static ChatRequestToolMessage GetToolCallResponseMessage(ChatCompletionsToolCall toolCall)
    {
        var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;
        if (functionToolCall?.Name == "get_current_weather")
        {
            // Validate and process the JSON arguments for the function call
            string jsonData = functionToolCall.Arguments;
            var location = JsonSerializer.Deserialize<ToolParam>(jsonData) ?? new ToolParam(location: "");
            var temperature = GetWeather(location.location); // GetYourFunctionResultData(unvalidatedArguments);
            var functionResultData = $"{temperature}F";
            return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
        }
        else if (functionToolCall?.Name == "getCityNickname")
        {
            // Validate and process the JSON arguments for the function call
            string jsonData = functionToolCall.Arguments;
            var location = JsonSerializer.Deserialize<ToolParam>(jsonData) ?? new ToolParam(location: "");
            var functionResultData = GetCityNickname(location.location); // GetYourFunctionResultData(unvalidatedArguments);
            return new ChatRequestToolMessage(functionResultData.ToString(), toolCall.Id);
        }
        else
        {
            // Handle other or unexpected calls
            throw new NotImplementedException();
        }
    }

    record ToolParam(string location);
}