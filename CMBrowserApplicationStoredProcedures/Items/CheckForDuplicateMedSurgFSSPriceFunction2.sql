IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CheckForDuplicateMedSurgFSSPriceFunction2')
	BEGIN
		DROP  Function  CheckForDuplicateMedSurgFSSPriceFunction2
	END

GO

CREATE FUNCTION CheckForDuplicateMedSurgFSSPriceFunction2
(
@ContractNumber nvarchar(20),
@ContractId int,
@PriceStartDate datetime,
@PriceStopDate datetime,
@Price decimal(18,2),
@IsTemporary bit,
@ItemId int, 
@ItemPriceIdBeingUpdated int -- this is populated during an update or -1 if insert
)

RETURNS bit



AS
BEGIN

	/* this version for new split item/price schema */

	DECLARE @PriceExists bit
	
	select @PriceExists = 0

	/* insert */
	if @ItemPriceIdBeingUpdated = -1
	BEGIN
	
		if exists ( select ItemPriceId from CM_ItemPrice p join CM_Items i on p.ItemId = i.ItemId
					where i.ItemId = @ItemId
					and i.ContractId = @ContractId
					and p.IsTemporary = @IsTemporary

	                and((
						datediff(dd,p.PriceStartDate,@PriceStartDate)>=0  and 
	                    datediff(dd,@PriceStartDate, p.PriceStopDate)>=0
                        )
                        or
                        (
                        datediff(dd,p.PriceStartDate,@PriceStopDate)>=0  and 
                        datediff(dd,@PriceStopDate, p.PriceStopDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@PriceStartDate,p.PriceStartDate)>=0  and 
                        datediff(dd,p.PriceStartDate,@PriceStopDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@PriceStartDate,p.PriceStopDate )>=0  and 
                        datediff(dd,p.PriceStopDate,@PriceStopDate)>=0                                        
						))					
					)
					BEGIN
						select @PriceExists = 1
					END
	END
	else /* update - must exclude the row being updated when checking for duplicates */
	BEGIN
		if exists ( select ItemPriceId from CM_ItemPrice p join CM_Items i on p.ItemId = i.ItemId
					where i.ItemId = @ItemId
					and i.ContractId = @ContractId
					and p.ItemPriceId <> @ItemPriceIdBeingUpdated
					and p.IsTemporary = @IsTemporary

	                and((
						datediff(dd,p.PriceStartDate,@PriceStartDate)>=0  and 
	                    datediff(dd,@PriceStartDate, p.PriceStopDate)>=0
                        )
                        or
                        (
                        datediff(dd,p.PriceStartDate,@PriceStopDate)>=0  and 
                        datediff(dd,@PriceStopDate, p.PriceStopDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@PriceStartDate,p.PriceStartDate)>=0  and 
                        datediff(dd,p.PriceStartDate,@PriceStopDate)>=0                                        
						)
						or
                        (
                        datediff(dd,@PriceStartDate,p.PriceStopDate )>=0  and 
                        datediff(dd,p.PriceStopDate,@PriceStopDate)>=0                                        
						))					
					)
					BEGIN
						select @PriceExists = 1
					END
	END
		

	RETURN @PriceExists

END