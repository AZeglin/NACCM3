IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteOperationalStatusGroup' )
BEGIN
	DROP PROCEDURE DeleteOperationalStatusGroup
END
GO

CREATE PROCEDURE DeleteOperationalStatusGroup
(
@UserLogin nvarchar(120),
@OperationalStatusGroupId int
)

AS

DECLARE  @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
		
BEGIN TRANSACTION

	delete SEC_OperationalStatusGroups
	where OperationalStatusGroupId = @OperationalStatusGroupId

	select @error = @@error, @rowCount = @@rowcount

	if @error <> 0 OR @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error deleting operational status group'
		goto ERROREXIT	
	END
	
	exec UpdateGeneratedGroupOperationalStatuses @UserLogin = @UserLogin

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error calling UpdateGeneratedGroupOperationalStatuss from within DeleteOperationalStatusGroup for groupId ' + convert( nvarchar(10), @OperationalStatusGroupId )
		goto ERROREXIT	
	END
	
GOTO OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16 , 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this the highest level */
		ROLLBACK TRANSACTION
	END
	
	RETURN ( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN ( 0 )








