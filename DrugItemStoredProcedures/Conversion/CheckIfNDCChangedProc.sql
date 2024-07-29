IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[CheckIfNDCChangedProc]') AND type in (N'P', N'PC'))
DROP PROCEDURE [CheckIfNDCChangedProc]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckIfNDCChangedProc]
(
@ndc_1 char(5),
@ndc_2 char(4),
@ndc_3 char(2),
@cnt_no nvarchar(20),
@ndc1_new char(5) OUTPUT,
@ndc2_new char(4) OUTPUT,
@ndc3_new char(2) OUTPUT,
@cnt_new nvarchar(20) OUTPUT

)
As


Declare 
	@ndc1_old char(5),
	@ndc2_old char(4),
	@ndc3_old char(2),
	@n_old char(1),
	@cnt_old nvarchar(20)
		
	
	IF @cnt_no = 'V797P-5716M'
	Begin
		Select @cnt_no = 'V797P-5779X'
	End

	Select  @ndc1_old = @ndc_1,
			@ndc2_old=@ndc_2,
			@ndc3_old=@ndc_3,
			@cnt_old = @cnt_no


	While (1 = 1)
	Begin
		If exists	(	Select top 1 1 From ndclink 
							Where cnt_old = @cnt_old
							And	ndc1_old = @ndc1_old
							And ndc2_old = @ndc2_old
							And ndc3_old = @ndc3_old
				)
		Begin
			Select  
				@ndc1_new = ndc1_new,
				@ndc2_new = ndc2_new,
				@ndc3_new = ndc3_new,
				@cnt_new  = cnt_new
			From ndclink
			Where cnt_old = @cnt_old
			And	ndc1_old = @ndc1_old
			And ndc2_old = @ndc2_old
			And ndc3_old = @ndc3_old

			If (@cnt_new = @cnt_old and
				@ndc1_new = @ndc1_old and 
				@ndc2_new = @ndc2_old and 
				@ndc3_new = @ndc3_old)
			Begin
				Break
			End
			Else If (@cnt_new = @cnt_no and
					@ndc1_new = @ndc_1 and 
					@ndc2_new = @ndc_2 and 
					@ndc3_new = @ndc_3)
			Begin
				Break
			End
			Else
			Begin
				Select  @cnt_old = @cnt_new ,
						@ndc1_old = @ndc1_new , 
						@ndc2_old = @ndc2_new , 
						@ndc3_old = @ndc3_new

				Continue						
			End
		End
		Else
		begin
			Break
		End
	End
