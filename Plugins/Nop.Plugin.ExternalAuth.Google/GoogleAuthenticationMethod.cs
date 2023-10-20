using Nop.Core;
using Nop.Plugin.ExternalAuth.Google;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using System.Collections.Generic;


namespace Nop.Plugin.ExternalAuth.Google
{
    /// <summary>
    /// Represents method for the authentication with Google account
    /// </summary>
    public class GoogleAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public GoogleAuthenticationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/GoogleAuthentication/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return GoogleAuthenticationDefaults.VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new GoogleExternalAuthSettings());

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.ExternalAuth.Google.ClientKeyIdentifier"] = "App ID/API Key",
                ["Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint"] = "Enter your app ID/API key here. You can find it on your Google application page.",
                ["Plugins.ExternalAuth.Google.ClientSecret"] = "App Secret",
                ["Plugins.ExternalAuth.Google.ClientSecret.Hint"] = "Enter your app secret here. You can find it on your Google application page.",
                ["Plugins.ExternalAuth.Google.Instructions"] = "<p>To configure authentication with Google, please follow these steps:<br/><br/><ol><li>Navigate to the <a href=\"https://developers.Google.com/apps\" target =\"_blank\" > Google for Developers</a> page and sign in. If you don't already have a Google account, use the <b>Sign up for Google</b> link on the login page to create one.</li><li>Tap the <b>+ Add a New App button</b> in the upper right corner to create a new App ID. (If this is your first app with Google, the text of the button will be <b>Create a New App</b>.)</li><li>Fill out the form and tap the <b>Create App ID button</b>.</li><li>The <b>Product Setup</b> page is displayed, letting you select the features for your new app. Click <b>Get Started</b> on <b>Google Login</b>.</li><li>Click the <b>Settings</b> link in the menu at the left, you are presented with the <b>Client OAuth Settings</b> page with some defaults already set.</li><li>Enter \"{0:s}signin-Google\" into the <b>Valid OAuth Redirect URIs</b> field.</li><li>Click <b>Save Changes</b>.</li><li>Click the <b>Dashboard</b> link in the left navigation.</li><li>Copy your App ID and App secret below.</li></ol><br/><br/></p>"
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GoogleExternalAuthSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.ExternalAuth.Google");

            base.Uninstall();
        }

        #endregion
    }
}