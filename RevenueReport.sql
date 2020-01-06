SELECT p.Id, pt.Id AS ProductTypeId, p.Price, p.Title, pt.[Name], op.OrderId AS OrderId, o.UserPaymentTypeId AS PaymentType FROM ProductType pt
LEFT JOIN Product p ON pt.id = p.ProductTypeId
LEFT JOIN OrderProduct op ON op.ProductId = p.Id
LEFT JOIN [Order] o ON o.Id = op.OrderId