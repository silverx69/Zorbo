﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Commands.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Commands.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are muzzled {0}.
        /// </summary>
        public static string AreMuzzled {
            get {
                return ResourceManager.GetString("AreMuzzled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User {0} has been banned.
        /// </summary>
        public static string Banned {
            get {
                return ResourceManager.GetString("Banned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been banned for too many invalid answers.
        /// </summary>
        public static string BannedCaptchaAnswers {
            get {
                return ResourceManager.GetString("BannedCaptchaAnswers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been banned for not answering within 2 hours of connecting.
        /// </summary>
        public static string BannedCaptchaTimeout {
            get {
                return ResourceManager.GetString("BannedCaptchaTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All user bans have been cleared by {0}.
        /// </summary>
        public static string BansCleared {
            get {
                return ResourceManager.GetString("BansCleared", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dns names matching the pattern ({0}) have been banned.
        /// </summary>
        public static string DnsBanAdd {
            get {
                return ResourceManager.GetString("DnsBanAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dns names matching the pattern ({0}) have been unbanned.
        /// </summary>
        public static string DnsBanRemove {
            get {
                return ResourceManager.GetString("DnsBanRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All dns bans have been cleared by {0}.
        /// </summary>
        public static string DnsBansCleared {
            get {
                return ResourceManager.GetString("DnsBansCleared", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User not found in history.
        /// </summary>
        public static string HistoryNotFound {
            get {
                return ResourceManager.GetString("HistoryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have ignored messages from {0}.
        /// </summary>
        public static string Ignored {
            get {
                return ResourceManager.GetString("Ignored", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not a valid index for the command you have attempted.
        /// </summary>
        public static string InvalidIndex {
            get {
                return ResourceManager.GetString("InvalidIndex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not a valid regular expression pattern ({0}).
        /// </summary>
        public static string InvalidPattern {
            get {
                return ResourceManager.GetString("InvalidPattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User {0} has been disconnected.
        /// </summary>
        public static string Kicked {
            get {
                return ResourceManager.GetString("Kicked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been muzzled.
        /// </summary>
        public static string Muzzled {
            get {
                return ResourceManager.GetString("Muzzled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IP ranges matching the pattern ({0}) have been banned.
        /// </summary>
        public static string RangeBanAdd {
            get {
                return ResourceManager.GetString("RangeBanAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IP ranges matching the pattern ({0}) have been unbanned.
        /// </summary>
        public static string RangeBanRemove {
            get {
                return ResourceManager.GetString("RangeBanRemove", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All range bans have been cleared by {0}.
        /// </summary>
        public static string RangeBansCleared {
            get {
                return ResourceManager.GetString("RangeBansCleared", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been redirected to: {0}.
        /// </summary>
        public static string Redirected {
            get {
                return ResourceManager.GetString("Redirected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User {0} has been unbanned.
        /// </summary>
        public static string Unbanned {
            get {
                return ResourceManager.GetString("Unbanned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have allowed messages from {0}.
        /// </summary>
        public static string Unignored {
            get {
                return ResourceManager.GetString("Unignored", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been unmuzzled.
        /// </summary>
        public static string Unmuzzled {
            get {
                return ResourceManager.GetString("Unmuzzled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your user id is: {0}.
        /// </summary>
        public static string UserId {
            get {
                return ResourceManager.GetString("UserId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have joined vroom {0}.
        /// </summary>
        public static string VroomNotice {
            get {
                return ResourceManager.GetString("VroomNotice", resourceCulture);
            }
        }
    }
}
