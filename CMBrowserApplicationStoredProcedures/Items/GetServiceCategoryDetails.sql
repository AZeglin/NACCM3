IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetServiceCategoryDetails' )
BEGIN
	DROP PROCEDURE GetServiceCategoryDetails
END
GO

CREATE PROCEDURE GetServiceCategoryDetails
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(50),
@ContractId int,
@ServiceCategoryId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
		
BEGIN TRANSACTION

	--select  [621I_Category_ID] as ServiceCategoryId,
	--	 [621I_Category_Description] as CategoryDescription,
	--	 [SIN] 

 --   from tlkup_621I_Category_List c 
	--where [621I_Category_ID] = @ServiceCategoryId

	select ServiceCategoryId, 	
	ServiceCategoryDescription,
	[SIN]
	from CM_ServiceCategories c 
	where ServiceCategoryId = @ServiceCategoryId
	                                               
	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 -- or @rowCount = 0
	BEGIN
		select @errorMsg = 'Error selecting service category details for id=' + convert( nvarchar(20), @ServiceCategoryId )
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



