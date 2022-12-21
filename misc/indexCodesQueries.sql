
/*Delete duplicate RecIDs*/
DELETE FROM [CherwellDB].[dbo].[IndexCodes]
	WHERE [RecID] IN (SELECT TOP (SELECT COUNT ([IndexCode]) - 1
			FROM [CherwellDB].[dbo].[IndexCodes] 
			WHERE [IndexCode] IN (SELECT TOP 1 [IndexCode]
				FROM [CherwellDB].[dbo].[IndexCodes]
				GROUP BY [IndexCode]
			HAVING COUNT(*) > 1))
		[RecID]
		FROM [CherwellDB].[dbo].[IndexCodes]
		WHERE [IndexCode] IN (SELECT TOP 1 [IndexCode]
				FROM [CherwellDB].[dbo].[IndexCodes]
				GROUP BY [IndexCode]
				HAVING COUNT(*) > 1));


/*Find duplicate RecID entries*/
SELECT [IndexCode]
	FROM [CherwellDB].[dbo].[IndexCodes]
	WHERE [IndexCode] IN (SELECT [IndexCode]
			FROM [CherwellDB].[dbo].[IndexCodes]
			GROUP BY [IndexCode]
			HAVING COUNT(*) > 1);
