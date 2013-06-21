using HelloWorld.Common;
using Lotaris.LmeCl.Store;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace HelloWorld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// The photo page product token defined by you when creating the Lotaris application.
        /// </summary>
        private const string PhotoPageProductToken = "ADD_YOUR_PRODUCT_TOKEN_HERE";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            App myApp = Application.Current as App;
            myApp.LicensingFinished += async possibleException =>
                                           {
                                               if (possibleException != null)
                                               {
                                                   await new MessageDialog("Unable to contact Lotaris server. Please try again...", "Hello world! Lotaris edition").ShowAsync();
                                               }
                                           };
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Restore values stored in session state.
            if (pageState != null && pageState.ContainsKey("greetingOutputText"))
            {
                greetingOutput.Text = pageState["greetingOutputText"].ToString();
            }

            // Restore values stored in app data.
            Windows.Storage.ApplicationDataContainer roamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("userName"))
            {
                nameInput.Text = roamingSettings.Values["userName"].ToString();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["greetingOutputText"] = greetingOutput.Text;

            // The user name is already saved, so you don't need to save it here.
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            greetingOutput.Text = "Hello, " + nameInput.Text + "!";
        }

        /// <summary>
        /// Handles the TextChanged event of the NameInput control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void NameInputTextChanged(object sender, TextChangedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["userName"] = nameInput.Text;
        }

        /// <summary>
        /// Handles the Click event of the PhotoPageButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void PhotoPageButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check the existence of this in-app offer
                if (!CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(PhotoPageProductToken))
                {
                    await new MessageDialog("This option is not available.", "Access denied").ShowAsync();
                    return;
                }

                // Check if the in-app offer license is valid
                if (CurrentApp.LicenseInformation.ProductLicenses[PhotoPageProductToken].IsActive)
                {
                    if (Frame != null)
                    {
                        Frame.Navigate(typeof(PhotoPage));
                    }
                }
                else
                {
                    // If not, then start the purchase flow
                    MessageDialog msg = new MessageDialog("This is a charged option, would you like to proceed to the payment page?", "Access protected");
                    msg.Commands.Add(new UICommand("Yes", async UICommandInvokedHandler =>
                    {
                        await CurrentApp.RequestProductPurchaseAsync(PhotoPageProductToken);

                        // Re-check the existence of this in-app offer
                        if (!CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(PhotoPageProductToken))
                        {
                            await new MessageDialog("This option is not available.", "Access denied").ShowAsync();
                            return;
                        }

                        // Check if the user has the in-app offer
                        if (CurrentApp.LicenseInformation.ProductLicenses[PhotoPageProductToken].IsActive)
                        {
                            if (Frame != null)
                            {
                                Frame.Navigate(typeof(PhotoPage));
                            }
                        }
                        else
                        {
                            msg = new MessageDialog("This option is not available.", "Access denied");
                            await msg.ShowAsync();
                        }
                    }));

                    msg.Commands.Add(new UICommand("No"));
                    await msg.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(UnauthorizedAccessException))
                {
                    new MessageDialog("An error occurred while trying to navigate to the PhotoPage", "Hello world! Lotaris edition").ShowAsync();
                }
            }
        }
    }
}