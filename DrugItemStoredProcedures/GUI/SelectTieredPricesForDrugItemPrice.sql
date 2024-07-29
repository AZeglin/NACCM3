IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SelectTieredPricesForDrugItemPrice]') AND type in (N'P', N'PC'))
DROP PROCEDURE [SelectTieredPricesForDrugItemPrice]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure SelectTieredPricesForDrugItemPrice
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20),
@DrugItemPriceId int,
@DrugItemPriceHistoryId int,
@WithAdd bit = 0,
@IsFromHistory bit = 0
)

AS

DECLARE @ContractId int,
	@error int,
	@rowcount int,
	@errorMsg nvarchar(250),
	
	@DrugItemTieredPriceId int,
	@TieredPriceStartDate datetime,
	@TieredPriceStopDate datetime,
	@Price decimal(9,2),
	@Minimum nvarchar(200),
	@MinimumValue int,
	
	@LastModificationType nchar(1),
	@ModificationStatusId int,
	
	@CreatedBy nvarchar(120),
	@CreationDate datetime,
	@LastModifiedBy nvarchar(120),
	@LastModificationDate datetime,
	@IsNewBlankRow bit
	
	
BEGIN

	select @ContractId = ContractId
	from DI_Contracts
	where NACCMContractNumber = @ContractNumber
	
	select @error = @@error, @rowcount = @@rowcount
	
	if @error <> 0 or @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error getting contractId from fss contract ' + @ContractNumber
		raiserror( @errorMsg, 16, 1 )
	END
	
	if @WithAdd = 0
	BEGIN
		if @IsFromHistory = 0
		BEGIN
			select
				t.DrugItemTieredPriceId,
				t.DrugItemPriceId,
				t.TieredPriceStartDate,
				t.TieredPriceStopDate,
				t.Price,
				t.Minimum,
				t.MinimumValue,
				t.CreatedBy ,     
				t.CreationDate ,         
				t.LastModifiedBy ,    
				t.LastModificationDate,
				0 as IsNewBlankRow
			from DI_DrugItemTieredPrice t
			where DrugItemPriceId = @DrugItemPriceId

			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving tiered prices for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
		else
		BEGIN
			/* note: only going to show active tiered pricing, never historical */
			/* since it is not currently possible to map an historical tiered price to a */
			/* particular DrugItemPriceHistoryId; and will only show it if the selected historical price has */
			/* a corresponding active price */
			select
				t.DrugItemTieredPriceId,
				t.DrugItemPriceId,
				t.TieredPriceStartDate,
				t.TieredPriceStopDate,
				t.Price,
				t.Minimum,
				t.MinimumValue,
				t.CreatedBy ,     
				t.CreationDate ,         
				t.LastModifiedBy ,    
				t.LastModificationDate,
				0 as IsNewBlankRow
			from DI_DrugItemTieredPrice t join DI_DrugItemPrice a on t.DrugItemPriceId = a.DrugItemPriceId
			where a.DrugItemPriceId = @DrugItemPriceId

			select @error = @@error
			
			if @error <> 0
			BEGIN
				select @errorMsg = 'Error retrieving historical tiered prices for fss contract ' + @ContractNumber
				raiserror( @errorMsg, 16, 1 )
			END
		END
	END
	else
	BEGIN
			/* blank row definition */
		select @DrugItemTieredPriceId = 0,
			@TieredPriceStartDate = ( select PriceStartDate from DI_DrugItemPrice where DrugItemPriceId = @DrugItemPriceId ),
			@TieredPriceStopDate = ( select PriceStopDate from DI_DrugItemPrice where DrugItemPriceId = @DrugItemPriceId ),
			@Price = 0,
			@Minimum = '',
			@MinimumValue = 0,
			@CreatedBy = '',
			@CreationDate = getdate(),
			@LastModifiedBy = '',
			@LastModificationDate = getdate(),
			@IsNewBlankRow = 1

		select
			t.DrugItemTieredPriceId,
			t.DrugItemPriceId,
			t.TieredPriceStartDate,
			t.TieredPriceStopDate,
			t.Price,
			t.Minimum,
			t.MinimumValue,
			t.CreatedBy ,     
			t.CreationDate ,         
			t.LastModifiedBy ,    
			t.LastModificationDate,
			0 as IsNewBlankRow
		from DI_DrugItemTieredPrice t
		where DrugItemPriceId = @DrugItemPriceId
		
		/* return a new blank row at the applications request */
		union
		
		select @DrugItemTieredPriceId as DrugItemTieredPriceId,  
			@DrugItemPriceId as DrugItemPriceId,
			@TieredPriceStartDate as TieredPriceStartDate,
			@TieredPriceStopDate as TieredPriceStopDate,
			@Price as Price,
			@Minimum as Minimum,
			@MinimumValue as MinimumValue,
			@CreatedBy as CreatedBy,
			@CreationDate as CreationDate,
			@LastModifiedBy as LastModifiedBy,
			@LastModificationDate as LastModificationDate,
			@IsNewBlankRow as IsNewBlankRow
		
		select @error = @@error
		
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error retrieving tiered prices for fss contract ' + @ContractNumber
			raiserror( @errorMsg, 16, 1 )
		END
	END
END
