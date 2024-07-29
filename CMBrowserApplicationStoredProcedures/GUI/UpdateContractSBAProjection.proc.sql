IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractSBAProjection' )
BEGIN
	DROP PROCEDURE UpdateContractSBAProjection
END
GO

CREATE PROCEDURE UpdateContractSBAProjection
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),

@ContractNumber nvarchar(20),
@SBAPlanId int,

@ProjectionId int, 
@ProjectionStartDate datetime, 
@ProjectionEndDate datetime, 
@SBDollars money, 
@SDBDollars money, 
@WomenOwnedDollars money, 
@DisabledVeteranOwnedDollars money, 
@HubZoneDollars money, 
@HBCUDollars money, 
@VeteranOwnedDollars money, 
@TotalSubcontractingDollars money, 
@ProjectionComments nvarchar(255)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@currentUserLogin nvarchar(120)

BEGIN TRANSACTION

	exec dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @currentUserLogin OUTPUT 

	Select @error = @@error		
	if @error <> 0 or @currentUserLogin is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	update tbl_sba_Projection
		set StartDate = @ProjectionStartDate, 
			EndDate = @ProjectionEndDate, 
			SBDollars = @SBDollars, 
			SDBDollars = @SDBDollars, 
			WomenOwnedDollars = @WomenOwnedDollars, 
			DisabledVetDollars = @DisabledVeteranOwnedDollars, 
			HubZoneDollars = @HubZoneDollars, 
			HBCUDollars = @HBCUDollars, 
			VeteranOwnedDollars = @VeteranOwnedDollars, 
			TotalSubConDollars = @TotalSubcontractingDollars, 
			Comments = @ProjectionComments, 
			LastModifiedBy = @CurrentUserLogin,
			LastModificationDate = GETDATE()

	where SBAPlanID = @SBAPlanId
	and ProjectionID = @ProjectionId

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 Or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating SBA projection with ProjectionId = ' + CONVERT( nvarchar(10), @ProjectionId )
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


