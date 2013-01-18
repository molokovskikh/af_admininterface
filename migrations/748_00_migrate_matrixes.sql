update Usersettings.RetClientsSet rcs
set BuyingMatrixAction = WarningOnBuyingMatrix;

alter table Farm.Matrices
add column PriceId int unsigned;

insert into Farm.Matrices(PriceId)
select PriceCode
from Usersettings.PricesData
where BuyingMatrix = 1;

update Usersettings.PricesData pd
join Farm.Matrices m on m.PriceId = pd.PriceCode
set pd.Matrix = m.Id;

update Farm.BuyingMatrix b
join Farm.Matrices m on m.PriceId = b.PriceId
set b.MatrixId = m.Id;

update Usersettings.RetClientsSet rcs
join Farm.Matrices m on rcs.BuyingMatrixPriceId = m.PriceId
set rcs.BuyingMatrix = m.Id;

update Usersettings.RetClientsSet rcs
join Farm.Matrices m on rcs.OfferMatrixPriceId = m.PriceId
set rcs.BuyingMatrix = m.Id;

update Usersettings.RetClientsSet rcs
set rcs.BuyingMatrixPriceId = null
where rcs.BuyingMatrix = null;

update Usersettings.RetClientsSet rcs
set rcs.OfferMatrixPriceId = null
where rcs.OfferMatrix = null;

alter table Farm.Matrices
drop column PriceId;
