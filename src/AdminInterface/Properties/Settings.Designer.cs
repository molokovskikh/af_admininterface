﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdminInterface.Properties {


    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {

        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default {
            get {
                return defaultInstance;
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string UserPreparedDataFormatString {
            get {
                return ((string)(this["UserPreparedDataFormatString"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string WCFServiceUrl {
            get {
                return ((string)(this["WCFServiceUrl"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string RegistrationCardEmailBodyForDrugstore {
            get {
                return ((string)(this["RegistrationCardEmailBodyForDrugstore"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string RegistrationCardEmailBodyForSupplier {
            get {
                return ((string)(this["RegistrationCardEmailBodyForSupplier"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string ClientInterfaceUrl {
            get {
                return ((string)(this["ClientInterfaceUrl"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string WCFOrderSenderServiceUrl {
            get {
                return ((string)(this["WCFOrderSenderServiceUrl"]));
            }
        }
    }
}
