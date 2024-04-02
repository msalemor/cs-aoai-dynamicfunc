namespace agent;

public static class SemanticIntent
{

    public enum IntentList
    {
        GetWeather,
        CityNickName,
        Unknown
    }
    public async static Task<string> GetIntent(GPTAgent agent, string input)
    {
        var result = await agent.ProcessPromptAsync(@"
system:
You are an agent that can help determine the user's intent from a question or statement. Valid intents are:

Intents:

GetWeather: Gets the weather in a city.
CityNickName: Gets the nickname of a city.
Unknow: Anything else

User:
<INPUT>

Output the intent ONLY.".Replace("<INPUT>", input), 4, 0.1f);
        return result;
    }

}