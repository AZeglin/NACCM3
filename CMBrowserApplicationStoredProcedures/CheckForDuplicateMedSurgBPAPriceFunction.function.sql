IF EXISTS (SELECT * FROM sysobjects WHERE type = 'FN' AND name = 'CheckForDuplicateMedSurgBPAPriceFunction')
	BEGIN
		DROP  Function  CheckForDuplicateMedSurgBPAPriceFunction
	END

GO

CREATE FUNCTION [dbo].[CheckForDuplicateMedSurgBPAPriceFunction]
(
@ContractNumber nvarchar(50),
@FSSLogNumber int = null,
@BPADescription nvarchar(255), 
@BPAPrice money, 
@DateEffective datetime, 
@ExpirationDate datetime, 
@BPALogNumberBeingUpdated int = null, -- this is populated during an update
@ComparePrice bit = 0
)

RETURNS bit

AS
BEGIN

	DECLARE @PriceExists int
	
	select @PriceExists = 0


		if @ComparePrice = 1
		BEGIN
			if @BPALogNumberBeingUpdated is null
			BEGIN
				if @FSSLogNumber is null
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [BPA/BOA Price] = @BPAPrice
						and Removed = 0
                        and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						)
							-- for my reference
							-- and ( @DateEffective between [DateEffective] and  [ExpirationDate] 
							-- or  @ExpirationDate between [DateEffective] and  [ExpirationDate] 
							-- or [DateEffective] between @DateEffective and @ExpirationDate
							-- or [ExpirationDate] between @DateEffective and @ExpirationDate )
						
						BEGIN
							select @PriceExists = 1
						END
				END
				else
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [FSSLogNumber] = @FSSLogNumber
						and [BPA/BOA Price] = @BPAPrice
						and Removed = 0
		                and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						)
						BEGIN
							select @PriceExists = 1
						END
				END
			END
			else
			BEGIN
				if @FSSLogNumber is null
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [BPA/BOA Price] = @BPAPrice
						and Removed = 0
	                    and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						and BPALogNumber <> @BPALogNumberBeingUpdated
						)
						BEGIN
							select @PriceExists = 1
						END
				END
				else
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [FSSLogNumber] = @FSSLogNumber
						and [BPA/BOA Price] = @BPAPrice
						and Removed = 0
		                and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
						))
						and BPALogNumber <> @BPALogNumberBeingUpdated
						)
						BEGIN
							select @PriceExists = 1
						END
				END
			END
		END
		else -- not comparing price
		BEGIN
			if @BPALogNumberBeingUpdated is null
			BEGIN
				if @FSSLogNumber is null
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and Removed = 0
		                and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						)
						BEGIN
							select @PriceExists = 1
						END
				END
				else
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [FSSLogNumber] = @FSSLogNumber
						and Removed = 0
	                    and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						)
						BEGIN
							select @PriceExists = 1
						END
				END
			END
			else
			BEGIN
				if @FSSLogNumber is null
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and Removed = 0
	                    and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						and BPALogNumber <> @BPALogNumberBeingUpdated
						)
						BEGIN
							select @PriceExists = 1
						END
				END
				else
				BEGIN
					if exists (
					select BPALogNumber from tbl_BPA_Pricelist
						where CntrctNum = @ContractNumber
						and LTRIM( RTRIM( [Description] )) = LTRIM( RTRIM( @BPADescription ))
						and [FSSLogNumber] = @FSSLogNumber
						and Removed = 0
                        and((
							datediff(dd,[DateEffective],@DateEffective)>=0  and 
	                        datediff(dd,@DateEffective, [ExpirationDate])>=0
                            )
                            or
                            (
                            datediff(dd,[DateEffective],@ExpirationDate)>=0  and 
                            datediff(dd,@ExpirationDate, [ExpirationDate])>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[DateEffective])>=0  and 
                            datediff(dd,[DateEffective],@ExpirationDate)>=0                                        
							)
							or
                            (
                            datediff(dd,@DateEffective,[ExpirationDate] )>=0  and 
                            datediff(dd,[ExpirationDate],@ExpirationDate)>=0                                        
							))
						and BPALogNumber <> @BPALogNumberBeingUpdated
						)
						BEGIN
							select @PriceExists = 1
						END
				END
			END

		END


	RETURN @PriceExists

END