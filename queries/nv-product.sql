SELECT  p.Id, pt.[Name] AS ProductType, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded, COUNT(op.ProductId) AS PopularityIndex FROM Product p
                                       
                                       LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id
                                       LEFT JOIN OrderProduct op ON p.Id = op.ProductId
                                      GROUP BY p.Id, p.ProductTypeId, pt.[Name], p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded
                                        HAVING 1=1 AND (p.Title LIKE '%e%') OR (p.[Description] LIKE '%e%')
                                        ORDER BY COUNT(op.ProductId) DESC