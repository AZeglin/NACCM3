IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertDrugItemUserActivity')
	BEGIN
		DROP  Procedure  InsertDrugItemUserActivity
	END

GO

CREATE Procedure InsertDrugItemUserActivity
(
@UserName nvarchar(100),
@ActionType nchar(1),
@ActionDetails nvarchar(80),
@ActionDetailsType nchar(1)
)

AS

BEGIN

	insert into DI_UserActivity
	( UserName, ActionType, ActionDate, ActionDetails, ActionDetailsType )
	values
	( @UserName, @ActionType, getdate(), @ActionDetails, @ActionDetailsType )

END