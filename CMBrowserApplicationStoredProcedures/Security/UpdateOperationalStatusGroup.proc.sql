﻿IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateOperationalStatusGroup' )
BEGIN
	DROP PROCEDURE UpdateOperationalStatusGroup
END
GO

CREATE PROCEDURE UpdateOperationalStatusGroup
(
@UserLogin nvarchar(120),
@OperationalStatusGroupId int,
@OperationalStatusGroupDescription nvarchar(60)
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	update SEC_OperationalStatusGroups
	set OperationalStatusGroupDescription = @OperationalStatusGroupDescription,
		LastModifiedBy = @UserLogin,	
		LastModificationDate = GETDATE()
	where OperationalStatusGroupId = @OperationalStatusGroupId
		
	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating operational status group'
		goto ERROREXIT	
	END
	
	exec UpdateGeneratedGroupOperationalStatuses @UserLogin = @UserLogin

	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error calling UpdateGeneratedGroupOperationalStatuses from within UpdateOperationalStatusGroup for group ' + @OperationalStatusGroupDescription
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



