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
    public sealed partial class MainPage : HelloWorld.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
            App myApp = Application.Current as App;
            if (myApp.IsLicensingInitializationFinished)
            {
                UpdateUIWithLicensingInformation();
            }
            else
            {
                myApp.LicensingFinished += () => UpdateUIWithLicensingInformation();
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            greetingOutput.Text = "Hello, " + nameInput.Text + "!";
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["userName"] = nameInput.Text;
        }

        private async void PhotoPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the in-app offer license is valid
            if (!CurrentApp.LicenseInformation.ProductLicenses["np_tl"].IsActive)
            {
                // If not, then start the purchase flow
                MessageDialog msg = new MessageDialog("This is a charged option, would you like to proceed to the payment page?", "Access protected");
                msg.Commands.Add(new UICommand("Yes", async (UICommandInvokedHandler) =>
                {
                    await CurrentApp.RequestProductPurchaseAsync("np_tl");

                    // Check if the user as validate the in-app offer
                    if (!CurrentApp.LicenseInformation.ProductLicenses["np_tl"].IsActive)
                    {
                        msg = new MessageDialog("This option is not available.", "Access denied");
                        await msg.ShowAsync();
                    }
                    else
                    {
                        if (this.Frame != null)
                        {
                            this.Frame.Navigate(typeof(PhotoPage));
                        }
                    }
                }));

                msg.Commands.Add(new UICommand("No", (UICommandInvokedHandler) => { }));
                await msg.ShowAsync();
            }
            else
            {
                if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(PhotoPage));
                }
            }
        }

        private async void UpdateUIWithLicensingInformation()
        {
            if (CurrentApp.LicenseInformation.IsTrial)
            {
                Button_LicenseInfo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (CurrentApp.LicenseInformation.IsActive)
                {
                    TextBlock_LicenseInformation.Text = "Trial license will expire on " + String.Format("{0:ddd, MMM d, yyyy}", CurrentApp.LicenseInformation.ExpirationDate);
                }
                else
                {
                    TextBlock_LicenseInformation.Text = "Trial license expired.";
                    MessageDialog msg = new MessageDialog("Your trial license has expired. You must buy a new license.", "Trial expired");
                    msg.Commands.Add(new UICommand("Buy license", async (UICommandInvokedHandler) =>
                    {
                        try
                        {
                            await CurrentApp.RequestAppPurchaseAsync();
                            if (CurrentApp.LicenseInformation.IsActive)
                            {
                                MessageDialog successMsg = new MessageDialog("Thanks for your purchase.", "Purchase succeeded");
                                await successMsg.ShowAsync();
                            }
                            else
                            {
                                MessageDialog cancelMsg = new MessageDialog("The license for this application has expired. You must buy a new license to use this app.", "Purchase failed");
                                await cancelMsg.ShowAsync();
                                Application.Current.Exit();
                            }
                        }
                        catch (Exception)
                        {
                            MessageDialog errorMsg = new MessageDialog("An error occurs, please try again later.", "Error");
                            errorMsg.ShowAsync();
                        }
                    }));
                    msg.Commands.Add(new UICommand("Exit", (UICommandInvokedHandler) =>
                    {
                        Application.Current.Exit();
                    }));
                    await msg.ShowAsync();
                }
            }
            else
            {
                if (CurrentApp.LicenseInformation.IsActive)
                {
                    TextBlock_LicenseInformation.Text = "Full version";
                    Button_LicenseInfo.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    MessageDialog msg = new MessageDialog("Your license has expired. You must buy a new license.", "License expired");
                    msg.Commands.Add(new UICommand("Buy license", async (UICommandInvokedHandler) =>
                    {
                        try
                        {
                            await CurrentApp.RequestAppPurchaseAsync();
                            if (CurrentApp.LicenseInformation.IsActive)
                            {
                                MessageDialog successMsg = new MessageDialog("Thanks for your purchase.", "Purchase succeeded");
                                await successMsg.ShowAsync();
                            }
                            else
                            {
                                MessageDialog cancelMsg = new MessageDialog("The license for this application has expired. You must buy a new license to use this app.", "Purchase failed");
                                await cancelMsg.ShowAsync();
                                Application.Current.Exit();
                            }
                        }
                        catch (Exception)
                        {
                            MessageDialog errorMsg = new MessageDialog("An error occurs, please try again later.", "Error");
                            errorMsg.ShowAsync();
                        }
                    }));
                    msg.Commands.Add(new UICommand("Exit", (UICommandInvokedHandler) =>
                    {
                        Application.Current.Exit();
                    }));
                    await msg.ShowAsync();
                }
            }
        }

        private async void ButtonLicenseInfo_Click(object sender, RoutedEventArgs e)
        {
            await CurrentApp.ShowLicenseInformation();
        }

    }
}