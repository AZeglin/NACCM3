IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DeleteChangeRequest')
	BEGIN
		DROP  Procedure  DeleteChangeRequest
	END

GO

CREATE Procedure DeleteChangeRequest
(
@CurrentUser uniqueidentifier,
@ChangeRequestId int
)

AS 

DECLARE @error int,
	@rowcount int,
	@loginName  nvarchar(120),
	@errorMsg nvarchar(250),
	@ChangeRequestStatus nchar(1),
	@ChangeRequestCreator nvarchar(120)
	
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 

	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	select @ChangeRequestStatus = ChangeRequestStatus,
		@ChangeRequestCreator = CreatedBy
	from DI_ChangeRequests
	where ChangeRequestId = @ChangeRequestId
	
	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error selecting change request for deletion. ChangeRequestId=' + Convert(nvarchar(20), @ChangeRequestId )
		GOTO ERROREXIT
	END

	if @ChangeRequestStatus = 'D' and @ChangeRequestCreator = @loginName
	BEGIN
		delete DI_ChangeRequests
		where ChangeRequestId = @ChangeRequestId

		select @error = @@error

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error deleting change request with ChangeRequestId ' + Convert(nvarchar(20), @ChangeRequestId )
			GOTO ERROREXIT
		END
	END	
	else
	BEGIN
		select @errorMsg = 'Deletion is only allowed for change requests created by the current user in a draft state. ChangeRequestId=' + Convert(nvarchar(20), @ChangeRequestId )
		GOTO ERROREXIT
	END
	
	GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:
	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)





