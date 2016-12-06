using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NotificationTest
{
	public interface IPickFileService
	{
		Task<string> PickFile();
	}

	public interface INotificationService
	{
		void Notify();
		void StartActivity();
	}

	public class App : Application
	{
		public App()
		{
			var page = new ContentPage { Title = "Activity and notification tests" };

			var buttonActivity = new Button { Text = "Start activity" };
			buttonActivity.Clicked += (sender, e) =>
			{
				DependencyService.Get<INotificationService>().StartActivity();
			};

			var buttonNotify = new Button { Text = "Notify" };
			buttonNotify.Clicked += (sender, e) =>
			{
				DependencyService.Get<INotificationService>().Notify();
			};

			var buttonPickFile = new Button { Text = "Pick file" };
			buttonPickFile.Clicked += async (sender, e) =>
			{
				var file = await DependencyService.Get<IPickFileService>().PickFile();
				await page.DisplayAlert("File picked", file, "OK");
			};

			page.Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					buttonActivity,
					buttonNotify,
					buttonPickFile
				}
			};

			MainPage = new NavigationPage(page);
		}
	}
}
