IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateContractSalesForEdit' )
BEGIN
	DROP PROCEDURE UpdateContractSalesForEdit
END
GO

CREATE PROCEDURE UpdateContractSalesForEdit
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@QuarterId int,
@SalesId int,
@IsNew bit,
@SIN nvarchar(10),
@VASales money,
@OGASales money,
@SLGSales money,
@Comments nvarchar(255),
@NewSalesId int OUTPUT
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

	if @IsNew = 0
	BEGIN
		update tbl_Cntrcts_Sales
		set VA_Sales = @VASales,
			OGA_Sales = @OGASales,
			SLG_Sales = @SLGSales,
			Comments = @Comments,
			LastModifiedBy = @currentUserLogin,
			LastModificationDate = GETDATE()
		where ID = @SalesId

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error updating sales for SIN=' + @SIN
			goto ERROREXIT
		END

		select @NewSalesId = @SalesId

	END
	else
	BEGIN
		insert into tbl_Cntrcts_Sales
		( CntrctNum, Quarter_ID, [SIN], VA_Sales, OGA_Sales, SLG_Sales, Comments, LastModifiedBy, LastModificationDate )
		values
		( @ContractNumber, @QuarterId, @SIN, @VASales, @OGASales, @SLGSales, @Comments, @currentUserLogin, GETDATE() )

		select @error = @@ERROR, @NewSalesId = SCOPE_IDENTITY()
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error inserting sales for SIN=' + @SIN
			goto ERROREXIT
		END
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


