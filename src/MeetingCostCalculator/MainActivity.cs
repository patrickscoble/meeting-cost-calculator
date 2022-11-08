using Android.Content;
using Android.Views;
using MeetingCostCalculator.Services;
using System.Timers;

namespace MeetingCostCalculator
{
	[Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/groups_48")]
	public class MainActivity : Activity
	{
		private static decimal AverageSalary = 114270; // https://www.herzing.edu/salary/software-developer#:~:text=Software%20development%20becomes%20more%20and%20more%20specialized%20as,%28%2454.94%20per%20hour%29.%2A%20Job%20outlook%20for%20software%20developers

		private SlackService _slackService;

		private TextView _nameTextView;
		private TextView _clockTextView;
		private TextView _costTextView;
		private Button _startButton;
		private Button _finishButton;
		private Button _resumeButton;
		private Button _resetButton;
		private Button _sendButton;

		private System.Timers.Timer _timer;

		public string Name { get; set; }
		public int Attendees { get; set; }
		public int Hours { get; set; }
		public int Minutes { get; set; }
		public int Seconds { get; set; }
		public decimal RunningCost { get; set; }
		public decimal CostPerSecond { get; set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			_slackService = new SlackService();

			_nameTextView = FindViewById<TextView>(Resource.Id.nameTextView);
			_clockTextView = FindViewById<TextView>(Resource.Id.clockTextView);
			_costTextView = FindViewById<TextView>(Resource.Id.costTextView);
			_startButton = FindViewById<Button>(Resource.Id.startButton);
			_finishButton = FindViewById<Button>(Resource.Id.finishButton);
			_resumeButton = FindViewById<Button>(Resource.Id.resumeButton);
			_resetButton = FindViewById<Button>(Resource.Id.resetButton);
			_sendButton = FindViewById<Button>(Resource.Id.sendButton);

			_startButton.Click += (sender, e) =>
			{
				LayoutInflater layoutInflater = LayoutInflater.From(this);
				View view = layoutInflater.Inflate(Resource.Layout.start_meeting, null);

				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetTitle("Start Meeting");
				builder.SetView(view);
				builder.SetPositiveButton("Start", StartAction);
				builder.SetNegativeButton("Cancel", CancelAction);

				builder.Show();
			};

			_finishButton.Click += (sender, e) =>
			{
				_timer.Stop();
				_startButton.Visibility = ViewStates.Gone;
				_finishButton.Visibility = ViewStates.Gone;
				_resumeButton.Visibility = ViewStates.Visible;
				_resetButton.Visibility = ViewStates.Visible;
				_sendButton.Visibility = ViewStates.Gone;
			};

			_resumeButton.Click += (sender, e) =>
			{
				_timer.Start();
				_startButton.Visibility = ViewStates.Gone;
				_finishButton.Visibility = ViewStates.Visible;
				_resumeButton.Visibility = ViewStates.Gone;
				_resetButton.Visibility = ViewStates.Gone;
				_sendButton.Visibility = ViewStates.Gone;
			};

			_resetButton.Click += (sender, e) =>
			{
				_timer.Stop();
				_startButton.Visibility = ViewStates.Visible;
				_finishButton.Visibility = ViewStates.Gone;
				_resumeButton.Visibility = ViewStates.Gone;
				_resetButton.Visibility = ViewStates.Gone;
				_sendButton.Visibility = ViewStates.Visible;
			};

			_sendButton.Click += (sender, e) =>
			{
				if (!_slackService.SendSlackMessage(Name, Attendees, _clockTextView.Text, _costTextView.Text))
				{
					Toast.MakeText(Application, "Unable to send Slack notification", ToastLength.Short).Show();
				}
			};

			_timer = new System.Timers.Timer();
			_timer.Interval = 1000; // 1 second  
			_timer.Elapsed += Timer_Elapsed;
		}

		private void StartAction(object sender, DialogClickEventArgs e)
		{
			AlertDialog alertDialog = (AlertDialog)sender;

			Name = alertDialog.FindViewById<EditText>(Resource.Id.start_meeting_name).Text;
			Attendees = Convert.ToInt32(alertDialog.FindViewById<EditText>(Resource.Id.start_meeting_attendees).Text);
		
			Hours = 0;
			Minutes = 0;
			Seconds = 0;
			RunningCost = 0;

			CostPerSecond = (AverageSalary * Attendees) / (52m * 38m * 60m * 60m); // 52 weeks x 38 hours * 60 minutes * 60 seconds

			_nameTextView.Text = $"{Name} ({Attendees})";
			_clockTextView.Text = String.Format("00:00:00");
			_costTextView.Text = String.Format("$0.00");
			_startButton.Visibility = ViewStates.Gone;
			_finishButton.Visibility = ViewStates.Visible;
			_resumeButton.Visibility = ViewStates.Gone;
			_resetButton.Visibility = ViewStates.Gone;
			_sendButton.Visibility = ViewStates.Gone;

			_timer.Start();
		}

		public void CancelAction(object sender, DialogClickEventArgs e)
		{
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (Seconds == 59)
			{
				if (Minutes == 59)
				{
					Hours++;
					Minutes = 0;
				}
				else
				{
					Minutes++;
				}

				Seconds = 0;
			}
			else
			{
				Seconds++;
			}
		
			RunningCost += CostPerSecond;

			RunOnUiThread(() =>
			{
				_clockTextView.Text = string.Format("{0:00}:{1:00}:{2:00}", Hours, Minutes, Seconds);
				_costTextView.Text = string.Format("${0:0.00}", RunningCost);
			});
		}		
	}
}
