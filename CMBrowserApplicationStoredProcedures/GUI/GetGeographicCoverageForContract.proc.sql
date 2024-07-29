IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetGeographicCoverageForContract' )
BEGIN
	DROP PROCEDURE GetGeographicCoverageForContract
END
GO

CREATE PROCEDURE GetGeographicCoverageForContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	select GeographicCoverageId,
		ContractNumber,
		Group52,  
		Group51,  
		Group50,  
		Group49,  
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
		YT -- 64
	from CM_GeographicCoverage
	where ContractNumber = @ContractNumber


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error getting geographic coverage for contract ' + @ContractNumber
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


