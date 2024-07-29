IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'GetStatesForContractFunction')
	BEGIN
		DROP  Function  GetStatesForContractFunction
	END

GO

CREATE FUNCTION GetStatesForContractFunction
(
@ContractNumber nvarchar(50)
)

RETURNS nvarchar(500)

AS

BEGIN

	DECLARE  @stateList as nvarchar(500),
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


	select @Group52 = Group52,
		@Group51 = Group51,
		@Group50 = Group50,
		@Group49 = Group49, 
		@AL = AL,
		@AK = AK,
		@AZ = AZ,
		@AR = AR,
		@CA = CA,
		@CO = CO,
		@CT = CT,
		@DE = DE,
		@DC = DC,
		@FL = FL,
		@GA = GA,
		@HI = HI,
		@ID = ID,
		@IL = IL,
		@IN = [IN],
		@IA = IA,
		@KS = KS,
		@KY = KY,
		@LA = LA,
		@ME = ME,
		@MD = MD,
		@MA = MA,
		@MI = MI,
		@MN = MN,
		@MS = MS,
		@MO = MO,
		@MT = MT,
		@NE = NE,
		@NV = NV,
		@NH = NH,
		@NJ = NJ,
		@NM = NM,
		@NY = NY,
		@NC = NC,
		@ND = ND,
		@OH = OH,
		@OK = OK,
		@OR = [OR],
		@PA = PA,
		@RI = RI,
		@SC = SC,
		@SD = SD,
		@TN = TN,
		@TX = TX,
		@UT = UT,
		@VT = VT,
		@VA = VA,
		@WA = WA,
		@WV = WV,
		@WI = WI,
		@WY = WY,
		@PR = PR,
		@AB = AB,
		@BC = BC,
		@MB = MB,
		@NB = NB,
		@NF = NF,
		@NT = NT,
		@NS = NS,
		@ON = [ON],
		@PE = PE,
		@QC = QC,
		@SK = SK,
		@YT = YT
	from CM_GeographicCoverage
	where ContractNumber = @ContractNumber



		set @stateList = ''

		if @Group52 = 1
		BEGIN
			set @stateList = @stateList + '50 States, DC, PR; '
		END
		if @Group51 = 1
		BEGIN
			set @stateList = @stateList + '50 States, DC; '
		END
		if @Group50 = 1
		BEGIN
			set @stateList = @stateList + '50 States; '
		END
		if @Group49 = 1
		BEGIN
			set @stateList = @stateList + '48 Contiguous, DC; '
		END
		if @AL = 1
		BEGIN
			set @stateList = @stateList + 'AL, '
		END
		if @AK = 1
		BEGIN
			set @stateList = @stateList + 'AK, '
		END
		if @AZ = 1
		BEGIN
			set @stateList = @stateList + 'AZ, '
		END
		if @AR = 1
		BEGIN
			set @stateList = @stateList + 'AR, '
		END
		if @CA = 1
		BEGIN
			set @stateList = @stateList + 'CA, '
		END
		if @CO = 1
		BEGIN
			set @stateList = @stateList + 'CO, '
		END
		if @CT = 1
		BEGIN
			set @stateList = @stateList + 'CT, '
		END
		if @DE = 1
		BEGIN
			set @stateList = @stateList + 'DE, '
		END
		if @DC = 1
		BEGIN
			set @stateList = @stateList + 'DC, '
		END
		if @FL = 1
		BEGIN
			set @stateList = @stateList + 'FL, '
		END
		if @GA = 1
		BEGIN
			set @stateList = @stateList + 'GA, '
		END
		if @HI = 1
		BEGIN
			set @stateList = @stateList + 'HI, '
		END
		if @ID = 1
		BEGIN
			set @stateList = @stateList + 'ID, '
		END
		if @IL = 1
		BEGIN
			set @stateList = @stateList + 'IL, '
		END
		if @IN = 1
		BEGIN
			set @stateList = @stateList + 'IN, '
		END
		if @IA = 1
		BEGIN
			set @stateList = @stateList + 'IA, '
		END
		if @KS = 1
		BEGIN
			set @stateList = @stateList + 'KS, '
		END
		if @KY = 1
		BEGIN
			set @stateList = @stateList + 'KY, '
		END
		if @LA = 1
		BEGIN
			set @stateList = @stateList + 'LA, '
		END
		if @ME = 1
		BEGIN
			set @stateList = @stateList + 'ME, '
		END
		if @MD = 1
		BEGIN
			set @stateList = @stateList + 'MD, '
		END
		if @MA = 1
		BEGIN
			set @stateList = @stateList + 'MA, '
		END
		if @MI = 1
		BEGIN
			set @stateList = @stateList + 'MI, '
		END
		if @MN = 1
		BEGIN
			set @stateList = @stateList + 'MN, '
		END
		if @MS = 1
		BEGIN
			set @stateList = @stateList + 'MS, '
		END
		if @MO = 1
		BEGIN
			set @stateList = @stateList + 'MO, '
		END
		if @MT = 1
		BEGIN
			set @stateList = @stateList + 'MT, '
		END
		if @NE = 1
		BEGIN
			set @stateList = @stateList + 'NE, '
		END
		if @NV = 1
		BEGIN
			set @stateList = @stateList + 'NV, '
		END
		if @NH = 1
		BEGIN
			set @stateList = @stateList + 'NH, '
		END
		if @NJ = 1
		BEGIN
			set @stateList = @stateList + 'NJ, '
		END
		if @NM = 1
		BEGIN
			set @stateList = @stateList + 'NM, '
		END
		if @NY = 1
		BEGIN
			set @stateList = @stateList + 'NY, '
		END
		if @NC = 1
		BEGIN		
			set @stateList = @stateList + 'NC, '
		END
		if @ND = 1
		BEGIN
			set @stateList = @stateList + 'ND, '
		END
		if @OH = 1
		BEGIN
			set @stateList = @stateList + 'OH, '
		END
		if @OK = 1
		BEGIN
			set @stateList = @stateList + 'OK, '
		END
		if @OR = 1
		BEGIN
			set @stateList = @stateList + 'OR, '
		END
		if @PA = 1
		BEGIN
			set @stateList = @stateList + 'PA, '
		END
		if @RI = 1
		BEGIN
			set @stateList = @stateList + 'RI, '
		END
		if @SC = 1
		BEGIN
			set @stateList = @stateList + 'SC, '
		END
		if @SD = 1
		BEGIN
			set @stateList = @stateList + 'SD, '
		END
		if @TN = 1
		BEGIN
			set @stateList = @stateList + 'TN, '
		END
		if @TX = 1
		BEGIN
			set @stateList = @stateList + 'TX, '
		END
		if @UT = 1
		BEGIN
			set @stateList = @stateList + 'UT, '
		END
		if @VT = 1
		BEGIN
			set @stateList = @stateList + 'VT, '
		END
		if @VA = 1
		BEGIN
			set @stateList = @stateList + 'VA, '
		END
		if @WA = 1
		BEGIN
			set @stateList = @stateList + 'WA, '
		END
		if @WV = 1
		BEGIN
			set @stateList = @stateList + 'WV, '
		END
		if @WI = 1
		BEGIN
			set @stateList = @stateList + 'WI, '
		END
		if @WY = 1
		BEGIN
			set @stateList = @stateList + 'WY, '
		END
		if @PR = 1
		BEGIN
			set @stateList = @stateList + 'PR, '
		END
		if @AB = 1
		BEGIN
			set @stateList = @stateList + 'AB, '
		END
		if @BC = 1
		BEGIN
			set @stateList = @stateList + 'BC, '
		END
		if @MB = 1
		BEGIN
			set @stateList = @stateList + 'MB, '
		END
		if @NB = 1
		BEGIN
			set @stateList = @stateList + 'NB, '
		END
		if @NF = 1
		BEGIN
			set @stateList = @stateList + 'NF, '
		END
		if @NT = 1
		BEGIN
			set @stateList = @stateList + 'NT, '
		END
		if @NS = 1
		BEGIN
			set @stateList = @stateList + 'NS, '
		END
		if @ON = 1
		BEGIN
			set @stateList = @stateList + 'ON, '
		END
		if @PE = 1
		BEGIN
			set @stateList = @stateList + 'PE, '
		END
		if @QC = 1
		BEGIN
			set @stateList = @stateList + 'QC, '
		END
		if @SK = 1
		BEGIN
			set @stateList = @stateList + 'SK, '
		END
		if @YT = 1
		BEGIN
			set @stateList = @stateList + 'YT'
		END

	RETURN @stateList
END