IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectDistributorNamesForDrugItem' )
BEGIN
	DROP PROCEDURE SelectDistributorNamesForDrugItem
END
GO

CREATE PROCEDURE SelectDistributorNamesForDrugItem
(
@currentUser uniqueidentifier
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@distributorName nvarchar(100),
		@order int

BEGIN TRANSACTION

	select @distributorName = 'To be determined'
	select @order = 0

	select @distributorName as DistributorName, @order as DistributorOrder

	union

	select distinct DistributorName, 1 as DistributorOrder
	from DI_DrugItemDistributors
	where DistributorName <> @distributorName

	order by DistributorOrder, DistributorName

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting distributor names.'
		goto ERROREXIT
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


