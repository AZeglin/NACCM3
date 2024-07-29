IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'DefineOperationalStatusGroup' )
BEGIN
	DROP PROCEDURE DefineOperationalStatusGroup
END
GO

CREATE PROCEDURE DefineOperationalStatusGroup
(
@UserLogin nvarchar(120),
@OperationalStatusGroupId int,
@OperationalStatusIdList nvarchar(240)
)

AS

DECLARE @error int,
	@rowCount int,
	@errorMsg nvarchar(250),
	@commaSearch int,
	/* ordinality helps to define the sort order. */
	/* currently the desired outcome is to have specialized groups ( created manually ) appear at the top of the list */
	@ordinality int 
BEGIN

	if LEN( @OperationalStatusIdList ) = 0
	BEGIN
		select @errorMsg = 'Error updating OperationalStatus group. List of OperationalStatuses was blank.'
		Raiserror( @errorMsg, 16, 1 )		
	END

	select @commaSearch = CHARINDEX( ',', @OperationalStatusIdList, 0 )

	if @commaSearch = 0
	BEGIN
		select @ordinality = 9
	END
	else
	BEGIN
		select @ordinality = 5
	END
	
	update SEC_OperationalStatusGroups
	set OperationalStatusIdList = @OperationalStatusIdList,
		Ordinality = @ordinality,
		LastModifiedBy = @UserLogin,
		LastModificationDate = GETDATE()
	where OperationalStatusGroupId = @OperationalStatusGroupId

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 or @rowCount <> 1
	BEGIN
		select @errorMsg = 'Error updating OperationalStatus group with new OperationalStatus list'
		Raiserror( @errorMsg, 16, 1 )		
	END

END



