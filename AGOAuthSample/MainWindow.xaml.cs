using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AGOAuthSample
{
    public partial class MainWindow : Window
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "urn:ietf:wg:oauth:2.0:oob";

        public MainWindow()
        {
            InitializeComponent();

            UpdateAuthenticationManager();
        }

        /// <summary>
        /// Sign in to Portal
        /// </summary>
        private async void signInButton_Click(object sender, RoutedEventArgs e)
        {
            signInButton.IsEnabled = false;

            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo();

                // Use the OAuth implicit grant flow
                challengeRequest.GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                };

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = new Uri(ArcGISOnlineUrl);

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                var cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                
                // Save credential
                AuthenticationManager.Current.AddCredential(cred);

                // creat a portal
                var currentPortal = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

                // show portal info
                MessageBox.Show(currentPortal.User.FullName);

                signOutButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Report error
                MessageBox.Show("Error while signing in: " + ex.Message);

                signInButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Sign out from Portal
        /// </summary>
        private void signOutButton_Click(object sender, RoutedEventArgs e)
        {
            signOutButton.IsEnabled = false;

            try
            {
                // get the current credential
                Credential cred = AuthenticationManager.Current.FindCredential(new Uri(ArcGISOnlineUrl));

                // remove the credential from AuthenticationManager (sign out)
                AuthenticationManager.Current.RemoveCredential(cred);

                signInButton.IsEnabled = true;
            }
            catch(Exception ex)
            {
                // Report error
                MessageBox.Show("Error while signing out: " + ex.Message);

                signOutButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set required OAuth paramters to AuthenticationManager 
        /// </summary>
        private void UpdateAuthenticationManager()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo();

            // ArcGIS Online URI
            portalServerInfo.ServerUri = new Uri(ArcGISOnlineUrl);

            // Type of token authentication to use
            portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;

            // Define the OAuth information
            OAuthClientInfo oAuthInfo = new OAuthClientInfo
            {
                ClientId = AppClientId,
                RedirectUri = new Uri(OAuthRedirectUrl)
            };
            portalServerInfo.OAuthClientInfo = oAuthInfo;

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the ArcGIS Online server information with the AuthenticationManager
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize class in this project to create a new web view to show the login UI
            thisAuthenticationManager.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        /// <summary>
        /// Executed by AuthenticationManager's ChallengeHandler to implicitly challenge user for authentication
        /// </summary>
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }
    }
}
