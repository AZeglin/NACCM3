IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSBAPlanDetails' )
BEGIN
	DROP PROCEDURE SelectSBAPlanDetails
END
GO

CREATE PROCEDURE SelectSBAPlanDetails
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@SBAPlanId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION
	-- note: SBAPlanID may not have been saved into the contract
	select p.SBAPlanID as SBAPlanId,
		PlanName,
		p.PlanTypeID as PlanTypeId, 
		isnull(t.PlanTypeDescription,'unknown plan type') as PlanTypeDescription,
		Plan_Admin_Name as PlanAdministratorName,
		Plan_Admin_Address1 as PlanAdministratorAddress,
		Plan_Admin_City as PlanAdministratorCity,
		Plan_Admin_CountryId as PlanAdministratorCountryId,
		c.CountryName as PlanAdministratorCountryName,
		Plan_Admin_State as PlanAdministratorState,
		Plan_Admin_Zip as PlanAdministratorZip,
		Plan_Admin_Phone as PlanAdministratorPhone,
		Plan_Admin_Fax as PlanAdministratorFax, 
		Plan_Admin_email as PlanAdministratorEmail,
		Plan_Notes as PlanNotes,
		Comments  
	from tbl_sba_sbaPlan p left outer join tbl_sba_PlanType t on p.PlanTypeID = t.PlanTypeID
	left outer join CM_Countries c on p.Plan_Admin_CountryId = c.CountryId
	where p.SBAPlanID = @SBAPlanId


	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting SBA Plan details for contract.'
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


