IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractingOfficersForDivision2')
	BEGIN
		DROP  Procedure  SelectContractingOfficersForDivision2
	END

GO

CREATE Procedure SelectContractingOfficersForDivision2
(
@DivisionId as int = -1,   -- -1 = all ( used for reports )
@SelectFlag int   -- -1 = include initial value of '--select--';  -2 = include initial value of 'All' for reports
)

AS

DECLARE @error int,
		@errorMsg nvarchar(200),
		@literalFullName nvarchar(50)

BEGIN 
		
	if @SelectFlag = -1 or @SelectFlag = -2
	BEGIN
	
		if @SelectFlag = -1
		BEGIN
			select @literalFullName = '--select--'
		END
		else
		BEGIN
			select @literalFullName = 'All'	
		END
	
		/* select for all divisions */
		if @DivisionId = -1
		BEGIN
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId
			from SEC_UserProfile
			where Inactive = 0
			and CO_ID in ( select distinct r.CO_ID
							from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
							where r.RoleId in 	
							(
								select distinct RoleId
								from SEC_RoleAccessPoints
								where AccessPointId in ( 5,9 ) /* contract related access points */
								union
								select distinct RoleId
								from SEC_Roles
								where RoleId in ( 1,2,3,7,8,9,20,27,28,29,30,31,32,33,34,35,36 ) /* roles that typically create contracts */
							))
			union
			
			select -1 as CO_ID, '' as FirstName, '' as LastName, @literalFullName as FullName, '' as UserName, '' as User_Email, '' as User_Phone, '00000000-0000-0000-0000-000000000000' as UserId	
			
			order by LastName
			
			select @error = @@error
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting users in role ( for all divisions )'
				raiserror( @errorMsg, 16, 1 )
			END	
		END
		else if @DivisionId = 6  /* temporary workaround until SAC is added to security manager or some other division relation is added */
		BEGIN
	
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId
			from SEC_UserProfile
			where Inactive = 0
			and CO_ID in ( select distinct r.CO_ID
							from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
							where r.RoleId = 36 )
							
			union
			
			select -1 as CO_ID, '' as FirstName, '' as LastName, @literalFullName as FullName, '' as UserName, '' as User_Email, '' as User_Phone, '00000000-0000-0000-0000-000000000000' as UserId	
			
			order by LastName
			
			select @error = @@error
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting users in role'
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else
		BEGIN
	
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId
			from SEC_UserProfile
			where Division = @DivisionId
			and Inactive = 0
			and CO_ID in ( select distinct r.CO_ID
							from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
							where r.RoleId in 	
							(
								select distinct RoleId
								from SEC_RoleAccessPoints
								where AccessPointId in ( 5,9 ) /* contract related access points */
								union
								select distinct RoleId
								from SEC_Roles
								where RoleId in ( 1,2,3,7,8,9,20,27,28,29,30,31,32,33,34,35,36 ) /* roles that typically create contracts */
							))
			union
			
			select -1 as CO_ID, '' as FirstName, '' as LastName, @literalFullName as FullName, '' as UserName, '' as User_Email, '' as User_Phone, '00000000-0000-0000-0000-000000000000' as UserId	
			
			order by LastName
			
			select @error = @@error
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting users in role'
				raiserror( @errorMsg, 16, 1 )
			END
		END
	END
	else
	BEGIN		

		if @DivisionId = 6  /* temporary workaround until SAC is added to security manager or some other division relation is added */
		BEGIN
	
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId
			from SEC_UserProfile
			where Inactive = 0
			and CO_ID in ( select distinct r.CO_ID
							from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
							where r.RoleId = 36 )
							

			select @error = @@error
			
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting users in role'
				raiserror( @errorMsg, 16, 1 )
			END
		END	
		else
		BEGIN	
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, UserId
			from SEC_UserProfile
			where Division = @DivisionId
			and Inactive = 0
			and CO_ID in ( select distinct r.CO_ID
							from SEC_UserProfileUserRoles r join SEC_UserProfile u on r.CO_ID = u.CO_ID
							where r.RoleId in 	
							(
								select distinct RoleId
								from SEC_RoleAccessPoints
								where AccessPointId in ( 5,9 ) /* contract related access points */
								union
								select distinct RoleId
								from SEC_Roles
								where RoleId in ( 1,2,3,7,8,9,20,27,28,29,30,31,32,33,34,35,36 ) /* roles that typically create contracts */
							))
			
			order by LastName
		
			select @error = @@error
		
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting users in role'
				raiserror( @errorMsg, 16, 1 )
			END
		END
	END
END
	
