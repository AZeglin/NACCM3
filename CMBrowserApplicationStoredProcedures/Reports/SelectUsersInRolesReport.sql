IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUsersInRolesReport')
	BEGIN
		DROP  Procedure  SelectUsersInRolesReport
	END
GO

CREATE Procedure SelectUsersInRolesReport
(
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@NACCMServerName nvarchar(255),
@NACCMDatabaseName nvarchar(255),
@Division int,
@Inactive bit
)

AS

Declare @rowCount int,
		@error int,
		@errorMsg nvarchar(200)
		
BEGIN TRANSACTION

/* runs in NACSEC */

/* return All Divisions */
if @Division = -1	
BEGIN
	select p.FirstName, p.LastName, p.FullName, p.Inactive, d.Description, isnull( r.RoleId, -1 ) as RoleId, isnull( o.RoleDescription, 'Not in any role' ) as RoleDescription
	from SEC_UserProfile p left outer join SEC_UserProfileUserRoles r on p.CO_ID = r.CO_ID
	left outer join SEC_Roles o on o.RoleId = r.RoleId
	join SEC_Divisions d on p.Division = d.Division
	where p.Inactive = @Inactive
	order by o.RoleDescription, p.LastName
END
else  /* specific division */
BEGIN
	select p.FirstName, p.LastName, p.FullName, p.Inactive, d.Description, isnull( r.RoleId, -1 ) as RoleId, isnull( o.RoleDescription, 'Not in any role' ) as RoleDescription
	from SEC_UserProfile p left outer join SEC_UserProfileUserRoles r on p.CO_ID = r.CO_ID
	left outer join SEC_Roles o on o.RoleId = r.RoleId
	join SEC_Divisions d on p.Division = d.Division
	where p.Division = @Division
		and p.Inactive = @Inactive
	order by o.RoleDescription, p.LastName
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


