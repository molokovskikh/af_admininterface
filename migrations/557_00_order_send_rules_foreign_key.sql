alter table OrderSendRules.order_send_rules
add constraint FK_OrderSendRules_FirmCode foreign key (FirmCode) references Customers.Suppliers(Id) on delete cascade;
