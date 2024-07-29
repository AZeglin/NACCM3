IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectUserProfiles')
	BEGIN
		DROP  Procedure  SelectUserProfiles
	END

GO

CREATE Procedure SelectUserProfiles
(
@ActiveOnly bit,
@Division int    --  -1 = all
)
AS

BEGIN
	/* all divisions */
	if @Division = -1
	BEGIN
		if @ActiveOnly = 1
		BEGIN
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, Inactive, UserId, Division 
			from SEC_UserProfile
			where Inactive = 0
			order by LastName
		END
		else
		BEGIN
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, Inactive, UserId, Division 
			from SEC_UserProfile
			order by LastName	
		END
	END
	else  /* specific division including Division 0 = other */	
	BEGIN	 			
		if @ActiveOnly = 1
		BEGIN
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, Inactive, UserId, Division 
			from SEC_UserProfile
			where Inactive = 0
			and Division = @Division
			order by LastName
		END
		else
		BEGIN
			select CO_ID, FirstName, LastName, FullName, UserName, User_Email, User_Phone, Inactive, UserId, Division 
			from SEC_UserProfile
			where Division = @Division
			order by LastName	
		END	
	END
END

