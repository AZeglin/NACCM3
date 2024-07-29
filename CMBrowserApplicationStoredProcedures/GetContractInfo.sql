IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractInfo')
	BEGIN
		DROP  Procedure  GetContractInfo
	END

GO

CREATE Procedure GetContractInfo
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@ContractId int OUTPUT,   
@ScheduleNumber int OUTPUT,
@OwnerId int OUTPUT,
@VendorName nvarchar(75) OUTPUT,
@Description nvarchar(50) OUTPUT,
@AwardDate datetime OUTPUT,
@ExpirationDate datetime OUTPUT,
@CompletionDate datetime OUTPUT,
@EffectiveDate datetime OUTPUT,
@ScheduleName nvarchar(75) OUTPUT,
@OwnerName nvarchar(80) OUTPUT,
@VendorWebAddress nvarchar(50) OUTPUT,  -- these are used to fill the parent for header display only
@VendorAddress1 nvarchar(100) OUTPUT,
@VendorAddress2 nvarchar(100) OUTPUT,
@VendorCity nvarchar(20) OUTPUT,
@VendorState nvarchar(2) OUTPUT,
@VendorCountryName nvarchar(100) OUTPUT,
@VendorCountryId int OUTPUT,
@VendorZip nvarchar(10) OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	select @ContractId = c.Contract_Record_ID,
	    @ScheduleNumber = c.Schedule_Number,
		@OwnerId = c.CO_ID,
		@VendorName = Contractor_Name,
		@Description = Drug_Covered,
		@AwardDate = Dates_CntrctAward,
		@ExpirationDate = Dates_CntrctExp,
		@CompletionDate = Dates_Completion,
		@EffectiveDate = Dates_Effective,
		@ScheduleName = s.Schedule_Name,
		@OwnerName = u.FullName,
		@VendorWebAddress = c.POC_VendorWeb,
		@VendorAddress1 = c.Primary_Address_1,
		@VendorAddress2 = c.Primary_Address_2,
		@VendorCity = c.Primary_City,
		@VendorState = c.Primary_State,
		@VendorCountryName = x.CountryName,
		@VendorCountryId = c.Primary_CountryId,
		@VendorZip = c.Primary_Zip
	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	join NACSEC.dbo.SEC_UserProfile u on c.CO_ID = u.CO_ID
	left outer join CM_Countries x on c.Primary_CountryId = x.CountryId
	where CntrctNum = @ContractNumber

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error retrieving contract info for contract ' + @ContractNumber
		goto ERROREXIT
	END

	if @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving contract info for contract ' + @ContractNumber + ' : contract not found.'
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
      	ROLLBACK TRANSACTION
	END

    RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END

	RETURN( 0 ) 

ENDEXIT:


END


