alter table Billing.PayerClients
add constraint FK_Payers_PayerId foreign key (PayerId) references Billing.Payers(PayerId) on delete cascade,
add constraint FK_Clients_ClientId foreign key (ClientId) references Future.Clients(Id) on delete cascade;
