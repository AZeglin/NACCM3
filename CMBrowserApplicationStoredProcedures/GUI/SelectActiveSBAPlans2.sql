IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveSBAPlans2' )
BEGIN
	DROP PROCEDURE SelectActiveSBAPlans2
END
GO

CREATE PROCEDURE SelectActiveSBAPlans2
(
@CurrentUser uniqueidentifier,
@IncludeSelect bit,
@IncludeInactive bit
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

if @IncludeSelect = 1
BEGIN

	select -1 as SBAPlanId,
		'-- select --' as PlanName,
		1 as PlanTypeId,			-- defaulting to company
		t.PlanTypeDescription,
		'' as PlanAdministratorName,
		'' as PlanAdministratorAddress,
		'' as PlanAdministratorCity,
		-1 as PlanAdministratorCountryId,
		'--' as PlanAdministratorState,
		'' as PlanAdministratorZip,
		'' as PlanAdministratorPhone,
		'' as PlanAdministratorFax, 
		'' as PlanAdministratorEmail,
		'' as PlanNotes,
		'' as Comments  
	from tbl_sba_PlanType t 
	where t.PlanTypeID = 1

	union 

	select p.SBAPlanID as SBAPlanId,
		PlanName,
		p.PlanTypeID as PlanTypeId, 
		t.PlanTypeDescription,
		Plan_Admin_Name as PlanAdministratorName,
		Plan_Admin_Address1 as PlanAdministratorAddress,
		Plan_Admin_City as PlanAdministratorCity,
		Plan_Admin_CountryId as PlanAdministratorCountryId,
		Plan_Admin_State as PlanAdministratorState,
		Plan_Admin_Zip as PlanAdministratorZip,
		Plan_Admin_Phone as PlanAdministratorPhone,
		Plan_Admin_Fax as PlanAdministratorFax, 
		Plan_Admin_email as PlanAdministratorEmail,
		Plan_Notes as PlanNotes,
		Comments  
	from tbl_sba_sbaPlan p left outer join tbl_sba_PlanType t on p.PlanTypeID = t.PlanTypeID
	where p.Inactive = 0   -- Inactive is manually maintained for now
	-- plan 396 has been deleted -- where SBAPlanId <> 396 -- delete this plan

	order by PlanName

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting Active SBA Plans (1).'
		goto ERROREXIT
	END
END
else
BEGIN
	if @IncludeInactive = 0
	BEGIN
		select p.SBAPlanID as SBAPlanId,
			PlanName,
			p.PlanTypeID as PlanTypeId, 
			t.PlanTypeDescription,
			Plan_Admin_Name as PlanAdministratorName,
			Plan_Admin_Address1 as PlanAdministratorAddress,
			Plan_Admin_City as PlanAdministratorCity,
			Plan_Admin_CountryId as PlanAdministratorCountryId,
			Plan_Admin_State as PlanAdministratorState,
			Plan_Admin_Zip as PlanAdministratorZip,
			Plan_Admin_Phone as PlanAdministratorPhone,
			Plan_Admin_Fax as PlanAdministratorFax, 
			Plan_Admin_email as PlanAdministratorEmail,
			Plan_Notes as PlanNotes,
			Comments  
		from tbl_sba_sbaPlan p left outer join tbl_sba_PlanType t on p.PlanTypeID = t.PlanTypeID	
		where p.Inactive = 0     -- Inactive is manually maintained for now

		order by PlanName

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting Active SBA Plans (2).'
			goto ERROREXIT
		END
	END
	ELSE -- include inactive plans
	BEGIN
		select p.SBAPlanID as SBAPlanId,
			PlanName,
			p.PlanTypeID as PlanTypeId, 
			t.PlanTypeDescription,
			Plan_Admin_Name as PlanAdministratorName,
			Plan_Admin_Address1 as PlanAdministratorAddress,
			Plan_Admin_City as PlanAdministratorCity,
			Plan_Admin_CountryId as PlanAdministratorCountryId,
			Plan_Admin_State as PlanAdministratorState,
			Plan_Admin_Zip as PlanAdministratorZip,
			Plan_Admin_Phone as PlanAdministratorPhone,
			Plan_Admin_Fax as PlanAdministratorFax, 
			Plan_Admin_email as PlanAdministratorEmail,
			Plan_Notes as PlanNotes,
			Comments  
		from tbl_sba_sbaPlan p left outer join tbl_sba_PlanType t on p.PlanTypeID = t.PlanTypeID	
		

		order by PlanName

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting Active SBA Plans (2).'
			goto ERROREXIT
		END
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


