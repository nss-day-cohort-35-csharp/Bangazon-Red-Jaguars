SELECT  p.Id, pt.[Name] AS ProductType, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded, COUNT(op.ProductId) FROM Product p
                                       
                                       LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id
                                       LEFT JOIN OrderProduct op ON p.Id = op.ProductId
                                       GROUP BY p.Id, pt.[Name], p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded
                                       