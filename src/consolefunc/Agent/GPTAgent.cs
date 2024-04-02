namespace agent;

using Azure;
using Azure.AI.OpenAI;

public delegate ChatRequestToolMessage? FunctionDelegate(ChatCompletionsToolCall toolCall);

public class GPTAgent
{

    public AgentSettings Settings { get; set; }
    public OpenAIClient Client { get; set; }

    public GPTAgent(AgentSettings? settings, OpenAIClient? client, List<string>? fileIds = null, string? dataFolder = null)
    {
        // Set or create the settings and client
        Settings = settings ?? new AgentSettings();
        // Set or create the client
        Client = client ?? new OpenAIClient(new Uri(Settings.APIEndpoint), new AzureKeyCredential(Settings.APIKey));
    }

    public async Task<string> ProcessPromptAsync(string input,
        int maxTokens = 100,
        float temperature = 0.3f,
        List<ChatCompletionsFunctionToolDefinition>? tools = null,
        FunctionDelegate? toolFunctionDelegate = null)
    {
        ChatCompletionsOptions chatCompletionsOptions = new()
        {
            DeploymentName = Settings.APIDeploymentName, // Use DeploymentName for "model" with non-Azure clients            
            MaxTokens = maxTokens,
            Temperature = temperature
        };

        if (tools is not null)
        {
            foreach (var item in tools)
            {
                chatCompletionsOptions.Tools.Add(item);
            }
        }

        // Add the user message to the history
        chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(input));

        try
        {
            // Process the message
            Response<ChatCompletions> response = await Client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatChoice responseChoice = response.Value.Choices[0];

            // See if it needs to call a function
            if (responseChoice.FinishReason == CompletionsFinishReason.ToolCalls)
            {
                // Add the assistant message with tool calls to the conversation history
                ChatRequestAssistantMessage toolCallHistoryMessage = new(responseChoice.Message);
                chatCompletionsOptions.Messages.Add(toolCallHistoryMessage);

                // Add a new tool message for each tool call that is resolved
                foreach (ChatCompletionsToolCall toolCall in responseChoice.Message.ToolCalls)
                {
                    if (toolFunctionDelegate is not null)
                    {
                        ChatRequestToolMessage? toolMessage = toolFunctionDelegate(toolCall);
                        if (toolMessage is not null)
                        {
                            chatCompletionsOptions.Messages.Add(toolMessage);
                        }
                    }

                    //chatCompletionsOptions.Messages.Add(Weather.GetToolCallResponseMessage(toolCall));
                }

                // Now make a new request with all the messages thus far, including the original
                response = await Client.GetChatCompletionsAsync(chatCompletionsOptions);

            }
            return response.Value.Choices[0].Message.Content;

        }
        catch (Exception)
        {
            return "";
        }
    }
}
