namespace Gitpod.Tool.Classes.Configuration
{
    class ConfigConfiguration
    {
        private bool allowPreReleases = false;

        public bool AllowPreReleases { get { return allowPreReleases; } set { allowPreReleases = value; }}
    }
}