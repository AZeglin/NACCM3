IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSINsForContract2' )
BEGIN
	DROP PROCEDURE SelectSINsForContract2
END
GO

CREATE PROCEDURE SelectSINsForContract2
(
@CurrentUser uniqueidentifier,
@ContractNumber nvarchar(50),
@WithAdd bit
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@SINs as nvarchar(10),
		@Recoverable as bit,
		@charIndex int,
		@testChar nchar(1),
		@allChars nvarchar(40),
		@testCharIndex int,
		@lexicalSIN nvarchar(10)

BEGIN TRANSACTION

	if @WithAdd = 0
	BEGIN

		select s.SINs as SIN, s.Recoverable
		from tbl_Cntrcts_SINs s 
		where s.CntrctNum = @ContractNumber
		and s.Inactive = 0
		order by s.LexicalSIN

		select @error = @@ERROR
		if @error <> 0  
		BEGIN
			select @errorMsg = 'Error selecting SINs for contract number: ' + @ContractNumber 
			goto ERROREXIT
		END
	END
	else
	BEGIN
		select @SINs = '',
		@Recoverable = 0,
		@lexicalSIN = ''

		select s.SINs as SIN, s.Recoverable, s.LexicalSIN
		from tbl_Cntrcts_SINs s 
		where s.CntrctNum = @ContractNumber
		and s.Inactive = 0

		union

		select @SINs as SIN, @Recoverable as Recoverable, @lexicalSIN as LexicalSIN

		order by LexicalSIN

		select @error = @@ERROR, @rowCount = @@ROWCOUNT
		if @error <> 0  or @rowCount = 0
		BEGIN
			select @errorMsg = 'Error selecting SINs for contract number: ' + @ContractNumber 
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


