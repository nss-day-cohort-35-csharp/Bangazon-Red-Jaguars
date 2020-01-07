-- QUERY FOR PRODUCTS WTIH EXTRAS
--SELECT  p.Id, pt.[Name] AS ProductType, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded, COUNT(op.ProductId) AS PopularityIndex FROM Product p
                                       
--                                       LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id
--                                       LEFT JOIN OrderProduct op ON p.Id = op.ProductId
--                                      GROUP BY p.Id, p.ProductTypeId, pt.[Name], p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded
--                                        HAVING 1=1 AND (p.Title LIKE '%e%') OR (p.[Description] LIKE '%e%')
--                                        ORDER BY COUNT(op.ProductId) DESC

--QUERY for Available PUTERS
--SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model From Computer c
--LEFT JOIN Employee e ON e.ComputerId = c.Id
--WHERE e.Id IS NULL AND c.DecomissionDate IS NULL

--QUERY for UNAVAILABLE PUTERS

--SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model From Computer c
--LEFT JOIN Employee e ON e.ComputerId = c.Id
--WHERE e.Id IS NOT NULL OR c.DecomissionDate IS NOT NULL

--Query for checking is PUTER IS ASSIGNED
--SELECT Id, FirstName from Employee
--WHERE ComputerId = 5

--PAGINATION
--SELECT  p.Id, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded, COUNT(op.ProductId) AS PopularityIndex FROM Product p
                                       
--LEFT JOIN OrderProduct op ON p.Id = op.ProductId
--GROUP BY p.Id, p.ProductTypeId, p.CustomerId, p.Price, p.[Description], p.Title, p.DateAdded
                                       
--HAVING 1=1

--OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY

--QUERY for REV REPORT

SELECT productId 


