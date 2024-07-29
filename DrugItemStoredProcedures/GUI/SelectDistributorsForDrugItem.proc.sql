IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDistributorsForDrugItem' )
BEGIN
	DROP PROCEDURE SelectDistributorsForDrugItem
END
GO

CREATE PROCEDURE SelectDistributorsForDrugItem
(
@CurrentUser uniqueidentifier,
@DrugItemId int,
@WithAdd bit  = 0,
@UseParentItem bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
	@currentUserLogin nvarchar(120),

	@DrugItemDistributorId int,
	@DistributorName nvarchar(100),
	@Phone nvarchar(15),
	@ContactPerson nvarchar(30),
	@Notes nvarchar(800),
	@isNewBlankRow bit,
	@ParentDrugItemId int

BEGIN TRANSACTION

	if @UseParentItem = 0
	BEGIN

		if @WithAdd = 0
		BEGIN

			select s.DrugItemDistributorId, s.DistributorName, s.Phone, s.ContactPerson, s.Notes, 0 as IsNewBlankRow
			from DI_DrugItemDistributors s 
			where s.DrugItemId = @DrugItemId
			order by s.DistributorName

			select @error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting specialty distributors for drug item id ' + CONVERT( nvarchar(20), @DrugItemId )
				goto ERROREXIT
			END

		END
		else
		BEGIN

			/* blank row definition */
			select @DrugItemDistributorId = -1,
				@DistributorName = '',
				@Phone = '',
				@ContactPerson = '',
				@Notes = '',
				@isNewBlankRow = 1

			select s.DrugItemDistributorId, s.DistributorName, s.Phone, s.ContactPerson, s.Notes, 0 as IsNewBlankRow
			from DI_DrugItemDistributors s 
			where s.DrugItemId = @DrugItemId

			union

			select @DrugItemDistributorId as DrugItemDistributorId,
				@DistributorName as DistributorName,
				@Phone as Phone,
				@ContactPerson as ContactPerson,
				@Notes as Notes,
				@isNewBlankRow as IsNewBlankRow

			order by IsNewBlankRow desc, DistributorName

			select @error = @@ERROR
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting specialty distributors for drug item id ' + CONVERT( nvarchar(20), @DrugItemId )
				goto ERROREXIT
			END
		END
	END
	else /* look up parent item id and use it to get distributors */
	BEGIN
		select @ParentDrugItemId = ParentDrugItemId
		from DI_DrugItems
		where DrugItemId = @DrugItemId

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0 or @rowCount <> 1
		BEGIN
			select @errorMsg = 'Error selecting parent id for bpa item specialty distributors for drug item id ' + CONVERT( nvarchar(20), @DrugItemId )
			goto ERROREXIT
		END

		select s.DrugItemDistributorId, s.DistributorName, s.Phone, s.ContactPerson, s.Notes, 0 as IsNewBlankRow
			from DI_DrugItemDistributors s 
			where s.DrugItemId = @ParentDrugItemId
			order by s.DistributorName

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting specialty distributors for bpa parent drug item id ' + CONVERT( nvarchar(20), @ParentDrugItemId )
			goto ERROREXIT
		END

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
		/* only rollback iff this is the highest level */
		ROLLBACK TRANSACTION
	END

	RETURN( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN( 0 )


