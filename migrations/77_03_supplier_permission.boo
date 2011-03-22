import AdminInterface.Models.Security

UserPermission(Name: "Просматривать\\редактировать контактную информацию",
	Shortcut: "VendorAllowContactInfo",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять условиями работы клиентов",
	Shortcut: "VendorAllowClients",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять прайс-листами",
	Shortcut: "VendorAllowPricesSet",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять источниками прайс-листов",
	Shortcut: "VendorAllowPricesSources",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять формализацией прайс-листов",
	Shortcut: "VendorAllowPricesFormalization",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять сопоставлением",
	Shortcut: "VendorAllowConManage",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять заказами",
	Shortcut: "VendorAllowOrdersManage",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять отправкой заказов",
	Shortcut: "VendorAllowSourceOrders",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Просматривать статистику документов",
	Shortcut: "VendorAllowDocumentLogs",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять доставкой документов",
	Shortcut: "VendorAllowDocumentSettings",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Изменять пароль",
	Shortcut: "VendorAllowChangePassword",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Просматривать отчеты",
	Shortcut: "VendorAllowReports",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()

UserPermission(Name: "Управлять акциями",
	Shortcut: "VendorAllowPromotions",
	Type: UserPermissionTypes.SupplierInterface,
	AssignDefaultValue: true,
	AvailableFor: UserPermissionAvailability.All).Save()
