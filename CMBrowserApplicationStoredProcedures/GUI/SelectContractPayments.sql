IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractPayments' )
BEGIN
	DROP PROCEDURE SelectContractPayments
END
GO

CREATE PROCEDURE SelectContractPayments
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)
		

BEGIN TRANSACTION

--CREATE TABLE CM_PaymentsReceived
--(
--	[SRPActivityId]                                   	int              NOT NULL IDENTITY (1, 1),
--	[ContractId]                                      	int              NOT NULL,
--	[ContractNumber]                                  	nvarchar(50)     NOT NULL,
--	[QuarterId]                                       	int              NOT NULL,
--	[SubmissionDate]                                  	datetime         NOT NULL,
--	[SubmittedByUserName]                             	nvarchar(100)    NOT NULL,
--	[PaymentAmount]                                   	decimal(18,2)    NOT NULL,
--	[PaymentMethod]                                   	nvarchar(50)     NOT NULL,
--	[PaymentSource]                                   	nvarchar(20)     NOT NULL,
--	[TransactionId]                                   	nvarchar(20)         NULL,
--	[PayGovTrackingId]                                	nvarchar(50)         NULL,
--	[DepositTicketNumber]                             	nvarchar(20)         NULL,
--	[DebitVoucherNumber]                              	nvarchar(20)         NULL,
--	[CheckNumber]                                     	nvarchar(50)         NULL,
--	[SettlementDate]                                  	datetime             NULL,
--	[Comments]                                        	nvarchar(500)        NULL,
--	[VendorLastModifiedBy]                            	nvarchar(120)    NOT NULL,
--	[VendorLastModificationDate]                      	datetime         NOT NULL,
--	[CreatedBy]                                       	nvarchar(120)    NOT NULL DEFAULT (user_name()),
--	[CreationDate]                                    	datetime         NOT NULL DEFAULT (getdate()),
--	[LastModifiedBy]                                  	nvarchar(120)    NOT NULL DEFAULT (user_name()),
--	[LastModificationDate]                            	datetime         NOT NULL DEFAULT (getdate()),
--	[InsertedDuringMigration]                         	bit              NOT NULL DEFAULT ((0))
--)
--go


		select 	p.ContractId,
			p.ContractNumber,   
			p.QuarterId,      
			y.Title as YearQuarterDescription,
			p.PaymentAmount,   
			p.SubmissionDate,    
			p.SubmittedByUserName,  			
			p.PaymentMethod,  
			p.PaymentSource,   
			p.TransactionId,   
			p.PayGovTrackingId,   
			p.DepositTicketNumber,   
			p.DebitVoucherNumber,   
			p.CheckNumber,    
			p.SettlementDate,   
			p.Comments,           
			p.VendorLastModifiedBy,     
			p.VendorLastModificationDate,
			p.SRPActivityId
		from CM_PaymentsReceived p join tlkup_year_qtr y ON p.QuarterId = y.Quarter_ID 
		WHERE p.ContractNumber = @ContractNumber 
		ORDER BY p.QuarterId DESC

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting payments for contract ' + @ContractNumber
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


