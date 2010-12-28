update billing.payers p
join billing.LegalEntities le on le.PayerId = p.PayerId
set p.RecipientId = le.RecipientId
where p.RecipientId is null and le.RecipientId is not null
