IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateGeographicCoverageForContract' )
BEGIN
	DROP PROCEDURE UpdateGeographicCoverageForContract
END
GO

CREATE PROCEDURE UpdateGeographicCoverageForContract
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(50),
@Group52 bit=0,  -- 3
@Group51 bit=0,  -- 2
@Group50 bit=0,  -- 4
@Group49 bit=0,  -- 1
@AL bit=0,
@AK bit=0,
@AZ bit=0,
@AR bit=0,
@CA bit=0,
@CO bit=0,
@CT bit=0,
@DE bit=0,
@DC bit=0,
@FL bit=0,
@GA bit=0,
@HI bit=0,
@ID bit=0,
@IL bit=0,
@IN bit=0,
@IA bit=0,
@KS bit=0,
@KY bit=0,
@LA bit=0,
@ME bit=0,
@MD bit=0,
@MA bit=0,
@MI bit=0,
@MN bit=0,
@MS bit=0,
@MO bit=0,
@MT bit=0,
@NE bit=0,
@NV bit=0,
@NH bit=0,
@NJ bit=0,
@NM bit=0,
@NY bit=0,
@NC bit=0,
@ND bit=0,
@OH bit=0,
@OK bit=0,
@OR bit=0,
@PA bit=0,
@RI bit=0,
@SC bit=0,
@SD bit=0,
@TN bit=0,
@TX bit=0,
@UT bit=0,
@VT bit=0,
@VA bit=0,
@WA bit=0,
@WV bit=0,
@WI bit=0,
@WY bit=0,  -- 51

@PR bit=0,
@AB bit=0,
@BC bit=0,
@MB bit=0,
@NB bit=0,
@NF bit=0,
@NT bit=0,
@NS bit=0,
@ON bit=0,
@PE bit=0,
@QC bit=0,
@SK bit=0,
@YT bit=0  -- 64

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


	update CM_GeographicCoverage
	set Group52 = @Group52,
		Group51 = @Group51,
		Group50 = @Group50,
		Group49 = @Group49, 
		AL = @AL,
		AK = @AK,
		AZ = @AZ,
		AR = @AR,
		CA = @CA,
		CO = @CO,
		CT = @CT,
		DE = @DE,
		DC = @DC,
		FL = @FL,
		GA = @GA,
		HI = @HI,
		ID = @ID,
		IL = @IL,
		[IN] = @IN,
		IA = @IA,
		KS = @KS,
		KY = @KY,
		LA = @LA,
		ME = @ME,
		MD = @MD,
		MA = @MA,
		MI = @MI,
		MN = @MN,
		MS = @MS,
		MO = @MO,
		MT = @MT,
		NE = @NE,
		NV = @NV,
		NH = @NH,
		NJ = @NJ,
		NM = @NM,
		NY = @NY,
		NC = @NC,
		ND = @ND,
		OH = @OH,
		OK = @OK,
		[OR] = @OR,
		PA = @PA,
		RI = @RI,
		SC = @SC,
		SD = @SD,
		TN = @TN,
		TX = @TX,
		UT = @UT,
		VT = @VT,
		VA = @VA,
		WA = @WA,
		WV = @WV,
		WI = @WI,
		WY = @WY,
		PR = @PR,
		AB = @AB,
		BC = @BC,
		MB = @MB,
		NB = @NB,
		NF = @NF,
		NT = @NT,
		NS = @NS,
		[ON] = @ON,
		PE = @PE,
		QC = @QC,
		SK = @SK,
		YT = @YT,
		LastModifiedBy = @currentUserLogin,
		LastModificationDate = GETDATE()
	where ContractNumber = @ContractNumber


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating geographic coverage for contract ' + @ContractNumber
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


