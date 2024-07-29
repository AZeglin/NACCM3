IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractAndVendorInfo' )
BEGIN
	DROP PROCEDURE GetContractAndVendorInfo
END
GO

CREATE PROCEDURE GetContractAndVendorInfo
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
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
@PrimaryName nvarchar(30) OUTPUT,
@PrimaryPhone nvarchar(15) OUTPUT,
@PrimaryExt nvarchar(5) OUTPUT,
@PrimaryFax nvarchar(15) OUTPUT,
@PrimaryEmail nvarchar(50) OUTPUT,
@VendorWebUrl nvarchar(50) OUTPUT,
@PrimaryAddress1 nvarchar(100) OUTPUT,
@PrimaryAddress2 nvarchar(100) OUTPUT,
@PrimaryCity nvarchar(20) OUTPUT,
@PrimaryCountryId int OUTPUT,
@PrimaryState nvarchar(2) OUTPUT,
@PrimaryZip nvarchar(10) OUTPUT
)

AS

DECLARE @error int,
	@rowcount int,
	@errorMsg nvarchar(250)
	
BEGIN

	select @ScheduleNumber = c.Schedule_Number,
		@OwnerId = c.CO_ID,
		@VendorName = Contractor_Name,
		@Description = Drug_Covered,
		@AwardDate = Dates_CntrctAward,
		@ExpirationDate = Dates_CntrctExp,
		@CompletionDate = Dates_Completion,
		@EffectiveDate = Dates_Effective,
		@ScheduleName = s.Schedule_Name,
		@OwnerName = u.FullName,

		@PrimaryPhone = c.POC_Primary_Phone,
		@PrimaryExt = c.POC_Primary_Ext,
		@PrimaryFax = c.POC_Primary_Fax,
		@PrimaryEmail = c.POC_Primary_Email,
		@VendorWebUrl = c.POC_VendorWeb,
		@PrimaryAddress1  = c.Primary_Address_1,
		@PrimaryAddress2  = c.Primary_Address_2,
		@PrimaryCity  = c.Primary_City,
		@PrimaryCountryId = c.Primary_CountryId,
		@PrimaryState  = c.Primary_State, 
		@PrimaryZip  = c.Primary_Zip  

	from tbl_Cntrcts c join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	join NACSEC.dbo.SEC_UserProfile u on c.CO_ID = u.CO_ID
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


