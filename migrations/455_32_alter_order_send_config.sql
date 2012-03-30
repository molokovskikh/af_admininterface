DROP PROCEDURE OrderSendRules.GetOrderSendConfig;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE OrderSendRules.`GetOrderSendConfig`(IN idparam INTEGER UNSIGNED)
BEGIN

  DECLARE delivery varchar(1000);
  SET delivery = (
    select GROUP_CONCAT(ct.ContactText SEPARATOR ',')
    from orders.OrdersHead ord
        join usersettings.PricesData prd on prd.PriceCode = ord.PriceCode
        join Customers.Suppliers supp on supp.Id = prd.FirmCode
        join contacts.Contact_Groups cgr on cgr.ContactGroupOwnerId = supp.ContactGroupOwnerId
        join contacts.RegionalDeliveryGroups rdgr on rdgr.ContactGroupId = cgr.Id and rdgr.RegionId = ord.RegionCode
        join contacts.Contacts ct on ct.ContactOwnerId = cgr.Id
        join Customers.Addresses a on a.Id = ord.AddressId
        join Customers.Users u on u.Id = ord.UserId
    where ord.RowId = idparam
);

IF (SELECT count(*) > 0
  FROM OrderSendRules.order_send_rules osr
    JOIN usersettings.pricesdata pd on pd.firmcode = osr.firmcode
      JOIN orders.ordershead oh on oh.pricecode = pd.pricecode and osr.RegionCode = oh.RegionCode
   WHERE oh.rowid = idparam) THEN

   SELECT osr.id,
          delivery Destination,
          ohs.ClassName as SenderClassName,
          ohf.ClassName as FormaterClassName,
          ErrorNotificationDelay,
          SendDebugMessage
   FROM OrderSendRules.order_send_rules osr
      JOIN OrderSendRules.order_handlers ohs on ohs.Id = osr.SenderId
      JOIN OrderSendRules.order_handlers ohf on ohf.Id = osr.FormaterId
      JOIN usersettings.pricesdata pd on pd.firmcode = osr.firmcode
        JOIN orders.ordershead oh on oh.pricecode = pd.pricecode and osr.RegionCode = oh.RegionCode
          JOIN usersettings.regionaldata rd on rd.regioncode = oh.regioncode and rd.firmcode = pd.firmcode
            JOIN Customers.Suppliers s on s.Id = pd.firmcode
              JOIN Customers.Clients customer on customer.Id = oh.clientcode
   WHERE oh.rowid = idparam;

ELSE

   SELECT osr.id,
          delivery Destination,
          ohs.ClassName as SenderClassName,
          ohf.ClassName as FormaterClassName,
          ErrorNotificationDelay,
          SendDebugMessage
    FROM OrderSendRules.order_send_rules osr
      JOIN OrderSendRules.order_handlers ohs on ohs.Id = osr.SenderId
      JOIN OrderSendRules.order_handlers ohf on ohf.Id = osr.FormaterId
      JOIN usersettings.pricesdata pd on pd.firmcode = osr.firmcode
        JOIN orders.ordershead oh on oh.pricecode = pd.pricecode
          JOIN usersettings.regionaldata rd on rd.regioncode = oh.regioncode and rd.firmcode = pd.firmcode
            JOIN Customers.Suppliers s on s.Id = pd.firmcode
              JOIN Customers.Clients customer on customer.Id = oh.clientcode
   WHERE osr.RegionCode is null
         AND oh.rowid =  idparam;

END IF;
END;
