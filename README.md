# Dynamic function calling with Azure OpenAI in C#

This is a sample console application leveraging Azure OpenAI C# SDK with making dynamic funciton calls with function defenitions and processing delegates.

## Processing signature

- At processing time determine which function will be loaded and executed.

```c#
public async Task<string> ProcessPromptAsync(string input,
        int maxTokens = 100,
        float temperature = 0.3f,
        List<ChatCompletionsFunctionToolDefinition>? tools = null,
        FunctionDelegate? toolFunctionDelegate = null)
```

## The delegate

- File: `Agent/GPTAgent.cs`

```C#
public delegate ChatRequestToolMessage? FunctionDelegate(ChatCompletionsToolCall toolCall);
```

## The Tool Functions and Processing Deletages

- File: `Functions/ToolFunctions.cs`

```c#
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
```

## Processing

- Based on intent load different tools across different user calls.

```c#
// Create the agent
GPTAgent agent = new(settings, client);

var input = "What is the speed of light?";
string intent = await SemanticIntent.GetIntent(agent, input);

if (intent == SemanticIntent.IntentList.Unknown.ToString())
{
    var result = await agent.ProcessPromptAsync(input, 100, 0.2f);
    Console.WriteLine(result);
}

// Get the user's intent
input = "What is the weather in San Francisco?";
intent = await SemanticIntent.GetIntent(agent, input);
Console.WriteLine(intent);

// if the intent is to get the weather, process the message with the weather tool
if (intent == SemanticIntent.IntentList.GetWeather.ToString())
{
    var weatherTool = ToolFunctions.getWeatherTool;
    var result = await agent.ProcessPromptAsync(input, 100, 0.3f, [weatherTool], ToolFunctions.GetToolCallResponseMessage);
    Console.WriteLine(result);
}

// Get the user's intent
input = "What is the nick name for Seattle, WA?";
intent = await SemanticIntent.GetIntent(agent, input);
Console.WriteLine(intent);

// If the intent is to get the city's nickname, process the message with the city nickname tool
if (intent == SemanticIntent.IntentList.CityNickName.ToString())
{
    var cityTool = ToolFunctions.getCityNicknameTool;
    var result = await agent.ProcessPromptAsync(input, 100, 0.3f, [cityTool], ToolFunctions.GetToolCallResponseMessage);
    Console.WriteLine(result);
}
```
