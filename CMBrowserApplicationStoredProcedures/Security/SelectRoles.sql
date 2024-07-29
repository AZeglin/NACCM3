IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectRoles')
	BEGIN
		DROP  Procedure  SelectRoles
	END

GO

CREATE Procedure SelectRoles

AS

DECLARE @error int,
		@errorMsg nvarchar(250)
	
BEGIN

	select RoleId, RoleDescription
	from SEC_Roles
	order by RoleDescription
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting roles'
		Raiserror( @errorMsg, 16, 1 )		
	END

END
