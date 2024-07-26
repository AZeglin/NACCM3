using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
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
using TextBox = System.Web.UI.WebControls.TextBox;

namespace VA.NAC.CM.DrugItems
{
    public partial class NDCChange : System.Web.UI.Page
    {
        private NDCChangeWindowParms _NDCChangeWindowParms = null;

        private ObjectDataSource _NDCChangeDataSource = null;

        NDCChangeContent _ndcChangeContent = null; 

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }
            
            if( Session[ "NDCChangeWindowParms" ] != null )
            {
                _NDCChangeWindowParms = ( NDCChangeWindowParms )Session[ "NDCChangeWindowParms" ];
                InitObjectDataSource();
            }
            else
            {
                throw new Exception( "Error: NDCChangeWindowParms not available for change window presentation." );
            }

            if( Page.IsPostBack == false )
            {
                BindNDCChange();
            }
           
            AddClientCloseEvent();
       }

        private void ClearSessionVariables()
        {
            Session[ "NDCChangeContent" ] = null;
            Session[ "NDCChangeDataSource" ] = null;
            Session[ "NDCChangeCurrentRowOffset" ] = null;
        }

        private void InitObjectDataSource()
        {
            InitNDCChangeContent();

            if( Session[ "NDCChangeDataSource" ] == null )
            {
                _NDCChangeDataSource = new ObjectDataSource();
                _NDCChangeDataSource.ID = "NDCChangeDataSource";
                _NDCChangeDataSource.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.NDCChangeContent";
                _NDCChangeDataSource.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.NDCChangeManager";
                _NDCChangeDataSource.SelectMethod = "GetNDCChangeContent";
                _NDCChangeDataSource.UpdateMethod = "ChangeNDC";
                _NDCChangeDataSource.Selecting += new ObjectDataSourceSelectingEventHandler( _NDCChangeDataSource_Selecting );
                _NDCChangeDataSource.Updating += new ObjectDataSourceMethodEventHandler( _NDCChangeDataSource_Updating );

            }
            else
            {
                _NDCChangeDataSource = ( ObjectDataSource )Session[ "NDCChangeDataSource" ];
            }

            NDCChangeFormView.DataSource = _NDCChangeDataSource;
        }


        private void InitNDCChangeContent()
        {
            if( Session[ "NDCChangeContent" ] == null )
            {
                _ndcChangeContent = new NDCChangeContent( _NDCChangeWindowParms );
                Session[ "NDCChangeContent" ] = _ndcChangeContent;
            }
            else
            {
                _ndcChangeContent = ( NDCChangeContent )Session[ "NDCChangeContent" ];
            }

            _ndcChangeContent.SelectedDrugItemId = _NDCChangeWindowParms.SelectedDrugItemId;
            _ndcChangeContent.ModificationStatusId = _NDCChangeWindowParms.ModificationStatusId;
            _ndcChangeContent.ContractNumber = _NDCChangeWindowParms.ContractNumber;

        }

        private void BindNDCChange()
        {
            NDCChangeFormView.DataBind();
        }

        void _NDCChangeDataSource_Selecting( object sender, ObjectDataSourceSelectingEventArgs e )
        {
            _ndcChangeContent.DiscontinuationDateForOldNDC = DateTime.Today;
            _ndcChangeContent.EffectiveDateForNewNDC = DateTime.Today.AddDays( 1 );
        }

        void _NDCChangeDataSource_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            NDCChangeContent ndcChangeContent = ( NDCChangeContent )e.InputParameters[ "ndcChangeContent" ];
            SaveScreenValuesToObject( ndcChangeContent );

            ndcChangeContent.SelectedDrugItemId = _NDCChangeWindowParms.SelectedDrugItemId;
            ndcChangeContent.ModificationStatusId = _NDCChangeWindowParms.ModificationStatusId;
            ndcChangeContent.ContractNumber = _NDCChangeWindowParms.ContractNumber;

            SaveCurrentRowOffset( ndcChangeContent );
        }

        private void SaveScreenValuesToObject( NDCChangeContent ndcChangeContent )
        {
            string fdaAssignedLabelerCode = "";
            string productCode = "";
            string packageCode = "";
            DateTime discontinuationDate = DateTime.Today;
            DateTime effectiveDate = DateTime.Today;
            bool bCopyPricing = true;
            bool bCopySubItems = true;
            
            FormView ndcChangeFormView = ( FormView )NDCChangeForm.FindControl( "NDCChangeFormView" );
            if( ndcChangeFormView != null )
            {
                if( ndcChangeFormView.CurrentMode == FormViewMode.Edit )
                {
                    TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                    if( FdaAssignedLabelerCodeTextBox != null )
                        fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                    TextBox ProductCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "ProductCodeTextBox" );
                    if( ProductCodeTextBox != null )
                        productCode = ProductCodeTextBox.Text;

                    TextBox PackageCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "PackageCodeTextBox" );
                    if( PackageCodeTextBox != null )
                        packageCode = PackageCodeTextBox.Text;

                    TextBox DiscontinuationDateForOldNDCTextBox = ( TextBox )ndcChangeFormView.FindControl( "DiscontinuationDateForOldNDCTextBox" );
                    if( DiscontinuationDateForOldNDCTextBox != null )
                    {
                        if( DiscontinuationDateForOldNDCTextBox.Text.Length > 0 )
                        {
                            discontinuationDate = DateTime.Parse( DiscontinuationDateForOldNDCTextBox.Text );
                        }
                        else
                        {
                            discontinuationDate = DateTime.MinValue; // flagged to represent null date
                        }
                    }

                    TextBox EffectiveDateForNewNDCTextBox = ( TextBox )ndcChangeFormView.FindControl( "EffectiveDateForNewNDCTextBox" );
                    if( EffectiveDateForNewNDCTextBox != null )
                    {
                        if( EffectiveDateForNewNDCTextBox.Text.Length > 0 )
                        {
                            effectiveDate = DateTime.Parse( EffectiveDateForNewNDCTextBox.Text );
                        }
                    }

                    CheckBox CopyPricingCheckBox = ( CheckBox )ndcChangeFormView.FindControl( "CopyPricingCheckBox" );
                    if( CopyPricingCheckBox != null )
                        bCopyPricing = CopyPricingCheckBox.Checked;

                    CheckBox CopySubItemsCheckBox = ( CheckBox )ndcChangeFormView.FindControl( "CopySubItemsCheckBox" );
                    if( CopySubItemsCheckBox != null )
                        bCopySubItems = CopySubItemsCheckBox.Checked;

                    ndcChangeContent.FdaAssignedLabelerCode = fdaAssignedLabelerCode;
                    ndcChangeContent.ProductCode = productCode;
                    ndcChangeContent.PackageCode = packageCode;
                    ndcChangeContent.DiscontinuationDateForOldNDC = discontinuationDate;
                    ndcChangeContent.EffectiveDateForNewNDC = effectiveDate;
                    ndcChangeContent.CopyPricing = bCopyPricing;
                    ndcChangeContent.CopySubItems = bCopySubItems;
                }
            }

        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "CloseWindow( \"true\", \"true\" );";
            CancelNDCChangeButton.Attributes.Add( "onclick", closeFunctionText );
        }

        // affects highlighting of current item in item grid
        private void SaveCurrentRowOffset( NDCChangeContent ndcChangeContent )
        {
            int offset = 0;

            if( ndcChangeContent.DiscontinuationDateForOldNDC.CompareTo( DateTime.Today ) < 0 )
                offset = -1;

            string oldNDC = string.Format( "{0}{1}{2}", _NDCChangeWindowParms.FdaAssignedLabelerCode, _NDCChangeWindowParms.ProductCode, _NDCChangeWindowParms.PackageCode );
            string newNDC = string.Format( "{0}{1}{2}", ndcChangeContent.FdaAssignedLabelerCode, ndcChangeContent.ProductCode, ndcChangeContent.PackageCode );

            if( newNDC.CompareTo( oldNDC ) < 0 )
                offset += 1;

            Session[ "NDCChangeCurrentRowOffset" ] = offset;
        }

        private void SetNDCChangeHeaderInfo()
        {
            SelectedDrugItemHeader.HeaderTitle = "NDC Change";
            SelectedDrugItemHeader.FdaAssignedLabelerCode = _NDCChangeWindowParms.FdaAssignedLabelerCode;
            SelectedDrugItemHeader.ProductCode = _NDCChangeWindowParms.ProductCode;
            SelectedDrugItemHeader.PackageCode = _NDCChangeWindowParms.PackageCode;
            SelectedDrugItemHeader.Generic = _NDCChangeWindowParms.GenericName;
            SelectedDrugItemHeader.TradeName = _NDCChangeWindowParms.TradeName;
        }

   


        protected void NDCChangeFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetNDCChangeHeaderInfo();

            NDCChangeUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

 

        protected void UpdateNDCChangeButton_OnClick( object sender, EventArgs e )
        {
            string validationMessage = "";

            try
            {
                if( ValidateNDCChange( ref validationMessage ) == true )
                {
                    _NDCChangeDataSource.Update();
                    NDCChangeFormView.DataBind();
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
            Button updateNDCChangeButton = ( Button )NDCChangeForm.FindControl( "UpdateNDCChangeButton" );
            if( updateNDCChangeButton != null )
            {
                updateNDCChangeButton.Enabled = false;
            }
        }

        private bool ValidateNDCChange( ref string validationMessage )
        {
            bool bSuccess = true;

            string fdaAssignedLabelerCode = "";
            string productCode = "";
            string packageCode = "";
            string discontinuationDate = "";
            string effectiveDate = "";

            try
            {
                FormView ndcChangeFormView = ( FormView )NDCChangeForm.FindControl( "NDCChangeFormView" );
                if( ndcChangeFormView != null )
                {
                    if( ndcChangeFormView.CurrentMode == FormViewMode.Edit )
                    {
                        TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                        if( FdaAssignedLabelerCodeTextBox != null )
                            fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                        TextBox ProductCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "ProductCodeTextBox" );
                        if( ProductCodeTextBox != null )
                            productCode = ProductCodeTextBox.Text;

                        TextBox PackageCodeTextBox = ( TextBox )ndcChangeFormView.FindControl( "PackageCodeTextBox" );
                        if( PackageCodeTextBox != null )
                            packageCode = PackageCodeTextBox.Text;

                        TextBox DiscontinuationDateForOldNDCTextBox = ( TextBox )ndcChangeFormView.FindControl( "DiscontinuationDateForOldNDCTextBox" );
                        if( DiscontinuationDateForOldNDCTextBox != null )
                            discontinuationDate = DiscontinuationDateForOldNDCTextBox.Text;

                        TextBox EffectiveDateForNewNDCTextBox = ( TextBox )ndcChangeFormView.FindControl( "EffectiveDateForNewNDCTextBox" );
                        if( EffectiveDateForNewNDCTextBox != null )
                            effectiveDate = EffectiveDateForNewNDCTextBox.Text;

                        int parseResult = 0;
                        if( int.TryParse( fdaAssignedLabelerCode, out  parseResult ) == false )
                        {
                            throw new Exception( "Fda Assigned Labeler Code is not valid." );
                        }

                        if( fdaAssignedLabelerCode.Length != 5 )
                        {
                            throw new Exception( "Fda Assigned Labeler Code must be exactly 5 characters in length." );
                        }

                        if( productCode.Length != 4 )
                        {
                            throw new Exception( "Product Code must be exactly 4 characters in length." );
                        }

                        if( packageCode.Length != 2 )
                        {
                            throw new Exception( "Package Code must be exactly 2 characters in length." );
                        }

                        DateTime parseDate;

                        if( discontinuationDate.Length > 0 )
                        {
                            if( DateTime.TryParse( discontinuationDate, out parseDate ) == false )
                            {
                                throw new Exception( "Old NDC discontinuation date is not a valid date." );
                            }
                        }

                        if( effectiveDate.Length > 0 )
                        {
                            if( DateTime.TryParse( effectiveDate, out parseDate ) == false )
                            {
                                throw new Exception( "New NDC effective date is not a valid date." );
                            }
                        }
                        else
                        {
                            throw new Exception( "New NDC effective date is required." );
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the NDC Change information {0}", ex.Message );
            }

            return ( bSuccess );
        }

 

        protected void CancelNDCChangeButton_OnClick( object sender, EventArgs e )
        {

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

        protected void DiscontinuationDateForOldNDCTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox discontinuationDateForOldNDCTextBox = ( TextBox )sender;

            FormView containingFormView = ( FormView )discontinuationDateForOldNDCTextBox.NamingContainer;

            NDCChangeContent NDCChangeContent = ( NDCChangeContent )containingFormView.DataItem;

            string discontinuationDateString = "";

            if( NDCChangeContent != null )
            {
                DateTime discontinuationDate = NDCChangeContent.DiscontinuationDateForOldNDC;

                if( discontinuationDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    discontinuationDateString = discontinuationDate.Date.ToString("d");
                }
            }

            discontinuationDateForOldNDCTextBox.Text = discontinuationDateString;
        }

        protected void EffectiveDateForNewNDCTextBox_OnDataBinding( object sender, EventArgs e )
        {
            TextBox effectiveDateForNewNDCTextBox = ( TextBox )sender;

            FormView containingFormView = ( FormView )effectiveDateForNewNDCTextBox.NamingContainer;

            NDCChangeContent NDCChangeContent = ( NDCChangeContent )containingFormView.DataItem;

            string effectiveDateString = "";

            if( NDCChangeContent != null )
            {
                DateTime effectiveDate = NDCChangeContent.EffectiveDateForNewNDC;

                if( effectiveDate.CompareTo( DateTime.MinValue ) != 0 )
                {
                    effectiveDateString = effectiveDate.Date.ToString( "d" );
                }
            }

            effectiveDateForNewNDCTextBox.Text = effectiveDateString;
        }

        protected void CopySubItemsCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox copySubItemsCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )copySubItemsCheckBox.NamingContainer;

            NDCChangeContent NDCChangeContent = ( NDCChangeContent )containingFormView.DataItem;

            bool bIsChecked = false;

            if( NDCChangeContent != null )
            {
                bIsChecked = NDCChangeContent.CopySubItems;
            }

            copySubItemsCheckBox.Checked = bIsChecked;
        }

        protected void CopyPricingCheckBox_OnDataBinding( object sender, EventArgs e )
        {
            CheckBox copyPricingCheckBox = ( CheckBox )sender;

            FormView containingFormView = ( FormView )copyPricingCheckBox.NamingContainer;

            NDCChangeContent NDCChangeContent = ( NDCChangeContent )containingFormView.DataItem;

            bool bIsChecked = false;

            if( NDCChangeContent != null )
            {
                bIsChecked = NDCChangeContent.CopyPricing;
            }

            copyPricingCheckBox.Checked = bIsChecked;
        }

        private void MultiLineButtonText( Button button, string[] buttonTextArray )
        {

            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < buttonTextArray.Count(); i++ )
            {
                sb.AppendLine( buttonTextArray[ i ] );
            }

            button.Text = sb.ToString();
        }

        protected void NDCChangeScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "NDCChangeErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "NDCChangeErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            NDCChangeScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void NDCChangeFormView_OnPreRender( object sender, EventArgs e )
        {
        }
    }
}
