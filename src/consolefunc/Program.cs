// See https://aka.ms/new-console-template for more information

using agent;
using Azure;
using Azure.AI.OpenAI;
using functions;

// Load the settings
AgentSettings settings = new();
OpenAIClient client = new(new Uri(settings.APIEndpoint), new AzureKeyCredential(settings.APIKey));

// Create the agent
GPTAgent agent = new(settings, client);

var input = "What is the speed of light?";
string intent = await SemanticIntent.GetIntent(agent, input);

Console.WriteLine($"user:\n{input}");
if (intent == SemanticIntent.IntentList.Unknown.ToString())
{
    Console.WriteLine($"intent:\n{intent}");
    var result = await agent.ProcessPromptAsync(input, 100, 0.2f);
    Console.WriteLine($"assistant:\n{result}\n");
}

// Get the user's intent
input = "What is the weather in San Francisco?";
intent = await SemanticIntent.GetIntent(agent, input);

// if the intent is to get the weather, process the message with the weather tool
Console.WriteLine($"user:\n{input}");
if (intent == SemanticIntent.IntentList.GetWeather.ToString())
{
    Console.WriteLine($"intent:\n{intent}");
    var weatherTool = ToolFunctions.getWeatherTool;
    var result = await agent.ProcessPromptAsync(input, 100, 0.3f, [weatherTool], ToolFunctions.GetToolCallResponseMessage);
    Console.WriteLine($"assistant:\n{result}\n");
}

// Get the user's intent
input = "What is the nick name for Seattle, WA?";
intent = await SemanticIntent.GetIntent(agent, input);

// If the intent is to get the city's nickname, process the message with the city nickname tool
Console.WriteLine($"user:\n{input}");
if (intent == SemanticIntent.IntentList.CityNickName.ToString())
{
    Console.WriteLine($"intent:\n{intent}");
    var cityTool = ToolFunctions.getCityNicknameTool;
    var result = await agent.ProcessPromptAsync(input, 100, 0.3f, [cityTool], ToolFunctions.GetToolCallResponseMessage);
    Console.WriteLine($"assistant:\n{result}\n");
}