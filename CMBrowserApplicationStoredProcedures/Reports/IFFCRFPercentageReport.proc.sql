IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'IFFCRFPercentageReport' )
BEGIN
	DROP PROCEDURE IFFCRFPercentageReport
END
GO

CREATE PROCEDURE IFFCRFPercentageReport
(
@ReportUserLoginId nvarchar(100), /* running the report, not a selection criteria */
@Division int,  /* 1 FSS, 2 NC, -1 All NAC, 6 SAC */ 
@ActiveStatus nchar(1) /* 'A' active only, 'B' both active and historical */
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	/* log the request for the report */
	exec InsertUserActivity @ReportUserLoginId, 'R', 'IFF CRF Percentage Report', '2'
	
	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error logging report request.'
		goto ERROREXIT
	END

	if @Division = 1 or @Division = 2 or @Division = 6
	BEGIN
		if @ActiveStatus = 'A'
		BEGIN
			select s.Schedule_Name, i.IFFID, i.VA_IFF, i.OGA_IFF, i.SLG_IFF, 
			t.Title as StartQuarter, e.Title as EndQuarter

			from tbl_IFF i join [tlkup_Sched/Cat] s on i.Schedule_Number = s.Schedule_Number
			join tlkup_year_qtr t on i.Start_Quarter_Id = t.Quarter_ID
			join tlkup_year_qtr e on i.End_Quarter_Id = e.Quarter_ID
			where GETDATE() between t.Start_Date and e.End_Date
			and s.Division = @Division

			order by Schedule_Name

			select @error = @@ERROR, @rowCount = @@ROWCOUNT
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting active IFF/CRF values.'
				goto ERROREXIT
			END
		END
		else
		BEGIN
		select s.Schedule_Name, i.IFFID, i.VA_IFF, i.OGA_IFF, i.SLG_IFF, 
			t.Title as StartQuarter, e.Title as EndQuarter

			from tbl_IFF i join [tlkup_Sched/Cat] s on i.Schedule_Number = s.Schedule_Number
			join tlkup_year_qtr t on i.Start_Quarter_Id = t.Quarter_ID
			join tlkup_year_qtr e on i.End_Quarter_Id = e.Quarter_ID
			where s.Division = @Division

			order by Schedule_Name, t.Start_Date

			select @error = @@ERROR, @rowCount = @@ROWCOUNT
			if @error <> 0 
			BEGIN
				select @errorMsg = 'Error selecting active and historical IFF/CRF values.'
				goto ERROREXIT
			END

		END
	END
	else /* any division except SAC */
	BEGIN
		if @ActiveStatus = 'A'
			BEGIN
				select s.Schedule_Name, i.IFFID, i.VA_IFF, i.OGA_IFF, i.SLG_IFF, 
				t.Title as StartQuarter, e.Title as EndQuarter

				from tbl_IFF i join [tlkup_Sched/Cat] s on i.Schedule_Number = s.Schedule_Number
				join tlkup_year_qtr t on i.Start_Quarter_Id = t.Quarter_ID
				join tlkup_year_qtr e on i.End_Quarter_Id = e.Quarter_ID
				where GETDATE() between t.Start_Date and e.End_Date
				and s.Division <> 6

				order by Schedule_Name

				select @error = @@ERROR, @rowCount = @@ROWCOUNT
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting active IFF/CRF values.'
					goto ERROREXIT
				END
			END
			else
			BEGIN
				select s.Schedule_Name, i.IFFID, i.VA_IFF, i.OGA_IFF, i.SLG_IFF, 
				t.Title as StartQuarter, e.Title as EndQuarter

				from tbl_IFF i join [tlkup_Sched/Cat] s on i.Schedule_Number = s.Schedule_Number
				join tlkup_year_qtr t on i.Start_Quarter_Id = t.Quarter_ID
				join tlkup_year_qtr e on i.End_Quarter_Id = e.Quarter_ID
				where s.Division <> 6

				order by Schedule_Name, t.Start_Date

				select @error = @@ERROR, @rowCount = @@ROWCOUNT
				if @error <> 0 
				BEGIN
					select @errorMsg = 'Error selecting active and historical IFF/CRF values.'
					goto ERROREXIT
				END

			END
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



