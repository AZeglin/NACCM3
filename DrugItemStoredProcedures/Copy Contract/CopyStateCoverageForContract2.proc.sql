IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CopyStateCoverageForContract2' )
BEGIN
	DROP PROCEDURE CopyStateCoverageForContract2
END
GO

CREATE PROCEDURE CopyStateCoverageForContract2
(
	@CopyContractLogId int,
	@OldContractNumber nvarchar(50),
	@NewContractNumber nvarchar(50),
	@UserLogin nvarchar(120)
)
As

	Declare @count int,
			@error int,
			@rowcount int,
			@errorMsg nvarchar(250),
			@retVal int	

	BEGIN TRANSACTION
		/* this version is for release 2 which uses the new table */

		Select @count = Count(*) 
		From CM_GeographicCoverage
		Where ContractNumber = @OldContractNumber
		
		IF @count = 0
		BEGIN
			Update tbl_CopyContractsLog
				Set TotalStateCoveragesCopied = 0
			Where CopyContractLogId = @CopyContractLogId
			
			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END			
		END
		ELSE
		BEGIN
			Insert Into CM_GeographicCoverage
			( ContractNumber,
				[Group52],
				[Group51],
				[Group50],
				[Group49],
				[AL]     ,
				[AK]     ,
				[AZ]     ,
				[AR]     ,
				[CA]     ,
				[CO]     ,
				[CT]     ,
				[DE]     ,
				[DC]     ,
				[FL]     ,
				[GA]     ,
				[HI]     ,
				[ID]     ,
				[IL]     ,
				[IN]     ,
				[IA]     ,
				[KS]     ,
				[KY]     ,
				[LA]     ,
				[ME]     ,
				[MD]     ,
				[MA]     ,
				[MI]     ,
				[MN]     ,
				[MS]     ,
				[MO]     ,
				[MT]     ,
				[NE]     ,
				[NV]     ,
				[NH]     ,
				[NJ]     ,
				[NM]     ,
				[NY]     ,
				[NC]     ,
				[ND]     ,
				[OH]     ,
				[OK]     ,
				[OR]     ,
				[PA]     ,
				[RI]     ,
				[SC]     ,
				[SD]     ,
				[TN]     ,
				[TX]     ,
				[UT]     ,
				[VT]     ,
				[VA]     ,
				[WA]     ,
				[WV]     ,
				[WI]     ,
				[WY]     ,
				[PR]     ,
				[AB]     ,
				[BC]     ,
				[MB]     ,
				[NB]     ,
				[NF]     ,
				[NT]     ,
				[NS]     ,
				[ON]     ,
				[PE]     ,
				[QC]     ,
				[SK]     ,
				[YT]     ,
				[CreatedBy] ,
				[CreationDate],
				[LastModifiedBy] ,
				[LastModificationDate]  
			)
			Select @NewContractNumber, 
				[Group52],
				[Group51],
				[Group50],
				[Group49],
				[AL]     ,
				[AK]     ,
				[AZ]     ,
				[AR]     ,
				[CA]     ,
				[CO]     ,
				[CT]     ,
				[DE]     ,
				[DC]     ,
				[FL]     ,
				[GA]     ,
				[HI]     ,
				[ID]     ,
				[IL]     ,
				[IN]     ,
				[IA]     ,
				[KS]     ,
				[KY]     ,
				[LA]     ,
				[ME]     ,
				[MD]     ,
				[MA]     ,
				[MI]     ,
				[MN]     ,
				[MS]     ,
				[MO]     ,
				[MT]     ,
				[NE]     ,
				[NV]     ,
				[NH]     ,
				[NJ]     ,
				[NM]     ,
				[NY]     ,
				[NC]     ,
				[ND]     ,
				[OH]     ,
				[OK]     ,
				[OR]     ,
				[PA]     ,
				[RI]     ,
				[SC]     ,
				[SD]     ,
				[TN]     ,
				[TX]     ,
				[UT]     ,
				[VT]     ,
				[VA]     ,
				[WA]     ,
				[WV]     ,
				[WI]     ,
				[WY]     ,
				[PR]     ,
				[AB]     ,
				[BC]     ,
				[MB]     ,
				[NB]     ,
				[NF]     ,
				[NT]     ,
				[NS]     ,
				[ON]     ,
				[PE]     ,
				[QC]     ,
				[SK]     ,
				[YT]     ,
				@UserLogin ,
				GETDATE(),
				@UserLogin ,
				GETDATE()
			From CM_GeographicCoverage
			Where ContractNumber = @OldContractNumber

			Select @error = @@ERROR
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error inserting CM_GeographicCoverage for contract: ' + @NewContractNumber
				GOTO ERROREXIT
			END	

			Update tbl_CopyContractsLog
				Set TotalStateCoveragesCopied = @count
			Where CopyContractLogId = @CopyContractLogId

			Select @error = @@ERROR			
			IF @error <> 0
			BEGIN
				select @errorMsg = 'Error updating tbl_CopyContractsLog for contract: ' + @NewContractNumber
				GOTO ERROREXIT
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


	