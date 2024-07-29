IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetServiceCategoriesForItems' )
BEGIN
	DROP PROCEDURE GetServiceCategoriesForItems
END
GO

CREATE PROCEDURE GetServiceCategoriesForItems
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20), 
@ContractId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
		
BEGIN TRANSACTION

	--select [621I_Category_ID] as ServiceCategoryId, 
	--		[SIN] + ' : ' + [621I_Category_Description] AS CategorySelected, 
	--		[SIN], 
	--		[621I_Category_Description] as CategoryDescription
	--   from tlkup_621I_Category_List c join tbl_Cntrcts_SINs s on c.[SIN] = s.[SINs]
	--where s.CntrctNum = @ContractNumber
	--order by [SIN], [621I_Category_Description]

	select ServiceCategoryId, 
	[SIN] + ' : ' + ServiceCategoryDescription AS CategorySelected, 
	[SIN],
	ServiceCategoryDescription	
	from CM_ServiceCategories c join tbl_Cntrcts_SINs s on c.[SIN] = s.[SINs]
	where s.CntrctNum = @ContractNumber
	and c.Inactive = 0
	order by c.[SIN], c.ServiceCategoryDescription
                                               

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error selecting service categories for contract ' + @ContractNumber
		goto ERROREXIT
	END


goto OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16, 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )



