IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectContractSBAProjections' )
BEGIN
	DROP PROCEDURE SelectContractSBAProjections
END
GO

CREATE PROCEDURE SelectContractSBAProjections
(
@UserLogin nvarchar(120),
@CurrentUser uniqueidentifier,
@SBAPlanId int,  /* -1 = no plan selected */
@ContractNumber nvarchar(20),
@WithAdd bit
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000),
		@ProjectionID int, 
		@StartDate datetime, 
		@EndDate datetime, 
		@SBDollars money, 
		@SDBDollars money, 
		@WomenOwnedDollars money, 
		@DisabledVetDollars money, 
		@HubZoneDollars money, 
		@HBCUDollars money, 
		@VeteranOwnedDollars money, 
		@TotalSubConDollars money, 

		@SBPercentage float,
		@SDBPercentage float, 
		@WomenOwnedPercentage float, 
		@DisabledVetPercentage float, 
		@HubZonePercentage float, 
		@HBCUPercentage float, 
		@VeteranOwnedPercentage float, 

		@SBAComments nvarchar(255), 
		@LastModifiedBy nvarchar(120)


BEGIN TRANSACTION

	if @WithAdd = 0
	BEGIN

		select ProjectionID, SBAPlanID, StartDate, EndDate, SBDollars, SDBDollars, WomenOwnedDollars, DisabledVetDollars, HubZoneDollars, HBCUDollars, VeteranOwnedDollars, TotalSubConDollars, 
									
									round( cast( ( cast( SBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 )  as SBPercentage,									
									round( cast( ( cast( SDBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as SDBPercentage,									
									round( cast( ( cast( WomenOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as WomenOwnedPercentage,								
									round( cast( ( cast( DisabledVetDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 )  as DisabledVetPercentage,
									round( cast( ( cast( HubZoneDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as HubZonePercentage,
									round( cast( ( cast( HBCUDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as HBCUPercentage,
									round( cast( ( cast( VeteranOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as VeteranOwnedPercentage,
									
									--round( cast( ( cast( SBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 )  as SBPercentage,									
									--round( cast( ( cast( SDBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 ) as SDBPercentage,									
									--round( cast( ( cast( WomenOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 ) as WomenOwnedPercentage,								
									--round( cast( ( cast( DisabledVetDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 )  as DisabledVetPercentage,
									--round( cast( ( cast( HubZoneDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 ) as HubZonePercentage,
									--round( cast( ( cast( HBCUDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 ) as HBCUPercentage,
									--round( cast( ( cast( VeteranOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 4 ) as VeteranOwnedPercentage,																		

									--( SBDollars / TotalSubConDollars ) as SBPercentage,
									--( SDBDollars / TotalSubConDollars ) as SDBPercentage,
									--( WomenOwnedDollars / TotalSubConDollars ) as WomenOwnedPercentage,
									--( DisabledVetDollars / TotalSubConDollars ) as DisabledVetPercentage,
									--( HubZoneDollars / TotalSubConDollars ) as HubZonePercentage,
									--( HBCUDollars / TotalSubConDollars ) as HBCUPercentage,
									--( VeteranOwnedDollars / TotalSubConDollars ) as VeteranOwnedPercentage,

		Comments, LastModifiedBy
		from tbl_sba_Projection  
		where SBAPlanId = @SBAPlanId
		order by StartDate desc

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting SBA projections for SBA plan.'
			goto ERROREXIT
		END

	END
	else
	BEGIN

		select 	@ProjectionID = -1,
			@StartDate = GETDATE(),
			@EndDate = GETDATE(),
			@SBDollars = 0,
			@SDBDollars  = 0,
			@WomenOwnedDollars = 0,
			@DisabledVetDollars  = 0,
			@HubZoneDollars  = 0, 
			@HBCUDollars  = 0,
			@VeteranOwnedDollars  = 0,
			@TotalSubConDollars  = 0,

			@SBPercentage   = 0,
			@SDBPercentage   = 0,
			@WomenOwnedPercentage   = 0,
			@DisabledVetPercentage   = 0,
			@HubZonePercentage   = 0,
			@HBCUPercentage   = 0,
			@VeteranOwnedPercentage   = 0,

			@SBAComments = '',
			@LastModifiedBy = ''
		
		select ProjectionID, SBAPlanID, StartDate, EndDate, SBDollars, SDBDollars, WomenOwnedDollars, DisabledVetDollars, HubZoneDollars, HBCUDollars, VeteranOwnedDollars, TotalSubConDollars, 
								round( cast( ( cast( SBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 )  as SBPercentage,									
									round( cast( ( cast( SDBDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as SDBPercentage,									
									round( cast( ( cast( WomenOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as WomenOwnedPercentage,								
									round( cast( ( cast( DisabledVetDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 )  as DisabledVetPercentage,
									round( cast( ( cast( HubZoneDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as HubZonePercentage,
									round( cast( ( cast( HBCUDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as HBCUPercentage,
									round( cast( ( cast( VeteranOwnedDollars as decimal(19,4))/ cast( TotalSubConDollars as decimal(19,4) )) as decimal(19,8) ), 3 ) as VeteranOwnedPercentage,
		Comments, LastModifiedBy
		from tbl_sba_Projection  
		where SBAPlanId = @SBAPlanId

		union

		select @ProjectionID as ProjectionID,
			@SBAPlanId as SBAPlanID,
			@StartDate as StartDate,
			@EndDate as EndDate,
			@SBDollars as SBDollars, 
			@SDBDollars  as SDBDollars, 
			@WomenOwnedDollars  as WomenOwnedDollars, 
			@DisabledVetDollars  as DisabledVetDollars, 
			@HubZoneDollars  as HubZoneDollars, 
			@HBCUDollars  as HBCUDollars, 
			@VeteranOwnedDollars  as VeteranOwnedDollars, 
			@TotalSubConDollars  as TotalSubConDollars, 

			@SBPercentage  as SBPercentage,
			@SDBPercentage  as SDBPercentage,
			@WomenOwnedPercentage as WomenOwnedPercentage,
			@DisabledVetPercentage  as DisabledVetPercentage,
			@HubZonePercentage  as HubZonePercentage,
			@HBCUPercentage as HBCUPercentage,
			@VeteranOwnedPercentage as VeteranOwnedPercentage,

			@SBAComments  as Comments, 
			@LastModifiedBy  as LastModifiedBy

		order by StartDate desc

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


