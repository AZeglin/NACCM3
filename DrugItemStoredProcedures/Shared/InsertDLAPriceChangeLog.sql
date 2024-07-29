IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'InsertDLAPriceChangeLog') AND type in (N'P', N'PC'))
	DROP PROCEDURE InsertDLAPriceChangeLog
GO

CREATE Procedure InsertDLAPriceChangeLog
(
@loginName  nvarchar(120),
@ContractNumber nvarchar(20),
@UpdateSource char(1),
@UpdateType char(1),  -- 'D' = price date, 'T' = price type, 'B' = both
@DrugItemId int,
@DrugItemPriceId int = null,  
@ExistingDrugItemPriceId int = null,  
       
@PriceStartDate datetime = null,    
@ExistingPriceStartDate datetime = null,  

@IsFSS bit = null,      
@ExistingIsFSS bit = null, 

@IsBIG4 bit = null,                                           	                                                             	                  
@ExistingIsBIG4 bit = null                                          	                  
)

AS

DECLARE @error int,
		@errorMsg nvarchar(250),
		@retVal int,
	
		@NewPriceType nvarchar(5),
		@ExistingPriceType nvarchar(5),
		@Division int
	


BEGIN TRANSACTION	

	if @DrugItemPriceId = -1
	BEGIN
		select @errorMsg = 'Error InsertDLAPriceChangeLog called with Id = -1 for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	if @UpdateType = 'T' or @UpdateType = 'B'
	BEGIN

		select @Division  = s.Division
		from NAC_CM.dbo.[tlkup_Sched/Cat] s join NAC_CM.dbo.tbl_Cntrcts c on s.Schedule_Number = c.Schedule_Number
		where c.CntrctNum = @ContractNumber

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving division for contract ' + @ContractNumber
			GOTO ERROREXIT
		END

		select @ExistingPriceType = ( Case
					When @ExistingIsFSS = 1 then 'FSS'
					When @ExistingIsBIG4 = 1 then 'Big4'
					When @ExistingIsFSS = 0 and @ExistingIsBIG4 = 0 and @Division = 2 then 'NC'
					When @ExistingIsFSS = 0 and @ExistingIsBIG4 = 0 and @Division = 1 then 'FSSR'
					else Null
				End )
	
		select @NewPriceType = ( Case
					When @IsFSS = 1 then 'FSS'
					When @IsBIG4 = 1 then 'Big4'
					When @IsFSS = 0 and @IsBIG4 = 0 and @Division = 2 then 'NC'
					When @IsFSS = 0 and @IsBIG4 = 0 and @Division = 1 then 'FSSR'
					else Null
				End )

		if @ExistingPriceType <> @NewPriceType
		BEGIN

			insert into DI_DLAPriceChangeLog
			(   
				UpdateSource, UpdateType, DrugItemId, ExistingDrugItemPriceId, NewDrugItemPriceId, ExistingPriceType, NewPriceType, ExistingPriceStartDate, NewPriceStartDate, LastModificationDate
			)		
			values
			(   
				@UpdateSource, 'T', @DrugItemId, @ExistingDrugItemPriceId, @DrugItemPriceId, @ExistingPriceType, @NewPriceType, null, null, getdate()
			)

			select @error = @@error
	 
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting price type into DLAPriceChangeLog for contract ' + @ContractNumber
				GOTO ERROREXIT
			END
		
		END
	END

	if @UpdateType = 'D' or @UpdateType = 'B'
	BEGIN

		if  datediff( dd, @ExistingPriceStartDate, @PriceStartDate ) <> 0 
		BEGIN

			insert into DI_DLAPriceChangeLog
			(   
				UpdateSource, UpdateType, DrugItemId, ExistingDrugItemPriceId, NewDrugItemPriceId, ExistingPriceType, NewPriceType, ExistingPriceStartDate, NewPriceStartDate, LastModificationDate
			)		
			values
			(   
				@UpdateSource, 'D', @DrugItemId, @ExistingDrugItemPriceId, @DrugItemPriceId, null, null, @ExistingPriceStartDate, @PriceStartDate, getdate()
			)

			select @error = @@error
	 
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting price date into DLAPriceChangeLog for contract ' + @ContractNumber
				GOTO ERROREXIT
			END
		
		END
	END

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



