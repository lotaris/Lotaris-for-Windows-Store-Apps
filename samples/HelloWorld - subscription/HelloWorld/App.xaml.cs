using Lotaris.LmeCl.Store;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace HelloWorld
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        /// <summary>
        /// Provides information regarding the licensing 
        /// </summary>
        public bool IsLicensingInitializationFinished { get; private set; }

        /// <summary>
        /// Raised when the licensing is loaded
        /// </summary>
        public event Action LicensingFinished;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                HelloWorld.Common.SuspensionManager.RegisterFrame(rootFrame, "appFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    await HelloWorld.Common.SuspensionManager.RestoreAsync();
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();

            /// Initialize the Lotaris licensing with an offline grace periode null
            await CurrentApp.InitializeLicensing("https://lme.onlotaris.com/core",
                "ADD_YOUR_APP_ID_HERE",
                "ADD_YOUR_PASSWORD_HERE",
                async exception =>
                {
                    MessageDialog msg = new MessageDialog("Unable to contact Lotaris server. Please try again...", "Hello world! Lotaris edition");
                    await msg.ShowAsync();
                },
                new TimeSpan(0, 0, 0));
            IsLicensingInitializationFinished = true;
            if (LicensingFinished != null)
            {
                LicensingFinished();
            }

            
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            await HelloWorld.Common.SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
