DROP PROCEDURE Usersettings.UpdateCostType;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`UpdateCostType`(in inPriceCode int unsigned, in inCostType tinyint(1))
BEGIN
  declare oldCostType tinyint(1);
  declare basePriceItemId, CurrentCostCode, LastSourceId, LastFormRuleId, LastPriceItemId int unsigned;
  declare baseCostId int unsigned;
  DECLARE done INT DEFAULT 0;
  DECLARE Costs CURSOR FOR
  select
    pc.CostCode
  from
    usersettings.pricescosts pc
  where
      pc.PriceCode = inPriceCode
  and pc.CostCode != baseCostId;
  DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

  select CostCode
  into baseCostId
  from UserSettings.PricesCosts
  where PriceCode = inPriceCode
  limit 1;

  select CostType
  into
    oldCostType
  from
    usersettings.pricesdata
  where
    PriceCode = inPriceCode;

  if ((oldCostType is null && inCostType is not null) || oldCostType <> inCostType) then
    update UserSettings.PricesData set CostType = inCostType where PriceCode = inPriceCode;


    if (oldCostType = 1) then
      delete from
        farm.sources
      using
        farm.sources,
        usersettings.priceitems,
        usersettings.pricescosts
      where
          sources.id = priceitems.SourceId
      and priceitems.Id = pricescosts.PriceItemId
      and pricescosts.PriceCode = inPriceCode
      and pricescosts.CostCode != baseCostId;
      delete from
        farm.formrules
      using
        farm.formrules,
        usersettings.priceitems,
        usersettings.pricescosts
      where
          formrules.id = priceitems.FormRuleId
      and priceitems.Id = pricescosts.PriceItemId
      and pricescosts.PriceCode = inPriceCode
      and pricescosts.CostCode != baseCostId;
      delete from
        usersettings.priceitems
      using
        usersettings.priceitems,
        usersettings.pricescosts
      where
          priceitems.Id = pricescosts.PriceItemId
      and pricescosts.PriceCode = inPriceCode
      and pricescosts.CostCode != baseCostId;
    end if;

    if (inCostType = 0) then

      select PriceItemId
      into
        basePriceItemId
      from
        usersettings.pricescosts
      where
          pricescosts.PriceCode = inPriceCode
      and CostCode = baseCostId;

      update
        usersettings.pricescosts
      set
        PriceItemId = basePriceItemId
      where
          pricescosts.PriceCode = inPriceCode
      and CostCode != baseCostId;
    else

      OPEN Costs;
      REPEAT
        FETCH Costs INTO CurrentCostCode;
        IF NOT done THEN

          insert into farm.sources () value ();
          select last_insert_id() into LastSourceId;

          insert into farm.formrules () values ();
          select last_insert_id() into LastFormRuleId;

          insert into usersettings.PriceItems
            (FormRuleId, SourceId)
            values
            (LastFormRuleId, LastSourceId);
          select last_insert_id() into LastPriceItemId;

          update usersettings.pricescosts set PriceItemId = LastPriceItemId where pricescosts.CostCode = CurrentCostCode;
        END IF;
      UNTIL done END REPEAT;
      CLOSE Costs;
    end if;
  end if;
END;
