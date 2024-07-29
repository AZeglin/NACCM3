IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateChangeRequest')
	BEGIN
		DROP  Procedure  UpdateChangeRequest
	END

GO

CREATE Procedure UpdateChangeRequest
(
@CurrentUser uniqueidentifier,
@ChangeRequestId int,
@ChangeRequestStatus nchar(1),   -- D Draft, S Submitted, R Reviewed, A Accepted, X Rejected, I Implemented
@ChangeRequestType nchar(1),  -- U Update, D Discontinuation
@ProposedDiscontinuationDate datetime,
@PackageDescription nvarchar(14),
@DispensingUnit nvarchar(10),
@ChangeRequestDescription nvarchar(1200)
)

AS 

DECLARE @error int,
	@rowcount int,
	@loginName  nvarchar(120),
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 
	
	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	update DI_ChangeRequests
	set ChangeRequestType = @ChangeRequestType, 
		ChangeRequestStatus = @ChangeRequestStatus,
		ProposedDiscontinuationDate = @ProposedDiscontinuationDate, 
		PackageDescription = @PackageDescription, 
		DispensingUnit = @DispensingUnit, 
		ChangeRequestDescription = @ChangeRequestDescription,
		LastModifiedBy = @loginName, 
		LastModificationDate = getdate()
	where DI_ChangeRequests.ChangeRequestId = @ChangeRequestId

	select @error = @@error, @rowcount = @@rowcount

	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error updating change request with ChangeRequestId ' + Convert(nvarchar(20), @ChangeRequestId )
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



