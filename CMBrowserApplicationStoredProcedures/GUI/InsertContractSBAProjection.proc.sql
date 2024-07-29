IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertContractSBAProjection' )
BEGIN
	DROP PROCEDURE InsertContractSBAProjection
END
GO

CREATE PROCEDURE InsertContractSBAProjection
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),

@ContractNumber nvarchar(20),
@SBAPlanId int, 

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
@ProjectionComments nvarchar(255),
@ProjectionId int OUTPUT
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

	insert into tbl_sba_Projection
	( SBAPlanID, StartDate, EndDate, SBDollars, SDBDollars, WomenOwnedDollars, DisabledVetDollars, HubZoneDollars, 
		HBCUDollars, VeteranOwnedDollars, TotalSubConDollars, Comments, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	select @SBAPlanId, @ProjectionStartDate, @ProjectionEndDate, @SBDollars, @SDBDollars, @WomenOwnedDollars, @DisabledVeteranOwnedDollars, @HubZoneDollars, 
		@HBCUDollars, @VeteranOwnedDollars, @TotalSubcontractingDollars, @ProjectionComments, @currentUserLogin, GETDATE(), @currentUserLogin, GETDATE()
		

	select @error = @@ERROR, @ProjectionId = SCOPE_IDENTITY()
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting new projection for SBAPlanId = ' + CONVERT( nvarchar(10), @SBAPlanId )
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


