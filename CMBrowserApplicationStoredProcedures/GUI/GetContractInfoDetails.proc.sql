IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetContractInfoDetails')
	BEGIN
		DROP  Procedure  GetContractInfoDetails
	END

GO

CREATE PROCEDURE GetContractInfoDetails
(
@CurrentUser uniqueidentifier,
@UserLogin nvarchar(120),
@SecurityServerName nvarchar(255),
@SecurityDatabaseName nvarchar(255),
@ContractNumber nvarchar(20)
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@query nvarchar(4000),
		@SQLParms nvarchar(400),
		@joinSecurityServerName nvarchar(300)
		
	
BEGIN TRANSACTION

	select @joinSecurityServerName = '[' + @SecurityServerName + '].[' + @SecurityDatabaseName + ']'

	select @query = 'select Contract_Record_ID, 
		v.SAMUEI,
		DUNS,
		TIN,
		PV_Participation,
		Solicitation_Number,
		Primary_Address_1,
		Primary_Address_2,
		Primary_City,
		Primary_State, 
		Primary_Zip, 
		Primary_CountryId,
		x.CountryName,
		POC_Primary_Name,
		POC_Primary_Phone,
		POC_Primary_Ext,
		POC_Primary_Fax,
		POC_Primary_Email,
		POC_VendorWeb,
		POC_Notes,
		POC_Alternate_Name,
		POC_Alternate_Phone, 
		POC_Alternate_Ext,
		POC_Alternate_Fax,
		POC_Alternate_Email, 
		POC_Emergency_Name,
		POC_Emergency_Phone, 
		POC_Emergency_Ext,
		POC_Emergency_Fax,
		POC_Emergency_Email, 
		POC_Tech_Name, 
		POC_Tech_Phone,
		POC_Tech_Ext,
		POC_Tech_Fax, 
		POC_Tech_Email, 
		Socio_VetStatus_ID, 
		Socio_Business_Size_ID, 
		Socio_SDB,
		Socio_8a, 
		Socio_Woman, 
		Socio_HubZone, 
		Discount_Basic,  
		Discount_Credit_Card,
		Discount_Prompt_Pay, 
		Discount_Quantity, 
		Geographic_Coverage_ID, 
		Tracking_Customer,
		Mininum_Order, 
		Delivery_Terms, 
		Expedited_Delivery_Terms, 
		Annual_Rebate, 
		BF_Offer, 
		Credit_Card_Accepted,                         
		Hazard, 
		Warranty_Duration,
		Warranty_Notes, 
		IFF_Type_ID, 
		Ratio, 
		Returned_Goods_Policy_Type, 
		Returned_Goods_Policy_Notes, 
		Incentive_Description, 
		Dist_Manf_ID,       
		Ord_Address_1,
		Ord_Address_2, 
		Ord_City, 
		Ord_CountryId,
		Ord_State, 
		Ord_Zip, 
		Ord_Telephone, 
		Ord_Ext, 
		Ord_Fax,  
		Ord_EMail, 
		Estimated_Contract_Value, 

		Dates_Effective,

		Dates_TotOptYrs,
		Pricelist_Verified, 
		Verification_Date, 
		Verified_By,
		Current_Mod_Number, 
		Pricelist_Notes, 
		SBAPlanID, 
		VA_DOD, 
		Terminated_Convenience, 
		Terminated_Default,  
  
		SBA_Plan_Exempt,  
		Insurance_Policy_Effective_Date, 
		Insurance_Policy_Expiration_Date, 
		Solicitation_ID, 
		Offer_ID,
		65IB_Contract_Type, 
		POC_Sales_Name, 
		POC_Sales_Phone,
		POC_Sales_Ext,
		POC_Sales_Fax, 
		POC_Sales_Email,
		TradeAgreementActCompliance, 
	
		StimulusAct, 
		RebateRequired, 
		Standardized, 
		c.CreatedBy, 
		c.CreationDate, 
		c.LastModifiedBy,
		c.LastModificationDate,
		s.FullName,
		s.User_Phone,
		t.Schedule_Name,
		t.Asst_Director,
		t.Schedule_Manager,
		m.FullName as ScheduleManagerName,  
		a.FullName as AssistantDirectorName
		                   	
	from tbl_Cntrcts c join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] s on c.CO_ID = s.CO_ID
		join [tlkup_Sched/Cat] t on c.Schedule_Number = t.Schedule_Number
		join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] m on t.Schedule_Manager = m.CO_ID
		join ' + @joinSecurityServerName + '.[dbo].[SEC_UserProfile] a on t.Asst_Director = a.CO_ID
		left outer join CM_SAMVendorInfo v on c.Contract_Record_ID = v.ContractId
		left outer join CM_Countries x on c.Primary_CountryId = x.CountryId
	where CntrctNum = @ContractNumber_parm '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string when retrieving contract info details for contract ' + @ContractNumber
		goto ERROREXIT
	END

	select @SQLParms = N'@ContractNumber_parm nvarchar(20)'

	exec SP_EXECUTESQL @query, @SQLParms, @ContractNumber_parm = @ContractNumber

	select @error = @@error, @rowCount = @@rowcount
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error retrieving contract info details for contract ' + @ContractNumber
		goto ERROREXIT
	END

	if @rowcount <> 1
	BEGIN
		select @errorMsg = 'Error retrieving contract info details for contract ' + @ContractNumber + ' : contract not found.'
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


