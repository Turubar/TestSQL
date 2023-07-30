
-- подключаем вывод статистики
SET STATISTICS TIME ON
SET STATISTICS IO ON

SELECT * FROM Person WHERE Fullname Like 'F%' AND Gender = 'Male'

CREATE INDEX IX_Person ON Person (Fullname, Gender) INCLUDE (Date_birthday) WHERE Gender = 'Male'
drop index IX_Person on Person

SELECT i.[name],
       ips.index_type_desc,
       ips.alloc_unit_type_desc,
       ips.index_depth,
       ips.index_level,
       ips.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('dbo.IndexKeySize'), NULL, NULL, 'DETAILED') ips
INNER JOIN sys.indexes i ON i.index_id = ips.index_id
AND [ips].[object_id] = [i].[object_id];
GO