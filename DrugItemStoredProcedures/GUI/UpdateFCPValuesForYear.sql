IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateFCPValuesForYear')
	BEGIN
		DROP  Procedure  UpdateFCPValuesForYear
	END

GO

CREATE Procedure UpdateFCPValuesForYear
(@year int
)
As

Declare 
	@error int,
	@YearId int,
	@errorMsg varchar(1000)

BEGIN TRANSACTION

	Select @YearId = YearId
	From DI_YearLookUp
	Where YearValue = @year

	Exec ProcessFCPStagingValuesForYear @year
	
	Select @error = @@error

	If @error <> 0
	Begin
		select @errorMsg = 'Error getting FCP values from PBM'
		goto ERROREXIT
	End
	Else
	Begin
		Update a
			Set a.FCP = b.FCP,
				a.LastModifiedby = 'UpdateFCPValuesForYear',
				a.LastModificationDate = getdate()
		From DI_FCP a
		Join NFAMP_STG b
		on a.ContractId = b.ContractId
		and a.DrugItemId = b.DrugItemId
		and a.DrugItemNDCId = b.DrugItemNDCId
		Where a.YearId = @YearId
		and (b.DrugItemId is not null or b.DrugItemId <> 0)

		Select @error = @@error
		If @error <> 0
		Begin
			select @errorMsg = 'Error Updating FCP values'
			goto ERROREXIT
		End	
	End
	
GOTO OKEXIT

ERROREXIT:
	raiserror( @errorMsg, 16, 1 ) 

	IF @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
	/* only rollback iff this the highest level */ 
		ROLLBACK TRANSACTION
	END

	RETURN (-1)

OKEXIT:

	IF @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	
	RETURN (0)

