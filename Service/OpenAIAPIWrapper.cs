using OpenAI.API;

namespace Helth_And_Nutrition.Service
{
	// OpenAIAPIWrapper.cs
	public class OpenAIAPIWrapper : IOpenAIAPI
	{
		public OpenAIAPI CreateInstance(string apiKey)
		{
			return new OpenAIAPI(apiKey);
		}
	}

}
