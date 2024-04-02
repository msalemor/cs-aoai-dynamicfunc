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

async static Task ProcessInput(GPTAgent agent, string input)
{
    Console.WriteLine($"user:\n{input}");

    string selectedIntent = await SemanticIntent.GetIntent(agent, input);
    Console.WriteLine($"intent:\n{selectedIntent}");

    string result;
    ChatCompletionsFunctionToolDefinition tool;
    FunctionDelegate processingDelegate = ToolFunctions.GetToolCallResponseMessage;

    if (selectedIntent == SemanticIntent.IntentList.GetWeather.ToString())
    {
        tool = ToolFunctions.getWeatherTool;
        result = await agent.ProcessPromptAsync(input, 100, 0.3f, [tool], processingDelegate);
    }
    else if (selectedIntent == SemanticIntent.IntentList.CityNickName.ToString())
    {
        tool = ToolFunctions.getCityNicknameTool;
        processingDelegate = ToolFunctions.GetToolCallResponseMessage;
        result = await agent.ProcessPromptAsync(input, 100, 0.3f, [tool], processingDelegate);
    }
    else
    {
        result = await agent.ProcessPromptAsync(input, 100, 0.2f);
    }
    Console.WriteLine($"agent:\n{result}\n");
}

// Process user inputs based on intents
var input = "What is the speed of light?";
await ProcessInput(agent, input);

input = "What is the weather in San Francisco?";
await ProcessInput(agent, input);

input = "What is the nick name for Seattle, WA?";
await ProcessInput(agent, input);

// Combine two or more tools in a GPT call
input = "What is the weather in San Francisco? What is the nick name for Seattle, WA?";

List<ChatCompletionsFunctionToolDefinition> tools = [ToolFunctions.getWeatherTool, ToolFunctions.getCityNicknameTool];
FunctionDelegate processingDelegate = ToolFunctions.GetToolCallResponseMessage;

var result1 = await agent.ProcessPromptAsync(input, 100, 0.3f, tools, processingDelegate);

Console.WriteLine($"agent:\n{result1}\n");

