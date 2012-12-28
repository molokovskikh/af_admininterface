insert into ordersendrules.SpecialHandlers (SupplierId, HandlerId, Name)
select osr.FirmCode, osr.SenderId, 'Специальная доставка'
from ordersendrules.order_send_rules osr left join ordersendrules.SpecialHandlers S on osr.FirmCode=S.SupplierID and osr.SenderId=S.HandlerId
where (osr.SenderId not in (1, 2, 9)) and S.Id is null;