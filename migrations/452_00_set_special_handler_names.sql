update OrderSendRules.SpecialHandlers s
join OrderSendRules.order_handlers h  on h.Id = s.HandlerId
set name = if(h.Type = 2, "Специальная доставка", "Специальный формат")
