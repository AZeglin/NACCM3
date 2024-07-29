IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectExtendableContracts' )
BEGIN
	DROP PROCEDURE SelectExtendableContracts
END
GO

CREATE PROCEDURE SelectExtendableContracts
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120),
@ProposalTypeId int,    /* 1 = offer; 2 = extension */
@ScheduleNumber int
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)

BEGIN TRANSACTION

	if @ProposalTypeId = 2
	BEGIN

		select c.CntrctNum as ContractNumber, c.Contractor_Name as VendorName, c.Dates_CntrctExp as ExpirationDate, c.CO_ID, u.FullName
		from tbl_Cntrcts c join NACSEC.dbo.SEC_UserProfile u on c.CO_ID = u.CO_ID
		where ( c.Dates_CntrctExp between DATEADD( DAY, -90, getdate() ) and  getdate() 
		
			-- or ( c.Dates_CntrctExp > getdate()  and c.Dates_Completion is null ))					
			-- change to allow future completion dates
			or (( c.Dates_CntrctExp >= cast( convert( char(8), getdate(), 112 ) as datetime ) and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= cast( convert( char(8), getdate(), 112 ) as datetime ) 
									and c.Dates_Completion >= cast( convert( char(8), getdate(), 112 ) as datetime ) )) )

			and c.Schedule_Number = @ScheduleNumber
		union

		select '-- select --' as ContractNumber, '' as VendorName, '1/1/1900' as ExpirationDate, -1 as CO_ID, '' as FullName

		order by c.CntrctNum
	
	END
	else
	BEGIN

		select c.CntrctNum as ContractNumber, c.Contractor_Name as VendorName, c.Dates_CntrctExp as ExpirationDate, c.CO_ID, u.FullName
		from tbl_Cntrcts c join NACSEC.dbo.SEC_UserProfile u on c.CO_ID = u.CO_ID
		where ( c.Dates_CntrctExp between DATEADD( YEAR, -1, getdate() ) and  getdate() 

			-- or ( c.Dates_CntrctExp > getdate() and c.Dates_Completion is null ))
			-- change to allow future completion dates
			or (( c.Dates_CntrctExp >= cast( convert( char(8), getdate(), 112 ) as datetime ) and c.Dates_Completion is null ) or
								( c.Dates_Completion is not null and c.Dates_CntrctExp >= cast( convert( char(8), getdate(), 112 ) as datetime ) 
									and c.Dates_Completion >= cast( convert( char(8), getdate(), 112 ) as datetime ) )) )
			
			and c.Schedule_Number = @ScheduleNumber
		
		union

		select '-- select --' as ContractNumber, '' as VendorName, '1/1/1900' as ExpirationDate, -1 as CO_ID, '' as FullName

		order by c.CntrctNum
	END

select @error = @@ERROR
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error selecting extendable contracts.'
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



