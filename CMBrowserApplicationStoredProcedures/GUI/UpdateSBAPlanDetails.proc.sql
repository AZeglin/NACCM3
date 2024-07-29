IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateSBAPlanDetails' )
BEGIN
	DROP PROCEDURE UpdateSBAPlanDetails
END
GO

CREATE PROCEDURE UpdateSBAPlanDetails
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@SBAPlanId int,
@PlanName	nvarchar(50),
@PlanTypeId 	int,
@PlanAdminName	nvarchar(50),
@PlanAdminAddress	nvarchar(50),
@PlanAdminCity	nvarchar(50),
@PlanAdminCountryId int,
@PlanAdminState nvarchar(2),
@PlanAdminZip	nvarchar(15),
@PlanAdminPhone	nvarchar(30),
@PlanAdminFax	nvarchar(15),
@PlanAdminEmail 	nvarchar(50)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	update tbl_sba_sbaPlan
	set PlanName = @PlanName,
		PlanTypeID = @PlanTypeId,
		Plan_Admin_Name = @PlanAdminName,
		Plan_Admin_Address1 = @PlanAdminAddress,
		Plan_Admin_City = @PlanAdminCity,
		Plan_Admin_CountryId = @PlanAdminCountryId,
		Plan_Admin_State = @PlanAdminState,
		Plan_Admin_Zip = @PlanAdminZip,
		Plan_Admin_Phone = @PlanAdminPhone,
		Plan_Admin_Fax = @PlanAdminFax,
		Plan_Admin_email = @PlanAdminEmail,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where SBAPlanID = @SBAPlanId


	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating SBA Plan details.'
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


