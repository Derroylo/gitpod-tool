using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class PersistConfig: ConfigHelper
    {
        public static Dictionary<string, string> Variables
        {
            get {
                return appConfig.Persist.Vars;
            }

            set {
                ConfigUpdated = true;

                appConfig.Persist.Vars = value;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> Files
        {
            get {
                return appConfig.Persist.Files;
            }

            set {
                ConfigUpdated = true;

                appConfig.Persist.Files = value;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> Folders 
        { 
            get { 
                return appConfig.Persist.Folders; 
            } 
            
            set { 
                ConfigUpdated = true;
                
                appConfig.Persist.Folders = value; 
            }
        }
    }
}