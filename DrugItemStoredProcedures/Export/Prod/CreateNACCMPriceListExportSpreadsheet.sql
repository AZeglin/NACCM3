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
	@error int,
	@ssisstr varchar(8000), 
	@packagename varchar(200),
	@servername varchar(100),
	@params varchar(8000),
	@filename varchar(255),
	@copyFile varchar(255)



Begin Transaction



	EXEC dbo.GetLoginNameFromUserIdLocalProc @CurrentUser, @loginName OUTPUT 
	SELECT @error = @@error		
	IF @error <> 0 or @loginName is null
	BEGIN
		SELECT @errorMsg = 'Error getting login name for UserId ' + convert(nvarchar(120), @CurrentUser )
		GOTO ERROREXIT
	END	


	IF (Len(@StartDate) > 0 and @startdate is not null and isdate(@StartDate) = 0) OR
	   (LEN(@EndDate) > 0 and @EndDate is not null and isdate(@EndDate) = 0)
	BEGIN
		SELECT @errorMsg = 'Start Date or End Date is not a valid date' 
		SET @filepath = Null
		GOTO ERROREXIT	
	END

	If (Len(@StartDate) = 0 or @startdate is  null) 
	Begin
		Set @StartDate = '01/01/1900'
	End
	
	If (Len(@EndDate) = 0 or @EndDate is  null) 
	Begin
		Set @EndDate = '01/01/1900'
	End	
	

	SET @currentdate = 	
				substring(Convert(Varchar(10), getdate() ,20),6,2) +
				substring(Convert(Varchar(10), getdate() ,20),9,2) +
				substring(Convert(Varchar(10), getdate() ,20),1,4) + 
											'_' +
				substring(Convert(Varchar(10), getdate() ,108),1,2)+
				substring(Convert(Varchar(10), getdate() ,108),4,2)+
				substring(Convert(Varchar(10), getdate() ,108),7,2)
				
	SET @servername = @DrugItemServerName				

	IF @ExportType = 'M'
	BEGIN
		SET @filename =  @ContractNumber + '_PriceList_' + @currentdate + '.xlsx'
		SET @filepath = @DestinationPath + @ContractNumber + '_PriceList_' + @currentdate +'.xlsx'
		SET @ssisstr = 'copy \\ammhinmul2\shared\NACCMDataFeed\MedSurgPriceListExport\MedSurgPriceListExportTemplate.xlsx ' + @filepath		

		EXEC @result =  master..xp_cmdshell @ssisstr --, no_output

		IF (@result <> 0)
		BEGIN
			SELECT @errorMsg = 'Error copying MedSurg spreadsheet for Contract Number:  ' + @ContractNumber
			SET @filepath = Null
			GOTO ERROREXIT
		END	
			
		SET @packagename = 'MedSurgPriceListExport'
		SET @ssisstr = 'dtexec /sql ' + @packagename + ' /server ' + @servername + ' '
	
		SELECT  @params = 
				'/set \package.variables[glvFileName].Value;"\"' + @filename + '\""' + 
				'/set \package.variables[glvContractNumber].Value;"\"' + @ContractNumber + '\""' + 				
				'/set \package.variables[glvDestinationPath].Value;"\"' + @DestinationPath + '\""'  
		
		SET @ssisstr = @ssisstr + @params				
				
		EXEC @result =  master..xp_cmdshell @ssisstr --, no_output

		IF (@result <> 0)
		BEGIN
			SELECT @errorMsg = 'Error in Creating MedSurg spreadsheet for Contract Number:  ' + @ContractNumber
			SET @filepath = Null
			GOTO ERROREXIT
		END
		ELSE
		BEGIN
			INSERT INTO DI_PriceListExportLog
			(ContractNumber,ExportType,Status,CreatedBy,CreationDate)
			SELECT @contractNumber,	'MedSurg',	'Success',@loginName,GETDATE()
		END		
	END
	ELSE
	BEGIN
		SET @filename =  @ContractNumber + '_' + @currentdate + '.xlsx'	
		SET @filepath =  @DestinationPath + @ContractNumber + '_' + @currentdate + '.xlsx'
		SET @ssisstr = 'copy \\ammhinmul2\shared\NACCMDataFeed\DrugItemPriceListExport\DrugItemPriceListExportTemplate.xlsx ' + @filepath		

		SELECT @covered =
			CASE
				WHEN @ExportType = 'C' then 'T'
				ELSE 'B'
			END


		EXEC @result =  master..xp_cmdshell @ssisstr , no_output

		IF (@result <> 0)
		BEGIN
			SELECT @errorMsg = 'Error copying DrugItem spreadsheet for Contract Number:  ' + @ContractNumber
			SET @filepath = Null
			GOTO ERROREXIT
		END	
			
		SET @packagename = 'DrugItemPriceListExport'
		SET @ssisstr = 'dtexec /sql ' + @packagename + ' /server ' + @servername + ' '
	
		SELECT  @params = 
				'/set \package.variables[glvFileName].Value;"\"' + @filename + '\""' + 
				'/set \package.variables[glvContractNumber].Value;"\"' + @ContractNumber + '\""' + 				
				' /set \package.variables[glvDestinationPath].Value;"\"' + @DestinationPath + '\""' + 
				' /set \package.variables[glvCovered].Value;"\"' + @covered + '\""' + 
				' /set \package.variables[glvstartdate].Value;"\"' + @StartDate + '\""' + 
				' /set \package.variables[glvstopdate].Value;"\"' + @EndDate + '\""'
		
		SET @ssisstr = @ssisstr + @params				
				
		EXEC @result =  master..xp_cmdshell @ssisstr --, no_output

		IF (@result <> 0)
		BEGIN
			SELECT @errorMsg = 'Error in Creating DrugItem spreadsheet for Contract Number:  ' + @ContractNumber
			SET @filepath = Null
			GOTO ERROREXIT
		END
		ELSE
		BEGIN
			INSERT INTO DI_PriceListExportLog
			(ContractNumber,ExportType,Status,CreatedBy,CreationDate)
			SELECT @contractNumber,	
					CASE
						WHEN @ExportType = 'B' THEN 'Covered And NonCovered'
						WHEN @ExportType = 'C' THEN 'Covered Only'
					END as ExportType,
					'Success',@loginName,GETDATE()
		END				
		
	END




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


