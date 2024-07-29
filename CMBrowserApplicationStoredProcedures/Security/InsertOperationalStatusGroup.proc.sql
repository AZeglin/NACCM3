IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'InsertOperationalStatusGroup' )
BEGIN
	DROP PROCEDURE InsertOperationalStatusGroup
END
GO

CREATE PROCEDURE InsertOperationalStatusGroup
(
@UserLogin nvarchar(120),
@OperationalStatusGroupDescription nvarchar(60),
@OperationalStatusGroupId int OUTPUT
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250)
	
BEGIN TRANSACTION

	if exists ( select OperationalStatusGroupDescription from SEC_OperationalStatusGroups where OperationalStatusGroupDescription = @OperationalStatusGroupDescription )
	BEGIN
		select @errorMsg = 'An entry for that group already exists'
		goto ERROREXIT	
	END
	else
	BEGIN
	
		/* 9 is default ordinality; actual ordinality is set when operational status id list is added */
		insert into SEC_OperationalStatusGroups
		( OperationalStatusGroupDescription, OperationalStatusIdList, Ordinality, LastModifiedBy, LastModificationDate )
		values
		( @OperationalStatusGroupDescription, '', 9, @UserLogin, GETDATE() )
		
		select @OperationalStatusGroupId = SCOPE_IDENTITY(), @error = @@error, @rowCount = @@rowcount
		
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error inserting new operational status group'
			goto ERROREXIT	
		END

		exec UpdateGeneratedGroupOperationalStatuses @UserLogin = @UserLogin

		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error calling UpdateGeneratedGroupOperationalStatuses from within InsertOperationalStatusGroup for group ' + @OperationalStatusGroupDescription
			goto ERROREXIT	
		END

	END
	
GOTO OKEXIT

ERROREXIT:


	raiserror( @errorMsg, 16 , 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this the highest level */
		ROLLBACK TRANSACTION
	END
	
	RETURN ( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN ( 0 )







