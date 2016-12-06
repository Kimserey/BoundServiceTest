using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms;

namespace NotificationTest.Droid
{
	public class PickFileService: IPickFileService
	{
		TaskCompletionSource<string> tcs;

		public Task<string> PickFile()
		{
			var uniqueId = Guid.NewGuid();
			var next = new TaskCompletionSource<string>(uniqueId); 

			// Interlocked.CompareExchange(ref object location1, object value, object comparand)
			// Compare location1 with comparand.
			// If equal replace location1 by value.
			// Returns the original value of location1.
			// ---
			// In this context, tcs is compared to null, if equal tcs is replaced by next,
			// and original tcs is returned.
			// We then compare original tcs with null, if not null it means that a task was 
			// already started.
			if (Interlocked.CompareExchange(ref tcs, next, null) != null)
			{
				return Task.FromResult<string>(null);
			}

			EventHandler<FilePicked> handler = null;

			handler = (sender, e) => {
				
				// Interlocaked.Exchange(ref object location1, object value)
				// Sets an object to a specified value and returns a reference to the original object.
				// ---
				// In this context, sets tcs to null and returns it.
				var task = Interlocked.Exchange(ref tcs, null);

				PickActivity.OnFilePicked -= handler;

				if (!String.IsNullOrWhiteSpace(e.AbsolutePath))
				{
					tcs.SetResult(e.AbsolutePath);
				}
				else
				{
					tcs.SetCanceled();
				}
			};

			PickActivity.OnFilePicked += handler;
			var pickIntent = new Intent(Forms.Context, typeof(PickActivity));
			pickIntent.SetFlags(ActivityFlags.NewTask);
			Forms.Context.StartActivity(pickIntent);

			return tcs.Task;
		}
	}

	public class FilePicked : EventArgs
	{
		public string AbsolutePath { get; set; }
	}

	[Activity]
	public class PickActivity: Activity
	{
		public static event EventHandler<FilePicked> OnFilePicked;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var intent = new Intent();
			intent.SetType("image/*");
			intent.SetAction(Intent.ActionPick);
			StartActivityForResult(intent, 0);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == 0 && resultCode == Result.Ok)
			{
				if (OnFilePicked != null)
				{
					OnFilePicked(this, new FilePicked { AbsolutePath = data.Data.Path });
				}
			}
		}
	}
}
