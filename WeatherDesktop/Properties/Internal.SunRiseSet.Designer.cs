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
    internal class Internal_SunRiseSet {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Internal_SunRiseSet() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WeatherDesktop.Properties.Internal.SunRiseSet", typeof(Internal_SunRiseSet).Assembly);
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
        ///   Looks up a localized string similar to Could not update, please try again.
        /// </summary>
        internal static string CouldNotUpdate {
            get {
                return ResourceManager.GetString("CouldNotUpdate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hour update.
        /// </summary>
        internal static string HourUpdateHeader {
            get {
                return ResourceManager.GetString("HourUpdateHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Enter the hour you want to update the call to get sun rise, set info..
        /// </summary>
        internal static string HourUpdateMessage {
            get {
                return ResourceManager.GetString("HourUpdateMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lat Long not set.
        /// </summary>
        internal static string LatLongSetHeader {
            get {
                return ResourceManager.GetString("LatLongSetHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Lat and Long not yet available, Manual enter (yes), or pick a supplier in sunriseset settings (no).
        /// </summary>
        internal static string LatLongSetMessage {
            get {
                return ResourceManager.GetString("LatLongSetMessage", resourceCulture);
            }
        }
    }
}