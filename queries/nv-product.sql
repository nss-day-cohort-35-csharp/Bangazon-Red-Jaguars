SELECT  p.Id, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded, COUNT(op.ProductId) FROM Product p
                                       LEFT JOIN OrderProduct op ON p.Id = op.ProductId
                                       GROUP BY p.Id, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded
                                       