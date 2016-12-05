using System;

using Xamarin.Forms;

namespace NotificationTest
{
	public interface INotificationService
	{
		void Notify();
		void StartActivity();
	}

	public class App : Application
	{
		public App()
		{
			var buttonActivity = new Button { Text = "Start activity" };
			buttonActivity.Clicked += (sender, e) => { 
				DependencyService.Get<INotificationService>().StartActivity();
			};

			var button = new Button { Text = "Notify" };
			button.Clicked += (sender, e) => {
				DependencyService.Get<INotificationService>().Notify();
			};

			var content = new ContentPage
			{
				Title = "NotificationTest",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						buttonActivity,
						button
					}
				}
			};

			MainPage = new NavigationPage(content);
		}
	}
}
