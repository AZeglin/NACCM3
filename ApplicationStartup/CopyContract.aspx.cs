using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

using GridView = VA.NAC.NACCMBrowser.BrowserObj.GridView;
using TextBox = VA.NAC.NACCMBrowser.BrowserObj.TextBox;

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class CopyContract : System.Web.UI.Page
    {
        private CopyContractWindowParms _copyContractWindowParms = null;

        private ObjectDataSource _copyContractDataSource = null;

        private CopyContractContent _copyContractContent = null; 

        protected void Page_Load( object sender, EventArgs e )
        {
            CMGlobals.CheckIfStartedProperly( this.Page );

            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "CopyContractWindowParms" ] != null )
            {
                _copyContractWindowParms = ( CopyContractWindowParms )Session[ "CopyContractWindowParms" ];
                InitObjectDataSource();
            }
            else
            {
                throw new Exception( "Error: CopyContractWindowParms not available for copy contract window presentation." );
            }

            if( Page.IsPostBack == false )
            {
                BindCopyContract();
            }

            //if( Page.IsPostBack == false )
            //{
            //    AddClientCloseEvent( false );
            //}
       }

        private void ClearSessionVariables()
        {
            Session[ "CopyContractContent" ] = null;
            Session[ "CopyContractDataSource" ] = null;
            Session[ "CopyContractCurrentRowOffset" ] = null;
        }

        private void InitObjectDataSource()
        {
            InitCopyContractContent();

            if( Session[ "CopyContractDataSource" ] == null )
            {
                _copyContractDataSource = new ObjectDataSource();
                _copyContractDataSource.ID = "CopyContractDataSource";
                _copyContractDataSource.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.CopyContractContent";
                _copyContractDataSource.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.CopyContractManager";
                _copyContractDataSource.SelectMethod = "GetCopyContractContent";
                _copyContractDataSource.UpdateMethod = "CopyContract";
                _copyContractDataSource.Selecting += new ObjectDataSourceSelectingEventHandler( _copyContractDataSource_Selecting );
                _copyContractDataSource.Updating += new ObjectDataSourceMethodEventHandler( _copyContractDataSource_Updating );
                _copyContractDataSource.Updated += new ObjectDataSourceStatusEventHandler( _copyContractDataSource_Updated );

            }
            else
            {
                _copyContractDataSource = ( ObjectDataSource )Session[ "CopyContractDataSource" ];
            }

            CopyContractFormView.DataSource = _copyContractDataSource;
        }

        private void InitCopyContractContent()
        {
            if( Session[ "CopyContractContent" ] == null )
            {
                _copyContractContent = new CopyContractContent( _copyContractWindowParms );
                Session[ "CopyContractContent" ] = _copyContractContent;
            }
            else
            {
                _copyContractContent = ( CopyContractContent )Session[ "CopyContractContent" ];
            }

            _copyContractContent.OldContractNumber = _copyContractWindowParms.ContractNumber;

        }

        private void BindCopyContract()
        {
            CopyContractFormView.DataBind();
        }

        void _copyContractDataSource_Selecting( object sender, ObjectDataSourceSelectingEventArgs e )
        {
            _copyContractContent.AwardDate = DateTime.Today;
            _copyContractContent.EffectiveDate = CMGlobals.GetNextEffectiveDate( DateTime.Today );
            _copyContractContent.ExpirationDate = CMGlobals.GetExpirationDate( _copyContractContent.EffectiveDate );
            _copyContractContent.OptionYears = 5;
        }

        void _copyContractDataSource_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            CopyContractContent copyContractContent = ( CopyContractContent )e.InputParameters[ "CopyContractContent" ];
            SaveScreenValuesToObject( copyContractContent );

            copyContractContent.OldContractNumber = _copyContractWindowParms.ContractNumber;

            SaveCurrentRowOffset( copyContractContent );
        }

        // make room for the new contract
        void _copyContractDataSource_Updated( object sender, ObjectDataSourceStatusEventArgs e )
        {
            CMGlobals.ExpireCache( this.Page, "ActiveContractHeaderDataSet" );
            CMGlobals.ExpireCache( this.Page, "AllContractHeaderDataSet" );
        }

        private void SaveScreenValuesToObject( CopyContractContent copyContractContent )
        {
            string newContractNumber = "";
            DateTime awardDate = DateTime.Today;
            DateTime effectiveDate = DateTime.Today;
            DateTime expirationDate = DateTime.Today;
            int optionYears = 4;


            FormView copyContractFormView = ( FormView )CopyContractForm.FindControl( "CopyContractFormView" );
            if( copyContractFormView != null )
            {
                if( copyContractFormView.CurrentMode == FormViewMode.Edit )
                {
                    TextBox NewContractNumberTextBox = ( TextBox )CopyContractFormView.FindControl( "NewContractNumberTextBox" );
                    if( NewContractNumberTextBox != null )
                        newContractNumber = NewContractNumberTextBox.Text;

                    TextBox AwardDateTextBox = ( TextBox )CopyContractFormView.FindControl( "AwardDateTextBox" );
                    if( AwardDateTextBox != null )
                        awardDate = DateTime.Parse( AwardDateTextBox.Text );

                    TextBox EffectiveDateTextBox = ( TextBox )CopyContractFormView.FindControl( "EffectiveDateTextBox" );
                    if( EffectiveDateTextBox != null )
                        effectiveDate = DateTime.Parse( EffectiveDateTextBox.Text );

                    TextBox ExpirationDateTextBox = ( TextBox )CopyContractFormView.FindControl( "ExpirationDateTextBox" );
                    if( ExpirationDateTextBox != null )
                        expirationDate = DateTime.Parse( ExpirationDateTextBox.Text );

                    TextBox OptionYearsTextBox = ( TextBox )CopyContractFormView.FindControl( "OptionYearsTextBox" );
                    if( OptionYearsTextBox != null )
                        optionYears = Int32.Parse( OptionYearsTextBox.Text );


                    //CheckBox CopySubItemsCheckBox = ( CheckBox )CopyContractFormView.FindControl( "CopySubItemsCheckBox" );
                    //if( CopySubItemsCheckBox != null )
                    //    bCopySubItems = CopySubItemsCheckBox.Checked;

                    copyContractContent.NewContractNumber = newContractNumber;
                    copyContractContent.AwardDate = awardDate;
                    copyContractContent.EffectiveDate = effectiveDate;
                    copyContractContent.ExpirationDate = expirationDate;
                    copyContractContent.OptionYears = optionYears;
                }
            }

        }

        //private void AddClientCloseEvent( bool bWithRefresh )
        //{
        //    string closeFunctionText = "";
        //    if( bWithRefresh == true )
        //        closeFunctionText = "CloseWindow( \"true\" );";
        //    else
        //        closeFunctionText = "CloseWindow( \"false\" );";

        //    CancelCopyContractButton.Attributes.Add( "onclick", closeFunctionText );
        //}

        // affects highlighting of current item in item grid
        private void SaveCurrentRowOffset( CopyContractContent copyContractContent )
        {
            int offset = 0;

            if( copyContractContent.OldContractNumber.CompareTo( copyContractContent.NewContractNumber ) > 0 )
                offset = 1;

            Session[ "CopyContractCurrentRowOffset" ] = offset;
        }

        private void SetCopyContractHeaderInfo()
        {
            SelectedItemHeader.HeaderTitle = "Copy Contract";
            SelectedItemHeader.ContractNumber = _copyContractWindowParms.ContractNumber;
            SelectedItemHeader.ContractorName = _copyContractWindowParms.ContractorName;
            SelectedItemHeader.CommodityCovered = _copyContractWindowParms.CommodityCovered;
        }

  

        protected void CopyContractFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetCopyContractHeaderInfo();

            CopyContractUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

 

        protected void UpdateCopyContractButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidateCopyContract( ref validationMessage ) == true )
                {
                    _copyContractDataSource.Update();
                    CopyContractFormView.DataBind();                    
                    DisableUpdateButton();
                }
                else
                {
                    MsgBox.Alert( validationMessage );
                }
            }
            catch( Exception ex )
            {
                MsgBox.ShowError( ex );
            }
        }

        private void DisableUpdateButton()
        {
            Button updateCopyContractButton = ( Button )CopyContractForm.FindControl( "UpdateCopyContractButton" );
            if( updateCopyContractButton != null )
            {
                updateCopyContractButton.Enabled = false;
            }
        }

        private bool WasContractCopied()
        {
            bool bWasCopied = false;
            Button updateCopyContractButton = ( Button )CopyContractForm.FindControl( "UpdateCopyContractButton" );
            if( updateCopyContractButton != null )
            {
                bWasCopied = !updateCopyContractButton.Enabled;
            }
            return ( bWasCopied );
        }

        protected void CancelCopyContractButton_OnClick( object sender, EventArgs e )
        {
            CloseWindow( WasContractCopied() );
        } 

        private void CloseWindow( bool bWithRefresh )
        {
            string closeWindowScript = "CloseWindow( \"false\", \"false\" );";

            if( bWithRefresh == true )
                closeWindowScript = "CloseWindow( \"true\", \"false\"  );";
            else
                closeWindowScript = "CloseWindow( \"false\", \"true\"  );";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CloseWindowScript", closeWindowScript, true );

            // allow the postback to occur 
            UpdatePanelEventProxy copyContractUpdateUpdatePanelEventProxy = ( UpdatePanelEventProxy )CopyContractForm.FindControl( "CopyContractUpdateUpdatePanelEventProxy" );
            copyContractUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        private bool ValidateCopyContract( ref string validationMessage )
        {
            bool bSuccess = true;

            string newContractNumber = "";
            string awardDate = "";
            string effectiveDate = "";
            string expirationDate = "";
            string optionYears = "";
            DateTime parseDate;
            int parseInt;

            try
            {

                FormView copyContractFormView = ( FormView )CopyContractForm.FindControl( "CopyContractFormView" );
                if( copyContractFormView != null )
                {
                    if( copyContractFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox NewContractNumberTextBox = ( TextBox )CopyContractFormView.FindControl( "NewContractNumberTextBox" );
                        if( NewContractNumberTextBox != null )
                            newContractNumber = NewContractNumberTextBox.Text;

                        TextBox AwardDateTextBox = ( TextBox )CopyContractFormView.FindControl( "AwardDateTextBox" );
                        if( AwardDateTextBox != null )
                            awardDate = AwardDateTextBox.Text;
                        
                        if( awardDate.Length > 0 )
                        {
                            if( DateTime.TryParse( awardDate, out parseDate ) == false )
                            {
                                throw new Exception( "Award date is not a valid date." );
                            }
                        }
                        else
                        {
                            throw new Exception( "Award date is required." );
                        }

                        TextBox EffectiveDateTextBox = ( TextBox )CopyContractFormView.FindControl( "EffectiveDateTextBox" );
                        if( EffectiveDateTextBox != null )
                            effectiveDate = EffectiveDateTextBox.Text;

                        if( effectiveDate.Length > 0 )
                        {
                            if( DateTime.TryParse( effectiveDate, out parseDate ) == false )
                            {
                                throw new Exception( "Effective date is not a valid date." );
                            }
                        }
                        else
                        {
                            throw new Exception( "Effective date is required." );
                        }
                        
                        TextBox ExpirationDateTextBox = ( TextBox )CopyContractFormView.FindControl( "ExpirationDateTextBox" );
                        if( ExpirationDateTextBox != null )
                            expirationDate = ExpirationDateTextBox.Text;

                        if( expirationDate.Length > 0 )
                        {
                            if( DateTime.TryParse( expirationDate, out parseDate ) == false )
                            {
                                throw new Exception( "Expiration date is not a valid date." );
                            }
                        }
                        else
                        {
                            throw new Exception( "Expiration date is required." );
                        }

                        TextBox OptionYearsTextBox = ( TextBox )CopyContractFormView.FindControl( "OptionYearsTextBox" );
                        if( OptionYearsTextBox != null )
                            optionYears = OptionYearsTextBox.Text;

                        if( optionYears.Length > 0 )
                        {
                            if( Int32.TryParse( optionYears, out parseInt ) == false )
                            {
                                throw new Exception( "Option years is not a valid number." );
                            }
                        }
                        else
                        {
                            throw new Exception( "Option years is required." );
                        }

                        ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                        bool bIsValidated = false;
                        string contractNumberValidationMessage = "";
                        bSuccess = contractDB.ValidateContractNumber( newContractNumber, _copyContractWindowParms.ScheduleNumber, ref bIsValidated, ref contractNumberValidationMessage );

                        if( bSuccess == true )
                        {
                            if( bIsValidated == false )
                            {
                                throw new Exception( contractNumberValidationMessage );
                            }
                        }
                     }
                } 
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the destination contract information {0}", ex.Message );
            }

            return ( bSuccess );
        }

        protected string MultilineText( string[] textArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < textArray.Count(); i++ )
            {
                sb.AppendLine( textArray[ i ] );
                if( i < textArray.Count() - 1 )
                    sb.Append( "<br>" );
            }

            return( sb.ToString() );
        }

 

        //protected void CopySubItemsCheckBox_OnDataBinding( object sender, EventArgs e )
        //{
        //    CheckBox copySubItemsCheckBox = ( CheckBox )sender;

        //    FormView containingFormView = ( FormView )copySubItemsCheckBox.NamingContainer;

        //    CopyContractContent CopyContractContent = ( CopyContractContent )containingFormView.DataItem;

        //    bool bIsChecked = false;

        //    if( CopyContractContent != null )
        //    {
        //        bIsChecked = CopyContractContent.CopySubItems;
        //    }

        //    copySubItemsCheckBox.Checked = bIsChecked;
        //}

   

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

        protected void CopyContractScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "CopyContractErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "CopyContractErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            CopyContractScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void CopyContractFormView_OnPreRender( object sender, EventArgs e )
        {
            FormView copyContractFormView = ( FormView )sender;
            TextBox NewContractNumberTextBox = ( TextBox )CopyContractFormView.FindControl( "NewContractNumberTextBox" );
            if( NewContractNumberTextBox != null )
            {
                ContractDB contractDB = ( ContractDB )Session[ "ContractDB" ];
                string prefix = "";
                bool bSuccess = contractDB.GetNewContractPrefix( _copyContractWindowParms.ScheduleNumber, ref prefix );
                if( bSuccess == true )
                {
                    NewContractNumberTextBox.Text = prefix;
                }
            }
        }
    }
}
