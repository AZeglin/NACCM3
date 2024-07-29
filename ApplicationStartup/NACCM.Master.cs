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
using System.Web.Caching;
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

namespace VA.NAC.CM.ApplicationStartup
{

    public partial class NACCM : BaseMasterPage
    {
        private const int EXPIRATIONDATECOLUMN = 7;

        private const int NOTIFICATIONGRIDVIEWROWHEIGHTESTIMATE = 48;
        private const int PersonalizedNotificationIdFieldNumber = 4;  //$$$ +

        private const int CONTRACTLISTGRIDVIEWROWHEIGHTESTIMATE = 48;

        //public UpdatePanel MasterUpdatePanel
        //{
        //    get
        //    {
        //        return ( this.MainUpdatePanel );
        //    }
        //}

        //public UpdatePanelEventProxy ContentEventProxy
        //{
        //    get
        //    {
        //        return ( this.ContentUpdatePanelEventProxy );
        //    }
        //}

        public ScriptManager MasterPageScriptManager
        {
            get
            {
                return ( this.CMMasterScriptManager );
            }
        }

        private const string SaveDocumentChangesOverrideKeyName = "SaveDocumentChangesUserOverride";

        protected override void Page_Init( object sender, EventArgs e )
        {
            base.Page_Init( sender, e );

            Page.EnableEventValidation = false;
            

   //         MainRightPanel.Attributes.Remove( "display" );
   //         MainRightPanel.Attributes.Add( "display", "none" );
   //         MainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

        }

        public Panel TheMainRightPanel
        {
            get
            {
                return ( this.MainRightPanel );
            }
        }

        public UpdatePanelEventProxy TheMainRightPanelUpdatePanelEventProxy
        {
            get
            {
                return ( this.MainRightPanelUpdatePanelEventProxy );
            }
        }


        private bool _bRightPanelOpenState = false;

        public bool RightPanelOpenState
        {
            get { return _bRightPanelOpenState; }
            set { _bRightPanelOpenState = value; }
        }



        private string _notificationPanelState = "open";

        public string NotificationPanelState
        {
            get { return _notificationPanelState; }
            set { _notificationPanelState = value; }
        }

        private string _personalizedContractListPanelState = "open";

        public string PersonalizedContractListPanelState
        {
            get { return _personalizedContractListPanelState; }
            set { _personalizedContractListPanelState = value; }
        }

        private DataSet _personalizedNotificationDataSet = null;

        public DataSet PersonalizedNotificationDataSet
        {
            get { return _personalizedNotificationDataSet; }
            set { _personalizedNotificationDataSet = value; }
        }

        private DataView _personalizedNotificationDataView = null;

        public DataView PersonalizedNotificationDataView
        {
            get { return _personalizedNotificationDataView; }
            set { _personalizedNotificationDataView = value; }
        }

        private DataSet _personalizedContractListDataSet = null;

        public DataSet PersonalizedContractListDataSet
        {
            get { return _personalizedContractListDataSet; }
            set { _personalizedContractListDataSet = value; }
        }

        private DataView _personalizedContractListDataView = null;

        public DataView PersonalizedContractListDataView
        {
            get { return _personalizedContractListDataView; }
            set { _personalizedContractListDataView = value; }
        }

        private void InitRedirectFlag()
        {
            bool bRedirectedFromMenu = false;
            if( Session[ "RedirectedFromMenu" ] != null )
            {
                string redirectedFromMenu = Session[ "RedirectedFromMenu" ].ToString();
                bRedirectedFromMenu = bool.Parse( redirectedFromMenu );
            }
            else
            {
                Session[ "RedirectedFromMenu" ] = false.ToString();
                bRedirectedFromMenu = false;
            }

   //         string logMsg = String.Format( "In Master Page_Load() IsPostBack={0} IsRedirectedFromMenu={1}", Page.IsPostBack.ToString(), bRedirectedFromMenu.ToString() );
   //         ShowException( new Exception( logMsg ) );
        }

        public bool IsRedirected()
        {
            bool bRedirectedFromMenu = false;
            if( Session[ "RedirectedFromMenu" ] != null )
            {
                string redirectedFromMenu = Session[ "RedirectedFromMenu" ].ToString();
                bRedirectedFromMenu = bool.Parse( redirectedFromMenu );
            }
            return ( bRedirectedFromMenu );
        }

        public void SetRedirected( bool bIsRedirected )
        {
            Session[ "RedirectedFromMenu" ] = bIsRedirected.ToString();
        }

        public bool IsPostbackFromPopupClose()
        {
            bool bPostbackFromPopupClose = false;
            if( Session[ "PostbackFromPopupClose" ] != null )
            {
                string postbackFromPopupClose = Session[ "PostbackFromPopupClose" ].ToString();
                bPostbackFromPopupClose = bool.Parse( postbackFromPopupClose );
            }
            return ( bPostbackFromPopupClose );
        }

        public void SetPostbackFromPopupClose( bool bPostbackFromPopupClose )
        {
            Session[ "PostbackFromPopupClose" ] = bPostbackFromPopupClose.ToString();
        }

        

        protected void Page_Load( object sender, EventArgs e )
        {
            string MultipleInstancePreventionIdString = "";

            InitRedirectFlag();

            Page.Title = "NACCM";

            string eTarget = "";
            string eArgument = "";
            if( Page.IsPostBack == true )
            {
                eTarget = Request.Params[ "__EVENTTARGET" ].ToString();
                eArgument = Request.Params[ "__EVENTARGUMENT" ].ToString();
              //  MsgBox.Alert( string.Format( "Postback with target={0} argument={1}", eTarget, eArgument ) );
            }

            if( Page.IsPostBack == false && IsRedirected() == false )
            {
                //Session.Clear();
                //Session.Abandon();
                //Session[ "Hack" ] = string.Empty;
                HiddenField SessionIdHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "SessionIdHiddenField" );
                SessionIdHiddenField.Value = Session.SessionID;

                Session[ "CurrentSelectedMainMenuItemValue" ] = null;
                Session[ "PersonalizedNotificationDataSet" ] = null;
                Session[ "PersonalizedContractListDataSet" ] = null;  // added 4/29 $$$
                Session[ "CurrentDocumentIsChanging" ] = null;
                Session[ "RequestedNextDocument" ] = null;
                Session[ "ContractSearchDataDirtyFlag" ] = null;  // moved up from contract search master 
                Session[ "ContractListFilterParms" ] = null;
                Session[ "CurrentDocumentMenuItemEnabled" ] = false;  // no current document at startup
                Session[ "MainMenuCreateContractPermissions" ] = null;
                Session[ "MainMenuCreateOfferPermissions" ] = null;
                Session[ "PriorSelectedMainMenuItemValue" ] = null;

                //          Session[ "DataInPersonalPaneHasChanged" ] = null;

                // this plus calling hideRightPanel(false); client side causes the right panel to start open
                HiddenField OneTimeInitHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "OneTimeInitHiddenField" );
                OneTimeInitHiddenField.Value = "true";
                RightPanelToggleCheckBox.Enabled = true;  // $$$ for some reason setting to true doesn't require an updatepanel update whereas setting to false does
                RightPanelToggleCheckBox.Checked = true;  // switch to false and remove afforementioned client code to start panel closed 
                
                //HiddenField PreventMultipleInstancesHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "PreventMultipleInstancesHiddenField" );
                //PreventMultipleInstancesHiddenField.Value = new Guid().ToString();
                //Session[ "MultipleInstancePreventionId" ] = PreventMultipleInstancesHiddenField.Value;
                
                UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.Login, "", VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.Undefined );
                log.LogUserActivity();
            }
            else
            {
                HiddenField OneTimeInitHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "OneTimeInitHiddenField" );
                OneTimeInitHiddenField.Value = "false";
                MainHeaderUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                // after the second instance is launched, the session will no longer match the hidden field
                //            MultipleInstancePreventionIdString = ( string )Session[ "MultipleInstancePreventionId" ];
                //            HiddenField PreventMultipleInstancesHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "PreventMultipleInstancesHiddenField" );
                //            if( MultipleInstancePreventionIdString.CompareTo( PreventMultipleInstancesHiddenField.Value.ToString() ) != 0 )
                //            {
                ////                Response.End();
                //            }
            }

            if( Session[ "FailsafeNoUser" ] != null )
            {
                if( Session[ "FailsafeNoUser" ].ToString().CompareTo( "true" ) == 0 )
                {
                    Response.End();  // this blocks refresh from the "no user" error screen
                }
            }

            if( Session[ "MultipleInstances" ] != null )
            {
                if( Session[ "MultipleInstances" ].ToString().CompareTo( "true" ) == 0 )
                {
                    Response.Redirect( "~/409.aspx" );                
                }
            }

            // personal pane is also reloaded if user has created a new contract or updated sales or modified contract dates
            // but not for rebate changes as the notification is keyed off of a rebate term end date
            if( Page.IsPostBack == false && IsRedirected() == false )
            {
                LoadPersonalizedContractList( true );
                LoadPersonalizedNotifications( true );
            }
            else
            {
                LoadPersonalizedContractList( false );
                LoadPersonalizedNotifications( false );
            }

            BindPersonalizedNotifications();
            BindPersonalizedContractList();
          
            // allow the highlight postback to occur - this was moved from a depricated function on 7/6/2016, however its effect is not tested
            //    MainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

            bool bDoNotRestoreHighlightingDueToCancel = false;

            // check if dialog related postback
            if( Page.IsPostBack == true )
            {
                string serverConfirmationDialogResults = "";
                HiddenField ServerConfirmationDialogResultsHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ServerConfirmationDialogResults" );

                if( ServerConfirmationDialogResultsHiddenField != null )
                {
                    serverConfirmationDialogResults = ServerConfirmationDialogResultsHiddenField.Value;

                    // save and then move on
                    if( serverConfirmationDialogResults.CompareTo( MsgBox.GetPopupWindowYesResult() ) == 0 )  /// $$$ this could be the problem - direct save is not possible from search scrn. user must manually return to doc and save manually.  This is only if user STILL on current doc and moves to different doc such as selecting a contract on the  "personalized area"
                    {
                        if( Session[ "RequestedNextDocument" ] != null )
                        {
                            RequestedNextDocument requestedNextDocument = ( RequestedNextDocument )Session[ "RequestedNextDocument" ];

                            if( HandleUserSaveRequest() == true )
                            {
                                // clear session before redirect
                                Session[ "RequestedNextDocument" ] = null;

                                // currently, there is no provision to proceed with creation of a contract from offer if the offer was
                                // found to be dirty.  The user must reclick the create button on the offer award screen after saving the offer.
                                // redirect
                                if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.Contract )
                                {
                                    MoveOnToDifferentContract( ( ( RequestedNextContract )requestedNextDocument ).ContractId, ( ( RequestedNextContract )requestedNextDocument ).ContractNumber, ( ( RequestedNextContract )requestedNextDocument ).ScheduleNumber );
                                }
                                else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.Offer )
                                {
                                    RequestedNextOffer requestedNextOffer = ( RequestedNextOffer )requestedNextDocument;
                                    MoveOnToDifferentOffer( requestedNextOffer.OfferId, requestedNextOffer.ScheduleNumber, requestedNextOffer.ScheduleName, requestedNextOffer.VendorName, requestedNextOffer.DateReceived, requestedNextOffer.DateAssigned, requestedNextOffer.OwnerId, requestedNextOffer.ContractingOfficerName, requestedNextOffer.ContractNumber, requestedNextOffer.ContractId, requestedNextOffer.IsOfferCompleted, requestedNextOffer.OfferNumber, requestedNextOffer.ProposalTypeId, requestedNextOffer.ExtendsContractNumber, requestedNextOffer.ExtendsContractId );
                                }
                                else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.ShortOffer )
                                {
                                    RequestedNextOffer requestedNextOffer = ( RequestedNextOffer )requestedNextDocument;
                                    MoveOnToDifferentOffer( requestedNextOffer.OfferId, requestedNextOffer.ScheduleNumber );
                                }
                                // these 2 cases added 7/28/2016
                                else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.NewOffer )
                                {
                                    MoveOnToNewOffer();
                                }
                                else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.NewContract )
                                {
                                    MoveOnToNewContract( -1, -1, RequestedNextDocument.DocumentChangeRequestSources.CreateContract );
                                }
                            }
                        }
                        else
                        {
                            ShowException( new Exception( "Requested Next Document object was null on postback." ) );
                        }

                        ServerConfirmationDialogResultsHiddenField.Value = "";
                    }

                    // dont save, just move on
                    else if( serverConfirmationDialogResults.CompareTo( MsgBox.GetPopupWindowNoResult() ) == 0 )
                    {
                        if( Session[ "RequestedNextDocument" ] != null  )
                        {
                            RequestedNextDocument requestedNextDocument = ( RequestedNextDocument )Session[ "RequestedNextDocument" ];

                            // clear session before redirect
                            Session[ "RequestedNextDocument" ] = null;

                            // redirect
                            if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.Contract )
                            {
                                MoveOnToDifferentContract( ( ( RequestedNextContract )requestedNextDocument ).ContractId, ( ( RequestedNextContract )requestedNextDocument ).ContractNumber, ( ( RequestedNextContract )requestedNextDocument ).ScheduleNumber );
                            }
                            else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.Offer )
                            {
                                RequestedNextOffer requestedNextOffer = ( RequestedNextOffer )requestedNextDocument;
                                MoveOnToDifferentOffer( requestedNextOffer.OfferId, requestedNextOffer.ScheduleNumber, requestedNextOffer.ScheduleName, requestedNextOffer.VendorName, requestedNextOffer.DateReceived, requestedNextOffer.DateAssigned, requestedNextOffer.OwnerId, requestedNextOffer.ContractingOfficerName, requestedNextOffer.ContractNumber, requestedNextOffer.ContractId, requestedNextOffer.IsOfferCompleted, requestedNextOffer.OfferNumber, requestedNextOffer.ProposalTypeId, requestedNextOffer.ExtendsContractNumber, requestedNextOffer.ExtendsContractId );
                            }
                            else if (requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.ShortOffer)
                            {                           
                                RequestedNextOffer requestedNextOffer = ( RequestedNextOffer )requestedNextDocument;
                                MoveOnToDifferentOffer( requestedNextOffer.OfferId, requestedNextOffer.ScheduleNumber );
                            }
                            // these 2 cases added 7/28/2016
                            else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.NewOffer )
                            {
                                MoveOnToNewOffer();
                            }
                            else if( requestedNextDocument.NextDocumentType == RequestedNextDocument.NextDocumentTypes.NewContract )
                            {
                                MoveOnToNewContract( -1, -1, RequestedNextDocument.DocumentChangeRequestSources.CreateContract );
                            }
                        }

                        ServerConfirmationDialogResultsHiddenField.Value = "";
                    }
                    // cancel request
                    else if( serverConfirmationDialogResults.CompareTo( MsgBox.GetPopupWindowCancelResult() ) == 0 )
                    {
                        if( Session[ "RequestedNextDocument" ] != null )
                        {
                            RequestedNextDocument requestedNextDocument = ( RequestedNextDocument )Session[ "RequestedNextDocument" ];

                            // clear session before redirect
                            Session[ "RequestedNextDocument" ] = null;

                            ServerConfirmationDialogResultsHiddenField.Value = "";

                            // must undo main menu selection  $$$ added this block 7/28/2016
                            string currentMenuValue = "";  
                            string priorMenuValue = "";

                            if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
                            {
                                currentMenuValue = Session[ "CurrentSelectedMainMenuItemValue" ].ToString();
                            }

                            if( Session[ "PriorSelectedMainMenuItemValue" ] != null )
                            {
                                priorMenuValue = Session[ "PriorSelectedMainMenuItemValue" ].ToString();
                            }
                           
                            if( currentMenuValue.CompareTo( priorMenuValue ) != 0 )
                            {
                                Session[ "CurrentSelectedMainMenuItemValue" ] = priorMenuValue;
                                MainHeaderUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
                            }

                            // must undo client side grid row selection and revert back to prior choice
                            RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest = requestedNextDocument.SourceOfDocumentChangeRequest;

                            if( sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.PersonalizedNotificationGridView || sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.PersonalizedContractListGridView )
                            {
                                string sourceGridName = "";
                                if( sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.PersonalizedContractListGridView )
                                {
                                    sourceGridName = "PersonalizedContractListGridView";
                                }
                                else
                                {
                                    sourceGridName = "PersonalizedNotificationGridView";
                                }
                                string revertNotificationAndContractHighlightingScript = string.Format( "revertNotificationAndContractHighlighting( '{0}' );", sourceGridName );
                                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RevertNotificationAndContractHighlightingScript", revertNotificationAndContractHighlightingScript, true ); // runs after controls established

                                // allow the enabled postback to occur 
                                MainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );

                                bDoNotRestoreHighlightingDueToCancel = true;
                            }
                        }

                    }                 
                }
            }

            // preserve states on postback
            if( Page.IsPostBack == true && IsPostbackFromPopupClose() == false )
            {
                // preserve size info on postback
                //HiddenField ScreenHeightHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ScreenHeightHiddenField" );
                //HiddenField ScreenWidthHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ScreenWidthHiddenField" );

                //if( ScreenHeightHiddenField != null && ScreenWidthHiddenField != null )
                //{
                //    int screenHeight = int.Parse( ScreenHeightHiddenField.Value.ToString() );
                //    int screenWidth = int.Parse( ScreenWidthHiddenField.Value.ToString() );

                //    CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                //    if( cmGlobals != null )
                //    {
                //        cmGlobals.ClientScreenHeight = screenHeight;
                //        cmGlobals.ClientScreenWidth = screenWidth;
                //    }
                //}

                // preserve panel states on postback
                HiddenField RightPanelControllerStateHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "RightPanelControllerStateHiddenField" );
                HiddenField NotificationStateHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "NotificationStateHiddenField" );
                HiddenField ContractListStateHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ContractListStateHiddenField" );
                HiddenField RightPanelToggleEnabledHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "RightPanelToggleEnabledHiddenField" );

                if( RightPanelControllerStateHiddenField != null && NotificationStateHiddenField != null && ContractListStateHiddenField != null && RightPanelToggleEnabledHiddenField != null )
                {
                    Session[ "RightPanelControllerState" ] = RightPanelControllerStateHiddenField.Value;
                    Session[ "NotificationState" ] = NotificationStateHiddenField.Value;
                    Session[ "ContractListState" ] = ContractListStateHiddenField.Value;
                    Session[ "RightPanelToggleEnabled" ] = RightPanelToggleEnabledHiddenField.Value;
                }

                // preserve personalized grid selections on postback $$$$
                HiddenField notificationPanelScrollPosHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "notificationPanelScrollPos" );
                HiddenField highlightedNotificationRowHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "highlightedNotificationRow" );
                HiddenField highlightedNotificationRowOriginalColorHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "highlightedNotificationRowOriginalColor" );

                if( notificationPanelScrollPosHiddenField != null && highlightedNotificationRowHiddenField != null && highlightedNotificationRowOriginalColorHiddenField != null )
                {
                    Session[ "notificationPanelScrollPosState" ] = notificationPanelScrollPosHiddenField.Value;
                    Session[ "highlightedNotificationRowState" ] = highlightedNotificationRowHiddenField.Value;
                    Session[ "highlightedNotificationRowOriginalColorState" ] = highlightedNotificationRowOriginalColorHiddenField.Value;
                }
                
                HiddenField contractPanelScrollPosHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "contractPanelScrollPos" );
                HiddenField highlightedPersonalizedContractRowHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "highlightedPersonalizedContractRow" );
                HiddenField highlightedPersonalizedContractRowOriginalColorHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "highlightedPersonalizedContractRowOriginalColor" );

                if( contractPanelScrollPosHiddenField != null && highlightedPersonalizedContractRowHiddenField != null && highlightedPersonalizedContractRowOriginalColorHiddenField != null )
                {
                    Session[ "contractPanelScrollPosState" ] = contractPanelScrollPosHiddenField.Value;
                    Session[ "highlightedPersonalizedContractRowState" ] = highlightedPersonalizedContractRowHiddenField.Value;
                    Session[ "highlightedPersonalizedContractRowOriginalColorState" ] = highlightedPersonalizedContractRowOriginalColorHiddenField.Value;
                }              
            }

            // restore states on redirect and on all control postbacks (if open, personalized area was closing after postback)
            if(( Page.IsPostBack == false && IsRedirected() == true ) || ( Page.IsPostBack == true )) // && IsPostbackFromPopupClose() == true )) 
            {
                // restore screen size info
                //int screenHeight = -1;
                //int screenWidth = -1;
                //CMGlobals cmGlobals = ( CMGlobals )Session[ "CMGlobals" ];
                //if( cmGlobals != null )
                //{
                //    screenHeight = cmGlobals.ClientScreenHeight;
                //    screenWidth = cmGlobals.ClientScreenWidth;
                //}

                //string restoreScreenSizeScript = string.Format( "restoreClientScreenResolutionInfo( {0}, {1} );", screenHeight, screenWidth );
                //ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreScreenSizeScript", restoreScreenSizeScript, true ); 
                
                // restore panel states on redirect
                // default state
                string rightPanelControllerState = "false";  
                string notificationState = "open";
                string contractListState = "open";
                string rightPanelToggleEnabled = "true";

                if( Session[ "RightPanelControllerState" ] != null )
                    rightPanelControllerState = ( string )Session[ "RightPanelControllerState" ];
                if( Session[ "NotificationState" ] != null )
                    notificationState = ( string )Session[ "NotificationState" ];
                if( Session[ "ContractListState" ] != null )
                    contractListState = ( string )Session[ "ContractListState" ];
                if( Session[ "RightPanelToggleEnabled" ] != null )
                    rightPanelToggleEnabled = ( string )Session[ "RightPanelToggleEnabled" ];

                _bRightPanelOpenState = bool.Parse( rightPanelControllerState );
                RightPanelToggleCheckBox.Checked = _bRightPanelOpenState;
                RightPanelToggleCheckBox.Enabled = bool.Parse( rightPanelToggleEnabled );

                string restoreMainPanelsScript = string.Format( "RestoreMainPanels( '{0}', '{1}', '{2}', '{3}' );", rightPanelControllerState, notificationState, contractListState, rightPanelToggleEnabled );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestoreMainPanelsScript", restoreMainPanelsScript, true ); // runs after controls established

                // restore personalized grid selection on redirect 
                // default values
                string notificationPanelScrollPos = "-1";
                string highlightedNotificationRow = "-1";
                string highlightedNotificationRowOriginalColor = "norm";
                string contractPanelScrollPos = "-1";
                string highlightedPersonalizedContractRow = "-1";
                string highlightedPersonalizedContractRowOriginalColor = "norm";

                if( Session[ "notificationPanelScrollPosState" ] != null )
                    notificationPanelScrollPos = ( string )Session[ "notificationPanelScrollPosState" ];
                if( Session[ "highlightedNotificationRowState" ] != null )
                    highlightedNotificationRow = ( string )Session[ "highlightedNotificationRowState" ];
                if( Session[ "highlightedNotificationRowOriginalColorState" ] != null )
                    highlightedNotificationRowOriginalColor = ( string )Session[ "highlightedNotificationRowOriginalColorState" ];

                if( Session[ "contractPanelScrollPosState" ] != null )
                    contractPanelScrollPos = ( string )Session[ "contractPanelScrollPosState" ];
                if( Session[ "highlightedPersonalizedContractRowState" ] != null )
                    highlightedPersonalizedContractRow = ( string )Session[ "highlightedPersonalizedContractRowState" ];
                if( Session[ "highlightedPersonalizedContractRowOriginalColorState" ] != null )
                    highlightedPersonalizedContractRowOriginalColor = ( string )Session[ "highlightedPersonalizedContractRowOriginalColorState" ];

                if( bDoNotRestoreHighlightingDueToCancel == false )
                {
                    string restorePersonalizedGridSelectionScript = string.Format( "RestorePersonalizedGridSelection( {0}, {1}, {2}, {3} );", notificationPanelScrollPos, highlightedNotificationRow, contractPanelScrollPos, highlightedPersonalizedContractRow );
                    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "RestorePersonalizedGridSelectionScript", restorePersonalizedGridSelectionScript, true ); // runs after controls established
                }

                     
            }

            // right panel
            RightPanelToggleCheckBox.Attributes.Add( "onclick", "toggleRightPanel();" );

            // clear flag which was set before each redirection
            SetRedirected( false );  // reset the flag
            // clear the flag which was set before popup close
            SetPostbackFromPopupClose( false );

           
        }

       
        protected void MainRightPanel_OnPreRender( object sender, EventArgs e )
        {
           // Thread.Sleep( 3000 );
        }

        private void SetCurrentDocument( int selectedContractId, string selectedContractNumber, int scheduleNumber )
        {
            CurrentDocument currentDocument = null;
            currentDocument = new CurrentDocument( selectedContractId, selectedContractNumber, scheduleNumber, ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ] );
            // new static function that sets a flag and raises an event
            CurrentDocument.SetCurrentDocument( currentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.SelectedContractFromSearch );
        
            currentDocument.LookupCurrentDocument();

            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            browserSecurity.SetDocumentEditStatus( currentDocument );

            DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
            Session[ "DocumentControlPresentation" ] = documentControlPresentation;

            EnableCurrentDocumentMenuItem( true );
        }

        // offer
        private void SetCurrentDocument( int offerId, int scheduleNumber, string scheduleName, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, string ownerName, string contractNumber, int contractId, bool bIsOfferCompleted, string offerNumber, int proposalTypeId, string extendsContractNumber, int extendsContractId )
        {
            CurrentDocument currentDocument = null;
            currentDocument = new CurrentDocument( offerId, scheduleNumber, scheduleName, vendorName, dateReceived, dateAssigned, ownerId, ownerName, contractNumber, contractId, bIsOfferCompleted, offerNumber, proposalTypeId, extendsContractNumber, extendsContractId, ( OfferDB )Session[ "OfferDB" ], ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ] );
            // new static function that sets a flag and raises an event
            CurrentDocument.SetCurrentDocument( currentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.SelectedOfferFromSearch );

            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            browserSecurity.SetDocumentEditStatus( currentDocument );

            DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
            Session[ "DocumentControlPresentation" ] = documentControlPresentation;

            EnableCurrentDocumentMenuItem( true );
        }

        // offer with less initial info
        private void SetCurrentDocument( int offerId, int scheduleNumber )
        {
            CurrentDocument currentDocument = null;
            currentDocument = new CurrentDocument( offerId, scheduleNumber, ( OfferDB )Session[ "OfferDB" ], ( ContractDB )Session[ "ContractDB" ], ( DrugItemDB )Session[ "DrugItemDB" ], ( ItemDB )Session[ "ItemDB" ] );
            
            // new static function that sets a flag and raises an event
            CurrentDocument.SetCurrentDocument( currentDocument, CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.SelectedOfferFromSearch );

            currentDocument.LookupCurrentDocument();

            BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            browserSecurity.SetDocumentEditStatus( currentDocument );

            DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( currentDocument );
            Session[ "DocumentControlPresentation" ] = documentControlPresentation;

            EnableCurrentDocumentMenuItem( true );
        }

        // used during create document and create offer
        private void ClearCurrentDocument( CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes currentDocumentUpdateType )
        {
            CurrentDocument.SetCurrentDocument( null, currentDocumentUpdateType );

            EnableCurrentDocumentMenuItem( false );

            DocumentControlPresentation documentControlPresentation = new DocumentControlPresentation( ( currentDocumentUpdateType == CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.CreateContract ) ? true : false );
            Session[ "DocumentControlPresentation" ] = documentControlPresentation;
        }

        private void EnableCurrentDocumentMenuItem( bool bEnable )
        {
            Session[ "CurrentDocumentMenuItemEnabled" ] = bEnable;
        }

        // $$$ this is faulty logic.  if its selected then it's enabled?  it can be enabled without being selected. where is this used?  
        private bool IsCurrentDocumentMenuItemEnabled()
        {
            bool bEnabled = false;
            if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
            {
                string currentSelectedMainMenuItemValue = ( string )Session[ "CurrentSelectedMainMenuItemValue" ].ToString();
                if( currentSelectedMainMenuItemValue.CompareTo( "CurrentDocument" ) == 0 )
                    bEnabled = true;
            }

            return( bEnabled );
        }

        // args.SelectedMenuItemValue is item.ItemValue
        private void MainMenu_EdgeMenuCommand( EdgeMenu theMenu, EdgeMenuCommandEventArgs args )
        {
            bool bSuccess = true;

            // if menu choice is changing, if moving off of current document, save the current tab to the front object
            if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
            {
                string currentMenuValue = Session[ "CurrentSelectedMainMenuItemValue" ].ToString();
                Session[ "PriorSelectedMainMenuItemValue" ] = currentMenuValue; // save in case selection is aborted due to dirty
                if( currentMenuValue.CompareTo( args.SelectedMenuItemValue ) != 0 )
                {
                    // log the access
                    UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.MenuSelect, string.Format( "OldMenuChoice={0} NewMenuChoice={1}", currentMenuValue, args.SelectedMenuItemValue ), -1, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.MainMenuSelect );
                    log.LogUserActivity();

                    if( currentMenuValue.CompareTo( "CurrentDocument" ) == 0 )
                    {
                        bSuccess = ShortSave();
                    }
                }
            }

            // cancel until the error on the current page is fixed
            if( bSuccess == false )
                return;

            switch( args.SelectedMenuItemValue )
            {
                case "CurrentDocument":
                    // intercept if no current document
                    if( Session[ "CurrentDocument" ] != null )
                    {
                        CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];
                        
                        // contract
                        if( currentDocument.DocumentType != CurrentDocument.DocumentTypes.Offer )
                        {
                            string contractDestinationUrl = "~/ContractGeneral.aspx"; // default

                            // re-select the most recent selected sub-menu
                            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
                            {
                                string currentSubMenuValue = Session[ "CurrentSelectedLevel2MenuItemValue" ].ToString();
                                contractDestinationUrl = ContractView.GetUrlFromMenuItemValue( currentSubMenuValue, (( currentDocument.Division == CurrentDocument.Divisions.National ) ? true : false ) );
                            }

                            Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                            SetRedirected( true );
                            EnableRightPanelToggleCheckBoxAfterRedirect( true );
                            Response.Redirect( contractDestinationUrl );
                        }
                        else // offer
                        {
                            string offerDestinationUrl = "~/OfferGeneral.aspx"; // default

                            // re-select the most recent selected sub-menu
                            if( Session[ "CurrentSelectedLevel2MenuItemValue" ] != null )
                            {
                                string currentSubMenuValue = Session[ "CurrentSelectedLevel2MenuItemValue" ].ToString();
                                offerDestinationUrl = OfferView.GetUrlFromMenuItemValue( currentSubMenuValue );
                            }

                            Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                            SetRedirected( true );
                            EnableRightPanelToggleCheckBoxAfterRedirect( true );
                            Response.Redirect( offerDestinationUrl );
                        }
                    }
                    break;
                case "RecentlyViewed":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    Response.Redirect( "~/UserRecentDocumentsBody.aspx" );
                    break;
                case "ContractSearch":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    Response.Redirect( "~/ContractSelectBody.aspx" );                    
                    break;
                case "OfferSearch":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    Response.Redirect( "~/OfferSelectBody.aspx" );
                    break;
                case "Reports":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    Session[ "CurrentSelectedLevel2MenuItemValue" ] = "Contracts";  // default to first report list tab
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    Response.Redirect( "~/ReportSelectBody.aspx" );
                    break;
                case "CreateContract":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    CreateContract( -1, -1, RequestedNextDocument.DocumentChangeRequestSources.CreateContract );
                    break;
                case "CreateOffer":                    
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    SetRedirected( true );
                    EnableRightPanelToggleCheckBoxAfterRedirect( false );
                    CreateOffer( RequestedNextDocument.DocumentChangeRequestSources.CreateOffer );
                    break;
                case "CCST":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue; 
                    // not a redirect - opens in another tab
                    // Response.Redirect( "http://www.va.gov/nac/index.cfm?action=search&template=search_menu", false );                  
                    break;
                case "ECMS":
                    Session[ "CurrentSelectedMainMenuItemValue" ] = args.SelectedMenuItemValue;
                    // not a redirect - opens in another tab
                    // https://vaww.aams.ecms.va.gov/AAMS_Production/default.aspx
                    break;
            }
            
            // added 4/2/2021
            MainHeaderUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private void EnableRightPanelToggleCheckBoxAfterRedirect( bool bEnable )
        {
            RightPanelToggleCheckBox.Enabled = bEnable;
            Session[ "RightPanelToggleEnabled" ] = bEnable.ToString();

            // if disabling, then also hide
            if( bEnable == false )
            {
                RightPanelToggleCheckBox.Checked = false;
                Session[ "RightPanelControllerState" ] = "false";
            }
        }

        // not used - seems like it is wtf
        private void EnableRightPanelToggleCheckBox( bool bEnable )
        {
            RightPanelToggleCheckBox.Enabled = bEnable;
            Session[ "RightPanelToggleEnabled" ] = bEnable.ToString();

            // if disabling, then also hide
            if( bEnable == false )
            {
                Session[ "RightPanelControllerState" ] = "false";
                string hideRightPanelScript = String.Format( "hideRightPanel(true);" );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "HideRightPanelScript", hideRightPanelScript, true ); // runs after controls established
            }

            // allow the enabled postback to occur 
            MainHeaderUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }


        protected void MainMenu_OnInit( object sender, EventArgs e )
        {
            EdgeMenu theMenu = ( EdgeMenu )sender;
            theMenu.EdgeMenuCommand += MainMenu_EdgeMenuCommand;

            theMenu.AddItem( "Current Document", "CurrentDocument" );
            theMenu.AddItem( "Recently Viewed", "RecentlyViewed" );
            theMenu.AddItem( "Contract Search", "ContractSearch" );
            theMenu.AddItem( "Offer Search", "OfferSearch" );
            theMenu.AddItem( "Reports", "Reports" );
            theMenu.AddItem( "Create Contract", "CreateContract" );
            theMenu.AddItem( "Create Offer", "CreateOffer" );
            theMenu.AddItem( "CCST", "CCST", "_new", "https://www.vendorportal.ecms.va.gov/nac/" );  // "http://www.va.gov/nac/" ); chgd 6/1/2023 
            theMenu.AddItem( "eCMS", "ECMS", "_new", "https://dvagov.sharepoint.com/sites/VACOOALALF/SitePages/Custom.aspx" );
        }



        protected void MainMenu_OnPreRender( object sender, EventArgs e )
        {
            EdgeMenu theMenu = ( EdgeMenu )sender;
            theMenu.Visible = false;

            //Request.Browser.Adapters.Clear()

            // disable creation based on user permissions
            BrowserSecurity2 bs = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];
            bool bCreateContract = false;
            bool bCreateOffer = false;

            if( bs != null )
            {
                // called frequently, so save to session
                if( Session[ "MainMenuCreateContractPermissions" ] != null )
                {
                    bCreateContract = bool.Parse( Session[ "MainMenuCreateContractPermissions" ].ToString() );
                }
                else
                {
                    bCreateContract = bs.CheckPermissions( BrowserSecurity2.AccessPoints.CreateContract );
                    Session[ "MainMenuCreateContractPermissions" ] = bCreateContract.ToString();
                }


                if( Session[ "MainMenuCreateOfferPermissions" ] != null )
                {
                    bCreateOffer = bool.Parse( Session[ "MainMenuCreateOfferPermissions" ].ToString() );
                }
                else
                {
                    bCreateOffer = bs.CheckPermissions( BrowserSecurity2.AccessPoints.CreateOffer );
                    Session[ "MainMenuCreateOfferPermissions" ] = bCreateOffer.ToString();
                }
            }

            MainMenu.MenuItems.Enable( "CreateContract", bCreateContract );
            MainMenu.MenuItems.Enable( "CreateOffer", bCreateOffer );
            
            if( Session[ "CurrentDocumentMenuItemEnabled" ] != null )
            {
                bool bCurrentDocumentEnabled = ( bool )Session[ "CurrentDocumentMenuItemEnabled" ];
                MainMenu.MenuItems.Enable( "CurrentDocument", bCurrentDocumentEnabled );
            }
           
            // re-highlight the selected menu item            
            if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
            {
                string menuItemValue = ( string )Session[ "CurrentSelectedMainMenuItemValue" ];
                MainMenu.MenuItems.Highlight( menuItemValue );
            }

            theMenu.Visible = true;
        }

        protected void CMMasterScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs  e )
        {
            String errorMsg = "";
        
            if( e.Exception.Data[ "CMMasterErrorMessage" ] != null )
            {
                errorMsg = String.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "CMMasterErrorMessage" ] );
            }
            else
            {
                errorMsg = String.Format("The following error was encountered during async postback: {0}", e.Exception.Message);
            }

            CMMasterScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void ShowException( Exception ex )
        {
            MasterPage master = Page.Master;
            if( master != null )
            {
                MsgBox msgBox = ( MsgBox )master.FindControl( "MsgBox" );

                if( msgBox != null )
                {
                    msgBox.ShowErrorFromUpdatePanelAsync( Page, ex );
                }

            }
        }

        public void CreateContract( int offerId, int scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            RequestedNextNewContract requestedNextNewContract = new RequestedNextNewContract();
            requestedNextNewContract.OfferId = offerId;
            requestedNextNewContract.ScheduleNumber = scheduleNumber;
            requestedNextNewContract.SourceOfDocumentChangeRequest = sourceOfDocumentChangeRequest;

            Session[ "RequestedNextDocument" ] = requestedNextNewContract;

            // allow the user to close out the current document first
            if( CheckForDirtyDocument( sourceOfDocumentChangeRequest ) == false )
            {
                // clear session before redirect
                Session[ "RequestedNextDocument" ] = null;

                // redirect
                MoveOnToNewContract( offerId, scheduleNumber, sourceOfDocumentChangeRequest );
            }
        }

        public void ViewSelectedContract( int contractId, string contractNumber, int scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            RequestedNextContract requestedNextContract = new RequestedNextContract();
            requestedNextContract.ContractNumber = contractNumber;
            requestedNextContract.ContractId = contractId;
            requestedNextContract.ScheduleNumber = scheduleNumber;
            requestedNextContract.SourceOfDocumentChangeRequest = sourceOfDocumentChangeRequest;

            Session[ "RequestedNextDocument" ] = requestedNextContract;

            // allow the user to close out the current document first
            if( CheckForDirtyDocument( sourceOfDocumentChangeRequest) == false )
            {
                // clear session before redirect
                Session[ "RequestedNextDocument" ] = null;

                // redirect
                MoveOnToDifferentContract( contractId, contractNumber, scheduleNumber );
            }
        }

        // this is also called when a newly created contract is saved
        public void MoveOnToDifferentContract( int contractId, string contractNumber, int scheduleNumber )
        {
            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "CurrentDocument";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = "General";

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            EnableRightPanelToggleCheckBox( true );
 
            SetCurrentDocument( contractId, contractNumber, scheduleNumber );

            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            // log the access
            UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.ViewDocument, contractNumber, currentDocument.ContractId, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ViewContract );
            log.LogDocumentAccess();

            Response.Redirect( "~/ContractGeneral.aspx" );

        }

        public void MoveOnToNewContract( int offerId, int scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources documentChangeRequestSource )
        {
            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "CreateContract";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = null;

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            EnableRightPanelToggleCheckBox( false );

            // clear out the current document
            ClearCurrentDocument( CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.CreateContract );

            string isContractCreatedFromOffer = "N";
            if( documentChangeRequestSource == RequestedNextDocument.DocumentChangeRequestSources.OfferAwardFormViewCreateContract )
                isContractCreatedFromOffer = "Y";

            Response.Redirect( string.Format( "~/CreateContract2.aspx?ScheduleNumber={0}&OfferId={1}&IsCreatedFromOffer={2}", scheduleNumber, offerId, isContractCreatedFromOffer ));
        }

        public void ViewSelectedOffer( int offerId, int scheduleNumber, string scheduleName, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, string ownerName, string contractNumber, int contractId, bool bIsOfferCompleted, string offerNumber, int proposalTypeId, string extendsContractNumber, int extendsContractId, RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            RequestedNextOffer requestedNextOffer = new RequestedNextOffer();
            requestedNextOffer.OfferId = offerId;
            requestedNextOffer.ScheduleNumber = scheduleNumber;
            requestedNextOffer.ScheduleName = scheduleName;
            requestedNextOffer.VendorName = vendorName;
            requestedNextOffer.DateReceived = dateReceived;
            requestedNextOffer.DateAssigned = dateAssigned;
            requestedNextOffer.OwnerId = ownerId;
            requestedNextOffer.ContractingOfficerName = ownerName;
            requestedNextOffer.ContractNumber = contractNumber;
            requestedNextOffer.ContractId = contractId;
            requestedNextOffer.IsOfferCompleted = bIsOfferCompleted;
            requestedNextOffer.OfferNumber = offerNumber;
            requestedNextOffer.ProposalTypeId = proposalTypeId;
            requestedNextOffer.ExtendsContractNumber = extendsContractNumber;
            requestedNextOffer.ExtendsContractId = extendsContractId;
            requestedNextOffer.SourceOfDocumentChangeRequest = sourceOfDocumentChangeRequest;

            Session[ "RequestedNextDocument" ] = requestedNextOffer;

            // allow the user to close out the current document first
            if( CheckForDirtyDocument( sourceOfDocumentChangeRequest ) == false )
            {
                // clear session before redirect
                Session[ "RequestedNextDocument" ] = null;

                MoveOnToDifferentOffer( offerId, scheduleNumber, scheduleName, vendorName, dateReceived, dateAssigned, ownerId, ownerName, contractNumber, contractId, bIsOfferCompleted, offerNumber, proposalTypeId, extendsContractNumber, extendsContractId );
            }
        }

        public void ViewSelectedOffer( int offerId, int scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            RequestedNextOffer requestedNextOffer = new RequestedNextOffer( true );
            requestedNextOffer.OfferId = offerId;
            requestedNextOffer.ScheduleNumber = scheduleNumber;
            requestedNextOffer.SourceOfDocumentChangeRequest = sourceOfDocumentChangeRequest;

            Session[ "RequestedNextDocument" ] = requestedNextOffer;

            // allow the user to close out the current document first
            if( CheckForDirtyDocument( sourceOfDocumentChangeRequest ) == false )
            {
                // clear session before redirect
                Session[ "RequestedNextDocument" ] = null;

                MoveOnToDifferentOffer( offerId, scheduleNumber );
            }
        }

        public void MoveOnToDifferentOffer( int offerId, int scheduleNumber, string scheduleName, string vendorName, DateTime dateReceived, DateTime dateAssigned, int ownerId, string ownerName, string contractNumber, int contractId, bool bIsOfferCompleted, string offerNumber, int proposalTypeId, string extendsContractNumber, int extendsContractId )
        {
            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "CurrentDocument";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = "General";

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            EnableRightPanelToggleCheckBox( true );

            SetCurrentDocument( offerId, scheduleNumber, scheduleName, vendorName, dateReceived, dateAssigned, ownerId, ownerName, contractNumber, contractId, bIsOfferCompleted, offerNumber, proposalTypeId, extendsContractNumber, extendsContractId );

            // log the access
            UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.ViewDocument, offerNumber, offerId, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ViewOffer );
            log.LogDocumentAccess();

            Response.Redirect( "~/OfferGeneral.aspx" );
        }

        public void MoveOnToDifferentOffer( int offerId, int scheduleNumber )
        {
            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "CurrentDocument";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = "General";

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            EnableRightPanelToggleCheckBox( true );

            SetCurrentDocument( offerId, scheduleNumber );

            // needed for OfferNumber for logging
            CurrentDocument currentDocument = null;
            currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

            // log the access
            UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.ViewDocument, currentDocument.OfferNumber, currentDocument.OfferId, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ViewOffer );
            log.LogDocumentAccess();

            Response.Redirect( "~/OfferGeneral.aspx" );
        }

        public void CreateOffer( RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            RequestedNextNewOffer requestedNextNewOffer = new RequestedNextNewOffer();
            requestedNextNewOffer.SourceOfDocumentChangeRequest = sourceOfDocumentChangeRequest;

            Session[ "RequestedNextDocument" ] = requestedNextNewOffer;

            // allow the user to close out the current document first
            if( CheckForDirtyDocument( sourceOfDocumentChangeRequest ) == false )
            {
                // clear session before redirect
                Session[ "RequestedNextDocument" ] = null;

                // redirect
                MoveOnToNewOffer();
            }
        }

        public void MoveOnToNewOffer()
        {
            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "CreateOffer";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = "General";

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            EnableRightPanelToggleCheckBox( false );

            // clear out the current document
            ClearCurrentDocument( CurrentDocumentUpdateEventArgs.CurrentDocumentUpdateTypes.CreateOffer );

            Response.Redirect( "~/OfferGeneral.aspx" );
        }


        internal void ViewStartScreen()
        {
            // clear session before redirect
            Session[ "RequestedNextDocument" ] = null;

            // indicates that a document is being viewed and which menu items should be highlighted
            Session[ "CurrentSelectedMainMenuItemValue" ] = "Start";
            Session[ "CurrentSelectedLevel2MenuItemValue" ] = "Start";

            // indicates that although its a post, it was a result of a user action that resulted in a redirect
            SetRedirected( true );

            
            EnableRightPanelToggleCheckBox( true );

            Response.Redirect( "~/Start.aspx" );
        }

        private bool IsDirtyDocumentVisible( RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            bool bVisible = false;

            if( sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.AssociatedBPAContractsGridView ||   // curr doc == Y
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.AssociatedContractsGridView ||    //SBA  curr doc == Y
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.ContractGeneralParentContractFormView ||  // curr doc == Y              
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.NewContractAfterSave || // curr doc == Y
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.NewOfferAfterSave ||    // curr doc == Y
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.OfferAwardFormViewCreateContract ||   // curr doc == Y
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.OfferAwardFormViewViewContract )       // curr doc == Y
              
            {
                bVisible = true;
            }
            else if( sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.ContractSearchGridView ||
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.OfferSearchGridView ||
                sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.UserRecentDocumentsGridView )
            {
                bVisible = false;
            }
            else
            {

            }

            //sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.PersonalizedContractListGridView ||
            //sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.PersonalizedNotificationGridView ||
            //sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.CreateContract ||
            //sourceOfDocumentChangeRequest == RequestedNextDocument.DocumentChangeRequestSources.CreateOffer ||

            return ( bVisible );
        }

        protected bool CheckForDirtyDocument( RequestedNextDocument.DocumentChangeRequestSources sourceOfDocumentChangeRequest )
        {
            bool bDirty = false;


            CurrentDocument currentDocument = ( CurrentDocument )HttpContext.Current.Session[ "CurrentDocument" ];
            if( currentDocument != null )
            {
                if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.Offer )
                {
                    EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];

                    if( editedOfferContentFront != null )
                    {
                        if( editedOfferContentFront.IsDocumentDirty() == true )
                        {
                            bDirty = true;
                         
                            // ask the user if they want to save
                            MasterPage master = Page.Master;
                            if( master != null )
                            {
                                MasterPage topMaster = master.Master;

                                if( topMaster != null )
                                {
                                    MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );


                                    if( msgBox != null )
                                    {
                                        string tmp = msgBox.NameofMsgBox;

                                        HiddenField serverConfirmationDialogResultsHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ServerConfirmationDialogResults" );

                                        if( serverConfirmationDialogResultsHiddenField != null )
                                        {
                                            string clientId = serverConfirmationDialogResultsHiddenField.ClientID;

                                            if( IsCurrentDocumentMenuItemEnabled() == true )
                                            {
                                                msgBox.PopupMessageFromUpdatePanel( Page, "Save pending changes?", "Yes", "No", "Cancel", "ServerConfirmationDialogResults", clientId );
                                            }
                                            else
                                            {
                                                msgBox.PopupMessageFromUpdatePanel( Page, "There are unsaved changes from the previous document. To save, cancel current selection and return to the previous document by selecting Current Document from the main menu.                     Discard pending changes and continue with current selection or cancel current selection?", "Discard Changes", "Cancel Current Selection", "ServerConfirmationDialogResults", clientId );
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception( "Could not find hidden field." );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else // contract
                {

                    // check document's dirty value
                    EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
                    if( editedDocumentContentFront != null )
                    {
                        if( editedDocumentContentFront.IsDocumentDirty() == true )
                        {
                            bDirty = true;

                            // ask the user if they want to save
                            MasterPage master = Page.Master;
                            if( master != null )
                            {
                                MasterPage topMaster = master.Master;

                                if( topMaster != null )
                                {
                                    MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );


                                    if( msgBox != null )
                                    {
                                        HiddenField serverConfirmationDialogResultsHiddenField = ( HiddenField )NACCMMasterForm.FindControl( "ServerConfirmationDialogResults" );

                                        if( serverConfirmationDialogResultsHiddenField != null )
                                        {
                                            string clientId = serverConfirmationDialogResultsHiddenField.ClientID;

                                            if( IsCurrentDocumentMenuItemEnabled() == true )
                                            {
                                                msgBox.PopupMessageFromUpdatePanel( Page, "Save pending changes?", "Yes", "No", "Cancel", "ServerConfirmationDialogResults", clientId );
                                            }
                                            else
                                            {
                                                msgBox.PopupMessageFromUpdatePanel( Page, "There are unsaved changes from the previous document. To save, cancel current selection and return to the previous document by selecting Current Document from the main menu.                     Discard pending changes and continue with current selection or cancel current selection?", "Discard Changes", "Cancel Current Selection", "ServerConfirmationDialogResults", clientId );
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception( "Could not find hidden field." );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ( bDirty );
        }

        //// not used $$$
        //public void TestMsgBox()
        //{
        //    MasterPage master = Page.Master;
        //    if( master != null )
        //    {
        //        MasterPage topMaster = master.Master;

        //        if( topMaster != null )
        //        {
        //            MsgBox msgBox = ( MsgBox )topMaster.FindControl( "MsgBox" );

        //            if( msgBox != null )
        //            {
        //                msgBox.PopupMessageFromUpdatePanel( Page, "Save pending changes?", "Yes", "No", "Cancel", "ServerConfirmationDialogResults" );
        //            }
        //        }
        //    }

        //}

        // save current tab 
        private bool ShortSave()
        {
            bool bSuccess = true;

            try
            {
                ( ( BaseDocumentEditorPage )this.Page ).ClearErrors();  // move this within try 10/19/2016

                
                Type pageType = this.Page.GetType();  // $$$ adding type testing to avoid known exception still to be tested 8/30/2016
                Type baseType = pageType.BaseType;
                Type realBaseType = baseType.BaseType;
                if( realBaseType.Name.CompareTo( "BaseDocumentEditorPage" ) == 0 )
                {
                    ( ( BaseDocumentEditorPage )this.Page ).ShortSave();  // synchronous
                }
                else
                {
                    // log the access
                    string exceptionMessage = String.Format( "In ShortSave() to trap error where page is not expected type.  Expected type is BaseDocumentEditorPage and actual type = {0}", realBaseType.Name );
                    UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.Exception, exceptionMessage, -1, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ExceptionDetect );
                    log.LogUserActivity();

                    throw new Exception( exceptionMessage );
                }
            }
            catch( Exception ex )
            {
                // log the access
                UserActivity log = ( UserActivity )new UserActivity( VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionTypes.Exception, ex.Message, -1, VA.NAC.NACCMBrowser.BrowserObj.UserActivity.ActionDetailsTypes.ExceptionHandler );
                log.LogUserActivity();

                if( ex.Message.Contains( "trap error" ) != true )
                {
                    ( ( BaseDocumentEditorPage )this.Page ).HideProgressIndicator();
                }
                bSuccess = false;
                ShowException( ex );
            }

            return ( bSuccess );
        }

        // user requested save 
        private bool HandleUserSaveRequest()
        {
      //      SerializableObjectDataSource editedDocumentDataSource = null;

            bool bSuccess = true;

            bSuccess = ShortSave();

            if( bSuccess == true )
            {

                if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.Contract )
                {
         //           editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedDocumentDataSourceBack" ];
                    DataRelay dataRelay = ( DataRelay )Session[ "DataRelay" ];
                    dataRelay.RestoreDelegatesAfterDeserialization();

                    if( dataRelay != null )
                    {
                        try
                        {
                            dataRelay.Update();
                        }
                        catch( Exception ex )
                        {
                            bSuccess = false;
                            ShowException( ex );
                        }

                    }
                }
                else if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.Offer )
                {
          //          editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedOfferDataSourceBack" ];
                    OfferDataRelay offerDataRelay = ( OfferDataRelay )Session[ "OfferDataRelay" ];
                    offerDataRelay.RestoreDelegatesAfterDeserialization();

                    if( offerDataRelay != null )
                    {
                        try
                        {
                            offerDataRelay.Update();
                        }
                        catch( Exception ex )
                        {
                            bSuccess = false;
                            ShowException( ex );
                        }

                    }
                }

                //if( editedDocumentDataSource != null )
                //{
                //    try
                //    {
                //        editedDocumentDataSource.RestoreDelegatesAfterDeserialization( this );
                //        editedDocumentDataSource.Update();
                //    }
                //    catch( Exception ex )
                //    {
                //        bSuccess = false;
                //        ShowException( ex );
                //    }
                //}


            }

            return( bSuccess );
        }

       // user requested save 
 //       private void HandleUserSaveRequestForCurrentDocument()
 //       {
 //           SerializableObjectDataSource editedDocumentDataSource = null;
             
 //            _dataRelay = ( DataRelay )Session[ "DataRelay" ];
 //                            _dataRelay.UpdateFront( _dataRelay.EditedDocumentContentFront );
 //_offerDataRelay.UpdateFront( _offerDataRelay.EditedOfferContentFront );
 //           if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.Contract )
 //           {
 //               editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedDocumentDataSourceBack" ];
 //           }
 //           else if( ( ( BaseDocumentEditorPage )this.Page ).DocumentEditorType == BaseDocumentEditorPage.DocumentEditorTypes.Offer )
 //           {
 //               editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedOfferDataSourceBack" ];
 //           }
 //           }
 //           else  // determine from current document $$$ perhaps always do it this way? but what about new documents?
 //           {
 //               CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];
 //               if( currentDocument != null )
 //               {
 //                   if( currentDocument.DocumentType == CurrentDocument.DocumentTypes.Offer )
 //                   {
 //                       editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedOfferDataSourceBack" ];
 //                   }
 //                   else
 //                   {
 //                       editedDocumentDataSource = ( SerializableObjectDataSource )Session[ "EditedDocumentDataSourceBack" ];
 //                   }
 //               }
 //           }

 //           if( editedDocumentDataSource != null )
 //           {
 //               try
 //               {
 //                   editedDocumentDataSource.RestoreDelegatesAfterDeserialization( this );
 //                   editedDocumentDataSource.Update();
 //               }
 //               catch( Exception ex )
 //               {
 //                   ShowException( ex );
 //               }
 //           }
 //       }

        protected void RightPanelToggleCheckBox_OnCheckedChanged( object sender, EventArgs e )
        {
            // true means it was open at the start of the click ( and will be closed at client side at the end of the click )
   //         RightPanelController.ClientState = RightPanelControllerStateHiddenField.Value;
    //        RightPanelController.Collapsed = bool.Parse( RightPanelControllerStateHiddenField.Value );
   //         string test = RightPanelControllerStateHiddenField.Value;
             string rightPanelControllerState = "";
            if( Session[ "RightPanelControllerState" ] != null )
                rightPanelControllerState = ( string )Session[ "RightPanelControllerState" ];

            bool bMemberVariable = RightPanelOpenState;

        }

        // ************************* personalized notifications ************

        //private DocumentDataSource _personalizedNotificationDataSource = null;

        //public DocumentDataSource PersonalizedNotificationDataSource
        //{
        //    get { return _personalizedNotificationDataSource; }
        //    set { _personalizedNotificationDataSource = value; }
        //}

        //// Personalized Notification parameters
        //private Parameter _includeSubordinatesParameter = null;

    
        protected void BindPersonalizedNotifications()
        {
            try
            {
                // bind
                PersonalizedNotificationGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }


        public void LoadPersonalizedNotifications( bool bReload )
        {
            bool bSuccess = false;

            if( Page.Session[ "PersonalizedNotificationDataSet" ] == null || bReload == true )  // added call to bReload == true 7/6/2016 $$$$
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( contractDB != null )
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    bSuccess = contractDB.SelectPersonalizedNotification( ref _personalizedNotificationDataSet, true );
                    if( bSuccess == false )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                    }
                    else
                    {
                        _personalizedNotificationDataView = new DataView( _personalizedNotificationDataSet.Tables[ 0 ] );
                        Session[ "PersonalizedNotificationDataSet" ] = _personalizedNotificationDataSet;                       
                    }
                }
            }
            else
            {
                _personalizedNotificationDataSet = ( DataSet )Session[ "PersonalizedNotificationDataSet" ];
                _personalizedNotificationDataView = new DataView( _personalizedNotificationDataSet.Tables[ 0 ] );
            }

            PersonalizedNotificationGridView.DataSource = _personalizedNotificationDataView;



            //    _personalizedNotificationDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, false );
            //    _personalizedNotificationDataSource.ID = "PersonalizedNotificationDataSource";
            //    _personalizedNotificationDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
            //    _personalizedNotificationDataSource.SelectCommand = "SelectPersonalizedNotification";

            //    CreatePersonalizedNotificationParameters();

            //    _personalizedNotificationDataSource.SetEventOwnerName( "PersonalizedNotification" );
            //    _personalizedNotificationDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
            //    _personalizedNotificationDataSource.SelectParameters.Add( _includeSubordinatesParameter );
            //    _includeSubordinatesParameter.DefaultValue = "true"; // $$$ add user checkbox for this later

            //    Page.Session[ "PersonalizedNotificationDataSource" ] = _personalizedNotificationDataSource;
            //}
            //else
            //{
            //    _personalizedNotificationDataSource = ( DocumentDataSource )Page.Session[ "PersonalizedNotificationDataSource" ];
            //    _personalizedNotificationDataSource.RestoreDelegatesAfterDeserialization( this, "PersonalizedNotification" );

            //    RestorePersonalizedNotificationParameters( _personalizedNotificationDataSource );
            //}

            //PersonalizedNotificationGridView.DataSource = PersonalizedNotificationDataSource;
        }

        //private void CreatePersonalizedNotificationParameters()
        //{
        //    _includeSubordinatesParameter = new Parameter( "IncludeSubordinates", TypeCode.Boolean );
        //}

        //private void RestorePersonalizedNotificationParameters( DocumentDataSource personalizedNotificationDataSource )
        //{
        //    _includeSubordinatesParameter = personalizedNotificationDataSource.SelectParameters[ "IncludeSubordinates" ];
        //}

        protected void PersonalizedNotificationGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {

                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;
                CurrentDocument currentDocument = ( CurrentDocument )Session[ "CurrentDocument" ];

 
                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    //Button testMsgBoxButton2 = null;
                    //testMsgBoxButton2 = ( Button )e.Row.FindControl( "TestMsgBoxButton2" );
                    //if( testMsgBoxButton2 != null )
                    //{
                    //    testMsgBoxButton2.Attributes.Add( "onclick", "presentMsgBox('Are ye sure mate?','yes','no','cancel','fred')" );

                    //}

                    Button selectContractButton = null;
                    selectContractButton = ( Button )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        string contractNumber = ( string )dataRowView[ "ContractNumber" ];

                        // colors match .PersonalizedNotificationGridItems and  .PersonalizedNotificationGridAltItems
                        string rowColor = "alt";
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );
                        if( odd > 0 )
                        {
                            rowColor = "norm";
                        }

                        string windowHighlightCommand = string.Format( "resetNotificationAndContractHighlighting( 'PersonalizedNotificationGridView', {0}, '{1}' );", rowIndex, rowColor );
                        string cursorChangeToHand = string.Format( "notificationContractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "notificationContractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectContractButton.Attributes.Add( "onclick", windowHighlightCommand );
                        selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }



        protected void PersonalizedNotificationGridView_OnRowCreated( object sender, GridViewRowEventArgs e )
        {
            // hide id fields
            if( ( e.Row.Cells.Count > PersonalizedNotificationIdFieldNumber ) )
            {
                e.Row.Cells[ PersonalizedNotificationIdFieldNumber ].Visible = false;
            }
        }

        //protected string FormatContractNumbers( object fssContractNumberObj, object bpaContractNumberObj )
        //{
        //    string combinedContractNumbers = "";
        //    string fssContractNumber = "";
        //    string bpaContractNumber = "";

        //    if( fssContractNumberObj != null )
        //    {
        //        fssContractNumber = fssContractNumberObj.ToString();
        //    }

        //    if( bpaContractNumberObj != null )
        //    {
        //        bpaContractNumber = bpaContractNumberObj.ToString(); 
        //    }

        //    if( fssContractNumber.Length > 0 && bpaContractNumber.Length > 0 )
        //    {
        //        combinedContractNumbers = string.Format( "bpa: {0} <br> fss: {1}", bpaContractNumber, fssContractNumber );
        //    }
        //    else
        //    {
        //        if( bpaContractNumber.Length > 0 )
        //            combinedContractNumbers = string.Format( "{0}", bpaContractNumber );
        //        else
        //            combinedContractNumbers = string.Format( "{0}", fssContractNumber );
        //    }

        //    return ( combinedContractNumbers );
        //}

        protected void PersonalizedNotificationGridView_OnRowCommand( object sender, GridViewCommandEventArgs e )
        {
            if( e.CommandName.CompareTo( "EditNotificationContract" ) == 0 )
            {
                // short save current tab if possible
                bool bSuccess = true;

                // if current menu choice is current document, save the current tab to the front object
                if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
                {
                    string currentMenuValue = Session[ "CurrentSelectedMainMenuItemValue" ].ToString();
                    Session[ "PriorSelectedMainMenuItemValue" ] = currentMenuValue; // save in case selection is aborted due to dirty $$$ added 10/21/2016

                    if( currentMenuValue.CompareTo( "CurrentDocument" ) == 0 )
                    {
                        bSuccess = ShortSave();
                    }
                }

                // cancel until the error on the current page is fixed
                if( bSuccess == false )
                    return;

                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int contractRecordId = Int32.Parse( commandArgs[ 1 ].ToString() );
                string contractNumber = commandArgs[ 2 ].ToString();
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );

                if( selectedItemIndex < PersonalizedNotificationGridView.Rows.Count )
                {
                    PersonalizedNotificationGridView.SelectedIndex = selectedItemIndex;
                    // clear the other grid's selection if any
                    PersonalizedContractListGridView.SelectedIndex = -1;
                }

                ViewSelectedContract( contractRecordId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.PersonalizedNotificationGridView );  // test data: "V797P-4681A", 34 );
            }

        }

        // PersonalizedNotificationGridView
        protected void SelectContractButton_OnDataBinding( object sender, EventArgs e )
        {
            Button SelectContractButton = ( Button )sender;

            GridViewRow containingGridViewRow = ( GridViewRow )SelectContractButton.NamingContainer;

            if( containingGridViewRow != null )
            {
                DataRowView currentRow = ( DataRowView )containingGridViewRow.DataItem;
                string parentContractNumber = "";
                string contractNumber = "";

                if( currentRow[ "ContractNumber" ] != DBNull.Value )
                {
                    parentContractNumber = currentRow[ "ContractNumber" ].ToString();
                }

                if( currentRow[ "BPAContractNumber" ] != DBNull.Value )
                {
                    contractNumber = currentRow[ "BPAContractNumber" ].ToString();
                }

                //  Text='<%#CMGlobals.MultilineButtonText( DataBinder.Eval( Container.DataItem, "ContractNumber" ), DataBinder.Eval( Container.DataItem, "BPAContractNumber" ) )%>'
                if( parentContractNumber.Length > 0 && contractNumber.Length > 0 )
                {
                    CMGlobals.MultilineButtonText( SelectContractButton, new String[] { "bpa:", contractNumber, "fss:" , parentContractNumber } );
                }
                else if( parentContractNumber.Length > 0 )
                {
                    SelectContractButton.Text = parentContractNumber;
                }
                else if( contractNumber.Length > 0 )
                {
                    SelectContractButton.Text = contractNumber;
                }
            }
        }

        /* not called */
        protected void ScrollToSelectedNotificationItem()
        {

            int rowIndex = PersonalizedNotificationGridView.SelectedIndex;

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( NOTIFICATIONGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( NOTIFICATIONGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setPersonalizedNotificationScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedNotificationItemIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }

        protected void PersonalizedNotificationGridView_PreRender( object sender, EventArgs e )
        {
   //         if( Page.IsPostBack == false )
    //            HighlightNotificationRow( 0 );  /* this is firing after form_load, and should not be setting to 0 since nothing selected $$$$ */
        }

        protected void HighlightNotificationRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( PersonalizedNotificationGridView.HasData() == true )
            {
                GridViewRow row = PersonalizedNotificationGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = PersonalizedNotificationGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = PersonalizedNotificationGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setNotificationHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveNotificationHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                MainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        //protected void RestorePersonalizedNotificationGridViewSelectedItem()
        //{
        //    if( Session[ "PersonalizedNotificationGridViewSelectedIndex" ] == null )
        //        return;

        //    PersonalizedNotificationGridView.SelectedIndex = int.Parse( Session[ "PersonalizedNotificationGridViewSelectedIndex" ].ToString() );
        //}


        // ************************* personalized contract list ************


        //private DocumentDataSource _personalizedContractListDataSource = null;

        //public DocumentDataSource PersonalizedContractListDataSource
        //{
        //    get { return _personalizedContractListDataSource; }
        //    set { _personalizedContractListDataSource = value; }
        //}

        //  personalized contract list parameters
  //      private Parameter _currentUserParameter = null;
        //private Parameter _COIDParameter = null;
        //private Parameter _loginIdParameter = null;
        //private Parameter _contractStatusFilterParameter = null;
        //private Parameter _contractOwnerFilterParameter = null;
        //private Parameter _filterTypeParameter = null;
        //private Parameter _filterValueParameter = null;
        //private Parameter _sortExpressionParameter = null;
        //private Parameter _sortDirectionParameter = null;

        private string _contractStatusFilterValue = "T"; // A - All, T - Active, C - Closed - may gui this in the future

        protected void BindPersonalizedContractList()
        {
            try
            {
                // bind
                PersonalizedContractListGridView.DataBind();
            }
            catch( Exception ex )
            {
                ShowException( ex );
            }
        }

        public void LoadPersonalizedContractList( bool bReload )
        {
            bool bSuccess = false;

            if( Page.Session[ "PersonalizedContractListDataSet" ] == null || bReload == true )  // added call to bReload == true 7/6/2016 $$$$
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                if( contractDB != null )
                {
                    contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                    contractDB.MakeConnectionString();

                    bSuccess = contractDB.SelectContractHeaders( ref _personalizedContractListDataSet, _contractStatusFilterValue, "M", "", "", "", "" );
                    if( bSuccess == false )
                    {
                        MsgBox.AlertFromUpdatePanel( Page, contractDB.ErrorMessage );
                    }
                    else
                    {
                        _personalizedContractListDataView = new DataView( _personalizedContractListDataSet.Tables[ 0 ] );
                        Session[ "PersonalizedContractListDataSet" ] = _personalizedContractListDataSet;
                    }
                }
            }
            else
            {
                _personalizedContractListDataSet = ( DataSet )Session[ "PersonalizedContractListDataSet" ];
                _personalizedContractListDataView = new DataView( _personalizedContractListDataSet.Tables[ 0 ] );
            }

            PersonalizedContractListGridView.DataSource = _personalizedContractListDataView;

            //if( Page.Session[ "PersonalizedContractListDataSource" ] == null || bReload == true )  // added call to bReload == true 7/6/2016 $$$$
            //{
            //    _personalizedContractListDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.NACCMCommonUser, false );
            //    _personalizedContractListDataSource.ID = "PersonalizedContractListDataSource";
            //    _personalizedContractListDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
            //    _personalizedContractListDataSource.SelectCommand = "SelectContractHeaders";

            //    CreatePersonalizedContractListParameters();

            //    _personalizedContractListDataSource.SetEventOwnerName( "PersonalizedContractList" );
            //    _personalizedContractListDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

            //    _personalizedContractListDataSource.SelectParameters.Add( _COIDParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _loginIdParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _contractStatusFilterParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _contractOwnerFilterParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _filterTypeParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _filterValueParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _sortExpressionParameter );
            //    _personalizedContractListDataSource.SelectParameters.Add( _sortDirectionParameter );

            //    Page.Session[ "PersonalizedContractListDataSource" ] = _personalizedContractListDataSource;
            //}
            //else
            //{
            //    _personalizedContractListDataSource = ( DocumentDataSource )Page.Session[ "PersonalizedContractListDataSource" ];
            //    _personalizedContractListDataSource.RestoreDelegatesAfterDeserialization( this, "PersonalizedContractList" );           // $$$ this is the start of the serialization exception -  added call to seteventownername 7/6/2016

            //    RestorePersonalizedContractListParameters( _personalizedContractListDataSource );
            //}

            //SetPersonalizedContractListSelectCriteria( _contractStatusFilterValue );
            //PersonalizedContractListGridView.DataSource = PersonalizedContractListDataSource;
        }

        // contractStatusFilter = nchar(1) A - All, T - Active, C - Closed 
        //private void SetPersonalizedContractListSelectCriteria( string contractStatusFilter )
        //{
        //    BrowserSecurity2 browserSecurity = ( BrowserSecurity2 )Session[ "BrowserSecurity" ];

        //    _COIDParameter.DefaultValue = browserSecurity.UserInfo.OldUserId.ToString();
        //    _loginIdParameter.DefaultValue = browserSecurity.UserInfo.LoginName;
        //    _contractStatusFilterParameter.DefaultValue = contractStatusFilter;
        //    _contractOwnerFilterParameter.DefaultValue = "M";        // nchar(1) A - All, M - Mine 
        //    _filterTypeParameter.DefaultValue = ""; // nchar(1) N - Number, O - CO Name, V - Vendor, D - Description, S - Schedule 
        //    _filterValueParameter.DefaultValue = ""; // leaving value blank and owner = M selects based on COID
        //    _sortExpressionParameter.DefaultValue = "";  // default, from SP, is ascending by contract number - see SP for other option strings
        //    _sortDirectionParameter.DefaultValue = "";
        //}
       
        //private void CreatePersonalizedContractListParameters()
        //{
        //     _COIDParameter = new Parameter( "COID", TypeCode.Int32 );
        //    _loginIdParameter = new Parameter( "LoginId", TypeCode.String );
        //    _contractStatusFilterParameter = new Parameter( "ContractStatusFilter", TypeCode.String );
        //    _contractOwnerFilterParameter = new Parameter( "ContractOwnerFilter", TypeCode.String );
        //    _filterTypeParameter = new Parameter( "FilterType", TypeCode.String );
        //    _filterValueParameter = new Parameter( "FilterValue", TypeCode.String );
        //    _sortExpressionParameter = new Parameter( "SortExpression", TypeCode.String );
        //    _sortDirectionParameter = new Parameter( "SortDirection", TypeCode.String );
        //}

        //private void RestorePersonalizedContractListParameters( DocumentDataSource personalizedContractListDataSource )
        //{
        //    _COIDParameter = personalizedContractListDataSource.SelectParameters[ "COID" ];
        //    _loginIdParameter = personalizedContractListDataSource.SelectParameters[ "LoginId" ];
        //    _contractStatusFilterParameter = personalizedContractListDataSource.SelectParameters[ "ContractStatusFilter" ];
        //    _contractOwnerFilterParameter = personalizedContractListDataSource.SelectParameters[ "ContractOwnerFilter" ];
        //    _filterTypeParameter = personalizedContractListDataSource.SelectParameters[ "FilterType" ];
        //    _filterValueParameter = personalizedContractListDataSource.SelectParameters[ "FilterValue" ];
        //    _sortExpressionParameter = personalizedContractListDataSource.SelectParameters[ "SortExpression" ];
        //    _sortDirectionParameter = personalizedContractListDataSource.SelectParameters[ "SortDirection" ];
        //}

        public void ContractCancellationDate_OnDataBinding( object sender, EventArgs e )
        {
            Label contractCancellationDateLabel = ( Label )sender;

            //  hide or format ContractCancellationDate label
            if( _contractStatusFilterValue.CompareTo( "T" ) != 0 ) // if not active only
            {
                GridViewRow gridViewRow = ( GridViewRow )contractCancellationDateLabel.NamingContainer;

                if( gridViewRow.DataItem != null )
                {
                    if( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ] ) != null )
                    {
                        DateTime completionDate;

                        if( DateTime.TryParse( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_Completion" ].ToString(), out completionDate ) == true )
                        {
                            DateTime expirationDate = DateTime.Parse( ( ( ( DataRowView )gridViewRow.DataItem )[ "Dates_CntrctExp" ] ).ToString() );
                            if( completionDate.CompareTo( expirationDate ) != 0 )
                            {
                                contractCancellationDateLabel.Text = "Completed: " + completionDate.ToString( "d" );
                            }
                            else
                            {
                                contractCancellationDateLabel.Visible = false;
                            }
                        }
                        else
                        {
                            contractCancellationDateLabel.Visible = false;
                        }
                    }
                    else
                    {
                        contractCancellationDateLabel.Visible = false;
                    }
                }
                else
                {
                    contractCancellationDateLabel.Visible = false;
                }
            }
            else
            {
                contractCancellationDateLabel.Visible = false;
            }
        }

        protected void PersonalizedContractListGridView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            try
            {
                GridView gv = ( GridView )sender;
                int rowIndex = e.Row.RowIndex + 1;
                DataRowView dataRowView = ( DataRowView )e.Row.DataItem;

                if( e.Row.RowType == DataControlRowType.DataRow )
                {
                    Button selectContractButton = null;
                    selectContractButton = ( Button )e.Row.FindControl( "SelectContractButton" );
                    if( selectContractButton != null )
                    {
                        string contractNumber = ( string )dataRowView[ "CntrctNum" ];
              //          int scheduleNumber = Int32.Parse( dataRowView[ "Schedule_Number" ].ToString() );

                        // colors match .PersonalizedGridItems and  .PersonalizedGridAltItems
                        string rowColor = "alt";
                        int odd = 0;
                        Math.DivRem( rowIndex, 2, out odd );
                        if( odd > 0 )
                        {
                            rowColor = "norm";
                        }

                        string windowHighlightCommand = string.Format( "resetNotificationAndContractHighlighting( 'PersonalizedContractListGridView', {0}, '{1}' );", rowIndex, rowColor );
                        string cursorChangeToHand = string.Format( "contractListContractNumberMouseChange(this, 'over', {0}, '{1}');", rowIndex, rowColor );
                        string cursorChangeToNormal = string.Format( "contractListContractNumberMouseChange(this, 'out', {0}, '{1}');", rowIndex, rowColor );

                        selectContractButton.Attributes.Add( "onclick", windowHighlightCommand );
                        selectContractButton.Attributes.Add( "onmouseover", cursorChangeToHand );
                        selectContractButton.Attributes.Add( "onmouseout", cursorChangeToNormal );
                    }
                }
            }
            catch( Exception ex )
            {
                string msg = ex.Message;
            }
        }

        // this uses odd/even vs. row state, consider deleting this in the future
        //protected void HighlightContractHeaderRow( int itemIndex )
        //{
        //    int highlightedRowIndex = itemIndex + 1;

        //    if( PersonalizedContractListGridView.HasData() == true )
        //    {
        //        if( itemIndex < PersonalizedContractListGridView.Rows.Count )
        //        {
        //            string rowColor = "alt";
        //            int odd = 0;
        //            Math.DivRem( highlightedRowIndex, 2, out odd );
        //            if( odd > 0 )
        //            {
        //                rowColor = "norm";
        //            }

        //            string setContractHighlightedRowIndexAndOriginalColorScript = String.Format( "setContractHighlightedRowIndexAndOriginalColor( {0}, '{1}' );", highlightedRowIndex, rowColor );
        //            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreserveItemHighlightingScript", setContractHighlightedRowIndexAndOriginalColorScript, true );

        //            // allow the highlight postback to occur 
        //       //     ChangeItemHighlightUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        //        }
        //    }
        //}

        protected void PersonalizedContractListGridView_OnRowCommand( object sender, GridViewCommandEventArgs e )
        {
            if( e.CommandName.CompareTo( "EditPersonalContract" ) == 0 )
            {
                // short save current tab if possible
                bool bSuccess = true;

                // if current menu choice is current document, save the current tab to the front object
                if( Session[ "CurrentSelectedMainMenuItemValue" ] != null )
                {
                    string currentMenuValue = Session[ "CurrentSelectedMainMenuItemValue" ].ToString();
                    Session[ "PriorSelectedMainMenuItemValue" ] = currentMenuValue; // save in case selection is aborted due to dirty $$$ added 10/21/2016

                    if( currentMenuValue.CompareTo( "CurrentDocument" ) == 0 )
                    {
                        bSuccess = ShortSave();
                    }                    
                }

                // cancel until the error on the current page is fixed
                if( bSuccess == false )
                    return;


                // argument is row index and row id
                string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { ',' } );
                int selectedItemIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                int contractRecordId = Int32.Parse( commandArgs[ 1 ].ToString() );
                string contractNumber = commandArgs[ 2 ].ToString();
                int scheduleNumber = Int32.Parse( commandArgs[ 3 ].ToString() );

                if( selectedItemIndex < PersonalizedContractListGridView.Rows.Count )
                {
                    PersonalizedContractListGridView.SelectedIndex = selectedItemIndex;
                    // clear the other grid's selection if any
                    PersonalizedNotificationGridView.SelectedIndex = -1;
                }

                ViewSelectedContract( contractRecordId, contractNumber, scheduleNumber, RequestedNextDocument.DocumentChangeRequestSources.PersonalizedContractListGridView );  // test data: "V797P-4681A", 34 );
            }

        }

        /* currently not called */
        protected void ScrollToSelectedPersonalizedContractListItem()
        {

            int rowIndex = PersonalizedContractListGridView.SelectedIndex;

            int fudge = ( int )Math.Floor( ( decimal )rowIndex / ( decimal )100.0 );
            int rowPosition = ( CONTRACTLISTGRIDVIEWROWHEIGHTESTIMATE * rowIndex ) + ( CONTRACTLISTGRIDVIEWROWHEIGHTESTIMATE * fudge );

            string scrollToRowScript = String.Format( "setPersonalizedContractListScrollOnChange( {0} );", rowPosition );
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SelectedPersonalizedContractListIndexChangedScript", scrollToRowScript, true ); // runs after controls established
        }


        protected void PersonalizedContractListGridView_PreRender( object sender, EventArgs e )
        {
      //      if( Page.IsPostBack == false )
      //          HighlightPersonalizedContractListRow( 0 );
        }

        protected void HighlightPersonalizedContractListRow( int itemIndex )
        {
            string highlightedRowOriginalColor = "";
            int highlightedRowIndex = itemIndex + 1;

            if( PersonalizedContractListGridView.HasData() == true )
            {
                GridViewRow row = PersonalizedContractListGridView.Rows[ itemIndex ];

                if( row.RowState == DataControlRowState.Alternate )
                {
                    highlightedRowOriginalColor = PersonalizedContractListGridView.AlternatingRowStyle.BackColor.ToString();
                }
                else
                {
                    highlightedRowOriginalColor = PersonalizedContractListGridView.RowStyle.BackColor.ToString();
                }

                string preserveHighlightingScript = String.Format( "setPersonalizedContractListHighlightedRowIndexAndOriginalColor( '{0}', '{1}' );", highlightedRowIndex, highlightedRowOriginalColor );
                ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "PreservePersonalizedContractListHighlightingScript", preserveHighlightingScript, true );

                // allow the highlight postback to occur 
                MainRightPanelUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
            }
        }

        //protected void RestorePersonalizedContractListGridViewSelectedItem()
        //{
        //    if( Session[ "PersonalizedContractListGridViewSelectedIndex" ] == null )
        //        return;

        //    PersonalizedNotificationGridView.SelectedIndex = int.Parse( Session[ "PersonalizedContractListGridViewSelectedIndex" ].ToString() );
        //}

    }
}