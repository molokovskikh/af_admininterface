update Billing.Payers p
set p.SendToMinimail = 1
where (select count(*) from Billing.PayerClients pc
	join Future.Clients c on c.Id = pc.ClientId
		join Future.Users u on u.ClientId = c.Id
	where pc.PayerId = p.PayerId and c.Status = 1 and u.Enabled = 1) <= 3;
