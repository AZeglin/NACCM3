IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ActiveContractsDataDumpReport' )
BEGIN
	DROP PROCEDURE ActiveContractsDataDumpReport
END
GO

CREATE PROCEDURE ActiveContractsDataDumpReport

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	create table #SinListForDumpReport
	(
	ContractNumber  nvarchar(50),
	SinList  nvarchar(500)
	)

	insert into #SinListForDumpReport
	(
	ContractNumber,
	SinList
	)
	select CntrctNum,
	( select dbo.GetSINsForContractFunction( CntrctNum )) as SinList
	from tbl_Cntrcts
	where dbo.IsContractActiveFunction( CntrctNum, GETDATE() ) =  1

	select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting SinLists for report.'
		goto ERROREXIT
	END


	select 
	c.[CntrctNum],	      
	s.[Schedule_Name],
	t.[SinList],
	s.[Division_Description],
	p.[FullName],  	     
	c.[Contractor_Name],   	      
	c.[DUNS],    	
	c.[TIN],     	
	c.[PV_Participation],  	      
	c.[Solicitation_Number],	
	c.[Primary_Address_1],       
	c.[Primary_Address_2],        
	c.[Primary_City],      	
	c.[Primary_State],     	
	c.[Primary_Zip],       	
	c.[POC_Primary_Name],  	
	c.[POC_Primary_Phone], 	
	c.[POC_Primary_Ext],   	
	c.[POC_Primary_Fax],   	
	c.[POC_Primary_Email], 	
	c.[POC_VendorWeb],     	
	c.[POC_Notes],        
	c.[POC_Alternate_Name],	
	c.[POC_Alternate_Phone],	
	c.[POC_Alternate_Ext], 	
	c.[POC_Alternate_Fax], 	
	c.[POC_Alternate_Email],	
	c.[POC_Emergency_Name],	
	c.[POC_Emergency_Phone],	
	c.[POC_Emergency_Ext], 	
	c.[POC_Emergency_Fax], 	
	c.[POC_Emergency_Email],	
	c.[POC_Tech_Name],     	
	c.[POC_Tech_Phone],    	
	c.[POC_Tech_Ext],      	
	c.[POC_Tech_Fax],      	
	c.[POC_Tech_Email],    	


	v.VetStatus_Description,
	b.Business_Size,
	c.[Socio_SDB],	      
	c.[Socio_8a],	      
	c.[Socio_Woman],       	      
	c.[Socio_HubZone],     	      
	c.[Discount_Basic],    	        
	c.[Discount_Credit_Card],        	        
	c.[Discount_Prompt_Pay],	        
	c.[Discount_Quantity], 	        
	dbo.GetStatesForContractFunction( c.CntrctNum ) as Geographic_Description,
	c.[Tracking_Customer], 	        
	c.[Mininum_Order],     	        
	c.[Delivery_Terms],    	        
	c.[Expedited_Delivery_Terms],    	        
	c.[Annual_Rebate],     	        
	c.[BF_Offer],	        
	c.[Credit_Card_Accepted],        	      
	c.[Hazard],  	      
	c.[Warranty_Duration], 	
	c.[Warranty_Notes],       

	i.IFF_Type_Description,
	c.[Ratio],   	        
	
	r.Returned_Goods_Policy_Description,  	        
	c.[Returned_Goods_Policy_Notes],   
	c.[Incentive_Description],       	        
	
	d.Dist_Manf_Description,       
	c.[Ord_Address_1],   
	c.[Ord_Address_2],  
	c.[Ord_City],	
	c.[Ord_State],	
	c.[Ord_Zip], 	
	c.[Ord_Telephone],     	
	c.[Ord_Ext], 	
	c.[Ord_Fax], 	
	c.[Ord_EMail],	
	c.[Estimated_Contract_Value],   
	c.[Dates_CntrctAward], 	
	c.[Dates_Effective],   	
	c.[Dates_CntrctExp],   	
	c.[Dates_Completion],  	   
	c.[Dates_TotOptYrs],   	        
	c.[Pricelist_Verified],	      
	c.[Verification_Date], 	   
	c.[Verified_By],       	
	c.[Current_Mod_Number],	
	c.[Pricelist_Notes],   	     
	   
	n.PlanName as SBAPlanName,
	
	c.[VA_DOD],  	      
	c.[Terminated_Convenience],      	      
	c.[Terminated_Default],	      
	c.[Drug_Covered] as ContractDescription,      	
	c.[BPA_FSS_Counterpart],	
	--c.[VA_IFF],  	        
	--c.[OGA_IFF], 	        
	--c.[Cost_Avoidance],    	        
	--c.[ICD_Exempt],        	      
	c.[On_GSA_Advantage],  	      
	c.[SBA_Plan_Exempt],   	      
	c.[Insurance_Policy_Effective_Date],       	   
	c.[Insurance_Policy_Expiration_Date],      	   
	--c.[Solicitation_ID],   	        
    --c.[Offer_ID],	    
	    
	--c.[65IB_Contract_Type],	
	c.[POC_Sales_Name],    	
	c.[POC_Sales_Phone],   	
	c.[POC_Sales_Ext],     	
	c.[POC_Sales_Fax],     	
	c.[POC_Sales_Email],   	
	c.[TradeAgreementActCompliance], 	 
	--c.[VietnamVetOwned],   	
	c.[StimulusAct],       	      
	c.[CreatedBy],	     
	c.[CreationDate],      	
	c.[LastModifiedBy],    	     
	c.[LastModificationDate]

	from tbl_Cntrcts c join tlkup_UserProfile p on c.CO_ID = p.CO_ID
	join [tlkup_Sched/Cat] s on c.Schedule_Number = s.Schedule_Number
	join #SinListForDumpReport t on c.CntrctNum = t.ContractNumber
	join [tlkup_VetStatus] v on c.[Socio_VetStatus_ID] = v.VetStatus_ID
	join [tlkup_Business_Size] b on c.[Socio_Business_Size_ID] = b.Business_Size_ID
	left outer join [tlkup_Geographic] g on c.[Geographic_Coverage_ID] = g.Geographic_ID
	left outer join [tlkup_IFF_Type] i on c.[IFF_Type_ID] = i.IFF_Type_ID
	left outer join [tlkup_Ret_Goods_Policy] r on c.[Returned_Goods_Policy_Type] = r.Returned_Goods_Policy_Type_ID
	left outer join [tlkup_Dist_Manf] d on c.[Dist_Manf_ID] = d.Dist_Manf_ID
	left outer join [tbl_sba_SBAPlan] n on c.[SBAPlanID] = n.SBAPlanID

	where dbo.IsContractActiveFunction( CntrctNum, GETDATE() ) =  1
	order by CntrctNum

	select @error = @@ERROR, @rowCount = @@ROWCOUNT
	if @error <> 0 or @rowCount <= 0
	BEGIN
		select @errorMsg = 'Error selecting active contracts for report.'
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


