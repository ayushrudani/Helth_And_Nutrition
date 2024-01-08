using OpenAI.API;

namespace Helth_And_Nutrition.Service
{
	public interface IOpenAIAPI
	{
		OpenAIAPI CreateInstance(string apiKey);
	}
}
