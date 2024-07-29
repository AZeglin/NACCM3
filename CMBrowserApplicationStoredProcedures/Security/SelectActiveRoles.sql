IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectActiveRoles')
	BEGIN
		DROP  Procedure  SelectActiveRoles
	END

GO

CREATE Procedure SelectActiveRoles
(
@UserId uniqueidentifier
)

AS

DECLARE @error int,
		@errorMsg nvarchar(250)
	
BEGIN
	
	/* for runtime use, returns all active roles for comparison */
	select RoleId, RoleDescription
	from SEC_Roles
	order by RoleDescription
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting active roles'
		Raiserror( @errorMsg, 16, 1 )		
	END

END