IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[NCFFSSContractsValidation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [NCFFSSContractsValidation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[NCFFSSContractsValidation]
As 

Declare @contractnumber nvarchar(20),
		@contractnumber1 nvarchar(20)

	Truncate table NCFFSSContractsLookUp

	Insert into NCFFSSContractsLookUp
	(ContractNumber,Found)
	Select distinct CNT_NO,0 from ncfprice

	Declare Contracts_Cursor Cursor for
		Select ContractNumber from NCFFSSContractsLookUp

	Open Contracts_Cursor
	FETCH NEXT FROM Contracts_Cursor
	INTO @contractnumber

	WHILE @@FETCH_STATUS = 0
	BEGIN
		If left(@contractnumber,5) = 'V797P'
		Begin
			Set @contractnumber1 = ltrim(rtrim(substring(@contractnumber,charindex('-',@contractnumber)+1,len(@contractnumber))))
		End
		
		If exists (Select top 1 1 
					From ammhinsql1.nac_Cm.dbo.tbl_cntrcts
					Where CntrctNum = @contractnumber 
				   )
		Begin
			Update NCFFSSContractsLookUp
			Set Found = 1, NACCMContractNumber = @contractnumber
			Where ContractNumber =  @contractnumber
		End
		Else If exists (Select top 1 1 
					From ammhinsql1.nac_Cm.dbo.tbl_cntrcts
					Where CntrctNum = @contractnumber1 
				   )
		Begin
			Update NCFFSSContractsLookUp
			Set Found = 1, NACCMContractNumber = @contractnumber1
			Where ContractNumber =  @contractnumber
		End

		Set @contractnumber1 = null

		FETCH NEXT FROM Contracts_Cursor
		INTO @contractnumber
	End	
	Close Contracts_Cursor		
	DeAllocate Contracts_Cursor

	Update NCFFSSContractsLookUp
	Set Found = 1, NACCMContractNumber = '5175X'
	Where ContractNumber = 'V797P-5175N'

	Update NCFFSSContractsLookUp
	Set Found = 1, NACCMContractNumber = '5612X'
	Where ContractNumber = 'V797P-5612M'

	Update NCFFSSContractsLookUp
	Set Found = 1, NACCMContractNumber = '5576X'
	Where ContractNumber = 'V797P-5576M'

	Update NCFFSSContractsLookUp
	Set Found = 1, NACCMContractNumber = '5769X'
	Where ContractNumber = 'V797P-5769N'

	Update NCFFSSContractsLookUp
	Set Found = 1, NACCMContractNumber = '5796X'
	Where ContractNumber = 'V797P-5796M'



