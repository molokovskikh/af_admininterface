insert into ordersendrules.SpecialHandlers (SupplierId, HandlerId, Name)
select osr.FirmCode, osr.FormaterId, 'Специальный формат'
from ordersendrules.order_send_rules osr left join ordersendrules.SpecialHandlers S on osr.FirmCode=S.SupplierID and osr.FormaterId=S.HandlerId
where (osr.FormaterId not in (197, 195, 172)) and S.Id is null;