using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;
using AjaxControlToolkit;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using ListView = VA.NAC.NACCMBrowser.BrowserObj.ListView;
using Menu = System.Web.UI.WebControls.Menu;
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class OfferGeneral : BaseDocumentEditorPage
    {
        public OfferGeneral()
            : base()
        {
        }


        public void Page_PreInit( object sender, EventArgs e )
        {
            SetCreatingNewOffer();

            if( CreatingNewOffer() == true )
            {
                base.DocumentEditorType = DocumentEditorTypes.NewOffer;
            }
            else
            {
                base.DocumentEditorType = DocumentEditorTypes.Offer;
            }
        }


        CurrentDocument _currentDocument = null;
        private bool _bCreatingNewOffer = false;

        protected new void Page_Load( object sender, EventArgs e )
        {
            if( Session[ "NACCMStartedProperly" ] == null || Request.UrlReferrer == null )
            {
                Response.Redirect( "~/Start.aspx" );
            }
            
            if( Page.IsPostBack == false && ( ( NACCM )Page.Master.Master ).IsRedirected() == true )  // chgd to true 4/3/2017
            {
                ClearSessionVariables();
            }

            CMGlobals.AddKeepAlive( this.Page, 12000 );

            LoadAndBindNonFormViewControls();

            AssignDataSourceToFormViews();

            if( Page.IsPostBack == false )
            {
                if( CurrentDocumentIsChanging == true )
                {
                    if( CreatingNewOffer() == true )
                    {
                        OfferDataRelay.Clear();  // prepare for offer creation
                    }
                    else
                    {
                        OfferDataRelay.Load();
                    }
                }
                BindFormViews();
            }
        
            if( Page.IsPostBack == true )
            {
                string offerAttributesRefreshDateType = "";
                bool bOfferAttributesRefreshOrNot = false;

                string offerActionRefreshDateType = "";
                bool bOfferActionRefreshOrNot = false;

                string offerAuditRefreshDateType = "";
                bool bOfferAuditRefreshOrNot = false;

                HiddenField refreshOfferAttributesDateValueOnSubmitHiddenField = ( HiddenField )OfferAttributesFormView.FindControl( "RefreshOfferAttributesDateValueOnSubmit" );
                HiddenField refreshOfferAttributesOrNotOnSubmitHiddenField = ( HiddenField )OfferAttributesFormView.FindControl( "RefreshOfferAttributesOrNotOnSubmit" );

                HiddenField refreshOfferActionDateValueOnSubmitHiddenField = ( HiddenField )OfferActionFormView.FindControl( "RefreshOfferActionDateValueOnSubmit" );
                HiddenField refreshOfferActionOrNotOnSubmitHiddenField = ( HiddenField )OfferActionFormView.FindControl( "RefreshOfferActionOrNotOnSubmit" );

                HiddenField refreshOfferAuditDateValueOnSubmitHiddenField = ( HiddenField )OfferAuditFormView.FindControl( "RefreshOfferAuditDateValueOnSubmit" );
                HiddenField refreshOfferAuditOrNotOnSubmitHiddenField = ( HiddenField )OfferAuditFormView.FindControl( "RefreshOfferAuditOrNotOnSubmit" );

                if( refreshOfferAttributesDateValueOnSubmitHiddenField != null )
                {
                    offerAttributesRefreshDateType = refreshOfferAttributesDateValueOnSubmitHiddenField.Value.ToString();

                    if( refreshOfferAttributesOrNotOnSubmitHiddenField != null )
                    {
                        bOfferAttributesRefreshOrNot = Boolean.Parse( refreshOfferAttributesOrNotOnSubmitHiddenField.Value );

                        if( offerAttributesRefreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( bOfferAttributesRefreshOrNot == true )
                            {
                                RefreshDate( offerAttributesRefreshDateType );
                            }
                            else
                            {
                                // reset date
                                Session[ offerAttributesRefreshDateType ] = Session[ "CalendarInitialDate" ];
                            }

                            refreshOfferAttributesDateValueOnSubmitHiddenField.Value = "Undefined";
                            refreshOfferAttributesOrNotOnSubmitHiddenField.Value = "False";
                        }
                    }
                }

                if( refreshOfferActionDateValueOnSubmitHiddenField != null )
                {
                    offerActionRefreshDateType = refreshOfferActionDateValueOnSubmitHiddenField.Value.ToString();

                    if( refreshOfferActionOrNotOnSubmitHiddenField != null )
                    {
                        bOfferActionRefreshOrNot = Boolean.Parse( refreshOfferActionOrNotOnSubmitHiddenField.Value );

                        if( offerActionRefreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( bOfferActionRefreshOrNot == true )
                            {
                                RefreshDate( offerActionRefreshDateType );
                            }
                            else
                            {
                                // reset date
                                Session[ offerActionRefreshDateType ] = Session[ "CalendarInitialDate" ];
                            }

                            refreshOfferActionDateValueOnSubmitHiddenField.Value = "Undefined";
                            refreshOfferActionOrNotOnSubmitHiddenField.Value = "False";
                        }
                    }
                }



                if( refreshOfferAuditDateValueOnSubmitHiddenField != null )
                {
                    offerAuditRefreshDateType = refreshOfferAuditDateValueOnSubmitHiddenField.Value.ToString();

                    if( refreshOfferAuditOrNotOnSubmitHiddenField != null )
                    {
                        bOfferAuditRefreshOrNot = Boolean.Parse( refreshOfferAuditOrNotOnSubmitHiddenField.Value );

                        if( offerAuditRefreshDateType.Contains( "Undefined" ) == false )
                        {
                            if( bOfferAuditRefreshOrNot == true )
                            {
                                RefreshDate( offerAuditRefreshDateType );
                            }
                            else
                            {
                                // reset date
                                Session[ offerAuditRefreshDateType ] = Session[ "CalendarInitialDate" ];
                            }

                            refreshOfferAuditDateValueOnSubmitHiddenField.Value = "Undefined";
                            refreshOfferAuditOrNotOnSubmitHiddenField.Value = "False";
                        }
                    }
                }
            }

        }

        private void SetCreatingNewOffer()
        {
            _bCreatingNewOffer = false;

            // current document is cleared to null if create offer is selected from the main menu
            CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];

            if( currentDocument == null )
                _bCreatingNewOffer = true;
        }

        private bool CreatingNewOffer()
        {
            return ( _bCreatingNewOffer );
        }

        private void AssignDataSourceToFormViews()
        {
            AssignDataSourceToHeader();

            OfferAttributesFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferAttributesFormView.DataKeyNames = new string[] { "OfferId" };

            OfferActionFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferActionFormView.DataKeyNames = new string[] { "OfferId" };

            OfferAuditFormView.DataSource = OfferDataRelay.EditedOfferDataSourceFront;
            OfferAuditFormView.DataKeyNames = new string[] { "OfferId" };
        }

        
        protected void ClearSessionVariables()
        {
            Session[ "O2Received" ] = null;
            Session[ "O2Assignment" ] = null;
            Session[ "O2Reassignment" ] = null;
            Session[ "O2EstimatedCompletion" ] = null;
            Session[ "O2Action" ] =  null;
            Session[ "O2Audit" ] = null;
            Session[ "O2Return" ] =  null;
            Session[ "IsOfferReceived" ] = null;
            Session[ "IsOfferAssigned" ] = null;
        }

        protected void BindFormViews()
        {
            BindHeader();

            OfferAttributesFormView.DataBind();
            OfferActionFormView.DataBind();
            OfferAuditFormView.DataBind();
        }

        protected void LoadAndBindNonFormViewControls()
        {
        }

        public override void BindAfterShortSave()
        {
            BindFormViews();
        }

        public override void RebindHeader()
        {
            BindHeader();
            TriggerViewMasterUpdatePanel();
        }

        protected bool ValidateOfferNumberFormat( string offerNumber, int selectedScheduleNumber, int offerId, ref string formatError )
        {
            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
            bool bIsValidated = false;
            string validationMessage = "";

            if( offerDB != null )
            {
                offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                offerDB.MakeConnectionString();
              
                bool bSuccess = offerDB.ValidateOfferNumber( offerNumber, selectedScheduleNumber, offerId, ref bIsValidated, ref validationMessage );

                if( bSuccess == true )
                {
                    if( bIsValidated == false )
                        formatError = validationMessage;
                }
                else
                {
                    ShowException( new Exception( string.Format( "The following database error was encountered when validating the offer number format: {0}", offerDB.ErrorMessage ) ) );
                }
            }
            else
            {
                ShowException( new Exception( string.Format( "The following database error was encountered when validating the offer number format: offerDB not set" ) ) );
            }

            return ( bIsValidated );
        }

        // per db support group meeting on 11/28/2017, assignment date will be required if an assignment is made and
        // the assignee is not a chief.  Current process has the initial assignee as a chief, in which case it's not really an assignment, its in an intermediate step
        protected bool IsAssignmentDateRequired( int assigneeCOID, bool bAssignedCOHasChanged )
        {
            bool bIsAssignmentDateRequired = false;
            bool bIsUserInFSSChiefRole = true;
            bool bSuccess = false;

            try
            {
                BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
                if( bs != null )
                {
                    bSuccess = bs.IsUserInFSSChiefRole( assigneeCOID, out bIsUserInFSSChiefRole );
                    if( bSuccess == true )
                    {
                        if( bIsUserInFSSChiefRole == false && bAssignedCOHasChanged == true )
                        {
                            bIsAssignmentDateRequired = true;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }

            return( bIsAssignmentDateRequired );
        }

        public override string GetValidationGroupName()
        {
            return ( "OfferGeneral" );
        }

        // validate after short save but before save to database
        public override bool ValidateBeforeSave( IDataRelay dataRelayInterface, bool bIsShortSave, string validationGroupName )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            ResetValidationGroup( validationGroupName );

            if( offerDataRelay.EditedOfferContentFront.VendorName.Length == 0 )
            {
                AppendValidationError( "Vendor name is required.", bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.SolicitationId == -1 )
            {
                AppendValidationError( "Solicitation is required.", bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.COID == -1 )
            {
                AppendValidationError( "CO name is required.", bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.ScheduleNumber == -1 )
            {
                AppendValidationError( "Schedule is required.", bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.OfferNumber.Length == 0 )
            {
                AppendValidationError( "Offer number is required.", bIsShortSave );
                bSuccess = false;
            }

            string formatErrorMessage = "";
            if( ValidateOfferNumberFormat( offerDataRelay.EditedOfferContentFront.OfferNumber, offerDataRelay.EditedOfferContentFront.ScheduleNumber, offerDataRelay.EditedOfferContentFront.OfferId, ref formatErrorMessage ) == false )
            {
                AppendValidationError( formatErrorMessage, bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.ProposalTypeId == -1 || offerDataRelay.EditedOfferContentFront.ProposalTypeId == 3 ) // 3 = "None Selected"
            {
                AppendValidationError( "Proposal type is required.", bIsShortSave );
                bSuccess = false;
            }

            if( offerDataRelay.EditedOfferContentFront.ProposalTypeId == 2 ) // extension proposal
            {
                if( offerDataRelay.EditedOfferContentFront.ExtendsContractNumber.Trim().Length == 0 )
                {
                    AppendValidationError( "Contract number is required for extension proposals.", bIsShortSave );
                    bSuccess = false;
                }
            }

            // date ranges entered into text boxes must conform to min and max standards
            DateTime offerReceivedDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERRECEIVEDMINDATE ).ToShortDateString() );
            DateTime offerAssignmentDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERASSIGNMENTMINDATE ).ToShortDateString() );
            DateTime offerReassignmentDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERREASSIGNMENTMINDATE ).ToShortDateString() );
            DateTime offerActionDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERACTIONMINDATE ).ToShortDateString() );
            DateTime offerEstimatedCompletionDateMax = Convert.ToDateTime( DateTime.Now.AddDays( OFFERESTIMATEDCOMPLETIONMAXDATE ).ToShortDateString() );
            DateTime offerAuditDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERAUDITORRETURNMINDATE ).ToShortDateString() );
            DateTime todayAsMaxOrMin = Convert.ToDateTime( DateTime.Now.ToShortDateString() );

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            bool bUnlimited = false;
            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferUnlimitedDateRange ) == true )
                {
                    bUnlimited = true;
                }
            }
            else if( currentDocument == null || base.DocumentEditorType == DocumentEditorTypes.NewOffer )
            {
                bUnlimited = true;
            }

            if( offerDataRelay.EditedOfferContentFront.DateReceived == null )
            {
                AppendValidationError( "Received date is required.", bIsShortSave );
                bSuccess = false;
            }
            else if( offerDataRelay.EditedOfferContentFront.DateReceived.CompareTo( DateTime.MinValue ) == 0 )
            {
                AppendValidationError( "Received date is required.", bIsShortSave );
                bSuccess = false;
            }
            // non-standard check - only if not new
            else
            {
                if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                {
                    if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferReceivedDateDirty == true )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DateReceived.CompareTo( offerReceivedDateMin ) < 0 && bUnlimited == false )
                        {
                            AppendValidationError( GetFSSValidationMessage( "received", offerReceivedDateMin, OFFERRECEIVEDMINDATE, true ), bIsShortSave );
                            bSuccess = false;
                        } // check max
                        else if( offerDataRelay.EditedOfferContentFront.DateReceived.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                        {
                             AppendValidationError( GetFSSValidationMessage( "received", todayAsMaxOrMin, 0, false ), bIsShortSave );
                             bSuccess = false;
                        }
                    }
                }
            }

            bool bIsAssignmentDateRequired = IsAssignmentDateRequired( offerDataRelay.EditedOfferContentFront.COID, offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferCOIDDirty );

            if( offerDataRelay.EditedOfferContentFront.DateAssigned != null )
            {
                if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( DateTime.MinValue ) != 0 )
                {
                    // only if not new 
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferAssignmentDateDirty == true )
                        {
                            if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( offerAssignmentDateMin ) < 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "assignment", offerAssignmentDateMin, OFFERASSIGNMENTMINDATE, true ), bIsShortSave );
                                bSuccess = false;
                            } // check max
                            else if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "assignment", todayAsMaxOrMin, 0, false ), bIsShortSave );
                                bSuccess = false;
                            }
                        }                       
                    }
                }
                else // no assignment date
                {
                    // error if required
                    if( bIsAssignmentDateRequired == true )
                    {
                        AppendValidationError( "Assignment date is required if the offer is being assigned.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }
            else // no assignment date 
            {                
                // error if required
                if( bIsAssignmentDateRequired == true )
                {
                    AppendValidationError( "Assignment date is required if the offer is being assigned.", bIsShortSave );
                    bSuccess = false;
                }   
            }           
              
            if( offerDataRelay.EditedOfferContentFront.DateReceived.CompareTo( DateTime.MinValue ) != 0 && offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( DateTime.MinValue ) != 0 )
            {
                if( offerDataRelay.EditedOfferContentFront.DateReceived.CompareTo( offerDataRelay.EditedOfferContentFront.DateAssigned ) > 0 )
                {
                    AppendValidationError( "Date received must be prior to or equal to date assigned.", bIsShortSave );
                    bSuccess = false;
                }
            }

            // re-assignment date is not required, but if present, must be greater than a min value
            if( offerDataRelay.EditedOfferContentFront.DateReassigned != null )
            {
                if( offerDataRelay.EditedOfferContentFront.DateReassigned.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferReassignmentDateDirty == true )
                        {
                            // non-standard check
                            if( offerDataRelay.EditedOfferContentFront.DateReassigned.CompareTo( offerReassignmentDateMin ) < 0 && bUnlimited == false )
                            {                            
                                AppendValidationError( GetFSSValidationMessage( "reassignment", offerReassignmentDateMin, OFFERREASSIGNMENTMINDATE, true ), bIsShortSave );
                                bSuccess = false;
                            } // check max
                            else if( offerDataRelay.EditedOfferContentFront.DateReassigned.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "reassignment", todayAsMaxOrMin, 0, false ), bIsShortSave );
                                bSuccess = false;
                            }
                        }
                    }
                }
            }

            if( offerDataRelay.EditedOfferContentFront.DateReassigned != null && offerDataRelay.EditedOfferContentFront.DateAssigned != null )
            {
                if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( DateTime.MinValue ) != 0 && offerDataRelay.EditedOfferContentFront.DateReassigned.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( offerDataRelay.EditedOfferContentFront.DateReassigned ) > 0 )
                    {
                        AppendValidationError( "Date assigned must be prior to or equal to date reassigned.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }

            // expected completion date is not required, but if present, must be greater than a min value
            if( offerDataRelay.EditedOfferContentFront.ExpectedCompletionDate != null )
            {
                if( offerDataRelay.EditedOfferContentFront.ExpectedCompletionDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferEstimatedCompletionDateDirty == true )
                        {
                            // non-standard check of max
                            if( offerDataRelay.EditedOfferContentFront.ExpectedCompletionDate.CompareTo( offerEstimatedCompletionDateMax ) > 0 && bUnlimited == false )
                            {                            
                                AppendValidationError( GetFSSValidationMessage( "expected completion", offerEstimatedCompletionDateMax, OFFERESTIMATEDCOMPLETIONMAXDATE, false ), bIsShortSave );
                                bSuccess = false;
                            } // check min
                            else if( offerDataRelay.EditedOfferContentFront.ExpectedCompletionDate.CompareTo( todayAsMaxOrMin ) < 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "expected completion", todayAsMaxOrMin, 0, true ), bIsShortSave );
                                bSuccess = false;
                            }
                        }
                    }
                }
            }

            // action completion date is not required, but if present, must be greater than a min value
            if( offerDataRelay.EditedOfferContentFront.ActionDate != null )
            {
                if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferActionDateDirty == true )
                        {
                            // non-standard check
                            if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( offerActionDateMin ) < 0 && bUnlimited == false )
                            {                           
                                AppendValidationError( GetFSSValidationMessage( "action completion", offerActionDateMin, OFFERACTIONMINDATE, true ), bIsShortSave );
                                bSuccess = false;
                            } // check max
                            else if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "action completion", todayAsMaxOrMin, 0, false ), bIsShortSave );
                                bSuccess = false;
                            }
                        }
                    }
                }
            }

            // audit date is not required, but if present, must be greater than a min value
            if( offerDataRelay.EditedOfferContentFront.DateSentForPreaward != null )
            {
                if( offerDataRelay.EditedOfferContentFront.DateSentForPreaward.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferPreawardDateDirty == true )
                        {
                            // non-standard check
                            if( offerDataRelay.EditedOfferContentFront.DateSentForPreaward.CompareTo( offerAuditDateMin ) < 0 && bUnlimited == false )
                            {                           
                                AppendValidationError( GetFSSValidationMessage( "audit", offerAuditDateMin, OFFERAUDITORRETURNMINDATE, true ), bIsShortSave );
                                bSuccess = false;
                            } // check max
                            else if( offerDataRelay.EditedOfferContentFront.DateSentForPreaward.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "audit", todayAsMaxOrMin, 0, false ), bIsShortSave );
                                bSuccess = false;
                            }
                        }
                    }
                }
            }

            // audit return date is not required, but if present, must be greater than a min value
            if( offerDataRelay.EditedOfferContentFront.DateReturnedToOffice != null )
            {
                if( offerDataRelay.EditedOfferContentFront.DateReturnedToOffice.CompareTo( DateTime.MinValue ) != 0 )
                {
                    if( base.DocumentEditorType != DocumentEditorTypes.NewOffer )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DirtyFlags.IsOfferGeneralOfferReturnedDateDirty == true )
                        {
                            // non-standard check
                            if( offerDataRelay.EditedOfferContentFront.DateReturnedToOffice.CompareTo( offerAuditDateMin ) < 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "audit return", offerAuditDateMin, OFFERAUDITORRETURNMINDATE, true ), bIsShortSave );
                                bSuccess = false;
                            } // check max
                            else if( offerDataRelay.EditedOfferContentFront.DateReturnedToOffice.CompareTo( todayAsMaxOrMin ) > 0 && bUnlimited == false )
                            {
                                AppendValidationError( GetFSSValidationMessage( "audit return", todayAsMaxOrMin, 0, false ), bIsShortSave );
                                bSuccess = false;
                            }
                        }
                    }
                }
            }

            if( offerDataRelay.EditedOfferContentFront.IsEditedOfferCompleted == true )
            {
                if( offerDataRelay.EditedOfferContentFront.ActionDate == null )
                {
                    AppendValidationError( "Action date is required if the offer is complete.", bIsShortSave );
                    bSuccess = false;
                }
                else if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( DateTime.MinValue ) == 0 )
                {
                    AppendValidationError( "Action date is required if the offer is complete.", bIsShortSave );
                    bSuccess = false;
                }

                // if reassigned, compare completion date to reassignment date
                bool bIsReassigned = false;
                if( offerDataRelay.EditedOfferContentFront.DateReassigned != null )
                {
                    if( offerDataRelay.EditedOfferContentFront.DateReassigned.CompareTo( DateTime.MinValue ) != 0 )
                    {
                        bIsReassigned = true;
                        if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( offerDataRelay.EditedOfferContentFront.DateReassigned ) < 0 )
                        {
                            AppendValidationError( "Completion date must be after or equal to reassignment date.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                }
                
                // else use assignment date
                if( bIsReassigned == false )
                {
                    if( offerDataRelay.EditedOfferContentFront.DateAssigned != null )
                    {
                        if( offerDataRelay.EditedOfferContentFront.DateAssigned.CompareTo( DateTime.MinValue ) != 0 )
                        {
                            if( offerDataRelay.EditedOfferContentFront.ActionDate.CompareTo( offerDataRelay.EditedOfferContentFront.DateAssigned ) < 0 )
                            {
                                AppendValidationError( "Completion date must be after or equal to assignment date.", bIsShortSave );
                                bSuccess = false;
                            }
                        }
                        else // cannot complete unless assigned
                        {
                            AppendValidationError( "Assignment date is required if the offer is complete.", bIsShortSave );
                            bSuccess = false;
                        }
                    }
                    else // cannot complete unless assigned
                    {
                        AppendValidationError( "Assignment date is required if the offer is complete.", bIsShortSave );
                        bSuccess = false;
                    }
                }
            }

            return ( bSuccess );
        }

        private string GetFSSValidationMessage( string dateName, DateTime date, int days, bool bGreaterThan )
        {
            if( days != 0 )
            {
                if( bGreaterThan == true )
                    return ( string.Format( "To conform with FSS standards for backdating an action, the {0} date must be greater than or equal to {1} ({2} days). Contact NAC Database Support if you feel that you have received this message in error, or to request back dating greater than {3} days. Please note that in order for these requests to be processed, you must first obtain approval from a Contracting Branch Chief.", dateName, date.ToShortDateString(), Math.Abs( days ), Math.Abs( days ) ) );
                else
                    return ( string.Format( "To conform with FSS standards for backdating an action, the {0} date must be less than or equal to {1} ({2} days). Contact NAC Database Support if you feel that you have received this message in error, or to request future dating greater than {3} days. Please note that in order for these requests to be processed, you must first obtain approval from a Contracting Branch Chief.", dateName, date.ToShortDateString(), Math.Abs( days ), Math.Abs( days ) ) );
            }
            else // exactly zero
            {
                if( bGreaterThan == true )
                    return ( string.Format( "To conform with FSS standards for backdating an action, the {0} date must be greater than or equal to {1} (today). Contact NAC Database Support if you feel that you have received this message in error, or to request back dating beyond today. Please note that in order for these requests to be processed, you must first obtain approval from a Contracting Branch Chief.", dateName, date.ToShortDateString() ) );
                else
                    return ( string.Format( "To conform with FSS standards for backdating an action, the {0} date must be less than or equal to {1} (today). Contact NAC Database Support if you feel that you have received this message in error, or to request future dating beyond today. Please note that in order for these requests to be processed, you must first obtain approval from a Contracting Branch Chief.", dateName, date.ToShortDateString() ) );
            }
        }

        public override bool SaveScreenValuesToObject( IDataRelay dataRelayInterface, bool bIsShortSave )
        {
            OfferDataRelay offerDataRelay = ( OfferDataRelay )dataRelayInterface;

            bool bSuccess = true;

            try
            {
                // OfferAttributesFormView
                TextBox vendorNameTextBox = ( TextBox )OfferAttributesFormView.FindControl( "VendorNameTextBox" );
                if( vendorNameTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.VendorName = vendorNameTextBox.Text;
                }

                DropDownList solicitationDropDownList = ( DropDownList )OfferAttributesFormView.FindControl( "SolicitationDropDownList" );
                if( solicitationDropDownList != null )
                {
                    if( solicitationDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.SolicitationId = Int32.Parse( solicitationDropDownList.SelectedItem.Value.ToString() );
                    }
                }

                DropDownList CONameDropDownList = ( DropDownList )OfferAttributesFormView.FindControl( "CONameDropDownList" );
                if( CONameDropDownList != null )
                {
                    if( CONameDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.COID = Int32.Parse( CONameDropDownList.SelectedItem.Value.ToString() );
                        offerDataRelay.EditedOfferContentFront.ContractingOfficerFullName = CONameDropDownList.SelectedItem.Text;
                    }
                }

                DropDownList ScheduleDropDownList = ( DropDownList )OfferAttributesFormView.FindControl( "ScheduleDropDownList" );
                if( ScheduleDropDownList != null )
                {
                    if( ScheduleDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.ScheduleNumber = Int32.Parse( ScheduleDropDownList.SelectedItem.Value.ToString() );
                    }
                }
                
                DropDownList ProposalTypeDropDownList = ( DropDownList )OfferAttributesFormView.FindControl( "ProposalTypeDropDownList" );
                if( ProposalTypeDropDownList != null )
                {
                    if( ProposalTypeDropDownList.SelectedItem != null )
                    {
                        offerDataRelay.EditedOfferContentFront.ProposalTypeId = Int32.Parse( ProposalTypeDropDownList.SelectedItem.Value.ToString() );
                        offerDataRelay.EditedOfferContentFront.ProposalTypeDescription = ProposalTypeDropDownList.SelectedItem.Text;
                    }
                }

                TextBox offerNumberTextBox = ( TextBox )OfferAttributesFormView.FindControl( "OfferNumberTextBox" );
                if( offerNumberTextBox != null )
                {
                    offerDataRelay.EditedOfferContentFront.OfferNumber = offerNumberTextBox.Text;
                }

                DropDownList ContractNumberDropDownList = ( DropDownList )OfferAttributesFormView.FindControl( "ContractNumberDropDownList" );
                if( ContractNumberDropDownList != null )
                {
                    if( ContractNumberDropDownList.SelectedItem != null )
                    {
                        if( ContractNumberDropDownList.SelectedItem.Text.CompareTo( "-- select --" ) != 0 )
                        {
                            offerDataRelay.EditedOfferContentFront.ExtendsContractNumber = ContractNumberDropDownList.SelectedItem.Text;
                        }
                        else  // user may change proposal type and blank out ext cont number
                        {
                            offerDataRelay.EditedOfferContentFront.ExtendsContractNumber = "";
                        }
                    }
                }

                string receivedDate = "";
                string assignmentDate = "";
                string reassignmentDate = "";
                string estimatedCompletionDate = "";
                string actionCompletionDate = "";
                string auditDate = "";
                string returnDate = "";
                DateTime parseDate;
 
                TextBox receivedDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "ReceivedDateTextBox" );
                if( receivedDateTextBox != null )
                    receivedDate = receivedDateTextBox.Text;

                if( receivedDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( receivedDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Received date is not a valid date." );
                        ErrorMessage = "Received date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.DateReceived = parseDate;
                        Session[ "O2Received" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }

                TextBox assignmentDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "AssignmentDateTextBox" );
                if( assignmentDateTextBox != null )
                    assignmentDate = assignmentDateTextBox.Text;

                if( assignmentDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( assignmentDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Assignment date is not a valid date." );
                        ErrorMessage = "Assignment date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.DateAssigned = parseDate;
                        Session[ "O2Assignment" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }
     
                TextBox reassignmentDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "ReassignmentDateTextBox" );
                if( reassignmentDateTextBox != null )
                    reassignmentDate = reassignmentDateTextBox.Text;

                if( reassignmentDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( reassignmentDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Reassignment date is not a valid date." );
                        ErrorMessage = "Reassignment date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.DateReassigned = parseDate;
                        Session[ "O2Reassignment" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }
  

                DropDownList currentActionDropDownList = ( DropDownList )OfferActionFormView.FindControl( "CurrentActionDropDownList" );
                if( currentActionDropDownList != null )
                {
                    if( currentActionDropDownList.SelectedItem != null )
                    {
                        int actionId = Int32.Parse( currentActionDropDownList.SelectedItem.Value.ToString() );
                        offerDataRelay.EditedOfferContentFront.IsEditedOfferCompleted = base.IsOfferActionCompleted( actionId );
                        offerDataRelay.EditedOfferContentFront.ActionId = actionId;
                        offerDataRelay.EditedOfferContentFront.OfferActionDescription = currentActionDropDownList.SelectedItem.Text;
                    }
                }

                TextBox estimatedCompletionDateTextBox = ( TextBox )OfferActionFormView.FindControl( "EstimatedCompletionDateTextBox" );
                if( estimatedCompletionDateTextBox != null )
                    estimatedCompletionDate = estimatedCompletionDateTextBox.Text;

                if( estimatedCompletionDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( estimatedCompletionDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        //throw new Exception( "Estimated completion date is not a valid date." );
                        ErrorMessage = "Estimated completion date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.ExpectedCompletionDate = parseDate;
                        Session[ "O2EstimatedCompletion" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }
  

                TextBox actionCompletionDateTextBox = ( TextBox )OfferActionFormView.FindControl( "ActionCompletionDateTextBox" );
                if( actionCompletionDateTextBox != null )
                    actionCompletionDate = actionCompletionDateTextBox.Text;

                if( actionCompletionDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( actionCompletionDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Action completion date is not a valid date." );
                        ErrorMessage = "Action completion date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.ActionDate = parseDate;
                        Session[ "O2Action" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }
               
                CheckBox auditRequiredCheckBox = ( CheckBox )OfferAuditFormView.FindControl( "AuditRequiredCheckBox" );
                if( auditRequiredCheckBox != null )
                    offerDataRelay.EditedOfferContentFront.AuditIndicator = auditRequiredCheckBox.Checked;


                TextBox auditDateTextBox = ( TextBox )OfferAuditFormView.FindControl( "AuditDateTextBox" );
                if( auditDateTextBox != null )
                    auditDate = auditDateTextBox.Text;

                if( auditDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( auditDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Audit date is not a valid date." );
                        ErrorMessage = "Audit date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.DateSentForPreaward = parseDate;
                        Session[ "O2Audit" ] = parseDate.ToString( "MM/dd/yyyy" );

                    }
                }


                TextBox returnDateTextBox = ( TextBox )OfferAuditFormView.FindControl( "ReturnDateTextBox" );
                if( returnDateTextBox != null )
                    returnDate = returnDateTextBox.Text;

                if( returnDate.Length > 0 )
                {
                    if( DateTime.TryParseExact( returnDate, CMGlobals.ValidGUIDateFormats(), null, System.Globalization.DateTimeStyles.None, out parseDate ) == false )
                    {
                        // throw new Exception( "Return date is not a valid date." );
                        ErrorMessage = "Return date is not a valid date.";
                        bSuccess = false;
                    }
                    else
                    {
                        offerDataRelay.EditedOfferContentFront.DateReturnedToOffice = parseDate;
                        Session[ "O2Return" ] = parseDate.ToString( "MM/dd/yyyy" );
                    }
                }
  
            }
            catch( Exception ex )
            {
                bSuccess = false;
                ErrorMessage = string.Format( "The following exception was encountered validating the offer information: {0}", ex.Message );
            }

            return ( bSuccess );
        }

 

        protected void OfferAttributesFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferAttributesFormView" );

            OfferAttributesFormView.Visible = bVisible;
            OfferAttributesFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferAttributesFormView" );
      
            if( bVisible == true )
            {
                EnableOfferAttributesNonDateEditing( ( FormView )sender );
                EnableOfferAttributesDateEditing( ( FormView )sender );
            }
        }

        protected void OfferActionFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferActionFormView" );

            OfferActionFormView.Visible = bVisible;
            OfferActionFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferActionFormView" );

            if( bVisible == true )
            {
                EnableOfferActionNonDateEditing( ( FormView )sender );
                EnableOfferActionDateEditing( ( FormView )sender );

                // default to received after offer creation
                if( CreatingNewOffer() == true )
                {  
                    SetDefaultOfferAction( "" );
                }
            }
        }

        protected void OfferAuditFormView_OnPreRender( object sender, EventArgs e )
        {
            DocumentControlPresentation documentControlPresentation = ( DocumentControlPresentation )Session[ "DocumentControlPresentation" ];

            bool bVisible = documentControlPresentation.IsFormViewVisible( "OfferAuditFormView" );

            OfferAuditFormView.Visible = bVisible;
            OfferAuditFormView.Enabled = documentControlPresentation.IsFormViewEnabled( "OfferAuditFormView" );

            if( bVisible == true )
            {
                EnableOfferAuditNonDateEditing( ( FormView )sender );
                EnableOfferAuditDateEditing( ( FormView )sender );
            }
        }

        protected void SolicitationDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {

            DropDownList solicitationDropDownList = ( DropDownList )sender;
            ListItem selectedItem = solicitationDropDownList.SelectedItem;
            int selectedSolicitationId = int.Parse( selectedItem.Value );

            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.SolicitationId = selectedSolicitationId;
            }

            // reset other associated drop down lists
        //    OfferAttributesFormView.DataBind();
        }

        protected void CONameDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {

            DropDownList CONameDropDownList = ( DropDownList )sender;
            ListItem selectedItem = CONameDropDownList.SelectedItem;
            int COID = int.Parse( selectedItem.Value );

            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.COID = COID;
            }

            // reset other associated drop down lists
            OfferAttributesFormView.DataBind();
            // reset permissions due to new owner
            OfferActionFormView.DataBind();

            // allow the postback to occur 
            OfferGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        protected void ScheduleDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {

            DropDownList scheduleDropDownList = ( DropDownList )sender;
            ListItem selectedItem = scheduleDropDownList.SelectedItem;
            int selectedScheduleNumber = int.Parse( selectedItem.Value );

            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.ScheduleNumber = selectedScheduleNumber;
            }

            // clear other related selections
            editedOfferContentFront.ExtendsContractNumber = "";

            // reset other associated drop down lists
            OfferAttributesFormView.DataBind();

            // allow the postback to occur 
            OfferGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected void ProposalTypeDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList proposalTypeDropDownList = ( DropDownList )sender;
            ListItem selectedItem = proposalTypeDropDownList.SelectedItem;
            int selectedProposalTypeId = int.Parse( selectedItem.Value );

            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.ProposalTypeId = selectedProposalTypeId;
            }

            OfferDB offerDB = ( OfferDB )Session[ "OfferDB" ];
            if( offerDB != null )
            {
                offerDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                offerDB.MakeConnectionString();

                string prefix = "";
                bool bSuccess = offerDB.GetNewOfferPrefix( selectedProposalTypeId, ref prefix );
                if( bSuccess == true )
                {
                    editedOfferContentFront.OfferNumber = prefix;
                }
                else
                {
                    ShowException( new Exception( string.Format( "The following database error was encountered when defaulting the offer prefix: {0}", offerDB.ErrorMessage ) ) );
                }
            }
            else
            {
                ShowException( new Exception( string.Format( "The following database error was encountered when defaulting the offer prefix: offerDB not set" ) ) );
            }

            // reset other associated drop down lists
            OfferAttributesFormView.DataBind();

            // allow the postback to occur 
            OfferGeneralUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected void VendorNameTextBox_OnTextChanged( object sender, EventArgs e )
        {

            TextBox vendorNameTextBox = ( TextBox )sender;
            string vendorName = vendorNameTextBox.Text;
          
            // save the selection
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            if( editedOfferContentFront != null )
            {
                editedOfferContentFront.VendorName = vendorName.Trim();
            }

            // reset other associated drop down lists
       //     OfferAttributesFormView.DataBind();
        }

        protected void OfferAttributesFormView_OnDataBound( object sender, EventArgs e )
        {
            base.LoadActiveSolicitations();
            DataSet dsSolicitations = ( DataSet )Cache[ SolicitationsCacheName ];
            DropDownList solicitationDropDownList;
            int selectedSolicitationId = -1;

            FormView offerAttributesFormView = ( FormView )sender;

            EditedOfferContent formViewContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

            if( formViewContent != null )
            {
                selectedSolicitationId = formViewContent.SolicitationId;
            }

            if( offerAttributesFormView != null )
            {
                solicitationDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "SolicitationDropDownList" );

                if( solicitationDropDownList != null )
                {
                    solicitationDropDownList.ClearSelection();
                    solicitationDropDownList.Items.Clear();

                    foreach( DataRow row in dsSolicitations.Tables[ OfferDB.SolicitationsTableName ].Rows )
                    {
                        string solicitationIdString = row[ "Solicitation_ID" ].ToString();
                        string solicitationNumber = row[ "Solicitation_Number" ].ToString();

                        solicitationDropDownList.Items.Add( new ListItem( solicitationNumber, solicitationIdString ) );
                    }

                    if( solicitationDropDownList.Items.FindByValue( selectedSolicitationId.ToString() ) != null )
                    {
                        solicitationDropDownList.Items.FindByValue( selectedSolicitationId.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadContractingOfficersForDivision( 1 ); // offers are all FSS
      //      DataSet dsContractingOfficers = ( DataSet )Session[ "ContractingOfficersDataSet" ];
            DropDownList CONameDropDownList;
            int selectedCOID = -1;
            string selectedCOFullName = "";

            if( formViewContent != null )
            {
                selectedCOID = formViewContent.COID;
                selectedCOFullName = formViewContent.ContractingOfficerFullName;
            }

            if( offerAttributesFormView != null )
            {
                CONameDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "CONameDropDownList" );

                if( CONameDropDownList != null )
                {
                    CONameDropDownList.ClearSelection();
                    CONameDropDownList.Items.Clear();

                    foreach( DataRow row in ContractingOfficersDataSet.Tables[ "UserTable" ].Rows )
                    {
                        string COIDString = row[ "CO_ID" ].ToString();
                        string COFullName = row[ "FullName" ].ToString();

                        CONameDropDownList.Items.Add( new ListItem( COFullName, COIDString ) );
                    }

                    if( CONameDropDownList.Items.FindByValue( selectedCOID.ToString() ) != null )
                    {
                        //  -1 matches -- select -- choice
                        CONameDropDownList.Items.FindByValue( selectedCOID.ToString() ).Selected = true;
                    }
                    else // add it for display purposes on historical records
                    {
                        CONameDropDownList.Items.Add( new ListItem( selectedCOFullName, selectedCOID.ToString() ) );
                        CONameDropDownList.Items.FindByValue( selectedCOID.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadSchedulesForDivision( 1 ); // Offers are all FSS
        //    DataSet dsSchedules = ( DataSet )Session[ "SchedulesDataSet" ]; using member variable instead of local 12/8/2014
            DropDownList ScheduleDropDownList;
            int selectedScheduleNumber = -1;

            if( formViewContent != null )
            {
                selectedScheduleNumber = formViewContent.ScheduleNumber;
                    
            }

            if( offerAttributesFormView != null )
            {
                ScheduleDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ScheduleDropDownList" );

                if( ScheduleDropDownList != null )
                {
                    ScheduleDropDownList.ClearSelection();
                    ScheduleDropDownList.Items.Clear();

                    foreach( DataRow row in SchedulesDataSet.Tables[ "SchedulesTable" ].Rows )
                    {
                        string scheduleNumberString = row[ "Schedule_Number" ].ToString();
                        string scheduleName = row[ "Schedule_Name" ].ToString();

                        ScheduleDropDownList.Items.Add( new ListItem( scheduleName, scheduleNumberString ) );
                    }

                    if( ScheduleDropDownList.Items.FindByValue( selectedScheduleNumber.ToString() ) != null )
                    {
                        ScheduleDropDownList.Items.FindByValue( selectedScheduleNumber.ToString() ).Selected = true;
                    }
                }
            }


            DropDownList ProposalTypeDropDownList;
            int selectedProposalTypeId = -1;

            if( formViewContent != null )
            {
                selectedProposalTypeId = formViewContent.ProposalTypeId;
                    
            }
          
            if( offerAttributesFormView != null )
            {
                ProposalTypeDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ProposalTypeDropDownList" );

                if( ProposalTypeDropDownList != null )
                {
                    ProposalTypeDropDownList.ClearSelection();
                    ProposalTypeDropDownList.Items.Clear();

                    foreach( EditedOfferContent.ProposalType pt in formViewContent.ProposalTypeDescriptions )
                    {
                        string proposalTypeIdString = pt.ProposalTypeId.ToString();
                        string proposalTypeDescription = pt.ProposalTypeDescription;

                        ProposalTypeDropDownList.Items.Add( new ListItem( proposalTypeDescription, proposalTypeIdString ) );
                    }

                    if( ProposalTypeDropDownList.Items.FindByValue( selectedProposalTypeId.ToString() ) != null )
                    {
                        ProposalTypeDropDownList.Items.FindByValue( selectedProposalTypeId.ToString() ).Selected = true;
                    }
                }
            }

            base.LoadExtendableContracts( selectedProposalTypeId, selectedScheduleNumber );

            DropDownList ContractNumberDropDownList;
            string selectedExtendableContract = "";

            if( formViewContent != null )
            {
                selectedExtendableContract = formViewContent.ExtendsContractNumber;                    
            }
          
            if( offerAttributesFormView != null )
            {

                ContractNumberDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ContractNumberDropDownList" );

                if( ContractNumberDropDownList != null )
                {
                    ContractNumberDropDownList.ClearSelection();
                    ContractNumberDropDownList.Items.Clear();

                    foreach( DataRow row in ExtendableContractsDataSet.Tables[ ContractDB.ExtendableContractsTableName ].Rows )
                    {
                        string contractNumber = row[ "ContractNumber" ].ToString();

                        ContractNumberDropDownList.Items.Add( new ListItem( contractNumber, contractNumber ) );
                    }

                    if( ContractNumberDropDownList.Items.FindByValue( selectedExtendableContract.ToString() ) != null )
                    {
                        ContractNumberDropDownList.Items.FindByValue( selectedExtendableContract.ToString() ).Selected = true;
                    }                    
                }
            }
        }


        protected void OfferActionFormView_OnDataBound( object sender, EventArgs e )
        {

            FormView offerActionFormView = ( FormView )sender;

            base.LoadOfferActionTypes();

            DropDownList currentActionDropDownList;
            int selectedActionId = -1;

 
            if( offerActionFormView != null )
            {
                EditedOfferContent formViewContent = ( EditedOfferContent )offerActionFormView.DataItem;

                if( formViewContent != null )
                {
                    selectedActionId = formViewContent.ActionId;
                }

                currentActionDropDownList = ( DropDownList )offerActionFormView.FindControl( "currentActionDropDownList" );

                if( currentActionDropDownList != null )
                {
                    currentActionDropDownList.ClearSelection();
                    currentActionDropDownList.Items.Clear();

                    foreach( DataRow row in base.OfferActionTypesDataSet.Tables[ OfferDB.OfferActionTypesTableName ].Rows )
                    {
                        string actionIdString = row[ "ActionId" ].ToString();
                        string actionDescription = row[ "ActionDescription" ].ToString();

                        currentActionDropDownList.Items.Add( new ListItem( actionDescription, actionIdString ) );
                    }

                    if( selectedActionId != -1 )
                    {
                        if( currentActionDropDownList.Items.FindByValue( selectedActionId.ToString() ) != null )
                        {
                            currentActionDropDownList.Items.FindByValue( selectedActionId.ToString() ).Selected = true;
                        }
                    }
                }
            }
        }

        protected void CurrentActionDropDownList_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            int selectedActionId = -1;

            DropDownList currentActionDropDownList = ( DropDownList )sender;

            FormView offerActionFormView = ( FormView )currentActionDropDownList.NamingContainer;

            EditedOfferContent editedOfferContent = OfferDataRelay.EditedOfferContentFront;
                                  
            selectedActionId = Int32.Parse( currentActionDropDownList.SelectedItem.Value.ToString() );

            bool bIsEditedOfferCompleted = base.IsOfferActionCompleted( selectedActionId );

            if( bIsEditedOfferCompleted == true )
            {
                TextBox actionCompletionDateTextBox = ( TextBox )offerActionFormView.FindControl( "ActionCompletionDateTextBox" );
                if( actionCompletionDateTextBox != null )
                {
                    actionCompletionDateTextBox.Text = "";

                    if( editedOfferContent != null )
                    {
                        editedOfferContent.ActionDate = DateTime.MinValue;
                    }
                }
            }

        }

        protected void OfferAuditFormView_OnDataBound( object sender, EventArgs e )
        {
        }
 

        protected void ReceivedDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox receivedDateTextBox = ( TextBox )sender;

            FormView offerAttributesFormView;

            if( receivedDateTextBox != null )
            {
                offerAttributesFormView = ( FormView )receivedDateTextBox.NamingContainer;

                if( offerAttributesFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

                    DateTime receivedDate = editedOfferContent.DateReceived;

                    if( receivedDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        receivedDateTextBox.Text = "";
                    }
                    else
                    {
                        receivedDateTextBox.Text = receivedDate.ToString( "MM/dd/yyyy" );
                        Session[ "O2Received" ] = receivedDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void AssignmentDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox assignmentDateTextBox = ( TextBox )sender;

            FormView offerAttributesFormView;

            if( assignmentDateTextBox != null )
            {
                offerAttributesFormView = ( FormView )assignmentDateTextBox.NamingContainer;

                if( offerAttributesFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

                    DateTime dateAssigned = editedOfferContent.DateAssigned;

                    if( dateAssigned.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        assignmentDateTextBox.Text = "";
                    }
                    else
                    {
                        assignmentDateTextBox.Text = dateAssigned.ToString( "MM/dd/yyyy" );
                        Session[ "O2Assignment" ] = dateAssigned.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void ReassignmentDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox reassignmentDateTextBox = ( TextBox )sender;

            FormView offerAttributesFormView;

            if( reassignmentDateTextBox != null )
            {
                offerAttributesFormView = ( FormView )reassignmentDateTextBox.NamingContainer;

                if( offerAttributesFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

                    DateTime dateReassigned = editedOfferContent.DateReassigned;

                    if( dateReassigned.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        reassignmentDateTextBox.Text = "";
                    }
                    else
                    {
                        reassignmentDateTextBox.Text = dateReassigned.ToString( "MM/dd/yyyy" );
                        Session[ "O2Reassignment" ] = dateReassigned.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void EstimatedCompletionDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox estimatedCompletionDateTextBox = ( TextBox )sender;

            FormView offerActionFormView;

            if( estimatedCompletionDateTextBox != null )
            {
                offerActionFormView = ( FormView )estimatedCompletionDateTextBox.NamingContainer;

                if( offerActionFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerActionFormView.DataItem;

                    DateTime estimatedCompletionDate = editedOfferContent.ExpectedCompletionDate;

                    if( estimatedCompletionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        estimatedCompletionDateTextBox.Text = "";
                    }
                    else
                    {
                        estimatedCompletionDateTextBox.Text = estimatedCompletionDate.ToString( "MM/dd/yyyy" );
                        Session[ "O2EstimatedCompletion" ] = estimatedCompletionDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void ActionCompletionDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox actionCompletionDateTextBox = ( TextBox )sender;

            FormView offerActionFormView;

            if( actionCompletionDateTextBox != null )
            {
                offerActionFormView = ( FormView )actionCompletionDateTextBox.NamingContainer;

                if( offerActionFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerActionFormView.DataItem;

                    DateTime actionDate = editedOfferContent.ActionDate;

                    if( actionDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        actionCompletionDateTextBox.Text = "";
                    }
                    else
                    {
                        actionCompletionDateTextBox.Text = actionDate.ToString( "MM/dd/yyyy" );
                        Session[ "O2Action" ] = actionDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void AuditDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox auditDateTextBox = ( TextBox )sender;

            FormView offerAuditFormView;

            if( auditDateTextBox != null )
            {
                offerAuditFormView = ( FormView )auditDateTextBox.NamingContainer;

                if( offerAuditFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAuditFormView.DataItem;

                    DateTime auditDate = editedOfferContent.DateSentForPreaward;

                    if( auditDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        auditDateTextBox.Text = "";
                    }
                    else
                    {
                        auditDateTextBox.Text = auditDate.ToString( "MM/dd/yyyy" );
                        Session[ "O2Audit" ] = auditDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

        protected void ReturnDateTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox returnDateTextBox = ( TextBox )sender;

            FormView offerAuditFormView;

            if( returnDateTextBox != null )
            {
                offerAuditFormView = ( FormView )returnDateTextBox.NamingContainer;

                if( offerAuditFormView != null )
                {
                    EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAuditFormView.DataItem;

                    DateTime returnDate = editedOfferContent.DateReturnedToOffice;

                    if( returnDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) == 0 )
                    {
                        returnDateTextBox.Text = "";
                    }
                    else
                    {
                        returnDateTextBox.Text = returnDate.ToString( "MM/dd/yyyy" );
                        Session[ "O2Return" ] = returnDate.ToString( "MM/dd/yyyy" );
                    }
                }
            }
        }

  

        protected void OfferAttributesDatesFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "OfferAttributesDatesFormView" );
        }

        protected void OfferAttributesFormView_OnChange( object sender, EventArgs e )
        {
  //          SetDirtyFlag( "OfferAttributesFormView" );
        }

        protected void OfferActionFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "OfferActionFormView" );
        }

        protected void OfferAuditFormView_OnChange( object sender, EventArgs e )
        {
   //         SetDirtyFlag( "OfferAuditFormView" );
        }

        // dates and other attributes are handled independently of each other because the dates have their own overriding access points
        protected void EnableOfferAttributesNonDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerAttributesFormView = sender;

            currentPage = offerAttributesFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                bool bCanEdit = false;
                if( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit )
                    bCanEdit = true;

                TextBox vendorNameTextBox = ( TextBox )offerAttributesFormView.FindControl( "VendorNameTextBox" );
                if( vendorNameTextBox != null )
                {
                    vendorNameTextBox.Enabled = bCanEdit;
                }

                DropDownList solicitationDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "SolicitationDropDownList" );
                if( solicitationDropDownList != null )
                {
                    solicitationDropDownList.Enabled = bCanEdit;
                }

                bool bCanAssign = false;
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferAssignment ) == true )
                    bCanAssign = true;

                DropDownList CONameDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "CONameDropDownList" );
                if( CONameDropDownList != null )
                {
                    CONameDropDownList.Enabled = bCanAssign;
                }

                DropDownList scheduleDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ScheduleDropDownList" );
                if( scheduleDropDownList != null )
                {
                    scheduleDropDownList.Enabled = bCanEdit;
                }

                TextBox offerNumberTextBox = ( TextBox )offerAttributesFormView.FindControl( "OfferNumberTextBox" );
                if( offerNumberTextBox != null )
                {
                    offerNumberTextBox.Enabled = bCanEdit;
                }

                DropDownList proposalTypeDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ProposalTypeDropDownList" );
                if( proposalTypeDropDownList != null )
                {
                    proposalTypeDropDownList.Enabled = bCanEdit;
                }

                DropDownList contractNumberDropDownList = ( DropDownList )offerAttributesFormView.FindControl( "ContractNumberDropDownList" );
                if( contractNumberDropDownList != null )
                {
                    contractNumberDropDownList.Enabled = bCanEdit;
                }
            }
        }

        // date ranges from original code base
        //<asp:SqlDataSource ID="AssignmentDateDataSource" runat="server" 
        //SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 90) AND (Date <= { fn NOW() }) ORDER BY Date DESC">                        
        //<asp:SqlDataSource ID="ReassignDataSource" runat="server" 
        //SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 14) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
        //<asp:SqlDataSource ID="ActionCompDataSource" runat="server" 
        //SelectCommand="SELECT CONVERT(varchar(10),Date,101) as Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 14) AND (Date <= { fn NOW() }) ORDER BY Date DESC">
        //<asp:SqlDataSource ID="ExpectDateDataSource" runat="server" 
        //SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() }) AND (Date <= { fn NOW() } + 90) ORDER BY Date">
        //// used for both audit and return dates                            
        //<asp:SqlDataSource ID="AuditDateDataSource" runat="server" 
        //SelectCommand="SELECT Date FROM tlkup_Dates WHERE (Date >= { fn NOW() } - 7) AND (Date <= { fn NOW() }) ORDER BY Date DESC">

        // the following constants are used with DateTime.Now.AddDays( C ) to calculate the particular date ( where C is the constant )
        public const int OFFERRECEIVEDMINDATE = -14;
        public const int OFFERASSIGNMENTMINDATE = -30;   // changed from 90 to 30 3/31/2017 see Shawn Davis email
        public const int OFFERREASSIGNMENTMINDATE = -14;
        public const int OFFERACTIONMINDATE = -14;
        public const int OFFERESTIMATEDCOMPLETIONMAXDATE = 90;
        public const int OFFERAUDITORRETURNMINDATE = -7;

        // each offer date is authorized separately
        // note the AdministerOfferDates access point has been restored for administrative use   
        // note as of 4/2017 update, ownership does not allow date editing.  Date permissions must be allocated even if owner.
        protected void EnableOfferAttributesDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerAttributesFormView = sender;

            bool bIsOfferReceived = false;
            if( offerAttributesFormView != null )
            {
                EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

                if( editedOfferContent != null )
                {
                    DateTime receivedDate = editedOfferContent.DateReceived;

                    if( receivedDate != null )
                    {
                        if( receivedDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) != 0 )
                        {
                            bIsOfferReceived = true;
                            Session[ "IsOfferReceived" ] = true;
                        }
                    }
                }
            }

            bool bIsOfferAssigned = false;
            if( offerAttributesFormView != null )
            {
                EditedOfferContent editedOfferContent = ( EditedOfferContent )offerAttributesFormView.DataItem;

                if( editedOfferContent != null )
                {
                    DateTime assignedDate = editedOfferContent.DateAssigned;

                    if( assignedDate != null )
                    {
                        if( assignedDate.ToShortDateString().CompareTo( DateTime.MinValue.ToShortDateString() ) != 0 )
                        {
                            bIsOfferAssigned = true;
                            Session[ "IsOfferAssigned" ] = true;                                
                        }
                    }
                }
            }

            currentPage = offerAttributesFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            
            // Access Points
            //OfferReceivedDate,    was not limited in the prior release, using today - 14 for now
            //OfferAssignmentDate,   from today - 90 through today
            //OfferReassignmentDate,  from today - 14 through today
            //OfferUnlimitedDateRange

            bool bUnlimited = false;
            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferUnlimitedDateRange ) == true  )
                {
                    bUnlimited = true;
                }
            }
            else  // same state as creating new offer i.e., no current document
            {
                bUnlimited = true;
            }

            DateTime offerReceivedDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERRECEIVEDMINDATE ).ToShortDateString() );
            DateTime offerAssignmentDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERASSIGNMENTMINDATE ).ToShortDateString() );
            DateTime offerReassignmentDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERREASSIGNMENTMINDATE ).ToShortDateString() );
            DateTime todayAsMaxOrMin = Convert.ToDateTime( DateTime.Now.ToShortDateString() );

            // create image button scripts
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2ReceivedDateButtonOnClickScript", GetDateButtonScript( "O2Received", bUnlimited, offerReceivedDateMin, todayAsMaxOrMin ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2AssignmentDateButtonOnClickScript", GetDateButtonScript( "O2Assignment", bUnlimited, offerAssignmentDateMin, todayAsMaxOrMin ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2ReassignmentDateButtonOnClickScript", GetDateButtonScript( "O2Reassignment", bUnlimited, offerReassignmentDateMin, todayAsMaxOrMin ), true );


            ImageButton receivedDateImageButton = ( ImageButton )offerAttributesFormView.FindControl( "OfferReceivedDateImageButton" );
            TextBox receivedDateTextBox = ( TextBox )offerAttributesFormView.FindControl( "ReceivedDateTextBox" );

            if( Session[ "IsOfferReceived" ] != null )
                bIsOfferReceived = ( bool )Session[ "IsOfferReceived" ];

            if( receivedDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferReceivedDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        if( bIsOfferReceived == false || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                        {
                            receivedDateImageButton.Visible = true;
                            receivedDateTextBox.Enabled = true;
                        }
                        else
                        {
                            receivedDateImageButton.Visible = false;
                            receivedDateTextBox.Enabled = false;
                        }
                    }
                    else
                    {
                        receivedDateImageButton.Visible = false;
                        receivedDateTextBox.Enabled = false;
                    }
                }
                else  // same state as creating new offer i.e., no current document
                {
                    receivedDateImageButton.Visible = true;
                    receivedDateTextBox.Enabled = true;
                }

            }

            ImageButton offerAssignmentDateImageButton = ( ImageButton )offerAttributesFormView.FindControl( "OfferAssignmentDateImageButton" );
            TextBox assignmentDateTextBox = ( TextBox )offerAttributesFormView.FindControl( "AssignmentDateTextBox" );

            if( Session[ "IsOfferAssigned" ] != null )
                bIsOfferAssigned = ( bool )Session[ "IsOfferAssigned" ];

            if( offerAssignmentDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferAssignmentDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        if( bIsOfferAssigned == false || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                        {
                            offerAssignmentDateImageButton.Visible = true;
                            assignmentDateTextBox.Enabled = true;
                        }
                        else
                        {
                            offerAssignmentDateImageButton.Visible = false;
                            assignmentDateTextBox.Enabled = false;
                        }
                    }
                    else
                    {
                        offerAssignmentDateImageButton.Visible = false;
                        assignmentDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerAssignmentDateImageButton.Visible = true;
                    assignmentDateTextBox.Enabled = true;
                }
            }

            ImageButton offerReassignmentDateImageButton = ( ImageButton )offerAttributesFormView.FindControl( "OfferReassignmentDateImageButton" );
            TextBox reassignmentDateTextBox = ( TextBox )offerAttributesFormView.FindControl( "ReassignmentDateTextBox" );

            if( offerReassignmentDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferReassignmentDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        offerReassignmentDateImageButton.Visible = true;
                        reassignmentDateTextBox.Enabled = true;
                    }
                    else
                    {
                        offerReassignmentDateImageButton.Visible = false;
                        reassignmentDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerReassignmentDateImageButton.Visible = true;
                    reassignmentDateTextBox.Enabled = true;
                }
            }

        }

        protected void EnableOfferActionNonDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerActionFormView = sender;

            currentPage = offerActionFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                bool bCanEdit = false;
                if( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit )
                    bCanEdit = true;

                DropDownList currentActionDropDownList = ( DropDownList )offerActionFormView.FindControl( "CurrentActionDropDownList" );
                if( currentActionDropDownList != null )
                {
                    currentActionDropDownList.Enabled = bCanEdit;
                }
            }
        }

        // each offer date is authorized separately
        protected void EnableOfferActionDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerActionFormView = sender;

            currentPage = offerActionFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            // Access Points
            //OfferActionDate       from today - 14 through today
            //OfferEstimatedCompletionDate,   from today through today+90

            bool bUnlimited = false;
            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferUnlimitedDateRange ) == true )
                {
                    bUnlimited = true;
                }
            }
            else // same state as creating new offer i.e., no current document
            {
                bUnlimited = true;  
            }

            DateTime offerActionDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERACTIONMINDATE ).ToShortDateString() );
            DateTime offerEstimatedCompletionDateMax = Convert.ToDateTime( DateTime.Now.AddDays( OFFERESTIMATEDCOMPLETIONMAXDATE ).ToShortDateString() );
            DateTime todayAsMaxOrMin = Convert.ToDateTime( DateTime.Now.ToShortDateString() );

            // create image button scripts
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2EstimatedCompletionDateButtonOnClickScript", GetDateButtonScript( "O2EstimatedCompletion", bUnlimited, todayAsMaxOrMin, offerEstimatedCompletionDateMax ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2ActionDateButtonOnClickScript", GetDateButtonScript( "O2Action", bUnlimited, offerActionDateMin, todayAsMaxOrMin ), true );
 
            ImageButton offerEstimatedCompletionDateImageButton = ( ImageButton )offerActionFormView.FindControl( "OfferEstimatedCompletionDateImageButton" );
            TextBox estimatedCompletionDateTextBox = ( TextBox )offerActionFormView.FindControl( "EstimatedCompletionDateTextBox" );

            if( offerEstimatedCompletionDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferEstimatedCompletionDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        offerEstimatedCompletionDateImageButton.Visible = true;
                        estimatedCompletionDateTextBox.Enabled = true;
                    }
                    else
                    {
                        offerEstimatedCompletionDateImageButton.Visible = false;
                        estimatedCompletionDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerEstimatedCompletionDateImageButton.Visible = true;
                    estimatedCompletionDateTextBox.Enabled = true;
                }
            }

            ImageButton offerActionDateImageButton = ( ImageButton )offerActionFormView.FindControl( "OfferActionDateImageButton" );
            TextBox actionCompletionDateTextBox = ( TextBox )offerActionFormView.FindControl( "ActionCompletionDateTextBox" );

            if( offerActionDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferActionDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        offerActionDateImageButton.Visible = true;
                        actionCompletionDateTextBox.Enabled = true;
                    }
                    else
                    {
                        offerActionDateImageButton.Visible = false;
                        actionCompletionDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerActionDateImageButton.Visible = true;
                    actionCompletionDateTextBox.Enabled = true;
                }
            }
        }

        protected void EnableOfferAuditNonDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerAuditFormView = sender;

            currentPage = offerAuditFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            if( currentDocument != null )
            {
                bool bCanEdit = false;
                if( currentDocument.EditStatus == CurrentDocument.EditStatuses.CanEdit )
                    bCanEdit = true;

                CheckBox auditRequiredCheckBox = ( CheckBox )offerAuditFormView.FindControl( "AuditRequiredCheckBox" );
                if( auditRequiredCheckBox != null )
                {
                    auditRequiredCheckBox.Enabled = bCanEdit;
                }
            }
        }

        // each offer date is authorized separately
        protected void EnableOfferAuditDateEditing( FormView sender )
        {
            Page currentPage;
            FormView offerAuditFormView = sender;

            currentPage = offerAuditFormView.Page;

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            bool bUnlimited = false;
            if( currentDocument != null )
            {
                if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferUnlimitedDateRange ) == true )
                {
                    bUnlimited = true;
                }
            }
            else
            {
                bUnlimited = true;
            }

            DateTime offerAuditDateMin = Convert.ToDateTime( DateTime.Now.AddDays( OFFERAUDITORRETURNMINDATE ).ToShortDateString() );
            DateTime todayAsMaxOrMin = Convert.ToDateTime( DateTime.Now.ToShortDateString() );

            // Access Points
            //OfferAuditDate,   from today - 7 through today
            //OfferReturnDate   from today - 7 through today

            // create image button scripts
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2AuditDateButtonOnClickScript", GetDateButtonScript( "O2Audit", bUnlimited, offerAuditDateMin, todayAsMaxOrMin ), true );
            ScriptManager.RegisterStartupScript( currentPage, currentPage.GetType(), "O2ReturnDateButtonOnClickScript", GetDateButtonScript( "O2Return", bUnlimited, offerAuditDateMin, todayAsMaxOrMin ), true );

            ImageButton offerAuditDateImageButton = ( ImageButton )offerAuditFormView.FindControl( "OfferAuditDateImageButton" );
            TextBox auditDateTextBox = ( TextBox )offerAuditFormView.FindControl( "AuditDateTextBox" );

            if( offerAuditDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferAuditDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        offerAuditDateImageButton.Visible = true;
                        auditDateTextBox.Enabled = true;
                    }
                    else
                    {
                        offerAuditDateImageButton.Visible = false;
                        auditDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerAuditDateImageButton.Visible = true;
                    auditDateTextBox.Enabled = true;
                }
            }

            ImageButton offerReturnDateImageButton = ( ImageButton )offerAuditFormView.FindControl( "OfferReturnDateImageButton" );
            TextBox returnDateTextBox = ( TextBox )offerAuditFormView.FindControl( "ReturnDateTextBox" );

            if( offerReturnDateImageButton != null )
            {
                if( currentDocument != null )
                {
                    if( currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.OfferReturnDate ) == true || currentDocument.IsAccessAllowed( BrowserSecurity2.AccessPoints.AdministerOfferDates ) == true )
                    {
                        offerReturnDateImageButton.Visible = true;
                        returnDateTextBox.Enabled = true;
                    }
                    else
                    {
                        offerReturnDateImageButton.Visible = false;
                        returnDateTextBox.Enabled = false;
                    }
                }
                else // same state as creating new offer i.e., no current document
                {
                    offerReturnDateImageButton.Visible = true;
                    returnDateTextBox.Enabled = true;
                }
            }
        }

        public string GetDateButtonScript( string dateTypeString, bool bUnlimitedDateRange, DateTime minAllowedDate, DateTime maxAllowedDate )
        {
            string defaultDateString = "";

            if( Session[ dateTypeString ] != null )
            {
                defaultDateString = Session[ dateTypeString ].ToString();
            }
            else
            {
                defaultDateString = DateTime.Today.ToShortDateString();
            }

            string minAllowedDateString = minAllowedDate.ToShortDateString();
            string maxAllowedDateString = maxAllowedDate.ToShortDateString();

            string script = String.Format( "function On{0}DateButtonClick() {{ window.open('Calendar.aspx?InitialDate={1}&DateType={2}&Unlimited={3}&MinAllowedDate={4}&MaxAllowedDate={5}','Calendar','toolbar=no,menubar=no,resizable=no,scrollbars=no,statusbar=0,location=0,width=250,height=340,left=660,top=300'); return false;}}", dateTypeString, defaultDateString, dateTypeString, ( ( bUnlimitedDateRange == true ) ? 1 : 0 ), minAllowedDateString, maxAllowedDateString );

            return ( script );
        }

        public void SetDefaultOfferAction( string dateTypeString )
        {
            DateTime actionDate = DateTime.Today;
            int actionId = -1;

            if( dateTypeString.Contains( "O2Received" ) == true )
            {
                if( Session[ "O2Received" ] != null )
                {
                    actionDate = DateTime.Parse( Session[ "O2Received" ].ToString() );
                    actionId = 16;
                }
            }
            else if( dateTypeString.Contains( "O2Assignment" ) == true )
            {
                if( Session[ "O2Assignment" ] != null )
                {
                    actionDate = DateTime.Parse( Session[ "O2Assignment" ].ToString() );
                    actionId = 1;
                }
            }
            else // default before any dates entered ( dateTypeString = "" )
            {
                actionId = 16;  // same as received, as requested by customer prior to go-live 2/2016
            }

            bool bSelected = false;
            if( actionId != -1 )
            {
                DropDownList currentActionDropDownList = ( DropDownList )OfferActionFormView.FindControl( "CurrentActionDropDownList" );
                if( currentActionDropDownList != null )
                {
                    if( currentActionDropDownList.SelectedItem == null )
                    {
                        if( currentActionDropDownList.Items.FindByValue( actionId.ToString() ) != null )
                        {
                            currentActionDropDownList.ClearSelection();
                            currentActionDropDownList.Items.FindByValue( actionId.ToString() ).Selected = true;
                            bSelected = true;
                        }
                    }
                    // customer has requested default to received, so this case will not occur. Leaving the following notes for future reference:
                    // if it's on Acquisition Review, then this is the same as no selection, if the offer is new
                    // $$$ should redo the list of actions to include a null selection as the default rather than the first in the list
                    else if( currentActionDropDownList.SelectedItem.Value.CompareTo( "15" ) == 0 && base.DocumentEditorType == DocumentEditorTypes.NewOffer )
                    {
                        if( currentActionDropDownList.Items.FindByValue( actionId.ToString() ) != null )
                        {
                            currentActionDropDownList.ClearSelection();
                            currentActionDropDownList.Items.FindByValue( actionId.ToString() ).Selected = true;
                            bSelected = true;
                        }
                    }

                    if( bSelected == true )
                    {
                        TextBox actionCompletionDateTextBox = ( TextBox )OfferActionFormView.FindControl( "ActionCompletionDateTextBox" );
                        if( actionCompletionDateTextBox != null )
                        {
                            actionCompletionDateTextBox.Text = actionDate.ToShortDateString();
                        }
                    }
                }
            }
        }

        public void RefreshDate( string dateTypeString )
        {
            DateTime displayDate;

            if( dateTypeString.Contains( "O2Received" ) == true )
            {
                TextBox receivedDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "ReceivedDateTextBox" );
                if( Session[ "O2Received" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Received" ].ToString() );
                    receivedDateTextBox.Text = displayDate.ToShortDateString();

                    SetDefaultOfferAction( dateTypeString );
                }
                else
                {
                    receivedDateTextBox.Text = "";
                }
            }


            if( dateTypeString.Contains( "O2Assignment" ) == true )
            {
                TextBox assignmentDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "AssignmentDateTextBox" );
                if( Session[ "O2Assignment" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Assignment" ].ToString() );
                    assignmentDateTextBox.Text = displayDate.ToShortDateString();

                    SetDefaultOfferAction( dateTypeString );
                }
                else
                {
                    assignmentDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "O2Reassignment" ) == true )
            {
                TextBox reassignmentDateTextBox = ( TextBox )OfferAttributesFormView.FindControl( "ReassignmentDateTextBox" );
                if( Session[ "O2Reassignment" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Reassignment" ].ToString() );
                    reassignmentDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    reassignmentDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "O2EstimatedCompletion" ) == true )
            {
                TextBox estimatedCompletionDateTextBox = ( TextBox )OfferActionFormView.FindControl( "EstimatedCompletionDateTextBox" );
                if( Session[ "O2EstimatedCompletion" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2EstimatedCompletion" ].ToString() );
                    estimatedCompletionDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    estimatedCompletionDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "O2Action" ) == true )
            {
                TextBox actionCompletionDateTextBox = ( TextBox )OfferActionFormView.FindControl( "ActionCompletionDateTextBox" );
                if( Session[ "O2Action" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Action" ].ToString() );
                    actionCompletionDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    actionCompletionDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "O2Audit" ) == true )
            {
                TextBox auditDateTextBox = ( TextBox )OfferAuditFormView.FindControl( "AuditDateTextBox" );
                if( Session[ "O2Audit" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Audit" ].ToString() );
                    auditDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    auditDateTextBox.Text = "";
                }
            }

            if( dateTypeString.Contains( "O2Return" ) == true )
            {
                TextBox returnDateTextBox = ( TextBox )OfferAuditFormView.FindControl( "ReturnDateTextBox" );
                if( Session[ "O2Return" ] != null )
                {
                    displayDate = DateTime.Parse( Session[ "O2Return" ].ToString() );
                    returnDateTextBox.Text = displayDate.ToShortDateString();
                }
                else
                {
                    returnDateTextBox.Text = "";
                }
            }
        }
    }
}