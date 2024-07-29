IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertFSSDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertFSSDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure InsertFSSDrugItemPrice
(
@CurrentUser uniqueidentifier,
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20),
@DrugItemId int,
@PriceStartDate datetime,         
@PriceEndDate datetime,       
@Price decimal(9,2),      
@IsTemporary bit,
@IsFSS bit,                                           	                  
@IsBIG4 bit,                                          	                  
@IsVA bit,                                            	                  
@IsBOP bit,                                           	                  
@IsCMOP bit,                                          	                  
@IsDOD bit,                                           	                  
@IsHHS bit,                                           	                  
@IsIHS bit,                                           	                  
@IsIHS2 bit,                                          	                  
@IsDIHS bit,                                          	                  
@IsNIH bit,                                           	                  
@IsPHS bit,                                           	                  
@IsSVH bit,                                           	                  
@IsSVH1 bit,                                          	                  
@IsSVH2 bit,                                          	                  
@IsTMOP bit,                                          	                  
@IsUSCG bit,
@IsFHCC bit,
@ModificationStatusId int,
@DrugItemSubItemId int = null,
@DrugItemPriceId int output                                    	                  
)

AS

DECLARE @ContractId int,
	@loginName  nvarchar(120),
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	@retVal int,
	@maxPriceId int,
	@SubItemIdentifier nchar(1)
	

BEGIN TRANSACTION	

	EXEC dbo.GetLoginNameFromUserId @CurrentUser, @SecurityServerName, @SecurityDatabaseName, @loginName OUTPUT 
	
	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END

	If cast(Convert(Varchar(10),@PriceStartDate ,20) as datetime) > cast(Convert(Varchar(10),@PriceEndDate ,20) as datetime)
	Begin
		select @errorMsg = 'Price start date : ' + cast(@PriceStartDate as varchar(12)) + 'cannot be greater than stop date: ' + cast(@PriceEndDate as varchar(12))
		GOTO ERROREXIT
	End

	
	If cast(Convert(Varchar(10),@PriceEndDate ,20) as datetime) < getdate()
	Begin
		select @errorMsg = 'Price stop date: ' + cast(@PriceEndDate as varchar(12))+  'is Expired'
		GOTO ERROREXIT
	
	End


/*	Select @maxPriceId = Case when Max(PriceId) is null then 1 else Max(PriceId) end
	From DI_DrugItemPrice
	Where DrugItemId = @DrugItemId
	And ISFSS = @IsFSS
	And IsBig4 = IsBIG4
	And	IsVA = @IsVA
	And	IsBOP = @IsBOP
	And	IsCMOP = @IsCMOP 
	And	IsDOD = @IsDOD 
	And	IsHHS = @IsHHS 
	And	IsIHS = @IsIHS
	And IsIHS2 = @IsIHS2
	And IsDIHS = @IsDIHS 
	And IsNIH =	@IsNIH 
	And IsPHS =	@IsPHS 
	And	IsSVH = @IsSVH
	And	IsSVH1 = @IsSVH1
	And IsSVH2 = @IsSVH2 
	And	IsTMOP = @IsTMOP 
	And	IsUSCG = @IsUSCG 
	And IsFHCC = @IsFHCC
*/
	
	insert into DI_DrugItemPrice
	( DrugItemId, PriceId, PriceStartDate, PriceStopDate, Price, IsTemporary, IsFSS, IsBIG4,                                          	                  
		IsVA, IsBOP, IsCMOP, IsDOD, IsHHS, IsIHS, IsIHS2, IsDIHS, 
		IsNIH, IsPHS, IsSVH, IsSVH1, IsSVH2, IsTMOP, IsUSCG, IsFHCC, DrugItemSubItemId, ExcludeFromExport,LastModificationType,
		ModificationStatusId,
		CreatedBy,
		CreationDate,
		LastModifiedBy, 
		LastModificationDate 
	)
	values
	( @DrugItemId, 1, @PriceStartDate, @PriceEndDate, @Price, @IsTemporary, @IsFSS, @IsBIG4,                                          	                  
		@IsVA, @IsBOP, @IsCMOP, @IsDOD, @IsHHS, @IsIHS, @IsIHS2, @IsDIHS, 
		@IsNIH, @IsPHS, @IsSVH, @IsSVH1, @IsSVH2, @IsTMOP, @IsUSCG, @IsFHCC, @DrugItemSubItemId, 0,'C',
		@ModificationStatusId,
		@loginName,
		getdate(),
		@loginName, 
		getdate()
	)

	select @error = @@error, @DrugItemPriceId = @@identity
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting drug item price for contract ' + @ContractNumber
		GOTO ERROREXIT
	END

	Exec @retVal = dbo.AdjustPriceSequence @DrugItemPriceId,@ModificationStatusId,@loginName, @ContractNumber, 'G'

	SELECT @error = @@ERROR
	IF @retVal = -1 OR @error > 0
	BEGIN
		select @errorMsg = 'Error returned when Adjusting price sqeuence for contract ' + @ContractNumber
  		GOTO ERROREXIT
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
