IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ValidateCheck')
	BEGIN
		DROP  Procedure  ValidateCheck
	END

GO

CREATE Procedure ValidateCheck
(
@UserLogin nvarchar(120),
@ContractNumber nvarchar(50),
@ContractId int,
@QuarterId int,
@CheckNumber nvarchar(50),
@CheckAmount  decimal(18,2),
@IsValidated bit OUTPUT,
@ValidationMessage nvarchar(300) OUTPUT
)

AS 


BEGIN

	if exists( select CheckId from CM_Checks where ContractNumber = @ContractNumber and ContractId = @ContractId and QuarterId = @QuarterId and CheckNumber = @CheckNumber and CheckAmount = @CheckAmount )
	BEGIN
		select @IsValidated = 0
		select @ValidationMessage = 'The current entry matches an existing record. Duplicate check numbers and amounts are not allowed for the same reporting quarter.'
	END
	else
	BEGIN
		select @IsValidated = 1
		select @ValidationMessage = ''
	END

END



