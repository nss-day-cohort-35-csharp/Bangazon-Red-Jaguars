 SELECT pt.Id AS ProductTypeId, ISNULL(SUM(sales.Price),0), pt.[Name] FROM ProductType pt
                                        LEFT JOIN 
                                            (
                                            SELECT p.Price, p.ProductTypeId FROM Product p 
                                            JOIN OrderProduct op ON op.ProductId = p.Id
                                            JOIN [Order] o ON o.Id = op.OrderId
                                            WHERE o.UserPaymentTypeId is not null
                                            ) 
                                            Sales ON sales.ProductTypeId = pt.Id
                                        GROUP BY pt.Id, pt.[Name]
