IF EXISTS ( SELECT * FROM sysobjects WHERE type = 'P' AND name = 'UpdateBPALookup' )
BEGIN
	DROP PROCEDURE UpdateBPALookup
END
GO

create procedure UpdateBPALookup
(
@LoginId nvarchar(120),
@FSSContractNumber nvarchar(50),
@BPAContractNumber nvarchar(50),
@Count int  OUTPUT      /* number of rows updated or inserted */
)

as

Declare @errorMsg nvarchar(1300),
		@error int,
		@ordinality int
		
BEGIN TRANSACTION

	select @Count = 0

	/* there is at most one row for each BPA */
	/* currently, under R2 there is no way to update a BPA's parent information, the parent must be selected at time of creation */
	if exists ( select * from CM_BPALookup where BPAContractNumber = @BPAContractNumber )
	BEGIN
		update CM_BPALookup
		set FSSContractNumber = @FSSContractNumber,
			LastModifiedBy = @LoginId,
			LastModificationDate = getdate()
		where BPAContractNumber = @BPAContractNumber
		
		select @error = @@error, @Count = @@ROWCOUNT
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error updating BPA Lookup table'
			goto ERROREXIT
		END
	
	END
	else
	BEGIN
		/* determine the ordinality of the new row relative to others with the same fss contract number */
		select @ordinality = -1
		
		select @ordinality = isnull( max( Ordinality ), -1 ) from CM_BPALookup
		where FSSContractNumber = @FSSContractNumber
		
		select @error = @@error
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error selecting o from BPA Lookup table'
			goto ERROREXIT
		END
		
		if @ordinality = -1
		BEGIN
			select @ordinality = 1
		END
		else
		BEGIN
			select @ordinality = @ordinality + 1
		END
		
		
		insert into CM_BPALookup
		( FSSContractNumber, Ordinality, BPAContractNumber, CreatedBy, CreationDate, LastModifiedBy, LastModificationDate )
		values
		( @FSSContractNumber, @ordinality, @BPAContractNumber, @LoginId, getdate(), @LoginId, getdate() )	
		
		select @error = @@error, @Count = @@ROWCOUNT
	
		if @error <> 0
		BEGIN
			select @errorMsg = 'Error inserting into BPA Lookup table'
			goto ERROREXIT
		END
		
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

