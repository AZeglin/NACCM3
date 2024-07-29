IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectOfferHeaders' )
BEGIN
	DROP PROCEDURE SelectOfferHeaders
END
GO

CREATE PROCEDURE SelectOfferHeaders
(
@CurrentUser uniqueidentifier,
@COID int,
@LoginId nvarchar(120),
@OfferStatusFilter nchar(1),  /* A - All, O - Open, C - Completed, N - none */
@OfferOwnerFilter nchar(1), /* A - All, M - Mine */
@FilterType nchar(1), /* O - CO Name, V - Vendor, T - Status, S - Schedule, X = none, B = OfferNumber, E = ExtendsContractNumber */
@FilterValue nvarchar(200),
@SortExpression nvarchar(100),
@SortDirection nvarchar(20)
)
    
AS

Declare @errorMsg nvarchar(1300),
		@error int,
		@rowcount int,
		@CurrentDateWithoutTime datetime,
		@query nvarchar(2000),
		@whereClause nvarchar(800),
		@filterValueClause nvarchar(350),
		@orderByClause nvarchar(280)
	
BEGIN TRANSACTION

	select @CurrentDateWithoutTime = CAST( CONVERT( CHAR(8), GETDATE(), 112 ) as DATETIME )

	create table #SelectedOffers
	(
		Offer_ID int not null,
		Solicitation_ID  int not null,
		Solicitation_Number nvarchar(50) not null,
		CO_ID int not null,
		FullName nvarchar(80) null,
		LastName nvarchar(40) null,
		Schedule_Number int not null,
		Schedule_Name nvarchar(75) not null,
		OfferNumber nvarchar(30) null,
		Proposal_Type_ID int not null,
		Proposal_Type_Description nvarchar(30) not null,
		Action_ID   int not null,
		Action_Description nvarchar(30) not null,
		Complete bit not null,
		Contractor_Name  nvarchar(75)  not null,
		Dates_Received       	datetime         NOT NULL,
		Dates_Assigned       	datetime             NULL,
		Dates_Reassigned       	datetime             NULL,
		Dates_Action           	datetime         NOT NULL,
		Dates_Expected_Completion   	datetime             NULL,
		Dates_Sent_for_Preaward   datetime             NULL,
		Dates_Returned_to_Office       	datetime             NULL,
		ContractNumber      	nvarchar(20)         NULL,
		ExtendsContractNumber      	nvarchar(20)         NULL
	)

	/* for speeding up startup */
	if @OfferStatusFilter = 'N'
	BEGIN

		  select Offer_ID,			
			Solicitation_Number,
			CO_ID,
			FullName,
			LastName,
			Schedule_Number,
			Schedule_Name,
			OfferNumber,   
			Proposal_Type_ID as ProposalTypeId,   
			Proposal_Type_Description,			
			Action_Description,
			Complete,
			Contractor_Name,
			Dates_Received,
			Dates_Assigned,
			Dates_Reassigned,
			Dates_Action,
			Dates_Expected_Completion,
			Dates_Sent_for_Preaward,
			Dates_Returned_to_Office,
			ContractNumber,
			ExtendsContractNumber
		from #SelectedOffers

		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting offers for status = N.'
			goto ERROREXIT
		END

		GOTO OKEXIT
	END

	select @query = ' insert into #SelectedOffers
	select o.Offer_ID,
		o.Solicitation_ID,
		t.Solicitation_Number,
		o.CO_ID,
		u.FullName,
		u.LastName,
		o.Schedule_Number,
		s.Schedule_Name,
		o.OfferNumber,
		o.Proposal_Type_ID,
		p.Proposal_Type_Description,
		o.Action_ID,
		a.Action_Description,
		a.Complete,
		o.Contractor_Name,
		o.Dates_Received,
		o.Dates_Assigned,
		o.Dates_Reassigned,
		o.Dates_Action,
		o.Dates_Expected_Completion,
		o.Dates_Sent_for_Preaward,
		o.Dates_Returned_to_Office,
		o.ContractNumber,
		o.ExtendsContractNumber

	from tbl_Offers o join tlkup_Offers_Action_Type a on o.Action_ID = a.Action_ID
	join tlkup_Offers_Proposal_Type p on o.Proposal_Type_ID = p.Proposal_Type_ID
	join [NACSEC].[dbo].[SEC_UserProfile] u on o.CO_ID = u.CO_ID
	join [tlkup_Sched/Cat] s on o.Schedule_Number = s.Schedule_Number
	join tlkup_Solicitation_Numbers t on o.Solicitation_ID = t.Solicitation_ID
	left outer join tbl_Cntrcts c on c.CntrctNum = o.ContractNumber
	left outer join tbl_Cntrcts d on d.CntrctNum = o.ExtendsContractNumber
	where o.Offer_ID is not null '

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 1' 
		goto ERROREXIT
	END

	if @OfferStatusFilter = 'O'
	BEGIN
		select @whereClause = ' and a.Complete = 0 '
	END
	else if @OfferStatusFilter = 'C'
	BEGIN
		select @whereClause = ' and a.Complete = 1 '
	END
	else if @OfferStatusFilter = 'N'  -- none
	BEGIN
		select @whereClause = ' and o.Offer_ID <> o.Offer_ID '
	END
	else
	BEGIN
		select @whereClause = ''
	END
	
	
	if @OfferOwnerFilter = 'M'
	BEGIN
		select @whereClause = @whereClause + ' and o.CO_ID = ' + convert( nvarchar(10), @COID )
	END

	 /* O - CO Name, V - Vendor, T - Status, S - Schedule, X = none, B = OfferNumber, E = ExtendsContractNumber */
	if LEN( LTRIM( RTRIM( @FilterValue ))) > 0
	BEGIN
	
		select @filterValueClause = '%' + LTRIM( RTRIM( @FilterValue )) + '%'
	
		if @FilterType <> 'X'
		BEGIN
	
			if @FilterType = 'O'
			BEGIN
				select @whereClause = @whereClause + ' and u.FullName like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'V'
			BEGIN
				select @whereClause = @whereClause + ' and o.Contractor_Name like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'T'
			BEGIN
				select @whereClause = @whereClause + ' and a.Action_Description like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'S'
			BEGIN
				select @whereClause = @whereClause + ' and s.Schedule_Name like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'B'
			BEGIN
				select @whereClause = @whereClause + ' and o.OfferNumber like ''' + @filterValueClause + ''''
			END
			else if @FilterType = 'E'
			BEGIN
				select @whereClause = @whereClause + ' and o.ExtendsContractNumber like ''' + @filterValueClause + ''''
			END
		END

	END

	select @orderByClause = ''

	if RTRIM( LTRIM( @SortExpression )) = 'Schedule_Name'
	BEGIN
		select @orderByClause = ' ORDER BY s.Schedule_Name '
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'LastName'
	BEGIN
		select @orderByClause = ' ORDER BY u.LastName '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Contractor_Name'
	BEGIN
		select @orderByClause = ' ORDER BY o.Contractor_Name '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Action_Description'
	BEGIN
		select @orderByClause = ' ORDER BY a.Action_Description '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Dates_Assigned'
	BEGIN
		select @orderByClause = ' ORDER BY o.Dates_Assigned '	
	END
	else if RTRIM( LTRIM( @SortExpression )) = 'Dates_Received'
	BEGIN
		select @orderByClause = ' ORDER BY o.Dates_Received '	
	END	
	else
	BEGIN
		select @orderByClause = ' ORDER BY o.Contractor_Name '		
	END
	
	if RTRIM( LTRIM( @SortDirection )) = 'Descending'
	BEGIN
		select @orderByClause = @orderByClause + ' DESC '
	END
	else
	BEGIN
		select @orderByClause = @orderByClause + ' ASC '
	END

	select @query = @query + @whereClause + @orderByClause

	select @error = @@error
	
	if @error <> 0 
	BEGIN
		select @errorMsg = 'Error assigning query string 2' 
		goto ERROREXIT
	END

	exec SP_EXECUTESQL @query 

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error inserting offers for status = ' + @OfferStatusFilter + ' owner = ' + @OfferOwnerFilter + ' type = ' + @FilterType + ' value = ' + @FilterValue
		goto ERROREXIT
	END
		
    select Offer_ID,
		-- Solicitation_ID,
		Solicitation_Number,
		CO_ID,
		FullName,
		LastName,
		Schedule_Number,
		Schedule_Name,
		OfferNumber,   -- added for R2
		Proposal_Type_ID as ProposalTypeId,   -- added for R2
		Proposal_Type_Description,
		-- Action_ID,
		Action_Description,
		Complete,
		Contractor_Name,
		Dates_Received,
		Dates_Assigned,
		Dates_Reassigned,
		Dates_Action,
		Dates_Expected_Completion,
		Dates_Sent_for_Preaward,
		Dates_Returned_to_Office,
		ContractNumber,
		ExtendsContractNumber
	from #SelectedOffers

	select @error = @@error
	
	if @error <> 0
	BEGIN
		select @errorMsg = 'Error selecting offers for status = ' + @OfferStatusFilter + ' owner = ' + @OfferOwnerFilter + ' type = ' + @FilterType + ' value = ' + @FilterValue
		goto ERROREXIT
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

