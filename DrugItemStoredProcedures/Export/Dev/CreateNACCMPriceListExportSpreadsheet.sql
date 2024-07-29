IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CreateNACCMPriceListExportSpreadsheet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [CreateNACCMPriceListExportSpreadsheet]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* saved new version on 6/29/2009 */
CREATE Procedure [dbo].[CreateNACCMPriceListExportSpreadsheet] -- XXX do not save this unless guid is updated
(
@currentUser uniqueidentifier ,
@ContractNumber nvarchar(20)  ,
@DestinationPath nvarchar(500),
@ExportType nchar(1), 
@StartDate nvarchar(10),
@EndDate nvarchar(10),
@DrugItemDatabaseName nvarchar(20),
@DrugItemServerName nvarchar(20),
@filepath nvarchar(1000) output
)
As  
   
   
DECLARE 
	@dtscommand varchar(1000),
	@errorMsg nvarchar(128),
	@result int,
	@currentdate varchar(20),
	@loginName nvarchar(120),
	@covered char(1),
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
																					   
		SET @dtsCommand = 'DTSRun /S "AMMHINDEVDB"  /N "NACCMPriceListExport" /V "{BE2B3247-2726-4D86-B440-0AC0DD2D32B0}" /A "glvdatevalue":"8"="' + @currentdate + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvdestinationpath":"8"="' + @DestinationPath + '" /W "0" /E'  
		Set @filepath = @DestinationPath + @ContractNumber + '_PriceList_' + @currentdate +'.xls'
	End
	Else If @ExportType = 'C'
	Begin
		SET @currentdate = 	substring(Convert(Varchar(10), getdate() ,20),6,2) +
					substring(Convert(Varchar(10), getdate() ,20),9,2) +
					substring(Convert(Varchar(10), getdate() ,20),1,4) + 
												'_' +
					substring(Convert(Varchar(10), getdate() ,108),1,2)+
					substring(Convert(Varchar(10), getdate() ,108),4,2)+
					substring(Convert(Varchar(10), getdate() ,108),7,2)
												
		SET @filepath = @DestinationPath +  @contractNumber + '_' + @currentdate + '.xls'		
		
		SET @covered = 'T'

		If (LEN(@StartDate) =0 or @StartDate is null) or (LEN(@EndDate) =0 or @EndDate is null)
		Begin
			SET @dtsCommand = 'DTSRun /S "AMMHINDEVDB" /N "DrugItemPriceListExport" /V "{40F691C5-6A3A-452C-A532-0BD940DB3FE8}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvcovered":"8"="' + @covered +  '" /W "0" /E'  		
		End
		Else If isdate(@StartDate) = 0 or isdate(@EndDate) = 0
		Begin
			select @errorMsg = 'Start Date or End Date is not a valid date' 
			Set @filepath = Null
			goto ERROREXIT	
		End
		Else
		Begin
			SET @dtsCommand = 'DTSRun /S "AMMHINDEVDB" /N "DrugItemPriceListExport" /V "{FADD3C80-3409-4B91-9421-33B0E02B5B77}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvstartdate":"7"="' + @StartDate +  '" /A "glvstopdate":"7"="' + @EndDate +   '" /A "glvcovered":"8"="' + @covered +  '" /W "0" /E'  
		End
	End
	Else If @ExportType = 'B'
	Begin

		SET @currentdate = 	substring(Convert(Varchar(10), getdate() ,20),6,2) +
					substring(Convert(Varchar(10), getdate() ,20),9,2) +
					substring(Convert(Varchar(10), getdate() ,20),1,4) + 
												'_' +
					substring(Convert(Varchar(10), getdate() ,108),1,2)+
					substring(Convert(Varchar(10), getdate() ,108),4,2)+
					substring(Convert(Varchar(10), getdate() ,108),7,2) 

		SET @filepath = @DestinationPath +  @contractNumber + '_' + @currentdate + '.xls' 
		
		SET @covered = 'B'  

		If (LEN(@StartDate) =0 or @StartDate is null or @StartDate = '') or (LEN(@EndDate) =0 or @EndDate is null or @EndDate = '')
		Begin
			SET @dtsCommand = 'DTSRun /S "AMMHINDEVDB" /N "DrugItemPriceListExport" /V "{40F691C5-6A3A-452C-A532-0BD940DB3FE8}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvcovered":"8"="' + @covered +  '" /W "0" /E'  
		End
		Else If isdate(@StartDate) = 0 or isdate(@EndDate) = 0
		Begin
			select @errorMsg = 'Start Date or End Date is not a valid date' 
			Set @filepath = Null
			goto ERROREXIT	
		End
		Else
		Begin
			SET @dtsCommand = 'DTSRun /S "AMMHINDEVDB" /N "DrugItemPriceListExport" /V "{FADD3C80-3409-4B91-9421-33B0E02B5B77}" /A "glvfilename":"8"="' + @filepath + '" /A "glvcontractnumber":"8"="' + @contractNumber +  '" /A "glvstartdate":"7"="' + @StartDate +  '" /A "glvstopdate":"7"="' + @EndDate +   '" /A "glvcovered":"8"="' + @covered +  '" /W "0" /E'  
		End
	End
	Else
	Begin
		select @errorMsg = 'Export Type Parameter: ' + @ExportType + '  is invalid' 
		Set @filepath = Null
		goto ERROREXIT		
	End


	EXEC @result =  master..xp_cmdshell @dtsCommand --, no_output
	

	If (@result <> 0)
	Begin
		select @errorMsg = 'Error in Creating Excel for Contract Number:  ' + @ContractNumber
		Set @filepath = Null
		goto ERROREXIT
	End
	Else
	Begin
		Insert into DI_PriceListExportLog
		(ContractNumber,ExportType,Status,CreatedBy,CreationDate)
		Select @contractNumber,	
				Case
					When @ExportType = 'M' then 'MedSurg'
					When @ExportType = 'B' then 'Covered And NonCovered'
					When @ExportType = 'C' then 'Covered Only'
				End,
				'Success',@loginName,GETDATE()
				
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
