IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'CreateNACCMPriceListExportSpreadsheet')
	BEGIN
		DROP  Procedure CreateNACCMPriceListExportSpreadsheet
	END

GO
/* saved new version on 6/29/2009 */
CREATE Procedure CreateNACCMPriceListExportSpreadsheetXXX do not save this unless guid is updated
(
@currentUser uniqueidentifier ,
@ContractNumber nvarchar(20)  ,
@DestinationPath nvarchar(500),
@ExportType nchar(1), 
@StartDate nvarchar(10),
@EndDate nvarchar(10),
@filepath nvarchar(1000) output
)
As  
   
   
DECLARE 
	@dtscommand varchar(1000),
	@errorMsg nvarchar(128),
	@result int,
	@currentdate varchar(20),
	@loginName nvarchar(120),
	@error int

Begin Transaction

	EXEC dbo.GetLoginNameFromUserIdLocalProc @CurrentUser, @loginName OUTPUT 
	Select @error = @@error		
	if @error <> 0 or @loginName is null
	BEGIN
		select @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	

	IF @ExportType = 'M'
	Begin
		SET @currentdate = 	substring(Convert(Varchar(10), getdate() ,20),6,2) +
					substring(Convert(Varchar(10), getdate() ,20),9,2) +
					substring(Convert(Varchar(10), getdate() ,20),1,4) + 
												'_' +
					substring(Convert(Varchar(10), getdate() ,108),1,2)+
					substring(Convert(Varchar(10), getdate() ,108),4,2)+
					substring(Convert(Varchar(10), getdate() ,108),7,2)
	  
		SET @dtsCommand = 'DTSRun /S "ammhindevdb"  /N "NACCMPriceListExport" /V "{FC0A21B1-BC52-423E-9205-634B1D985CAF}" /A "glvdatevalue":"8"="' + @currentdate + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvdestinationpath":"8"="' + @DestinationPath + '" /W "0" /E'  
		Set @filepath = @DestinationPath + @ContractNumber + '_PriceList_' + @currentdate +'.xls'
	End
	Else If @ExportType = 'C'
	Begin
		Select @contractNumber = ContractNumber 
		From DI_contracts 
		Where NACCMContractNumber = @contractNumber

		SET @currentdate = 	substring(Convert(Varchar(10), getdate() ,20),6,2) +
					substring(Convert(Varchar(10), getdate() ,20),9,2) +
					substring(Convert(Varchar(10), getdate() ,20),1,4) + 
												'_' +
					substring(Convert(Varchar(10), getdate() ,108),1,2)+
					substring(Convert(Varchar(10), getdate() ,108),4,2)+
					substring(Convert(Varchar(10), getdate() ,108),7,2)
												
		SET @filepath = @DestinationPath +  @contractNumber + '_' + @currentdate + '.xls'   
		SET @dtsCommand = 'DTSRun /S "ammhindevdb" /N "CreateSpreadsheetForCoveredDrugItems" /V "{9D1DA87E-9E7E-4008-8FC9-4B2828F244DE}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /W "0" /E'  
	End
	Else If @ExportType = 'B'
	Begin
		Select @contractNumber = ContractNumber 
		From DI_contracts 
		Where NACCMContractNumber = @contractNumber

		SET @currentdate = 	substring(Convert(Varchar(10), getdate() ,20),6,2) +
					substring(Convert(Varchar(10), getdate() ,20),9,2) +
					substring(Convert(Varchar(10), getdate() ,20),1,4) + 
												'_' +
					substring(Convert(Varchar(10), getdate() ,108),1,2)+
					substring(Convert(Varchar(10), getdate() ,108),4,2)+
					substring(Convert(Varchar(10), getdate() ,108),7,2) 

		SET @filepath = @DestinationPath +  @contractNumber + '_' + @currentdate + '.xls'   
		SET @dtsCommand = 'DTSRun /S "ammhindevdb" /N "CreateSpreadsheetForCoveredAndNonCoveredDrugItems" /V "{FBF7992D-1A07-43B1-B9B7-1F272C7B9132}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /W "0" /E'  
	End
	Else
	Begin
		select @errorMsg = 'Export Type Parameter: ' + @ExportType + '  is invalid' 
		Set @filepath = Null
		goto ERROREXIT		
	End


	EXEC @result =  master..xp_cmdshell @dtsCommand , no_output
	
	
	If (@result <> 0)
	Begin
		select @errorMsg = 'Error in Creating Excel for Contract Number:  ' + @ContractNumber
		Set @filepath = Null
		goto ERROREXIT
	End


	
GOTO OKEXIT

ERROREXIT:

	raiserror( @errorMsg, 16 , 1 )
	if @@TRANCOUNT > 1
	BEGIN
		COMMIT TRANSACTION
	END
	Else if @@TRANCOUNT = 1
	BEGIN
		/* only rollback iff this the highest level */
		ROLLBACK TRANSACTION
	END
	
	RETURN ( -1 )

OKEXIT:

	If @@TRANCOUNT > 0
	BEGIN
		COMMIT TRANSACTION
	END
	RETURN ( 0 )
