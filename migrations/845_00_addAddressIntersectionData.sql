DROP temporary table if exists customers.AddressIntersectionData;
CREATE TEMPORARY TABLE customers.AddressIntersectionData (
AdressId INT unsigned,
IntersectionId INT unsigned
)engine=MEMORY ;

insert into  customers.AddressIntersectionData
select a.id, i.id
from Customers.Intersection i
join Customers.Addresses a on a.ClientId = i.ClientId and i.LegalEntityID = a.LegalEntityId
join usersettings.PricesData pd on i.PriceId = pd.PriceCode
join customers.Suppliers s on s.id = pd.FirmCode
left join customers.addressintersection ad on ad.intersectionid = i.id and ad.addressid = a.id
where
ad.id is null
and s.RegionMask & i.RegionId > 0
group by a.id, i.id;

insert into customers.AddressIntersection (AddressId, IntersectionId)
select AdressId, IntersectionId from customers.AddressIntersectionData;