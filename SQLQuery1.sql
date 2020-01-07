 SELECT pt.Id AS ProductTypeId, SUM(p.Price), pt.[Name] FROM ProductType pt
                                        LEFT JOIN Product p ON pt.id = p.ProductTypeId
                                        LEFT JOIN OrderProduct op ON op.ProductId = p.Id
                                        LEFT JOIN [Order] o ON o.Id = op.OrderId
                                        --WHERE o.UserPaymentTypeId is not null
                                        GROUP BY pt.Id, pt.[Name]
