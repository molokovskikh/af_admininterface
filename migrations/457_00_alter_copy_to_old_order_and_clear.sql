DROP EVENT Orders.CopyToOldOrdersAndCLear;
CREATE DEFINER=`RootDBMS`@`dbms.adc.analit.net` EVENT Orders.`copyToOldOrdersAndClear` ON SCHEDULE EVERY 1 DAY STARTS '2010-08-11 00:01:00' ON COMPLETION PRESERVE ENABLE DO BEGIN

  DELETE
  FROM
    OrdersHead
  WHERE
    pricecode != 2647
    AND deleted = 1;

  DELETE
  FROM
    OrdersHead
  WHERE
    Deleted = 1;

  DELETE
  FROM
    OrdersHead
  WHERE
    WriteTime < curdate() - INTERVAL 1 WEEK
    AND Submited = 0;


  INSERT INTO OrdersOld.OrdersHead
    (
    RowID, WriteTime, ClientCode, AddressId, UserId, PriceCode, RegionCode, PriceDate, SubmitDate
    )
  SELECT Oh.RowID
       , Oh.WriteTime
       , Oh.ClientCode
       , Oh.AddressId
       , Oh.UserId
       , Oh.PriceCode
       , Oh.RegionCode
       , Oh.PriceDate
       , Oh.SubmitDate
  FROM
    Customers.Users U,
    usersettings.RetClientsSet R,
    OrdersHead Oh
  LEFT JOIN ordersold.OrdersHead Ooh
  USING (RowId)
  WHERE
    Oh.WriteTime < curdate()
    AND oh.`UserId` = U.`Id`
    AND U.`PayerId` != 921
    AND U.`ClientId` = R.`ClientCode`
    AND R.`ServiceClient` = 0
    AND R.`InvisibleOnFirm` = 0
    AND Oh.deleted = 0
    AND oh.Submited = 1
    AND Ooh.RowId IS NULL;


  INSERT INTO OrdersOld.OrdersList
    (
    OrderID, ProductId, CodeFirmCr, SynonymCode, SynonymFirmCrCode, Code, CodeCr, Quantity, Junk, RequestRatio, OrderCost, MinOrderCount, Cost
    )
  SELECT ol.OrderID
       , ol.ProductId
       , ol.CodeFirmCr
       , ol.SynonymCode
       , ol.SynonymFirmCrCode
       , max(ol.Code)
       , max(ol.CodeCr)
       , sum(ol.Quantity)
       , ol.Junk
       , ol.RequestRatio
       , ol.OrderCost
       , ol.MinOrderCount
       , (sum(ol.Cost*ol.Quantity)/sum(ol.Quantity))
  FROM
    Customers.Users U,
    usersettings.RetClientsSet R,
    OrdersHead Oh,
    OrdersList Ol
  LEFT JOIN ordersold.OrdersList Ool
  USING (OrderId)
  WHERE
    ool.orderid IS NULL
    AND oh.RowId = Ol.OrderId
    AND oh.`UserId` = U.`Id`
    AND U.`PayerId` != 921
    AND R.ClientCode = Oh.ClientCode
    AND R.`ServiceClient` = 0
    AND R.`InvisibleOnFirm` = 0
    AND Oh.deleted = 0
    AND oh.Submited = 1
    AND WriteTime < curdate()
  GROUP BY
    ol.orderid
  , ol.productid
  , ol.codefirmcr
  , ol.junk;


  DELETE
  FROM
    OrdersHead
  WHERE
    WriteTime < curdate() - INTERVAL 3 MONTH;



END;
