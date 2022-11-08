using Newtonsoft.Json;

namespace MeetingCostCalculator.Models
{
	internal class SlackMessage
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("channel")]
		public string Channel { get { return ""; } }

		[JsonProperty("icon_emoji")]
		public string IconEmoji { get { return ""; } }

		[JsonProperty("username")]
		public string Username { get { return ""; } }
	}
}
