IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[LogDuplicateItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [LogDuplicateItems]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Proc [dbo].[LogDuplicateItems]
As

Declare   @cntid int,
	@ndc1 char(5), 
	@ndc2 char(4),
	@ndc3 char(2),
	@minitemid int,
	@maxitemid int,
	@minPackageDescription nvarchar(14),
	@minGeneric nvarchar(64),
	@minTradeName nvarchar(45),
	@minDispensingUnit nvarchar(10),
	@maxPackageDescription nvarchar(14),
	@maxGeneric nvarchar(64),
	@maxTradeName nvarchar(45),
	@maxDispensingUnit nvarchar(10)


DECLARE Items_cursor CURSOR FOR 
SELECT Distinct ContractId,fdaassignedlabelercode,productcode,packagecode
FROM duplicatendcitems1
Order by 3

OPEN Items_cursor 

FETCH NEXT FROM Items_cursor 
INTO @cntid,@ndc1, @ndc2, @ndc3

WHILE @@FETCH_STATUS = 0
BEGIN
	Select      @minitemid = min(drugitemid),
				@maxitemid = max(drugitemid)
	From duplicatendcitems1
	Where ContractId = @cntid 
	And fdaassignedlabelercode = @ndc1
	And productcode = @ndc2
	And packagecode = @ndc3


	Select    @minPackageDescription = PackageDescription,
		@minGeneric = Generic,
		@minTradeName = TradeName,
--		@minDiscontinuationDate = DiscontinuationDate,
--		@minDiscontinuationEnteredDate = DiscontinuationEnteredDate,
--		@minPrimeVendor = PrimeVendor,
--		@minPrimeVendorChangedDate = PrimeVendorChangedDate,
--		@minPassThrough = PassThrough,
		@minDispensingUnit = DispensingUnit
--		@minVAClass = VAClass
	From duplicatendcitems1 
	Where DrugItemId = @minitemid 

	Select    @maxPackageDescription = PackageDescription,
		@maxGeneric = Generic,
		@maxTradeName = TradeName,
--		@maxDiscontinuationDate = DiscontinuationDate,
--		@maxDiscontinuationEnteredDate = DiscontinuationEnteredDate,
--		@maxPrimeVendor = PrimeVendor,
--		@maxPrimeVendorChangedDate = PrimeVendorChangedDate,
--		@maxPassThrough = PassThrough,
		@maxDispensingUnit = DispensingUnit
--		@maxVAClass = VAClass
	From duplicatendcitems1 
	Where DrugItemId = @maxitemid 

	If @minPackageDescription = @maxPackageDescription 
	Begin
		If @minGeneric  = @maxGeneric 
		Begin
			If @minTradeName  = @maxTradeName 
			Begin
				If @minDispensingUnit = @maxDispensingUnit 
				Begin
					Insert into LogDuplicateNDCItems
					(MinDrugItemId, MaxDrugItemId,PacakgeDescription,Generic, TradeName,DispensingUnit)
					Select @minitemid,@maxitemid,'Same','Same','Same','Same'
				End
				Else
				Begin
					Insert into LogDuplicateNDCItems
					(MinDrugItemId, MaxDrugItemId,PacakgeDescription,Generic, TradeName,DispensingUnit)
					Select @minitemid,@maxitemid,'Same','Same','Same','Different'
				End
			End
			Else
			Begin
				Insert into LogDuplicateNDCItems
				(MinDrugItemId, MaxDrugItemId,PacakgeDescription,Generic, TradeName,DispensingUnit)
				Select @minitemid,@maxitemid,'Same','Same','Different',Null
				
			End
		End
		Else
		Begin
			Insert into LogDuplicateNDCItems
			(MinDrugItemId, MaxDrugItemId,PacakgeDescription,Generic, TradeName,DispensingUnit)
			Select @minitemid,@maxitemid,'Same','Different',Null,Null
		End
	End
	Else
	Begin
		Insert into LogDuplicateNDCItems
		(MinDrugItemId, MaxDrugItemId,PacakgeDescription,Generic, TradeName,DispensingUnit)
		Select @minitemid,@maxitemid,'Different',Null,Null,Null
	End

	FETCH NEXT FROM Items_cursor 
	INTO @cntid,@ndc1, @ndc2, @ndc3	
End
Close Items_cursor 
Deallocate Items_cursor 
