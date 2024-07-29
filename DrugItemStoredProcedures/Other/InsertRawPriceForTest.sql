IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[InsertRawPriceForTest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [InsertRawPriceForTest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[InsertRawPriceForTest]
(
@DrugItemId int,
@ExistingStartDate datetime,
@ExistingEndDate datetime,
@Price decimal(10,2),
@IsTemporary bit,
@IsFSS bit,
@IsBig4 bit,
@IsRestricted bit,
@LastModificationType nchar(1),
@ModificationStatusId int,
@UserName nvarchar(120),
@DrugItemPriceId int OUTPUT
)

AS

BEGIN
	/* perform a simple parameterized insert - used to avoid clutter in the test scenario code */
	insert into DI_DrugItemPrice
		(
			DrugItemId ,                                     	              
			PriceId    ,                                     	                  
			PriceStartDate ,                                 	             
			PriceStopDate   ,                                	             
			Price        ,                      
			IsTemporary  ,                                   	                  
			IsFSS        ,                                   	                  
			IsBIG4       ,                                   	                  
			IsVA         ,                                   	                  
			IsBOP        ,                                   	                  
			IsCMOP       ,                                   	                  
			IsDOD        ,                                   	                  
			IsHHS        ,                                   	                  
			IsIHS        ,                                   	                  
			IsIHS2       ,                                   	                  
			IsDIHS        ,                                   	                  
			IsNIH        ,                                   	                  
			IsPHS        ,                                   	                  
			IsSVH        ,                                   	                  
			IsSVH1       ,                                   	                  
			IsSVH2       ,                                   	                  
			IsTMOP       ,                                   	                  
			IsUSCG       ,                                   	                  
			AwardedFSSTrackingCustomerRatio    , 
			CurrentTrackingCustomerPrice       ,             
			ExcludeFromExport,
			LastModificationType,		             
			ModificationStatusId               ,             	                  
			CreatedBy                          ,             	
			CreationDate                       ,             	         
			LastModifiedBy                     ,             
			LastModificationDate               
		)             	         
		values
		(
			@DrugItemId,
			1,
			@ExistingStartDate,
			@ExistingEndDate,
			@Price,
			@IsTemporary,
			@IsFSS,
			@IsBig4,
			@IsRestricted, /* VA */
			0,
			0,
			0,
			0,
			@IsRestricted, /* IHS */
			@IsRestricted, /* IHS2 */
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			null,
			null,
			0,
			'T',
			@ModificationStatusId,
			@UserName,
			getdate(),
			@UserName,
			getdate()
		)

		select @DrugItemPriceId = @@IDENTITY	

END