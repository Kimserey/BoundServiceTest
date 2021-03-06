﻿using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using System.Collections.Generic;
using Android.Util;
using Java.Util;
using Java.Lang;

[assembly: Dependency(typeof(NotificationTest.Droid.NotificationService))]
namespace NotificationTest.Droid
{
	public class NotificationService : INotificationService
	{
		public void Notify()
		{
			Notification.Builder builder = new Notification.Builder(Forms.Context)
				.SetContentTitle("Sample Notification")
				.SetContentText("Hello World! This is my first notification!")
				.SetSmallIcon(Resource.Drawable.icon);

			Notification notification = builder.Build();

			NotificationManager notificationManager = Forms.Context.GetSystemService(Context.NotificationService) as NotificationManager;

			const int notificationId = 0;
			notificationManager.Notify(notificationId, notification);
		}

		public void StartActivity()
		{
			Forms.Context.StartActivity(typeof(ServiceActivity));
		}
	}

	public class HelloServiceBinder : Binder
	{
		HelloService service;

		public HelloServiceBinder(HelloService service)
		{
			this.service = service;
		}

		public HelloService Service
		{
			get
			{
				return service;
			}
		}
	}

	// External receiver
	// Bound 
	[BroadcastReceiver]
	[IntentFilter(new string[] { HelloService.HelloReceivedAction }, Priority = (int)IntentFilterPriority.LowPriority)]
	public class HelloNotificationReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			Notification.Builder builder = new Notification.Builder(context)
				.SetContentTitle("Hello received!")
				.SetSmallIcon(Resource.Drawable.icon);

			Notification notification = builder.Build();

			NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

			const int notificationId = 0;
			notificationManager.Notify(notificationId, notification);

		}
	}

	[Service]
	[IntentFilter(new System.String[] { "com.kimserey.servicetest" })]
	public class HelloService : IntentService
	{
		IBinder binder;

		public const string HelloReceivedAction = "HelloReceived";

		protected override void OnHandleIntent(Intent i)
		{
			var intent = new Intent(HelloReceivedAction);
			SendOrderedBroadcast(intent, null);
		}

		public override IBinder OnBind(Intent intent)
		{
			binder = new HelloServiceBinder(this);
			return binder;
		}

		public string GetHello()
		{
			return "Hello";
		}

		string SayHello()
		{
			return "Hello from service";
		}

	}

	[Activity]
	public class ServiceActivity : Activity
	{
		HelloReceiver helloReceiver;
		Intent serviceIntent;
		HelloServiceBinder binder;
		bool isBound;
		HelloServiceConnection serviceConnection;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			serviceIntent = new Intent("com.kimserey.servicetest");
			helloReceiver = new HelloReceiver();
		}

		protected override void OnStart()
		{
			base.OnStart();

			var intentFilter = new IntentFilter(HelloService.HelloReceivedAction) { Priority = (int)IntentFilterPriority.LowPriority };
			
			// Register local receiver
			RegisterReceiver(helloReceiver, intentFilter);

			serviceConnection = new HelloServiceConnection(this);
			BindService(serviceIntent, serviceConnection, Bind.AutoCreate);

			ScheduleHello();
		}

		protected override void OnStop()
		{
			base.OnStop();

			if (isBound)
			{
				UnbindService(serviceConnection);

				isBound = false;
			}

			UnregisterReceiver(helloReceiver);

			Cancel();
		}


		void ScheduleHello()
		{
			if (!IsAlarmSet())
			{
				var alarm = (AlarmManager)GetSystemService(Context.AlarmService);
				var pendingServiceIntent = PendingIntent.GetService(this, 0, serviceIntent, PendingIntentFlags.CancelCurrent);
				
				// Start alarm around 8.30 and repeat every day
				Calendar calendar = Calendar.Instance;
				calendar.TimeInMillis = JavaSystem.CurrentTimeMillis();
				calendar.Set(CalendarField.HourOfDay, 8);
				calendar.Set(CalendarField.Minute, 30);

				//alarm.SetInexactRepeating(AlarmType.Rtc, calendar.TimeInMillis, AlarmManager.IntervalDay, pendingServiceIntent);
				alarm.SetInexactRepeating(AlarmType.Rtc, 0, 1000, pendingServiceIntent);
			}
		}

		void Cancel()
		{ 
			var alarm = (AlarmManager)GetSystemService(Context.AlarmService);
			alarm.Cancel(PendingIntent.GetService(this, 0, serviceIntent, PendingIntentFlags.CancelCurrent));
		}

		void Hello()
		{
			if (isBound)
			{
				var hello = binder.Service.GetHello();
				Log.Debug("HelloService", hello);
			}
		}

		bool IsAlarmSet()
		{
			return PendingIntent.GetBroadcast(this, 0, serviceIntent, PendingIntentFlags.NoCreate) != null;
		}

		// Local receiver
		// Hardcoded into the Activity
		class HelloReceiver : BroadcastReceiver
		{
			public override void OnReceive(Context context, Android.Content.Intent intent)
			{
				((ServiceActivity)context).Hello();

				InvokeAbortBroadcast();
			}
		}

		class HelloServiceConnection : Java.Lang.Object, IServiceConnection
		{
			ServiceActivity activity;

			public HelloServiceConnection(ServiceActivity activity)
			{
				this.activity = activity;
			}

			public void OnServiceConnected(ComponentName name, IBinder service)
			{
				if (service is HelloServiceBinder)
				{
					activity.binder = (HelloServiceBinder)service;
					activity.isBound = true;
				}
			}

			public void OnServiceDisconnected(ComponentName name)
			{
				activity.isBound = false;
			}
		}
	}

	[Activity(Label = "NotificationTest.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());
		}
	}
}
