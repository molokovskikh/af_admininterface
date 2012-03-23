alter table OrderSendRules.order_send_rules
drop foreign key FK_order_sender_rules_FirmCode;

alter table OrderSendRules.order_send_rules
add constraint FK_order_send_rules_FirmCode foreign key(FirmCode) references Future.Suppliers(Id) on delete cascade;
