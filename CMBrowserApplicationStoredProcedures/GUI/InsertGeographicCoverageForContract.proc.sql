IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertGeographicCoverageForContract' )
BEGIN
	DROP PROCEDURE InsertGeographicCoverageForContract
END
GO

CREATE PROCEDURE InsertGeographicCoverageForContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@GeographicCoverageId int OUTPUT
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

		-- called from CreateContract sp

		insert into CM_GeographicCoverage
			(	
			ContractNumber, 
			Group52,  -- 3
			Group51,  -- 2
			Group50,  -- 4
			Group49,  -- 1
			AL,
			AK,
			AZ,
			AR,
			CA,
			CO,
			CT,
			DE,
			DC,
			FL,
			GA,
			HI,
			ID,
			IL,
			[IN],
			IA,
			KS,
			KY,
			LA,
			ME,
			MD,
			MA,
			MI,
			MN,
			MS,
			MO,
			MT,
			NE,
			NV,
			NH,
			NJ,
			NM,
			NY,
			NC,
			ND,
			OH,
			OK,
			[OR],
			PA,
			RI,
			SC,
			SD,
			TN,
			TX,
			UT,
			VT,
			VA,
			WA,
			WV,
			WI,
			WY,  -- 51

			PR,
			AB,
			BC,
			MB,
			NB,
			NF,
			NT,
			NS,
			[ON],
			PE,
			QC,
			SK,
			YT, -- 64
			CreatedBy,
			CreationDate,
			LastModifiedBy,
			LastModificationDate
		) 
		values
		(
			@ContractNumber,
			0,
			0,
			0,
			0,

			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0, -- 20
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,  -- 40
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,  -- 50
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			@currentUserLogin,
			GETDATE(),
			@currentUserLogin,
			GETDATE()	
		)


	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @GeographicCoverageId = SCOPE_IDENTITY()
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting geographic coverage for contract ' + @ContractNumber
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


