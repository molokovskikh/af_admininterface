drop temporary table if exists Usersettings.PriceCodeItems;
CREATE TEMPORARY TABLE Usersettings.PriceCodeItems (
PriceCode INT unsigned,
PriceItem INT unsigned
)engine=MEMORY ;

insert into Usersettings.PriceCodeItems
SELECT pc.PriceCode, pc.PriceItemId as PriceItem FROM usersettings.pricescosts p
join usersettings.pricesdata pd on pd.PriceCode = p.pricecode
join usersettings.pricescosts pc on pc.pricecode= pd.pricecode and pc.costcode <> p.costcode
where p.priceitemid = 0
group by p.pricecode;

update usersettings.pricescosts pc, Usersettings.PriceCodeItems pci
set pc.priceitemid = pci.Priceitem
where pc.priceitemid = 0 and pci.PriceCode = pc.PriceCode;