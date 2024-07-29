IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractChecks2' )
BEGIN
	DROP PROCEDURE SelectContractChecks2
END
GO

CREATE PROCEDURE SelectContractChecks2
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractId int,
@ContractNumber nvarchar(20),
@WithAdd bit = 0
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@CheckId int, 		
		@QuarterId int, 
		@CheckAmount money,
		@CheckNumber nvarchar(50), 
		@DepositTicketNumber nvarchar(20), 
		@DateReceived datetime, 
		@SettlementDate datetime,
		@InsertedDuringMigration bit,
		@CheckComments nvarchar(255),
		@IsNewBlankRow bit,
		@YearQuarterDescription nvarchar(20),
		@SentToGSA bit,
		@SentDate datetime


BEGIN TRANSACTION

--create table CM_Checks
--(
--	CheckId												int              NOT NULL IDENTITY (1, 1),
--	[ContractId]                                        int              NOT NULL,
--	[ContractNumber]                                    nvarchar(50)     NOT NULL,
--	[QuarterId]                                         int              NOT NULL,
--	[DateReceived]										datetime		 NOT NULL,
--	[CheckAmount]                                   	decimal(18,2)    NOT NULL,
--	[CheckNumber]										nvarchar(50)         NULL,
--	[DepositTicketNumber]                             	nvarchar(20)         NULL,
--	[SettlementDate]                                   	datetime             NULL,

--	[Comments]                                   		nvarchar(255)        NULL,

--	[CreatedBy]                                  		nvarchar(120)    NOT NULL DEFAULT (user_name()),
--	[CreationDate]                                    	datetime         NOT NULL DEFAULT (getdate()),
--	[LastModifiedBy]                                  	nvarchar(120)    NOT NULL DEFAULT (user_name()),
--	[LastModificationDate]                            	datetime         NOT NULL DEFAULT (getdate()),
--	InsertedDuringMigration								bit NOT NULL DEFAULT ( 0 ),
--	SentToGSA											bit NOT NULL DEFAULT ( 0 ),
--	SentDate											datetime		NULL
--)
	if @WithAdd = 0
	BEGIN

		SELECT k.CheckId, k.ContractId, k.ContractNumber, k.QuarterId, k.CheckAmount, k.CheckNumber, k.DepositTicketNumber, k.DateReceived, k.SettlementDate, 
				k.Comments as CheckComments, y.Title as YearQuarterDescription, k.InsertedDuringMigration, k.SentToGSA, k.SentDate,
				0 as IsNewBlankRow
		FROM CM_Checks k join tlkup_year_qtr y ON k.QuarterId = y.Quarter_ID 
		WHERE k.ContractNumber = @ContractNumber 
		ORDER BY k.QuarterId DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting checks for contract (WithAdd = 0).'
			goto ERROREXIT
		END
	END
	else
	BEGIN

		/* blank row definition */
		select @CheckId = -1,
			@QuarterId = -1,
			@CheckAmount = 0,
			@CheckNumber = '',
			@DepositTicketNumber = '',
			@DateReceived = GETDATE(),
			@SettlementDate = GETDATE(),
			@CheckComments = '',
			@YearQuarterDescription = '',
			@InsertedDuringMigration = 0,			
			@SentToGSA = 0,
			@SentDate = GETDATE(),
			@IsNewBlankRow = 1

		SELECT @CheckId as CheckId,
			@ContractId as ContractId,
			@ContractNumber as ContractNumber, 
			@QuarterId as QuarterId,
			@CheckAmount as CheckAmount,
			@CheckNumber as CheckNumber,
			@DepositTicketNumber as DepositTicketNumber,
			@DateReceived as DateReceived,
			@SettlementDate as SettlementDate,
			@CheckComments as CheckComments,
			@YearQuarterDescription as YearQuarterDescription,
			@InsertedDuringMigration as InsertedDuringMigration,			
			@SentToGSA as SentToGSA,
			@SentDate as SentDate,
			@IsNewBlankRow as IsNewBlankRow

		UNION

		SELECT k.CheckId, k.ContractId, k.ContractNumber, k.QuarterId, k.CheckAmount, k.CheckNumber, k.DepositTicketNumber, k.DateReceived, k.SettlementDate, 
				k.Comments as CheckComments, y.Title as YearQuarterDescription, k.InsertedDuringMigration, k.SentToGSA, k.SentDate,
				0 as IsNewBlankRow
		FROM CM_Checks k join tlkup_year_qtr y ON k.QuarterId = y.Quarter_ID 
		WHERE k.ContractNumber = @ContractNumber 
		ORDER BY IsNewBlankRow DESC, QuarterId DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting checks for contract (WithAdd = 1).'
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


