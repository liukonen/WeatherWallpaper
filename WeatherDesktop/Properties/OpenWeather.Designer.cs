﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WeatherDesktop.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class OpenWeather {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal OpenWeather() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WeatherDesktop.Properties.OpenWeather", typeof(OpenWeather).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the API key provided by openweathermap.org.
        /// </summary>
        internal static string EnterAPIKeyMessage {
            get {
                return ResourceManager.GetString("EnterAPIKeyMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter API.
        /// </summary>
        internal static string EnterAPIKeyTitle {
            get {
                return ResourceManager.GetString("EnterAPIKeyTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter New interval between 10 and 120.
        /// </summary>
        internal static string EnterIntervalMessage {
            get {
                return ResourceManager.GetString("EnterIntervalMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update Interval Minutes.
        /// </summary>
        internal static string EnterIntervalTitle {
            get {
                return ResourceManager.GetString("EnterIntervalTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to API key.
        /// </summary>
        internal static string MenuAPIKey {
            get {
                return ResourceManager.GetString("MenuAPIKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update Interval.
        /// </summary>
        internal static string MenuUpdate {
            get {
                return ResourceManager.GetString("MenuUpdate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter a number between 10 and 120.
        /// </summary>
        internal static string UpdateIntervalMessage {
            get {
                return ResourceManager.GetString("UpdateIntervalMessage", resourceCulture);
            }
        }
    }
}