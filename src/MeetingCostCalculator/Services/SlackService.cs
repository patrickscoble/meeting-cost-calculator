using MeetingCostCalculator.Models;
using Newtonsoft.Json;
using System.Text;

namespace MeetingCostCalculator.Services
{
	internal class SlackService
	{
		private static string SlackWebhookUri = "";

		public bool SendSlackMessage(string name, int attendees, string duration, string cost)
		{
			if (string.IsNullOrWhiteSpace(SlackWebhookUri))
			{
				return false;
			}

			string slackMessagetext = $"Meeting: {name}\nAttendees: {attendees}\nDuration: {duration}\nEstimated Cost: {cost}\n\n:thumbsup: if you think this meeting was worth the cost or :thumbsdown: if you do not";

			SlackMessage slackMessage = new SlackMessage()
			{
				Text = slackMessagetext,
			};

			string content = JsonConvert.SerializeObject(slackMessage);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, SlackWebhookUri)
			{
				Content = new StringContent(content, Encoding.UTF8, "application/json")
			};

			HttpClient client = new HttpClient();
			client.SendAsync(request);

			return true;
		}
	}
}
