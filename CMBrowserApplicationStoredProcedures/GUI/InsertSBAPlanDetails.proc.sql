IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertSBAPlanDetails' )
BEGIN
	DROP PROCEDURE InsertSBAPlanDetails
END
GO

CREATE PROCEDURE InsertSBAPlanDetails
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
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
@PlanAdminEmail 	nvarchar(50),
@PlanNotes 	nvarchar(500) =null,
@Comments  char(255)  =null,
@NewSBAPlanId int output
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	insert into tbl_sba_sbaPlan
	( PlanName, PlanTypeID, Plan_Admin_Name, Plan_Admin_Address1, Plan_Admin_City, Plan_Admin_CountryId, Plan_Admin_State, Plan_Admin_Zip, Plan_Admin_Phone, Plan_Admin_Fax, Plan_Admin_email, 
		Plan_Notes, Comments, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
	values
	( @PlanName, @PlanTypeId, @PlanAdminName, @PlanAdminAddress, @PlanAdminCity, @PlanAdminCountryId, @PlanAdminState, @PlanAdminZip, @PlanAdminPhone, @PlanAdminFax, @PlanAdminEmail,
		@PlanNotes, @Comments, @UserLogin, GETDATE(), @UserLogin, GETDATE() )

	select @error = @@ERROR, @rowCount = @@ROWCOUNT, @NewSBAPlanId = SCOPE_IDENTITY()
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error inserting new SBA Plan for contract.'
		goto ERROREXIT
	END

	--update tbl_Cntrcts
	--set SBAPlanID = @NewSBAPlanId
	--where CntrctNum = @ContractNumber

	--select @error = @@ERROR, @rowCount = @@ROWCOUNT
	--if @error <> 0 or @rowCount <> 1
	--BEGIN
	--	select @errorMsg = 'Error updating SBA Plan Id into contract.'
	--	goto ERROREXIT
	--END

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


