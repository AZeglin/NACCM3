IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractSalesForEdit' )
BEGIN
	DROP PROCEDURE SelectContractSalesForEdit
END
GO

CREATE PROCEDURE SelectContractSalesForEdit
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(50),
@QuarterId int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	IF OBJECT_ID('tempdb..#NullSales') IS NOT NULL 
	BEGIN
		drop table #NullSales
	
		select @error = @@error
	
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error dropping #NullSales temp table.'
			goto ERROREXIT
		END
	END

	create table #NullSales
	(
		SalesId int identity(1,1) not null,
		ExternalSalesId nvarchar(30) null,  -- string representation of the id
		IsNew bit not null,
		[SIN] nvarchar(10) not null,
		VASales money null,
		OGASales money null,
		SLGSales money null,
		Comments nvarchar(255) null,
		LastModifiedBy nvarchar(120) null,
		LastModificationDate datetime null
	)

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error creating temp table #NullSales for sales edit.'
		goto ERROREXIT
	END

	insert into #NullSales
	( IsNew, [SIN], VASales, OGASales, SLGSales, Comments, LastModifiedBy, LastModificationDate )
	select 1, [SINs], 0, 0, 0, '', '', GETDATE()
	from tbl_Cntrcts_SINs
	where CntrctNum = @ContractNumber
	and Inactive = 0
	and [SINs] not in ( select [SIN] from tbl_Cntrcts_Sales
						where CntrctNum = @ContractNumber
						and Quarter_ID = @QuarterId )

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error inserting into temp table #NullSales for sales edit.'
		goto ERROREXIT
	END

	update #NullSales
	set ExternalSalesId = 'T' + CONVERT( nvarchar(29), SalesId )
	from #NullSales

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error updating id into temp table #NullSales for sales edit.'
		goto ERROREXIT
	END
		

	select 'R' + CONVERT( nvarchar(29), ID ) as ExternalSalesId, ID as SalesId, 0 as IsNew, [SIN], VA_Sales as VASales, OGA_Sales as OGASales, SLG_Sales as SLGSales, Comments, LastModifiedBy, LastModificationDate
	from tbl_Cntrcts_Sales
	where CntrctNum = @ContractNumber
	and Quarter_ID = @QuarterId

	union

	select ExternalSalesId, SalesId, IsNew, [SIN], VASales, OGASales, SLGSales, Comments, LastModifiedBy, LastModificationDate
	from #NullSales

	order by [SIN]

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error in final select for sales edit.'
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


