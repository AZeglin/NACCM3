IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[NCFContractsValidation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [NCFContractsValidation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[NCFContractsValidation]
As 

Declare @contractnumber nvarchar(20),
		@contractnumber1 nvarchar(20)


	Truncate table NCFContractsLookUp

	Insert into NCFContractsLookUp
	(ContractNumber,Found)
	Select distinct NCF_CNT,0 from ncfprice

	Declare Contracts_Cursor Cursor for
		Select ContractNumber from NCFContractsLookUp

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
			Update NCFContractsLookUp
			Set Found = 1, NACCMContractNumber = @contractnumber
			Where ContractNumber =  @contractnumber
		End
		Else If exists (Select top 1 1 
					From ammhinsql1.nac_Cm.dbo.tbl_cntrcts
					Where CntrctNum = @contractnumber1 
				   )
		Begin
			Update NCFContractsLookUp
			Set Found = 1, NACCMContractNumber = @contractnumber1
			Where ContractNumber =  @contractnumber
		End

		Set @contractnumber1 = null

		FETCH NEXT FROM Contracts_Cursor
		INTO @contractnumber
	End	
	Close Contracts_Cursor		
	DeAllocate Contracts_Cursor

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5979X'
	Where ContractNumber = 'V797P-5979N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '797P-99-NC-0001'
	Where ContractNumber = 'V797P-99-NC-0001'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5011B'
	Where ContractNumber = 'V797P-5011X'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5078b'
	Where ContractNumber = 'V797P-5078X'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5995X'
	Where ContractNumber = 'V797P-5995N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5615X'
	Where ContractNumber = 'V797P-5615N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5733X'
	Where ContractNumber = 'V797P-5733N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5526X'
	Where ContractNumber = 'V797P-5526N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5939X'
	Where ContractNumber = 'V797P-5939N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5990X'
	Where ContractNumber = 'V797P-5990N'

	Update NCFContractsLookUp
	Set Found = 1, NACCMContractNumber = '5074B'
	Where ContractNumber = 'V797P-5074X'


