IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'SelectSolicitations' )
BEGIN
	DROP PROCEDURE SelectSolicitations
END
GO

CREATE PROCEDURE SelectSolicitations
(
@CurrentUser uniqueidentifier,
@Active nchar(1)             --  'A' = Active Only  'B' = Both Active and Inactive
)

AS

Declare 	@error int,
		@rowCount int,
		@errorMsg nvarchar(1000)



BEGIN TRANSACTION

	if @Active = 'A'
	BEGIN
		SELECT Solicitation_ID, Solicitation_Number 
		FROM tlkup_Solicitation_Numbers 
		where Inactive = 0
	
		union
	
		select -1 as Solicitation_ID, '--select--' as Solicitation_Number

		ORDER BY Solicitation_Number

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting active solicitations.'
			goto ERROREXIT
		END

	END
	else if @Active = 'B'
	BEGIN
		SELECT Solicitation_ID, Solicitation_Number 
		FROM tlkup_Solicitation_Numbers 

		union
	
		select -1 as Solicitation_ID, '--select--' as Solicitation_Number

		ORDER BY Solicitation_Number

		select @error = @@ERROR
		if @error <> 0 
		BEGIN
			select @errorMsg = 'Error selecting solicitations.'
			goto ERROREXIT
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


