IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'GetNewContractPrefix')
	BEGIN
		DROP  Procedure  GetNewContractPrefix
	END

GO

CREATE Procedure GetNewContractPrefix
(
@ScheduleNumber int,
@Prefix nvarchar(50) OUTPUT
)

AS

DECLARE	   @DivisionId int

BEGIN

	--select @DivisionId = Division from [tlkup_Sched/Cat] where Schedule_Number = @ScheduleNumber

	--if @DivisionId = 1
	--BEGIN
	--	select @Prefix = 'V797D-'
	--END
	--else if @DivisionId = 2
	--BEGIN
	--	select @Prefix = 'VA797'
	--END
	--else if @DivisionId = 3
	--BEGIN
	--	select @Prefix = 'VA791'	
	--END
	--else if @DivisionId = 6
	--BEGIN
	--	select @Prefix = 'VA119'	
	--END
	--else /* default prefix is expected to follow general VA format */
	--BEGIN
	--	select @Prefix = 'VA797'
	--END

	select @Prefix = ''

	-- this is release 2.1G code.  It is commented out for "two months" until folks get used to the new prefix and stop using the old prefix.  Use of new prefix is mandated for 10/1/2017
	if @ScheduleNumber = 36 or @ScheduleNumber = 42 or @ScheduleNumber =  1 or @ScheduleNumber =  34 or @ScheduleNumber =  4 or @ScheduleNumber =  7 or @ScheduleNumber =  9 or @ScheduleNumber =  8 or @ScheduleNumber =  10 or @ScheduleNumber =  48 
	BEGIN
		select @Prefix = '36F797'
	END
	else if @ScheduleNumber = 46
	BEGIN
		select @Prefix = '36C791'
	END
	else if @ScheduleNumber = 44 or @ScheduleNumber = 45
	BEGIN
		select @Prefix = '36S797'
	END
	else if @ScheduleNumber = 62 or @ScheduleNumber = 59 or @ScheduleNumber = 60 or @ScheduleNumber =  40 
	BEGIN
		select @Prefix = '36H797'
	END
	else if @ScheduleNumber = 33 
	BEGIN
		select @Prefix = '36L797'  -- added on 12/16/2020 ( used to use 36H )
	END
	else if @ScheduleNumber = 18 or @ScheduleNumber = 61 or @ScheduleNumber = 49 
	BEGIN
		select @Prefix = '36W797'
	END
	else if @ScheduleNumber = 30 or @ScheduleNumber = 39 or @ScheduleNumber = 52 
	BEGIN
		select @Prefix = '36E797'
	END
	else if @ScheduleNumber = 55 or @ScheduleNumber = 53 or @ScheduleNumber = 58 or @ScheduleNumber = 56 or @ScheduleNumber = 54 or @ScheduleNumber = 57 
	BEGIN
		-- select @Prefix = '36C119'  leave as blank per email from Ray on 1/8/2018.
		select @Prefix = ''
	END
END

	
