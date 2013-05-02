using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using IrssiNotifier.PushNotificationContext;
using IrssiNotifier.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace IrssiNotifier
{
	public partial class App : Application
	{
#if(DEBUG)
		public static readonly string Baseaddress = "http://2.irssinotifierwp.appspot.com/";
#else
		public static readonly string Baseaddress = "https://irssinotifierwp.appspot.com/";
#endif
		public static readonly string Servicename = "appengine.google.com";
		public static readonly string Channelname = "IrssiNotifier";
		public static readonly int Version = 2;

		public static readonly string Hilitepageurl = "/Pages/HilitePage.xaml?NavigatedFrom=Tile";

		public static readonly Uri[] AllowedDomains =
			{
				new Uri(Baseaddress)
			};

		public static string AppGuid;

		private static PushContext _pushContext;

		public static PhoneApplicationPage GetCurrentPage()
		{
			return ((App)Current).RootFrame.Content as PhoneApplicationPage;
		}

		private static readonly Version TargetedVersion = new Version(7, 10, 8858);		//WP7.8 -> tuki erikokoisille livetiilille

		public static bool IsTargetedVersion
		{
			get { return Environment.OSVersion.Version >= TargetedVersion; }
		}

		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public PhoneApplicationFrame RootFrame { get; private set; }

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App()
		{
			// Global handler for uncaught exceptions. 
			UnhandledException += Application_UnhandledException;

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// Display the current frame rate counters.
				//Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Show the areas of the app that are being redrawn in each frame.
				//Application.Current.Host.Settings.EnableRedrawRegions = true;

				// Enable non-production analysis visualization mode, 
				// which shows areas of a page that are handed off to GPU with a colored overlay.
				//Application.Current.Host.Settings.EnableCacheVisualization = true;

				// Disable the application idle detection by setting the UserIdleDetectionMode property of the
				// application's PhoneApplicationService object to Disabled.
				// Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
				// and consume battery power when the user is not using the phone.
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}

			if (!IsolatedStorageSettings.ApplicationSettings.Contains("GUID"))
			{
				AppGuid = Guid.NewGuid().ToString();
				IsolatedStorageSettings.ApplicationSettings["GUID"] = AppGuid;
			}
			else
			{
				AppGuid = IsolatedStorageSettings.ApplicationSettings["GUID"].ToString();
			}


		}

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void ApplicationLaunching(object sender, LaunchingEventArgs e)
		{
			_pushContext = PushContext.Current ?? new PushContext(Channelname, Servicename, AllowedDomains);
			_pushContext.Error += OnPushContextError;
		}

		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void ApplicationActivated(object sender, ActivatedEventArgs e)
		{
			_pushContext = PushContext.Current ?? new PushContext(Channelname, Servicename, AllowedDomains);
			_pushContext.Error += OnPushContextError;
		}

		private static void OnPushContextError(object sender, PushContextErrorEventArgs args)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				MessageBox.Show("Push Context Error: "+args.Exception.Message, AppResources.ErrorTitle, MessageBoxButton.OK);
			}
		}

		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void ApplicationDeactivated(object sender, DeactivatedEventArgs e)
		{
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void ApplicationClosing(object sender, ClosingEventArgs e)
		{
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		#region Phone application initialization

		// Avoid double-initialization
		private bool _phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication()
		{
			if (_phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Ensure we don't initialize again
			_phoneApplicationInitialized = true;
		}

		// Do not add any additional code to this method
		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
		{
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		#endregion
	}
}