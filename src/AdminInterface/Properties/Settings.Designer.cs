﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdminInterface.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"Направляем Вам регистрационную карту Аптеки с учетными данными (имя и пароль) для работы в системе АналитФармация – в приложении.

Направляем Вам материалы для самостоятельной установки (переустановки) копии программы АналитФармация (если Вы хотите сделать это самостоятельно)
Последний релиз программы доступен по адресу www.analit.net в разделе""Загрузить программу""
Вы получите саморазворачивающийся архив.
Запустите его на исполнение.  После распаковки на выбранном Вами диске появится папка AnalitFNew.  Из нее запустите файл analitf.exe
Далее програма запросит вписать учетные данные  (имя и пароль) - возьмите их из регистрационной карты для каждого из пользователей - в приложении.
Там же во вкладке ""Соединение"" настройте соединение с Интернет, если Вы соединяетесь через модем по телефонной линии, то посавьте галочку
""Устанавливать удаленное соединение"" и чуть ниже выберите используемое соединение.  Если работаете через Прокси-сервер, войдите в соответствующую вкладку и пропишите необходимые настройки.

Успехов в работе.  Пожалуйста, сохраните регистрационную карту

При необходимости, готовы перезвонить и дать все необходимые консультации")]
        public string RegistrationCardEmailBodyForDrugstore {
            get {
                return ((string)(this["RegistrationCardEmailBodyForDrugstore"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Направляем Вам регистрационную карту Поставщика с учетными данными (имя и пароль)" +
            " для доступа к управлению Вашей информацией в системе электронного заказа Аналит" +
            "Фармация. Регистрационная карта - в приложении.\r\n\r\nДля доступа к интерфейсу упра" +
            "вления на www.analit.net войдите в раздел “Для зарегистрированных пользователей”" +
            ", введите имя и пароль из рег.карты.\r\nПросим Вас заполнить контактную информацию" +
            " в разделе ”Контактная информация” и систематически проверять актуальность внесё" +
            "нных Вами данных.\r\nВ разделе \"Прайс-листы\" Вы можете просмотреть информацию об и" +
            "х обработке и настройке.\r\nВ разделе \"Управление сопоставлением\" работать с сопос" +
            "тавлением позиций Вашего прайс-листа в системе.\r\nВ разделе ”Клиенты” необходимо " +
            "указать E-mail для доставки вам заказов (если доставка осуществляется по e-mail)" +
            ". При входе в соответствующий регион, Вы получаете список клиентов-аптек, по кот" +
            "орым можете посмотреть информацию по обновлению данных той или иной аптеки, выст" +
            "авить скидки-наценки, сумму минимального заказа, определить и выбрать ценовую ко" +
            "лонку. А также именно здесь вам необходимо прописывать ваши внутренние коды, при" +
            "своенные конкретным аптекам (если эти коды должны прописываться в направляемую В" +
            "ам заявку).\r\nВ разделе ”Управление заказами” Вам доступны заказы, сделанные апте" +
            "ками в Ваш адрес, которые при необходимости вы можете отправить себе повторно.\r\n" +
            "\r\nНадеемся, что работа с остальными разделами (опциями) нашей системы не состави" +
            "т для Вас труда\r\n\r\nГотовы ответить на любые Ваши вопросы, как по работе в предст" +
            "авленном выше интерфейсе, так и по любым аспектам нашего IT-взаимодействия.\r\nВНИ" +
            "МАНИЕ! Информацию, указанную в регистрационной карте, необходимо хранить, т.к. о" +
            "на является ключом доступа к интерфейсу управления настройками клиентов.\r\nПросьб" +
            "а подтвердить получение.")]
        public string RegistrationCardEmailBodyForSupplier {
            get {
                return ((string)(this["RegistrationCardEmailBodyForSupplier"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:50645/ci/auth/logon.aspx")]
        public string ClientInterfaceUrl {
            get {
                return ((string)(this["ClientInterfaceUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("U:\\wwwroot\\ios\\Results\\{0}.zip")]
        public string UserPreparedDataFormatString {
            get {
                return ((string)(this["UserPreparedDataFormatString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("net.tcp://localhost:900/RemotePriceProcessorService")]
        public string WCFServiceUrl {
            get {
                return ((string)(this["WCFServiceUrl"]));
            }
        }
    }
}
