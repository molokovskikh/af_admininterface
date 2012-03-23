DROP PROCEDURE Usersettings.DeleteCost;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`DeleteCost`(IN inCostCode INT UNSIGNED)
BEGIN
  declare costType tinyint(1);
  declare baseCost, sourceId, ruleId, itemId int unsigned;

  select pd.CostType, base.CostCode, f.Id as RuleId, s.Id as SourceId, pi.Id as ItemId
  into
    costType, baseCost, ruleId, sourceId, itemId
  from usersettings.PricesCosts pc
        join usersettings.PricesData pd on pd.PriceCode = pc.PriceCode
        join usersettings.pricescosts base on base.PriceCode = pc.PriceCode
                join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
                        join Farm.FormRules f on f.Id = pi.FormRuleId
                        join Farm.sources s on s.Id = pi.SourceId
  where pc.CostCode = inCostCode and base.BaseCost = 1;

  if (inCostCode <> basecost) then
    update usersettings.intersection
    set CostCode = baseCost
    where CostCode = inCostCode;

    update Future.Intersection
    set CostId = baseCost
    where CostId = inCostCode;

    if (costType = 1) then

      delete from Farm.FormRules
      where Id = ruleId;

      delete from Farm.sources
      where Id = sourceId;

      delete from usersettings.PriceItems
      where Id = itemId;

      delete Farm.Core0
      from Farm.Core0
        join Farm.CoreCosts on Farm.Core0.Id = Farm.CoreCosts.Core_Id
      where Farm.CoreCosts.PC_CostCode = inCostCode;
    end if;

    delete from Farm.CoreCosts
    where PC_CostCode = inCostCode;

    delete from Farm.CostFormRules
    where CostCode = inCostCode;

    delete from usersettings.pricescosts where costcode = inCostCode;
  end if;
END;
