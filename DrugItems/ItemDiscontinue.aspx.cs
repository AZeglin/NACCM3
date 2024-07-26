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
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.CM.DrugItems
{
    public partial class ItemDiscontinue : System.Web.UI.Page
    {
        private ItemDiscontinueWindowParms _itemDiscontinueWindowParms = null;

        private SerializableObjectDataSource _itemDiscontinueDataSource = null;

        private DocumentDataSource _discontinuationReasonDataSource = null;

        ItemDiscontinueContent _itemDiscontinueContent = null;

        protected void Page_Load( object sender, EventArgs e )
        {
            if( Page.IsPostBack == false )
            {
                ClearSessionVariables();
            }

            if( _itemDiscontinueWindowParms != null )
            {
                InitObjectDataSource();
                LoadItemDiscontinuationReasonList();

                if( Page.IsPostBack == false )
                {
                    BindItemDiscontinue();
                    BindItemDiscontinuationReasonList();
                }
            }

            AddClientCloseEvent();
       }

        protected void ItemDiscontinueForm_OnInit( object sender, EventArgs e )
        {
            if( Session[ "ItemDiscontinueWindowParms" ] != null )
            {
                _itemDiscontinueWindowParms = ( ItemDiscontinueWindowParms )Session[ "ItemDiscontinueWindowParms" ];
            }
            else
            {
                throw new Exception( "Error: ItemDiscontinueWindowParms not available for discontinue window presentation." );
            } 
        }

        private void BindItemDiscontinuationReasonList()
        {
            DropDownList DiscontinuationReasonDropDownList = ( DropDownList )ItemDiscontinueFormView.FindControl( "DiscontinuationReasonDropDownList" );
            DiscontinuationReasonDropDownList.DataSource = _discontinuationReasonDataSource;
            DiscontinuationReasonDropDownList.DataTextField = "DiscontinuationReason";
            DiscontinuationReasonDropDownList.DataValueField = "DiscontinuationReasonId";
            DiscontinuationReasonDropDownList.DataBind();
        }

        private void LoadItemDiscontinuationReasonList()
        {
            if( Session[ "DiscontinuationReasonDataSource" ] == null )
            {
                _discontinuationReasonDataSource = new DocumentDataSource( ( BrowserSecurity2 )Page.Session[ "BrowserSecurity" ], DocumentDataSource.TargetDatabases.DrugItem, false );
                _discontinuationReasonDataSource.ID = "DiscontinuationReasonDataSource";
                _discontinuationReasonDataSource.DataSourceMode = SqlDataSourceMode.DataSet;
                _discontinuationReasonDataSource.SelectCommand = "SelectItemDiscontinuationReasons";
                _discontinuationReasonDataSource.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;

                Parameter itemDiscontinuationCategoryParameter = new Parameter( "ItemDiscontinuationCategory", TypeCode.String );
                itemDiscontinuationCategoryParameter.DefaultValue = _itemDiscontinueWindowParms.GetDiscontinuationReasonCategoryStringFromEnum( _itemDiscontinueWindowParms.DiscontinuationReasonCategory );
                _discontinuationReasonDataSource.SelectParameters.Add( itemDiscontinuationCategoryParameter );

                _discontinuationReasonDataSource.SetEventOwnerName( "DiscontinuationReason" );

                Session[ "DiscontinuationReasonDataSource" ] = _discontinuationReasonDataSource;
            }
            else
            {
                _discontinuationReasonDataSource = ( DocumentDataSource )Session[ "DiscontinuationReasonDataSource" ];
                _discontinuationReasonDataSource.RestoreDelegatesAfterDeserialization( this, "DiscontinuationReason" );
            }
        }

 

        private void ClearSessionVariables()
        {
            Session[ "ItemDiscontinueContent" ] = null;
            Session[ "ItemDiscontinueDataSource" ] = null;
            Session[ "ItemDiscontinueCurrentRowOffset" ] = null;
            Session[ "DiscontinuationReasonDataSource" ] = null;
        }

        private void InitObjectDataSource()
        {
            InitItemDiscontinueContent();

            if( Session[ "ItemDiscontinueDataSource" ] == null )
            {
                _itemDiscontinueDataSource = new SerializableObjectDataSource();
                _itemDiscontinueDataSource.ID = "ItemDiscontinueDataSource";
                _itemDiscontinueDataSource.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.ItemDiscontinueContent";
                _itemDiscontinueDataSource.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.ItemDiscontinueManager";
               _itemDiscontinueDataSource.SelectMethod = "GetItemDiscontinueContent";
                _itemDiscontinueDataSource.UpdateMethod = "DiscontinueItem";
                _itemDiscontinueDataSource.SetEventOwnerName( "ItemDiscontinue" );
                _itemDiscontinueDataSource.Selecting += new ObjectDataSourceSelectingEventHandler( _ItemDiscontinueDataSource_Selecting );
                _itemDiscontinueDataSource.Updating += new ObjectDataSourceMethodEventHandler( _ItemDiscontinueDataSource_Updating );
 
                Session[ "ItemDiscontinueDataSource" ] = _itemDiscontinueDataSource;
            }
            else
            {
                _itemDiscontinueDataSource = ( SerializableObjectDataSource )Session[ "ItemDiscontinueDataSource" ];
                _itemDiscontinueDataSource.RestoreDelegatesAfterDeserialization( this, "ItemDiscontinue" );
            }

            ItemDiscontinueFormView.DataSource = _itemDiscontinueDataSource;
        }

  

        private void InitItemDiscontinueContent()
        {
            if( Session[ "ItemDiscontinueContent" ] == null )
            {
                _itemDiscontinueContent = new ItemDiscontinueContent( _itemDiscontinueWindowParms );
                Session[ "ItemDiscontinueContent" ] = _itemDiscontinueContent;
            }
            else
            {
                _itemDiscontinueContent = ( ItemDiscontinueContent )Session[ "ItemDiscontinueContent" ];
            }
        }

        private void BindItemDiscontinue()
        {
            ItemDiscontinueFormView.DataBind();
            BindItemDiscontinuationReasonList();
        }

        void _ItemDiscontinueDataSource_Selecting( object sender, ObjectDataSourceSelectingEventArgs e )
        {
        }

        void _ItemDiscontinueDataSource_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            ItemDiscontinueContent itemDiscontinueContentForUpdate = ( ItemDiscontinueContent )e.InputParameters[ "ItemDiscontinueContent" ];
            ItemDiscontinueContent itemDiscontinueContent = ( ItemDiscontinueContent )Session[ "ItemDiscontinueContent" ];

            itemDiscontinueContentForUpdate.FillItemDiscontinueContent( itemDiscontinueContent );

            SaveScreenValuesToObject( itemDiscontinueContentForUpdate );

            Session[ "ItemDiscontinueContent" ] = itemDiscontinueContentForUpdate;

            SaveCurrentRowOffset( itemDiscontinueContentForUpdate );
        }

        private void SaveScreenValuesToObject( ItemDiscontinueContent itemDiscontinueContent )
        {
            string fdaAssignedLabelerCode = "";
            string productCode = "";
            string packageCode = "";
            string tradeName = "";
            string tradeNameCleansed = "";
            string genericName = "";
            string genericNameCleansed = "";
            string dispensingUnit = "";
            string packageDescription = "";

            string selectedDiscontinuationReasonString = "";
            DateTime discontinuationDate = DateTime.Today;

            FormView ItemDiscontinueFormView = ( FormView )ItemDiscontinueForm.FindControl( "ItemDiscontinueFormView" );
            if( ItemDiscontinueFormView != null )
            {
                if( ItemDiscontinueFormView.CurrentMode == FormViewMode.Edit )
                {
                    TextBox FdaAssignedLabelerCodeTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "FdaAssignedLabelerCodeTextBox" );
                    if( FdaAssignedLabelerCodeTextBox != null )
                        fdaAssignedLabelerCode = FdaAssignedLabelerCodeTextBox.Text;

                    TextBox ProductCodeTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "ProductCodeTextBox" );
                    if( ProductCodeTextBox != null )
                        productCode = ProductCodeTextBox.Text;

                    TextBox PackageCodeTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "PackageCodeTextBox" );
                    if( PackageCodeTextBox != null )
                        packageCode = PackageCodeTextBox.Text;

                    TextBox TradeNameTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "TradeNameTextBox" );
                    if( TradeNameTextBox != null )
                    {
                        tradeName = TradeNameTextBox.Text;
                        tradeNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( tradeName ).Trim();
                    }

                    TextBox GenericNameTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "GenericNameTextBox" );
                    if( GenericNameTextBox != null )
                    {
                        genericName = GenericNameTextBox.Text;
                        genericNameCleansed = CMGlobals.ReplaceNonPrintableCharacters( genericName ).Trim();
                    }

                    TextBox DispensingUnitTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "DispensingUnitTextBox" );
                    if( DispensingUnitTextBox != null )
                        dispensingUnit = DispensingUnitTextBox.Text;

                    TextBox PackageDescriptionTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "PackageDescriptionTextBox" );
                    if( PackageDescriptionTextBox != null )
                        packageDescription = PackageDescriptionTextBox.Text;

                    DropDownList DiscontinuationReasonDropDownList = ( DropDownList )ItemDiscontinueFormView.FindControl( "DiscontinuationReasonDropDownList" );
                    if( DiscontinuationReasonDropDownList != null )
                        if( DiscontinuationReasonDropDownList.SelectedItem != null )
                            selectedDiscontinuationReasonString = DiscontinuationReasonDropDownList.SelectedItem.Text;

                    TextBox DiscontinuationDateTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "DiscontinuationDateTextBox" );
                    if( DiscontinuationDateTextBox != null )
                        discontinuationDate = DateTime.Parse( DiscontinuationDateTextBox.Text );

                    itemDiscontinueContent.FdaAssignedLabelerCode = fdaAssignedLabelerCode.Trim();
                    itemDiscontinueContent.ProductCode = productCode.Trim();
                    itemDiscontinueContent.PackageCode = packageCode.Trim();
                    itemDiscontinueContent.TradeName = tradeNameCleansed.Trim();
                    itemDiscontinueContent.GenericName = genericNameCleansed.Trim();
                    itemDiscontinueContent.DispensingUnit = dispensingUnit.Trim();
                    itemDiscontinueContent.PackageDescription = packageDescription.Trim();
                       
                    // editable fields
                    itemDiscontinueContent.DiscontinuationReasonString = selectedDiscontinuationReasonString;
                    itemDiscontinueContent.DiscontinuationDate = discontinuationDate;

                }
            }
        }

        private bool ValidateDiscontinue( ref string validationMessage )
        {
            bool bSuccess = true;

            try
            {
                DateTime discontinuationDate = DateTime.Today;
                TextBox DiscontinuationDateTextBox = ( TextBox )ItemDiscontinueFormView.FindControl( "DiscontinuationDateTextBox" );
                if( DiscontinuationDateTextBox != null )
                {
                    if( DateTime.TryParse( DiscontinuationDateTextBox.Text, out discontinuationDate ) == false )
                    {
                        throw new Exception( "Discontinuation Date is invalid" );
                    }
                }

                DropDownList discontinuationReasonDropDownList = ( DropDownList )ItemDiscontinueFormView.FindControl( "DiscontinuationReasonDropDownList" );

                int itemDiscontinuationReasonId = -1;

                if( discontinuationReasonDropDownList != null )
                {
                    ListItem selectedItem = discontinuationReasonDropDownList.SelectedItem;
                    if( selectedItem != null )
                    {
                        itemDiscontinuationReasonId = int.Parse( selectedItem.Value );

                        if( itemDiscontinuationReasonId == -1 )
                        {
                            throw new Exception( "Discontinuation Reason is not specified" );
                        }
                    }
                }

            }
            catch( Exception ex )
            {
                bSuccess = false;
                validationMessage = string.Format( "The following exception was encountered validating the item discontinuation information: {0}", ex.Message );
            }

            return ( bSuccess );
        }

        private void AddClientCloseEvent()
        {
            string closeFunctionText = "CloseWindow( \"true\", \"true\", \"false\" );";
            CancelItemDiscontinueButton.Attributes.Add( "onclick", closeFunctionText );
        }

        private void CloseWindow()
        {
            string closeWindowScript = "CloseWindow( \"true\", \"true\", \"refresh\" );";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CloseWindowScript", closeWindowScript, true );

            // allow the postback to occur 
            UpdatePanelEventProxy itemDiscontinueUpdateUpdatePanelEventProxy = ( UpdatePanelEventProxy )ItemDiscontinueForm.FindControl( "ItemDiscontinueUpdateUpdatePanelEventProxy" );
            itemDiscontinueUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }

        // affects highlighting of current item in item grid
        private void SaveCurrentRowOffset( ItemDiscontinueContent itemDiscontinueContent )
        {
            int offset = 0;

            if( itemDiscontinueContent.DiscontinuationDate.CompareTo( DateTime.Today ) < 0 )
                offset = -1;

            Session[ "ItemDiscontinueCurrentRowOffset" ] = offset;
        }

        private void SetItemDiscontinueHeaderInfo()
        {
            SelectedDrugItemHeader.HeaderTitle = "Discontinue Item";
 
            SelectedDrugItemHeader.FdaAssignedLabelerCode = _itemDiscontinueWindowParms.FdaAssignedLabelerCode;
            SelectedDrugItemHeader.ProductCode = _itemDiscontinueWindowParms.ProductCode;
            SelectedDrugItemHeader.PackageCode = _itemDiscontinueWindowParms.PackageCode;
            SelectedDrugItemHeader.Generic = _itemDiscontinueWindowParms.GenericName;
            SelectedDrugItemHeader.TradeName = _itemDiscontinueWindowParms.TradeName;
        }

   


        protected void ItemDiscontinueFormView_OnDataBound( Object sender, EventArgs e )
        {
            SetItemDiscontinueHeaderInfo();

            //// populate current values for edit
            //FormView itemDiscontinueFormView = ( FormView )sender;
            //TextBox discontinuationDateTextBox = ( TextBox )itemDiscontinueFormView.FindControl( "DiscontinuationDateTextBox" );
            //if( discontinuationDateTextBox != null )
            //{
            //    if( _itemDiscontinueWindowParms.DiscontinuationDateString.Length > 0 )
            //    {
            //        discontinuationDateTextBox.Text = _itemDiscontinueWindowParms.DiscontinuationDateString;
            //    }
            //}

            ItemDiscontinueUpdateUpdatePanelEventProxy.InvokeEvent( new EventArgs() );
        }



        protected void DiscontinueItemButton_OnClick( object sender, EventArgs e )
        {
 
           string validationMessage = "";
 
            try
            {
                if( ValidateDiscontinue( ref validationMessage ) == true )
                {
                    _itemDiscontinueDataSource.Update();
                    ItemDiscontinueFormView.DataBind();
                    BindItemDiscontinuationReasonList();
    
                    CloseWindow();
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
            Button DiscontinueItemButton = ( Button )ItemDiscontinueForm.FindControl( "DiscontinueItemButton" );
            if( DiscontinueItemButton != null )
            {
                DiscontinueItemButton.Enabled = false;
            }
        }

        protected void CancelItemDiscontinueButton_OnClick( object sender, EventArgs e )
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

        // select discontinuation reason from drop down during edit
        protected void DiscontinuationReasonDropDownList_OnDataBound( object sender, EventArgs e )
        {
            DropDownList discontinuationReasonDropDownList = ( DropDownList )sender;
            FormView containingFormView = ( FormView )discontinuationReasonDropDownList.NamingContainer;

            ItemDiscontinueContent ItemDiscontinueContent = ( ItemDiscontinueContent )containingFormView.DataItem;

            string discontinuationReasonString = "";

            if( ItemDiscontinueContent != null )
            {
                discontinuationReasonString = ItemDiscontinueContent.DiscontinuationReasonString;
            }

            ListItem listItem = discontinuationReasonDropDownList.Items.FindByText( discontinuationReasonString ); 
            if( listItem != null )
            {
                listItem.Selected = true;
            }
            else
            {
                ListItem newItem = new ListItem( "Unspecified", "-1" );
                discontinuationReasonDropDownList.Items.Add( newItem );
                newItem.Selected = true;
            }

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

        protected void ItemDiscontinueScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "ItemDiscontinueErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "ItemDiscontinueErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            ItemDiscontinueScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

        protected void ItemDiscontinueFormView_OnPreRender( object sender, EventArgs e )
        {

        }

    }
}
