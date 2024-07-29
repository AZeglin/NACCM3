IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractingOfficers')
	BEGIN
		DROP  Procedure  SelectContractingOfficers
	END

GO

CREATE Procedure SelectContractingOfficers
AS

DECLARE
		@rowCount int,
		@error int,
		@errorMsg nvarchar(200),
		@query nvarchar(600)

BEGIN 

	select distinct r.CO_ID, u.FullName
	from SEC_UserProfileUserRoles r, SEC_UserProfile u
	where r.CO_ID = u.CO_ID
	and r.RoleId in 	
	(
		select distinct RoleId
		from SEC_RoleAccessPoints
		where AccessPointId in ( 5,7,9 ) /* contract related access points */
		union
		select distinct RoleId
		from SEC_Roles
		where RoleId in ( 1,2,3,7,8,9,20 ) /* roles that typically create contracts */
	)
	order by u.FullName

	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting users in role'
		raiserror( @errorMsg, 16, 1 )
	END
	
END

