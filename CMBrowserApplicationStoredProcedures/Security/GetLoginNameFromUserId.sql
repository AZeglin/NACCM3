IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetLoginNameFromUserId')
	BEGIN
		DROP  Procedure  GetLoginNameFromUserId
	END

GO

CREATE Procedure GetLoginNameFromUserId
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@LoginName nvarchar(120) OUTPUT
)

AS

DECLARE		@SERVERNAME nvarchar(255),
			@error int,
			@errorMsg nvarchar(200),
			@SQL nvarchar(2400),
			@SQLParms nvarchar(1000)


BEGIN

	-- test for SQL1 usage
	SELECT @SERVERNAME = @@SERVERNAME
	
	if @SERVERNAME is null
	BEGIN
		-- RUNNING ON SQL1 - SQL2000 cant handle server name, so omit it
		select @SQL = N'select @LoginName_parm = u.UserName 
		from [' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile] u
		where u.UserId = @CurrentUser_parm'
	END
	else
	BEGIN
		select @SQL = N'select @LoginName_parm = u.UserName 
		from [' + @SecurityServerName + '].[' + @SecurityDatabaseName + '].[dbo].[SEC_UserProfile] u
		where u.UserId = @CurrentUser_parm'
	END
	
	select @SQLParms = N'@CurrentUser_parm uniqueidentifier, @LoginName_parm nvarchar(120) OUTPUT'

	Exec SP_executeSQL @SQL, @SQLParms, @CurrentUser_parm = @CurrentUser, @LoginName_parm = @LoginName OUTPUT

	select @error = @@error

	if @error <> 0
	BEGIN
		select @errorMsg = 'Error retrieving login name for user id:' + convert( nvarchar(100), @CurrentUser )
		raiserror( @errorMsg, 16, 1 )
	END
	
END

