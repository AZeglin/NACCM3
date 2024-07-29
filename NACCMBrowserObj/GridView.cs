using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using AjaxControlToolkit;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void TotalRowsForPagingChangedEventHandler( GridView theGridView, PagerCommandEventArgs args );

    public class GridView : System.Web.UI.WebControls.GridView  //, System.Web.UI.WebControls.IPageableItemContainer
    {
        private IOrderedDictionary _insertValues;
   //     private int _dataSourceCount;
        private GridViewRow _insertRow;
  //      private GridViewRow _headerRow;
        private char _defaultCommandArgumentSeparator = ',';


        public event ContextMenuCommandEventHandler ContextMenuItemCommand;
        public event TotalRowsForPagingChangedEventHandler TotalRowsForPagingChangedEvent;

        // fires before a row is inserted.
        [Category( "Action" )]
        [Description( "Fires before a row is inserted." )]
        public event EventHandler<GridViewInsertEventArgs> RowInserting;

        // fires after a row has been inserted.
        [Category( "Action" )]
        [Description( "Fires after a row has been inserted." )]
        public event EventHandler<GridViewInsertedEventArgs> RowInserted;

        // The row in the grid that contains the insert controls.
        [Browsable( false )]
        public GridViewRow InsertRow
        {
            get { return this._insertRow; }
        }


        // Gets the row in the grid that corresponds to the header.
        //[Browsable( false )]
        //public override GridViewRow HeaderRow
        //{
        //    get
        //    {
        //        if( this._myHeaderRow != null )
        //            return this._myHeaderRow;
        //        else
        //            return base.HeaderRow;
        //    }
        //}


        [DefaultValue( false )]
        [Category( "Behavior" )]
        [Description( "Whether the GridView will allow rows to be inserted." )]
        public bool AllowInserting
        {
            get
            {
                if( this.ViewState[ "AllowInserting" ] == null )
                    return false;
                else
                    return ( bool )this.ViewState[ "AllowInserting" ];
            }
            set
            {
                if( this.AllowInserting != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "AllowInserting" ] = value;
            }
        }
      
        [DefaultValue( 0 )]
        [Category( "Behavior" )]
        [Description( "Total rows in the resultset." )]
        public int TotalRowsForPaging
        {
            get
            {
                if( this.ViewState[ "TotalRowsForPaging" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "TotalRowsForPaging" ];
            }
            set
            {
                if( this.TotalRowsForPaging != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "TotalRowsForPaging" ] = value;
            }
        }

        [DefaultValue( 0 )]
        [Category( "Behavior" )]
        [Description( "Number of rows to be displayed when paging." )]
        public int RowsPerPage
        {
            get
            {
                if( this.ViewState[ "RowsPerPage" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "RowsPerPage" ];
            }
            set
            {
                if( this.RowsPerPage != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "RowsPerPage" ] = value;
            }
        }

        [DefaultValue( 0 )]
        [Category( "Behavior" )]
        [Description( "Row number of the top row on the current page." )]
        public int CurrentStartingRow
        {
            get
            {
                if( this.ViewState[ "CurrentStartingRow" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "CurrentStartingRow" ];
            }
            set
            {
                if( this.CurrentStartingRow != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "CurrentStartingRow" ] = value;
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "Data pager control id." )]
        public string PagerID
        {
            get
            {
                if( this.ViewState[ "PagerID" ] == null )
                    return ( "" );
                else
                {
                    string pagerID = ( string )this.ViewState[ "PagerID" ];
                    return ( pagerID );
                }
            }
            set
            {
                if( this.PagerID != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "PagerID" ] = value;
            }
        }

        // raise the row count available event
        private void RaiseCountAvailableEvent( int totalRowCount )
        {
            // only raise if updated
            if( TotalRowsForPaging != totalRowCount )
            {
                TotalRowsForPaging = totalRowCount;
                
                if( TotalRowsForPagingChangedEvent != null )
                {
                    TotalRowsForPagingChangedEvent( this, new PagerCommandEventArgs( "TotalRowsForPagingUpdate", totalRowCount ) );
                }
            }
        }


        [DefaultValue( false )]
        [Category( "Appearance" )]
        [Description( "Whether the insert row is active." )]
        public bool InsertRowActive
        {
            get
            {
                if( this.ViewState[ "InsertRowActive" ] == null )
                {
                    return ( false ); // default
                }
                else
                {
                    return( ( bool )this.ViewState[ "InsertRowActive" ] );
                }
            }
            set
            {
                if( this.InsertRowActive != value )
                {
                    this.ViewState[ "InsertRowActive" ] = value;

                    if( base.Initialized == true )
                    {
              //          base.RequiresDataBinding = true;
                    }
                }
            }
        }

        private bool DisplayInsertRow
        {
            get
            { 
                return( this.AllowInserting && this.PageIndex == 0 && this.InsertRowActive ); 
            }
        }

        // currently only used as a flag, setting context menu object directly into the grid on Page Init
        // replaces the need for the ID
        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "Right-click context menu." )]
        public string ContextMenuID
        {
            get
            {
                if( this.ViewState[ "ContextMenuID" ] == null )
                    return ( "" );
                else
                {
                    string contextMenuID = ( string )this.ViewState[ "ContextMenuID" ];
                    return ( contextMenuID );
                    //ContextMenu contextMenu = ( ContextMenu )FindControlRecursive( this.NamingContainer, contextMenuID );
                    //if( contextMenu != null )
                    //    return ( contextMenu.ClientID );
                    //return ( "" );
                }
            }
            set
            {
                if( this.ContextMenuID != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "ContextMenuID" ] = value;
            }
        }


        // currently not used, setting context menu object directly into the grid on Page Init
        // replaces the need for the ID or this flag
        [DefaultValue( false )]
        [Category( "Behavior" )]
        [Description( "Is context menu in an update panel?" )]
        public bool IsContextMenuInUpdatePanel
        {
            get
            {
                if( this.ViewState[ "IsContextMenuInUpdatePanel" ] == null )
                    return ( false );
                else
                {
                    bool bIsContextMenuInUpdatePanel = ( bool )this.ViewState[ "IsContextMenuInUpdatePanel" ];
                    return ( bIsContextMenuInUpdatePanel );
                }
            }
            set
            {
                if( this.IsContextMenuInUpdatePanel != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "IsContextMenuInUpdatePanel" ] = value;
            }
        }


        [DefaultValue( ',' )]
        [Category( "Behavior" )]
        [Description( "The separator character used in command arguments." )]
        public char CommandSeparator
        {
            get
            {
                if( this.ViewState[ "CommandSeparator" ] == null )
                    return ( _defaultCommandArgumentSeparator );
                else
                    return ( char )this.ViewState[ "CommandSeparator" ];
            }
            set
            {
                if( this.CommandSeparator != value )
                    this.RequiresDataBinding = true;

                this.ViewState[ "CommandSeparator" ] = value;
            }
        }

        private Pager _associatedPager = null;

        public void SetPager( Pager associatedPager )
        {
            _associatedPager = associatedPager;
            if( _associatedPager != null )
            {
                _associatedPager.PagerCommand += new PagerCommandEventHandler( GridView_PagerCommand );
            }
        }

        public Pager GetPager()
        {
            return ( _associatedPager );
        }

        // the pager commands will be handled by the instance of the Grid, not the base class. 
        // Putting this here as a temporary place holder in case something is needed later. Else, delete this function.
        protected void GridView_PagerCommand( Pager thePager, PagerCommandEventArgs args )
        {
            string commandName = args.CommandName;

            if( commandName.CompareTo( Pager.CurrentPageUpdateCommand ) == 0 )
            {


            }
            else if( commandName.CompareTo( Pager.RowsPerPageUpdateCommand ) == 0 )
            {
                RowsPerPage = args.RowsPerPage;
             //   TotalRowsForPaging = args.TotalRowsInDataSet;    //  this is set in another event
             //   CurrentStartingRow = args.PageNumber * args.RowsPerPage;        // this is currently not required and is untested
            }
        }

        private ContextMenu _associatedContextMenu = null;

        public void SetContextMenu( ContextMenu associatedContextMenu )
        {
            _associatedContextMenu = associatedContextMenu;
            if( _associatedContextMenu != null )
            {
                 _associatedContextMenu.ItemCommand += new ContextMenuCommandEventHandler( contextMenu_ItemCommand );
         //       Page.ClientScript.RegisterHiddenField( GetContextMenuRowIndexFieldName(), "-1" );

            }
        }

        // currently not used, setting context menu object directly into the grid on Page Init
        // replaces the need for the ID
        public string GetContextMenuClientID()
        {
            return ( ContextMenu.GetContextMenuClientID( ContextMenuID ) );
        }

        // currently not used, setting context menu object directly into the grid on Page Init
        // replaces the need for the ID
        public string GetContextMenuIdForSearch()
        {
            if( IsContextMenuInUpdatePanel == true )
                return ( GetContextMenuClientID() );
            else
                return ( ContextMenuID );
        }

        // the GridViewRow where the ContextMenu was right-clicked
        public GridViewRow RightClickedRow
        {
            get
            {
                if( string.IsNullOrEmpty( ContextMenuID ) == false )
                {
                    // get the selected row id from the hidden field
                    int rowID = -1;
                    string rowIDString = _associatedContextMenu.GetCurrentMenuContextValue();

                    if( Int32.TryParse( rowIDString, out rowID ) == true )
                    {
                        if( rowID != -1 )
                        {
                            return ( this.Rows[ rowID ] );
                        }
                        else
                        {
                            return ( null );
                        }
                    }
                    else
                    {
                        return ( null );
                    }
                }
                else
                {
                    return ( null );
                }
            }
        }

        public GridView() : base()
        {
            PagerSettings.Visible = false;
        }
        
		protected virtual void OnRowInserting( GridViewInsertEventArgs e )
		{
			if( this.RowInserting != null )
				this.RowInserting( this, e );
			else if ( !this.IsBoundUsingDataSourceID && !e.Cancel )
				throw new System.Web.HttpException(string.Format("The GridView '{0}' fired event RowInserting which wasn't handled.", this.ID));
		}

		protected virtual void OnRowInserted( GridViewInsertedEventArgs e )
		{
			if( this.RowInserted != null )
				this.RowInserted( this, e );
		}

        // insert a row after the header
        public void Insert()
        {
            InsertRowActive = true;
        }

        //private int InsertNewRow()
        //{
        //    // create insertion row
        //    int newRowIndex = 1; // this.Controls[ 0 ].Controls.IndexOf( this.HeaderRow ) + 1;
        //    this._insertRow = this.CreateRow( newRowIndex, 0, DataControlRowType.DataRow, DataControlRowState.Insert ); // this.InsertRowActive ? DataControlRowState.Insert : DataControlRowState.Normal );
        //    this._insertRow.ControlStyle.MergeWith( this.AlternatingRowStyle );

        //    // create fields
        //    ICollection cols = this.CreateColumns( null, false );
        //    DataControlField[] fields = new DataControlField[ cols.Count ];
        //    cols.CopyTo( fields, 0 );

        //    // add field to new row
        //    this.InitializeRow( this._insertRow, fields );

        //    // adjust fields for insert
        //    SetVisibleControlsForCell( this._insertRow, 2, "EditButton", false );
        //    SetVisibleControlsForCell( this._insertRow, 2, "SaveButton", true );
        //    SetVisibleControlsForCell( this._insertRow, 2, "CancelButton", true );

        //    // raise row created event
        //    GridViewRowEventArgs insertRowArgs = new GridViewRowEventArgs( this._insertRow );
        //    this.OnRowCreated( insertRowArgs );

        //    // add row to top of table, just below header
        //    this.Controls[ 0 ].Controls.AddAt( newRowIndex, this._insertRow );

        //    // raise databound event for the new row
        //    // if( dataBinding )
        //    this.OnRowDataBound( insertRowArgs );

        //    return ( newRowIndex );

        //}

        // $$$ put back if binding test doesnt work
        //protected override void OnRowCreated( GridViewRowEventArgs e )
        //{
        //    if( e.Row.RowType == DataControlRowType.DataRow && e.Row.RowState == DataControlRowState.Insert )
        //    {
        //        Button editButton = ( Button )e.Row.FindControl( "EditButton" );

        //        if( editButton != null )
        //        {
        //            editButton.CommandArgument = String.Format( "{0},0", e.Row.RowIndex.ToString() );
        //        }

        //        Button saveButton = ( Button )e.Row.FindControl( "SaveButton" );

        //        if( saveButton != null )
        //        {
        //            saveButton.CommandArgument = String.Format( "{0},0", e.Row.RowIndex.ToString() );
        //        }

        //        Button cancelButton = ( Button )e.Row.FindControl( "CancelButton" );

        //        if( cancelButton != null )
        //        {
        //            cancelButton.CommandArgument = String.Format( "{0},0", e.Row.RowIndex.ToString() );
        //        }
        //    }
        //    base.OnRowCreated( e );
        //} 

        // $$$ put back if binding test doesnt work
        //private void SetVisibleControlsForCell( GridViewRow selectedRow, int columnIndex, string controlId, bool bVisible )
        //{
        //    ControlCollection controlsInCell = null;
        //    string tempId = "";

        //    if( selectedRow != null )
        //    {
        //        controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

        //        if( controlsInCell != null )
        //        {
        //            foreach( Control control in controlsInCell )
        //            {
        //                if( control != null )
        //                {
        //                    Type controlType = control.GetType();
        //                    string typeName = controlType.Name;
        //                    if( typeName.CompareTo( "Button" ) == 0 )
        //                    {
        //                        if( control.ID.CompareTo( controlId ) == 0 )
        //                        {
        //                            ( ( Button )control ).Visible = bVisible;
        //                        }
        //                    }
        //                    else // debug
        //                    {
        //                        tempId = control.UniqueID;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //protected override GridViewRow CreateRow( int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState )
        //{
        //    GridViewRow returnRow = null;
        //    returnRow = base.CreateRow( rowIndex, dataSourceIndex, rowType, rowState );

        //    if( rowState == DataControlRowState.Insert )
        //    {
        //        // add the command cell into the insert row

        //    }
        //}

        // $$$ put back if binding test doesnt work
        // creates the control's child controls, including the inserted row if required
        //protected override int CreateChildControls( IEnumerable dataSource, bool dataBinding )
        //{
        //    int controlsCreated = base.CreateChildControls( dataSource, dataBinding );

        //    if( this.DisplayInsertRow )
        //    {
        //        // create insertion row
        //        int newRowIndex = 1; // this.Controls[ 0 ].Controls.IndexOf( this.HeaderRow ) + 1;
        //        this._insertRow = this.CreateRow( newRowIndex, 0, DataControlRowType.DataRow, DataControlRowState.Insert ); // this.InsertRowActive ? DataControlRowState.Insert : DataControlRowState.Normal );
        //        this._insertRow.ControlStyle.MergeWith( this.AlternatingRowStyle );

        //        // create fields
        //        ICollection cols = this.CreateColumns( null, false );
        //        DataControlField[] fields = new DataControlField[ cols.Count ];
        //        cols.CopyTo( fields, 0 );

        //        // add field to new row
        //        this.InitializeRow( this._insertRow, fields );

        //        // adjust fields for insert
        //        SetVisibleControlsForCell( this._insertRow, 2, "EditButton", false );
        //        SetVisibleControlsForCell( this._insertRow, 2, "SaveButton", true );
        //        SetVisibleControlsForCell( this._insertRow, 2, "CancelButton", true );

        //        // handle no rows/data scenario
        //        if( this.Controls.Count == 0 )
        //        {
        //            // create dummy table for inserting the first ( ever ) entry
        //            Table tableControl = new Table();

        //            if( this.ShowHeader )
        //            {
        //                // create header
        //                this._headerRow = this.CreateRow( -1, -1, DataControlRowType.Header, DataControlRowState.Normal );
        //                this.InitializeRow( this._headerRow, fields );

        //                // raise events
        //                GridViewRowEventArgs headerRowArgs = new GridViewRowEventArgs( this._headerRow );
        //                this.OnRowCreated( headerRowArgs );
        //                tableControl.Rows.Add( this._headerRow );
        //                if( dataBinding )
        //                    this.OnRowDataBound( headerRowArgs );
        //            }
 
        //            // add the dummy table
        //            this.Controls.Add( tableControl );
        //        }
        //        // data?
        //    //    this._insertRow.DataBinding += new EventHandler( _insertRow_DataBinding );


        //        // raise row created event
        //        GridViewRowEventArgs insertRowArgs = new GridViewRowEventArgs( this._insertRow );
        //        this.OnRowCreated( insertRowArgs );

        //        // add row to top of table, just below header
        //        this.Controls[ 0 ].Controls.AddAt( newRowIndex, this._insertRow );

        //        // raise databound event for the new row
        //        if( dataBinding == true )
        //            this.OnRowDataBound( insertRowArgs );
        //    }

        //    return ( controlsCreated );
        //}

        //void _insertRow_DataBinding( object sender, EventArgs e )
        //{
        //    throw new NotImplementedException();
        //}
     
     

        //void Page_LoadComplete( object sender, EventArgs e )
        //{
        //    if( Page.IsPostBack == false )
        //    {
        //        // set up context menu command event
        //        if( string.IsNullOrEmpty( ContextMenuID ) == false )
        //        {
        //            ContextMenu contextMenu = _associatedContextMenu;
        //            if( contextMenu != null )
        //                contextMenu.ItemCommand += new ContextMenuCommandEventHandler( contextMenu_ItemCommand );
        //        }
        //    }
        //}

        protected void contextMenu_ItemCommand( ContextMenu theMenu, ContextMenuCommandEventArgs args )
        {
            if( ContextMenuItemCommand != null )
            {
                string rowIDString = _associatedContextMenu.GetCurrentMenuContextValue();
                args.GridViewRowId = int.Parse( rowIDString );
                args.GridViewRow = RightClickedRow;
                ContextMenuItemCommand( theMenu, args );
            }
        }

   
        protected override void OnRowCreated( GridViewRowEventArgs e )
        {
            base.OnRowCreated( e );

            GridViewRow row = e.Row;

            try
            {

                if( row.RowIndex >= 0 )
                {
                    ContextMenu contextMenu = _associatedContextMenu;
                    if( contextMenu != null )
                    {
                        // oncontextmenu also works
                        string test = GetContextMenuRowIndexFieldName2();

                        // leaving this in, row is blocked from editing
                        row.Attributes.Add( "onmousedown", contextMenu.GetRightClickMenuReference( row.RowIndex, GetContextMenuRowIndexFieldName2() ) );

                        // leaving this in, row can still be edited
                        row.Attributes.Add( "onclick", contextMenu.GetOnClickReference() );
                   
                        // this did nothing
                        //     row.Attributes.Add( "pointerdown", "TestNewClickEvent( event );" );
                        
                        string x = contextMenu.GetEscReference(); 
                        //    row.Attributes.Add( "onkeydown", x );
                        //    row.Attributes.Add( "onKeyPress", contextMenu.GetEscReference() ); // tried keypress, onkeypress and keydown without success
                        //     contextMenu.BoundControls.Add( row );
                    }
                    else
                    {
                        int i = 1;
                    }
                }
            }
            catch( Exception ex )
            {
                string tmp = ex.Message;
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        public string GetContextMenuRowIndexFieldName()
        {
            return ( string.Format( "ContextMenuRowIndex_{0}", this.ID ) );
        }

        public string GetContextMenuRowIndexFieldName2()
        {
            return ( ContextMenu.GetContextMenuCurrentMenuContextHiddenFieldName( ContextMenuID ));
        }

        protected override void OnRowDataBound( GridViewRowEventArgs e )
        {
            int selectedItemId = -1;

            base.OnRowDataBound( e );

            GridViewRow row = e.Row;

            if( InsertRowActive == true )
            {
                if( row.RowIndex >= 0 )
                {
                    selectedItemId = Int32.Parse( this.DataKeys[ row.RowIndex ].Value.ToString() );

                    // dummy insert row
                    if( selectedItemId == 0 )
                    {
                        row.RowState = DataControlRowState.Edit; // $$$ what if used insert?
                    }
                }
            }

            // if paging is supported
            if( PagerID.Trim().Length > 0 )
            {
                // the inserted row will not have the correct total, other rows should
                if( row.RowIndex > 0 )
                {
                    DataRowView rowView = ( DataRowView )row.DataItem;

                    String totalRowsString = rowView[ "TotalRows" ].ToString();

                    int totalRows = 0;
                    if( int.TryParse( totalRowsString, out totalRows ) == true )
                    {
                        RaiseCountAvailableEvent( totalRows );
                    }
                }
            }

            //if( row.RowIndex >= 0 )
            //{
            //    ContextMenu contextMenu = GetContextMenuFromPage();
            //    if( contextMenu != null )
            //    {
            //        row.Attributes.Add( "onclick", contextMenu.GetRightClickMenuReference() );
            //        contextMenu.BoundControls.Add( row );
            //    }
            //}

        }

  //      public const string GridViewAverageRowHeightHiddenFieldName = "GridViewAverageRowHeightHiddenField";

        // insert the script code needed to refresh the UI
        //private void EmbedScriptCode()
        //{
        //    // add the script to declare the function
        //    string js = ReadResourceString( "GridViewScript.js" );

        //    string scriptKey = string.Format( "GridViewScript{0}", this.UniqueID );

        //    if( !Page.ClientScript.IsStartupScriptRegistered( scriptKey ) )
        //        Page.ClientScript.RegisterStartupScript( this.GetType(), scriptKey, js );
        //}

        //// read the specified string resource from the current assembly
        //private string ReadResourceString( string resourceName )
        //{
        //    string script = "";

        //    if( resourceName.Length > 0 )
        //    {
        //        Assembly asm = Assembly.GetExecutingAssembly();
        //        StreamReader reader;
        //        string completeResourceName = string.Format( "{0}.{1}", this.GetType().Namespace, resourceName );

        //        Stream resourceStream = asm.GetManifestResourceStream( completeResourceName );
        //        if( resourceStream != null )
        //        {
        //            reader = new StreamReader( resourceStream );
        //            script = reader.ReadToEnd();
        //            reader.Close();
        //        }
        //        else
        //        {
        //            throw new Exception( "Failed to load GridView supporting javascript resource" );
        //        }
        //    }
        //    return ( script );
      //  }

 //       private HiddenField _gridViewAverageRowHeightHiddenField = null;

        //protected override int CreateChildControls( IEnumerable dataSource, bool dataBinding )
        //{
        //    int rows = base.CreateChildControls( dataSource, dataBinding );

            // add hidden input field to hold average row height
            //HtmlInputHidden gridViewAverageRowHeightHiddenField = new HtmlInputHidden();
            //gridViewAverageRowHeightHiddenField.EnableViewState = true;
            //gridViewAverageRowHeightHiddenField.Name = GridViewAverageRowHeightHiddenFieldName;
            //gridViewAverageRowHeightHiddenField.ID = GridViewAverageRowHeightHiddenFieldName;
            //gridViewAverageRowHeightHiddenField.Attributes.Add( "runat", "server" );

            //gridViewAverageRowHeightHiddenField.Value = "0";
            //this.Controls.Add( gridViewAverageRowHeightHiddenField );

   //         _gridViewAverageRowHeightHiddenField = new HiddenField();
   //         _gridViewAverageRowHeightHiddenField.EnableViewState = true;
   //         _gridViewAverageRowHeightHiddenField.ClientIDMode = System.Web.UI.ClientIDMode.Static;
   //         _gridViewAverageRowHeightHiddenField.ID = GridViewAverageRowHeightHiddenFieldName;
   //         _gridViewAverageRowHeightHiddenField.ValueChanged += gridViewAverageRowHeightHiddenField_ValueChanged;

   ////         _gridViewAverageRowHeightHiddenField.Value = "0";
   //         this.Controls.Add( _gridViewAverageRowHeightHiddenField );

     //       this.Attributes.Add( "onload", String.Format( "SaveGridViewRowHeight( '{0}', '{1}' );", this.ClientID, GridViewAverageRowHeightHiddenFieldName ));

            // add the supporting js into the page
     //       EmbedScriptCode();

            // the below is for the built in pager. We're using a custom control instead.
            ////  if the paging feature is enabled, determine the total number of rows in the datasource
            //if( this.AllowPaging )
            //{
            //    //// if we are databinding, use the number of rows that were created, 
            //    //// otherwise cast the datasource to an Collection and use that as the count
            //    //int totalRowCount = dataBinding ? rows : ( ( ICollection )dataSource ).Count;

            //    ////  raise the row count available event
            //    //IPageableItemContainer pageableItemContainer = this as IPageableItemContainer;
            //    //this.OnTotalRowCountAvailable( new PageEventArgs ( pageableItemContainer.StartRowIndex, pageableItemContainer.MaximumRows, totalRowCount ) );

            //    //  make sure the top and bottom pager rows are not visible
            //    if( this.TopPagerRow != null )
            //        this.TopPagerRow.Visible = false;

            //    if( this.BottomPagerRow != null )
            //        this.BottomPagerRow.Visible = false;
            //}
        //    return rows;
        //}

        //protected override void OnLoad( EventArgs e )
        //{
        //    base.OnLoad( e );

        //    if( Page.IsPostBack == false )
        //    {
        //        //if( string.IsNullOrEmpty( ContextMenuID ) == false )
        //        //    Page.ClientScript.RegisterHiddenField( GetContextMenuRowIndexFieldName(), "-1" );
        //    }

            // calling SaveGridViewRowHeight( gridViewName, rowHeightHiddenFieldName )
            //string saveGridViewRowHeightScript = String.Format( "SaveGridViewRowHeight( '{0}', '{1}' );", this.ClientID, GridViewAverageRowHeightHiddenFieldName );
            //ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SaveGridViewRowHeightScript", saveGridViewRowHeightScript, true ); // runs after controls established

            //HiddenField gridViewAverageRowHeightHiddenField = ( HiddenField )FindControl( GridViewAverageRowHeightHiddenFieldName );

            //if( gridViewAverageRowHeightHiddenField != null )
            //{
            //    int averageRowHeightTemp = 0;
            //    if( int.TryParse( gridViewAverageRowHeightHiddenField.Value, out averageRowHeightTemp ) == true )
            //    {
            //        AverageRowHeight = averageRowHeightTemp;
            //    }
            //}
    

            //if( Page.IsPostBack == false )
            //{
            //    Page.LoadComplete += new EventHandler( Page_LoadComplete );
            //}
      //  }
        
        //protected override void OnInit( EventArgs e )
        //{
        //    EnsureChildControls();
        //    base.OnInit( e );

        //    // calling SaveGridViewRowHeight( gridViewName, rowHeightHiddenFieldName )
        //    //string saveGridViewRowHeightScript = String.Format( "SaveGridViewRowHeight( '{0}', '{1}' );", this.ClientID, GridViewAverageRowHeightHiddenFieldName );
        //    //ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SaveGridViewRowHeightScript", saveGridViewRowHeightScript, true ); // runs after controls established

 
        //}

        //public void CalculateRowHeight()
        //{
        //    // calling SaveGridViewRowHeight( gridViewName, rowHeightHiddenFieldName )
        //    string saveGridViewRowHeightScript = String.Format( "SaveGridViewRowHeight( '{0}', '{1}' );", this.ClientID, GridViewAverageRowHeightHiddenFieldName );
        //    ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "SaveGridViewRowHeightScript", saveGridViewRowHeightScript, true ); // runs after controls established
        //}

        //protected override void OnDataBound( EventArgs e )
        //{
            //HiddenField gridViewAverageRowHeightHiddenField = ( HiddenField )this.FindControl( GridViewAverageRowHeightHiddenFieldName );

            //if( gridViewAverageRowHeightHiddenField != null )
            //{
            //    int averageRowHeightTemp = 0;
            //    if( int.TryParse( gridViewAverageRowHeightHiddenField.Value, out averageRowHeightTemp ) == true )
            //    {
            //        AverageRowHeight = averageRowHeightTemp;
            //    }
            //}
     //   }
		// initialises the row using the fields 
        //protected override void InitializeRow( GridViewRow row, DataControlField[] fields )
        //{
        //    base.InitializeRow( row, fields );

        //    //if( row.RowType == DataControlRowType.Header && this.AscendingImageUrl != null )
        //    //{
        //    //    for (int i = 0; i < fields.Length; i++)
        //    //    {
        //    //        if (this.SortExpression.Length > 0 && fields[i].SortExpression == this.SortExpression)
        //    //        {
        //    //            // Add sort indicator
        //    //            Image sortIndicator = new Image();
        //    //            sortIndicator.ImageUrl = this.SortDirection == SortDirection.Ascending ? this.AscendingImageUrl : this.DescendingImageUrl;
        //    //            sortIndicator.Style.Add(HtmlTextWriterStyle.VerticalAlign, "TextTop");
        //    //            row.Cells[i].Controls.Add(sortIndicator);
        //    //            break;
        //    //        }
        //    //    }
        //    //}

        //    // adjust buttons as required for the insert row
        //    // ( presumes not using an insert button within the row )
        //    if( row == this._insertRow )
        //    {
        //        // adjust only if in insert mode
        //        if( row.RowState == DataControlRowState.Insert )
        //        {
        //            // $$$ taking this out to try InsertVisible = "true" instead - didn't work
        //            TableCell insertCommandCell = row.Cells[ InsertCommandColumnIndex ];

        //            ArrayList dataControlFieldsArrayList = new ArrayList();
        //            fields.CopyTo( dataControlFieldsArrayList, 0 );
        //            DataControlField dataControlField = ( DataControlField )dataControlFieldsArrayList[ InsertCommandColumnIndex ];


        //            Button saveButton = new Button();
        //            saveButton.Text = "Saveo";
        //            saveButton.CommandName = SaveNewCommandName;
        //            //       saveButton.Command += new CommandEventHandler( saveButton_Command );
        //            saveButton.Click += new EventHandler( saveButton_Click );
        //            saveButton.ID = "SaveAfterInsertButton";

        //            //        saveButton.CommandArgument = row.RowIndex.ToString();

        //            Button cancelButton = new Button();
        //            cancelButton.Text = "Cancelo";
        //            cancelButton.CommandName = CancelNewCommandName;
        //            //       cancelButton.Command += new CommandEventHandler( cancelButton_Command );
        //            cancelButton.Click += new EventHandler( cancelButton_Click );
        //            cancelButton.ID = "CancelAfterInsertButton";

        //            //          cancelButton.CommandArgument = row.RowIndex.ToString();

        //            insertCommandCell.Controls.Add( saveButton );
        //            insertCommandCell.Controls.Add( cancelButton );

        //            //foreach( TableCell cell in row.Cells )
        //            //{
        //            //    foreach( Control control in cell.Controls )
        //            //    {
        //            //        if( control.GetType().Name.CompareTo( "Button" ) == 0 )
        //            //        {
        //            //            IButtonControl button = control as IButtonControl;

        //            //            if( button != null )
        //            //            {
        //            //                // replace edit with update and cancel
        //            //                if( button.CommandName.CompareTo( "Edit" ) == 0 )
        //            //                    control.Visible = false;
        //            //                else if( button.CommandName.CompareTo( "Update" ) == 0 )
        //            //                    control.Visible = true;
        //            //                else if( button.CommandName.CompareTo( "Cancel" ) == 0 )
        //            //                    control.Visible = true;
        //            //            }
        //            //        }
        //            //    }
        //            //}
        //        }
        //    }
        //}


        //void cancelButton_Click( object sender, EventArgs e )
        //{
        //}

        //void saveButton_Click( object sender, EventArgs e )
        //{
        //}

       // // handles cancel after insert command button
       // void cancelButton_Command( object sender, CommandEventArgs e )
       // {
       ////     int rowIndex = int.Parse( e.CommandArgument.ToString() );
       // }

       // // handles save after insert command button
       // void saveButton_Command( object sender, CommandEventArgs e )
       // {

       // }

		private bool HandleInsertCallback( int affectedRows, Exception ex )
		{
			GridViewInsertedEventArgs e = new GridViewInsertedEventArgs( this._insertValues, ex );
			this.OnRowInserted( e );
			if( ex != null && !e.ExceptionHandled )
				return( false );

			this.RequiresDataBinding = true;
			return( true );
		}

        // returns null if ContextMenuID is not set
        // currently not used, replaced by setting context menu object directly into the grid on Page Init
        private ContextMenu GetContextMenuFromPage()
        {
            Control returnControl = null;

            // if there is a context menu defined
            if( ContextMenuID.Length > 0 )
            {   
                // changed from this.NamingContainer, ContextMenuID which worked if the menu was at the top
                returnControl = FindControlRecursive( GetParentForm( this ), GetContextMenuIdForSearch() );

                if( returnControl == null )
                    return ( null );

                if( returnControl.GetType().Name.CompareTo( "ContextMenu" ) == 0 )
                {
                    return ( ( ContextMenu )returnControl );
                }
                else
                {
                    return ( null );
                }
            }
            else
            {
                return ( null );
            }
        }

        
        private Control FindControlRecursive( Control control, string controlId )
        {
            Control nextControl = null;

            if( control == null )
                return ( null );

            object tmp = control.ID;
            if( tmp == null )
                return ( null );

            if( control.ID == string.Empty )
                return ( null );

            if( control.ID.CompareTo( controlId ) == 0 )
            {
                return( control );
            }

            if( control.Controls != null )
            {
                foreach( Control testControl in control.Controls )
                {
                    nextControl = this.FindControlRecursive( testControl, controlId );

                    if( nextControl != null )
                    {
                        return ( nextControl );
                    }
                }
            }

            return( null );
        }

        private HtmlForm GetParentForm( Control aControl )
        {
            if( aControl == null )
                return ( null );
            if( aControl.GetType().Name.CompareTo( "HtmlForm" ) == 0 )
                return ( ( HtmlForm )aControl );
            return ( GetParentForm( aControl.Parent ) );
        }

        protected override void OnRowCommand( GridViewCommandEventArgs e )
        {
            int rowIndex = -1;
            string rowItemIdString = "-1";
            bool bValidationPassed = true;
            GridViewRow currentRow = null;

            if( e.CommandName.CompareTo( "Sort" ) == 0 )
            {
                if( e.CommandArgument != null )
                {
                    string sortExpression = e.CommandArgument.ToString();
                }
            }
            else
            {
                // argument is presumed to minimally be row index and row id
                if( e.CommandArgument != null )
                {
                    if( e.CommandArgument.ToString().Length > 0 )
                    {
                        string[] commandArgs = e.CommandArgument.ToString().Split( new char[] { CommandSeparator } );
                        if( commandArgs.Count() > 0 )
                        {
                            rowIndex = Int32.Parse( commandArgs[ 0 ].ToString() );
                            if( commandArgs.Count() > 1 )
                            {
                                rowItemIdString = commandArgs[ 1 ].ToString();
                            }

                            currentRow = this.Rows[ rowIndex ];
                        }
                    }

                    // command when update is clicked on an insert row
                    if( e.CommandName.CompareTo( "Insert" ) == 0 )
                    {
                        // check validation
                        IButtonControl button = e.CommandSource as IButtonControl;
                        if( button != null )
                        {
                            if( button.CausesValidation )
                            {
                                this.Page.Validate( button.ValidationGroup );
                                bValidationPassed = this.Page.IsValid;
                            }
                        }

                        if( bValidationPassed == true )
                        {
                            // gather row values
                            this._insertValues = new OrderedDictionary();
                            this.ExtractRowValues( this._insertValues, this._insertRow, true, false );
                            GridViewInsertEventArgs insertArgs = new GridViewInsertEventArgs( this._insertRow, this._insertValues );

                            // raise user event
                            this.OnRowInserting( insertArgs );

                            // original insert code
                            //if( !insertArgs.Cancel && this.IsBoundUsingDataSourceID )
                            //{
                            //    // Get data source
                            //    DataSourceView data = this.GetData();
                            //    data.Insert( this._insertValues, this.HandleInsertCallback );
                            //}

                            if( !insertArgs.Cancel )
                            {
                                if( !this.IsBoundUsingDataSourceID )
                                {
                                    OnDataBinding( EventArgs.Empty ); // need to revisit this $$$
                                }

                                // use data source insert
                                DataSourceView data = this.GetData();
                                data.Insert( this._insertValues, this.HandleInsertCallback );

                            }
                        }
                    }
                    else
                    {
                        base.OnRowCommand( e );
                    }
                }
            }
        }

        #region WillMoveToGridControl

        public int GetRowIndexFromId( int id, int indexOfGridKeyToMatch )
        {
            int rowIndex = -1;
            int keyValue = 0;

            for( int i = 0; i < this.Rows.Count; i++ )
            {
                // flag to indicate the new row
                keyValue = int.Parse( this.DataKeys[ i ].Values[ indexOfGridKeyToMatch ].ToString() );
                if( keyValue == id )
                {
                    rowIndex = i;
                    break;
                }
            }

            return ( rowIndex );
        }

        public bool HasData()
        {
            bool bHasData = false;

            if( this.DataKeys.Count > 0 )
                bHasData = true;

            return ( bHasData );
        }

        public int GetRowIdFromSelectedIndex( int selectedIndex, int indexOfGridKeyToMatch )
        {
            int selectedItemId = -1;
            if( selectedIndex >= 0 )
            {
                //GridViewRow selectedRow = this.Rows[ selectedIndex ];
                //TableCell aCell = selectedRow.Cells[ 0 ];
                if( this.DataKeys.Count > 0 ) // no data case
                {
                    DataKey dataKeyForRow = this.DataKeys[ selectedIndex ];
                    selectedItemId = Int32.Parse( dataKeyForRow[ indexOfGridKeyToMatch ].ToString() );
                }
            }
            return ( selectedItemId );
        }

        public string GetRowIdStringFromSelectedIndex( int selectedIndex, int indexOfGridKeyToMatch )
        {
            string selectedItemIdString = "-1";
            if( selectedIndex >= 0 )
            {
                //GridViewRow selectedRow = this.Rows[ selectedIndex ];
                //TableCell aCell = selectedRow.Cells[ 0 ];
                if( this.DataKeys.Count > 0 ) // no data case
                {
                    DataKey dataKeyForRow = this.DataKeys[ selectedIndex ];
                    selectedItemIdString = dataKeyForRow[ indexOfGridKeyToMatch ].ToString();
                }
            }
            return ( selectedItemIdString );
        }

        // use only during or after the databound event of the gridview
        public string GetStringValueFromSelectedIndexOnDatabind( int selectedIndex, string databaseResultsetFieldName )
        {
            string cellValue = string.Empty;
            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    DataRowView dataRowView = ( DataRowView )selectedRow.DataItem;
                    if( dataRowView != null )
                    {
                        object cellValueObj = dataRowView[ databaseResultsetFieldName ];
                        if( cellValueObj != null )
                            cellValue = cellValueObj.ToString();
                        else
                            throw new Exception( string.Format( "Null cellValueObj returned for index {0} field name {1}", selectedIndex, databaseResultsetFieldName ) );
                    }
                }
            }
            return ( cellValue );
        }

        // use only during or after the databound event of the gridview
        // $$$ this function is untested
        public Button GetButtonControlFromSelectedIndexOnDatabind( int rowIndex, int columnIndex, string buttonId )
        {
            GridViewRow selectedRow = this.Rows[ rowIndex ];
            ControlCollection controlsInCell = null;
            string tempId = "";
            Button returnButton = null;

            if( selectedRow != null )
            {
                controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Button" ) == 0 )
                            {
                                if( control.ID.CompareTo( buttonId ) == 0 )
                                {
                                    returnButton = ( Button )control;
                                }
                            }
                            else // debug
                            {
                                tempId = control.UniqueID;
                            }
                        }
                    }
                }
            }

            return ( returnButton );
        }

        // can't use this during databind, because grid doesn't have any rows yet
        public string GetStringValueFromSelectedIndexForBoundField( int selectedIndex, string databaseResultsetFieldName )
        {
            string cellValue = string.Empty;
            int cellIndex = -1;

            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    cellIndex = GetIndexFromFieldNameForBoundField( databaseResultsetFieldName );
                    cellValue = selectedRow.Cells[ cellIndex ].Text;
                }
            }
            return ( cellValue );
        }

        public bool GetValueFromSelectedIndexForCheckBoxField( int selectedIndex, string databaseResultsetFieldName )
        {
            bool cellValue = false;
            int cellIndex = -1;

            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    cellIndex = GetIndexFromFieldNameForCheckBoxField( databaseResultsetFieldName );
                    Control control = selectedRow.Cells[ cellIndex ].Controls[ 0 ];
                    if( control != null )
                    {
                        cellValue = ( ( CheckBox )control ).Checked;
                    }
                }
            }
            return ( cellValue );
        }
      
        public void SetCheckBoxValueInControlInCell( int rowIndex, string databaseResultsetFieldName, bool bValueToSet )
        {
            int cellIndex = -1;

            if( rowIndex < 0 ) 
                return;

            GridViewRow selectedRow = this.Rows[ rowIndex ];
            if( selectedRow != null )
            {
                cellIndex = GetIndexFromFieldNameForCheckBoxField( databaseResultsetFieldName );
                Control control = selectedRow.Cells[ cellIndex ].Controls[ 0 ];
                if( control != null )
                {
                    Type controlType = control.GetType();
                    string typeName = controlType.Name;
                    if( typeName.CompareTo( "CheckBox" ) == 0 )
                    {
                        ( ( CheckBox )control ).Checked = bValueToSet;
                    }
                }
            }
        }

        //public const string CRTrap = "javascript:alert('hello!'); var evt = window.event; if( evt ){ if(evt.which || evt.keyCode) { if ((evt.which == 13) || (evt.keyCode == 13)) { evt.stopPropogation(); evt.preventDefault(); return false; } return true; } return true; } ";  //          simulatePriceDefaultButton(evt);";
        /*
         * 
         * 
         * //This gets the required javascript code that fires 
//when the UPDATE link button is clicked:
String evt = Page.ClientScript.GetPostBackClientHyperlink(
              sender as GridView, "Update$" + 
              e.NewEditIndex.ToString());
        */

        public void AddCRTrapToCheckBoxes( int rowIndex, int startingColumnIndex, int endingColumnIndex )
        {           
            if( rowIndex < 0 )
                return;

            StringBuilder js = new StringBuilder();
            js.Append( @"if(event.which || event.keyCode)" );
            js.Append( @"{if ((event.which == 13) || (event.keyCode == 13)) " );
            js.Append( "{return false;}} else {return true;}" );

            ControlCollection controlsInCell = null;
            GridViewRow selectedRow = this.Rows[ rowIndex ];
            if( selectedRow != null )
            {
                for( int columnIndex = startingColumnIndex; columnIndex <= endingColumnIndex; columnIndex++ )
                {
                    controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                    if( controlsInCell != null )
                    {
                        foreach( Control control in controlsInCell )
                        {
                            if( control != null )
                            {
                                Type controlType = control.GetType();
                                string typeName = controlType.Name;
                                if( typeName.CompareTo( "CheckBox" ) == 0 )
                                {
                                    ( ( System.Web.UI.WebControls.CheckBox )control ).Attributes[ "onkeydown" ] = js.ToString();
                                }
                            }
                        }
                    }
                }
            }           
        }

        public void SetCellFocusInPanelInLiteral( int rowIndex, int columnIndex, string nestedTemplatedControlName )
        {
            ControlCollection controlsInCell = null;
            GridViewRow selectedRow = this.Rows[ rowIndex ];
            if( selectedRow != null )
            {
                controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Panel" ) == 0 )
                            {
                                Control nestedControl = selectedRow.FindControl( nestedTemplatedControlName );
                                if( nestedControl != null )
                                {
                                    nestedControl.Focus();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }    

        // control index will usually be zero as there will usually be only one control in a cell
        // if a cell is read only, it gets accessed differently
        public string GetStringValueFromSelectedControl( int selectedIndex, int columnIndex, int controlIndex, bool bIsReadOnly, string nestedTemplatedControlName )
        {
            string cellValue = string.Empty;

            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    if( bIsReadOnly == false )
                    {
                        Control control = selectedRow.Cells[ columnIndex ].Controls[ controlIndex ];
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "LiteralControl" ) == 0 )
                            {
                                Control nestedControl = selectedRow.FindControl( nestedTemplatedControlName );
                                if( nestedControl != null )
                                {
                                    cellValue = GetStringValueFromControl( nestedControl );
                                }
                            }
                            else
                            {
                                cellValue = GetStringValueFromControl( control );
                            }
                        }
                    }
                    else // read only has data directly in cell
                    {
                        cellValue = selectedRow.Cells[ columnIndex ].Text;
                    }
                }
            }
            return ( cellValue );
        }

        private string GetStringValueFromControl( Control control )
        {
            string cellValue = string.Empty;

            if( control != null )
            {
                Type controlType = control.GetType();
                string typeName = controlType.Name;
                string fullName = controlType.FullName;
                if( typeName.CompareTo( "TextBox" ) == 0 )
                {
                    typeName = fullName;
                }

                if( typeName.CompareTo( "Label" ) == 0 )
                {
                    cellValue = ( ( Label )control ).Text;
                }
                else if( typeName.CompareTo( "VA.NAC.NACCMBrowser.BrowserObj.TextBox" ) == 0 )
                {
                    cellValue = ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )control ).Text;
                }
                else if( typeName.CompareTo( "System.Web.UI.WebControls.TextBox" ) == 0 )
                {
                    cellValue = ( ( System.Web.UI.WebControls.TextBox )control ).Text;
                }
                else if( typeName.CompareTo( "CheckBox" ) == 0 )
                {
                    cellValue = ( ( CheckBox )control ).Checked.ToString();
                }
                else if( typeName.CompareTo( "DropDownList" ) == 0 || typeName.CompareTo( "PaddedDropDownList" ) == 0 )
                {
                    cellValue = ( ( System.Web.UI.WebControls.DropDownList )control ).SelectedValue.ToString();
                }
                else if( typeName.CompareTo( "ComboBox" ) == 0 )
                {
                    cellValue = ( ( ComboBox )control ).Text;    //SelectedValue.ToString();
                }
            }

            return ( cellValue );
        }

        public string GetStringValueFromSelectedIndexForTemplateField( int selectedIndex, string containedControlId )
        {
            string cellValue = string.Empty;

            // changed on 4/9/2010 to work with other controls besides labels
            //if( selectedIndex >= 0 )
            //{
            //    GridViewRow selectedRow = this.Rows[ selectedIndex ];
            //    if( selectedRow != null )
            //    {
            //        Label containedLabel = ( Label )selectedRow.FindControl( containedControlId );
            //        if( containedLabel != null )
            //            cellValue = containedLabel.Text;
            //    }
            //}
            //return ( cellValue );

            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    Control containedControl = ( Control )selectedRow.FindControl( containedControlId );
                    if( containedControl != null )
                    {
                        Type controlType = containedControl.GetType();
                        string typeName = controlType.Name;
                        string fullName = controlType.FullName;
                        if( typeName.CompareTo( "TextBox" ) == 0 )
                        {
                            typeName = fullName;
                        }

                        if( typeName.CompareTo( "Button" ) == 0 )
                        {
                            cellValue = ( ( Button )containedControl ).Text;
                        }
                        else if( typeName.CompareTo( "LinkButton" ) == 0 )
                        {
                            cellValue = ( ( LinkButton )containedControl ).Text;
                        }
                        else if( typeName.CompareTo( "Label" ) == 0 )
                        {
                            cellValue = ( ( Label )containedControl ).Text;
                        }
                        else if( typeName.CompareTo( "VA.NAC.NACCMBrowser.BrowserObj.TextBox" ) == 0 )
                        {
                            cellValue = ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )containedControl ).Text;
                        }
                        else if( typeName.CompareTo( "System.Web.UI.WebControls.TextBox" ) == 0 )
                        {
                            cellValue = ( ( System.Web.UI.WebControls.TextBox )containedControl ).Text;
                        }
                    }
                }
            }
            return ( cellValue );
        }

        public void SetStringValueInControlInCell( int rowIndex, int columnIndex, string containedControlId, string textValueToSet )
        {
            if( this.Rows.Count == 0 ) // only one new row in grid being cancelled
                return;

            if( rowIndex < 0 ) // perhaps saving row caused it to fall off of select criteria
                return;

            GridViewRow selectedRow = this.Rows[ rowIndex ];
            ControlCollection controlsInCell = null;

            if( selectedRow != null )
            {
                controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                if( controlsInCell != null )
                {
                   
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            if( control.ID.CompareTo( containedControlId ) == 0 )
                            {
                                Type controlType = control.GetType();
                                string typeName = controlType.Name;
                                string fullName = controlType.FullName;
                                if( typeName.CompareTo( "TextBox" ) == 0 )
                                {
                                    typeName = fullName;
                                }

                                if( typeName.CompareTo( "Button" ) == 0 )
                                {
                                    ( ( Button )control ).Text = textValueToSet;
                                }
                                else if( typeName.CompareTo( "Label" ) == 0 )
                                {
                                    ( ( Label )control ).Text = textValueToSet;
                                }
                                else if( typeName.CompareTo( "VA.NAC.NACCMBrowser.BrowserObj.TextBox" ) == 0 )
                                {
                                    ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )control ).Text = textValueToSet;
                                }
                                else if( typeName.CompareTo( "System.Web.UI.WebControls.TextBox" ) == 0 )
                                {
                                    ( ( System.Web.UI.WebControls.TextBox )control ).Text = textValueToSet;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetEnabledControlsForCell( int rowIndex, int columnIndex, bool bEnabled )
        {
            if( this.Rows.Count == 0 ) // only one new row in grid being cancelled
                return;

            if( rowIndex < 0 ) // perhaps saving row caused it to fall off of select criteria
                return;

            GridViewRow selectedRow = this.Rows[ rowIndex ];
            ControlCollection controlsInCell = null;

            if( selectedRow != null )
            {
                controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Button" ) == 0 )
                            {
                                ( ( Button )control ).Enabled = bEnabled;
                            }
                        }
                    }
                }
            }
        }

        public void SetVisibleControlsForCellInTemplateField( int selectedIndex, string containedControlId, bool bVisible )
        {
            string cellValue = string.Empty;

            if( this.Rows.Count == 0 ) 
                return;

            if( selectedIndex >= 0 )
            {
                GridViewRow selectedRow = this.Rows[ selectedIndex ];
                if( selectedRow != null )
                {
                    Control containedControl = ( Control )selectedRow.FindControl( containedControlId );
                    if( containedControl != null )
                    {
                        Type controlType = containedControl.GetType();
                        string typeName = controlType.Name;
                        string fullName = controlType.FullName;
                        if( typeName.CompareTo( "TextBox" ) == 0 )
                        {
                            typeName = fullName;
                        }

                        if( typeName.CompareTo( "Button" ) == 0 )
                        {
                            ( ( Button )containedControl ).Visible = bVisible;
                        }
                        else if( typeName.CompareTo( "LinkButton" ) == 0 )
                        {
                            ( ( LinkButton )containedControl ).Visible = bVisible;
                        }
                        else if( typeName.CompareTo( "Label" ) == 0 )
                        {
                            ( ( Label )containedControl ).Visible = bVisible;
                        }
                        else if( typeName.CompareTo( "VA.NAC.NACCMBrowser.BrowserObj.TextBox" ) == 0 )
                        {
                            ( ( VA.NAC.NACCMBrowser.BrowserObj.TextBox )containedControl ).Visible = bVisible;
                        }
                        else if( typeName.CompareTo( "System.Web.UI.WebControls.TextBox" ) == 0 )
                        {
                            ( ( System.Web.UI.WebControls.TextBox )containedControl ).Visible = bVisible;
                        }
                    }
                }
            }
        }


        public void SetVisibleControlsForCell( int rowIndex, int columnIndex, string controlId, bool bVisible )
        {
            if( this.Rows.Count == 0 ) // only one new row in grid being cancelled
                return;

            if( rowIndex < 0 ) // perhaps saving row caused it to fall off of select criteria
                return;

            GridViewRow selectedRow = this.Rows[ rowIndex ];
            ControlCollection controlsInCell = null;
            string tempId = "";

            if( selectedRow != null )
            {
                controlsInCell = selectedRow.Cells[ columnIndex ].Controls;

                if( controlsInCell != null )
                {
                    foreach( Control control in controlsInCell )
                    {
                        if( control != null )
                        {
                            Type controlType = control.GetType();
                            string typeName = controlType.Name;
                            if( typeName.CompareTo( "Button" ) == 0 )
                            {
                                if( control.ID.CompareTo( controlId ) == 0 )
                                {
                                    ( ( Button )control ).Visible = bVisible;
                                }
                            }
                            else // debug
                            {
                                tempId = control.UniqueID;
                            }
                        }
                    }
                }
            }
        }

        public int GetIndexFromFieldNameForBoundField( string databaseResultsetFieldName )
        {
            int columnIndex = 0;

            DataControlFieldCollection gvFields = this.Columns;

            foreach( DataControlField field in gvFields )
            {
                if( field.GetType().Name.CompareTo( "BoundField" ) == 0 )
                {
                    BoundField boundField = ( BoundField )field;

                    if( boundField.DataField.CompareTo( databaseResultsetFieldName ) == 0 )
                    {
                        columnIndex = gvFields.IndexOf( field );
                        break;
                    }
                }
                //else if( field.GetType().Name.CompareTo( "TemplateField" ) == 0 )
                //{
                //    TemplateField templateField = ( TemplateField )field;

                //    templateField.n




                //}
            }

            return ( columnIndex );
        }


        private int GetIndexFromFieldNameForCheckBoxField( string databaseResultsetFieldName )
        {
            int columnIndex = 0;

            DataControlFieldCollection gvFields = this.Columns;

            foreach( DataControlField field in gvFields )
            {
                if( field.GetType().Name.CompareTo( "CheckBoxField" ) == 0 )
                {
                    CheckBoxField checkBoxField = ( CheckBoxField )field;

                    if( checkBoxField.DataField.CompareTo( databaseResultsetFieldName ) == 0 )
                    {
                        columnIndex = gvFields.IndexOf( field );
                        break;
                    }
                }
            }

            return ( columnIndex );
        }

        //protected override void OnSorted( EventArgs e )
        //{
        //    base.OnSorted( e );
        //}

        //public string GetStringValueFromSelectedIndex( GridView gv, int selectedIndex, SqlDataSource dataSource, string databaseResultsetFieldName )
        //{
        //    string cellValue = string.Empty;
        //    int selectedRowId = -1;

        //    selectedRowId = GetRowIdFromSelectedIndex( gv, selectedIndex );

        //    if( selectedRowId >= 0 )
        //    {
        //        ControlBindingsCollection 


        //    }


        //}

        #endregion WillMoveToGridControl

    }



        // only to render header summary
        //protected override void RenderContents( HtmlTextWriter writer )
        //{
        //    if( this.ShowResultSummary && this.PageCount != 0 )
        //    {
        //        // Create summary controls
        //        int firstResultIndex = this.PageIndex * this.PageSize;
        //        HtmlGenericControl topSummaryControl = new HtmlGenericControl("div");
        //        topSummaryControl.Style.Add("float", "left");
        //        topSummaryControl.InnerHtml = string.Format("<b>Records:</b> {0} to {1} of {2}", firstResultIndex + 1, firstResultIndex + this.Rows.Count, this.DataSourceCount);
        //        HtmlGenericControl bottomSummaryControl = new HtmlGenericControl("div");
        //        bottomSummaryControl.Style.Add("float", "left");
        //        bottomSummaryControl.InnerHtml = topSummaryControl.InnerHtml;

        //        if (this.PageCount == 1)
        //        {
        //            // Add summary to table at the top
        //            this.Controls[0].Controls.AddAt(0, this.CreateSummaryRow(topSummaryControl));
        //            // Add summary to table at the bottom
        //            this.Controls[0].Controls.Add(this.CreateSummaryRow(bottomSummaryControl));
        //        }
        //        else
        //        {
        //            // Add summary control to top pager
        //            if (this.TopPagerRow != null)
        //                this.TopPagerRow.Cells[0].Controls.Add(topSummaryControl);
        //            // Add summary control to bottom pager
        //            if (this.BottomPagerRow != null)
        //                this.BottomPagerRow.Cells[0].Controls.Add(bottomSummaryControl);
        //        }
        //    }

        //    base.RenderContents(writer);
        //}






        ///// <summary>
        ///// Image that is displayed when <see cref="SortDirection"/> is ascending.
        ///// </summary>
        //[Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        //[Description("Image that is displayed when SortDirection is ascending.")]
        //[Category("Appearance")]
        //public string AscendingImageUrl
        //{
        //    get { return this.ViewState["AscendingImageUrl"] as string; }
        //    set { this.ViewState["AscendingImageUrl"] = value; }
        //}
        ///// <summary>
        ///// Image that is displayed when <see cref="SortDirection"/> is descending.
        ///// </summary>
        //[Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        //[Description("Image that is displayed when SortDirection is descending.")]
        //[Category("Appearance")]
        //public string DescendingImageUrl
        //{
        //    get { return this.ViewState["DescendingImageUrl"] as string; }
        //    set { this.ViewState["DescendingImageUrl"] = value; }
        //}


        // the following few used for paging and results summary
        //private TableRow CreateSummaryRow(Control summaryControl)
        //{
        //    TableRow summaryRow = new TableRow();
        //    TableCell summaryCell = new TableCell();
        //    summaryCell.ColumnSpan = this.HeaderRow.Cells.Count;
        //    summaryRow.Cells.Add(summaryCell);
        //    summaryCell.Controls.Add(summaryControl);
        //    return summaryRow;
        //}        
        //    // The total number of rows in the data source.
        //[Browsable( false )]
        //public int DataSourceCount
        //{
        //    get
        //    {
        //        if( this.Rows.Count == 0 )
        //            return 0;
        //        else if( this.AllowPaging )
        //            return this._dataSourceCount;
        //        else
        //            return this.Rows.Count;
        //    }
        //}

        ///// <summary>
        ///// Whether the results summary should be shown.
        ///// </summary>
        //[DefaultValue( false )]
        //[Category( "Appearance" )]
        //[Description( "Whether the results summary should be shown." )]
        //public bool ShowResultSummary
        //{
        //    get
        //    {
        //        if( this.ViewState[ "ShowResultSummary" ] == null )
        //            return false;
        //        else
        //            return ( bool )this.ViewState[ "ShowResultSummary" ];
        //    }
        //    set { this.ViewState[ "ShowResultSummary" ] = value; }
        //}

        //        /// <summary>
        ///// Create the pager.
        ///// </summary>
        ///// <param name="row">The pager row.</param>
        ///// <param name="columnSpan">The number of columns the pager should span.</param>
        ///// <param name="pagedDataSource">The data source.</param>
        //protected override void InitializePager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource)
        //{
        //    // Nab total row count from data source
        //    this._dataSourceCount = pagedDataSource.DataSourceCount;

        //    #region Bonus code - not in article!
        //    /* Almost as a treat for downloading the code, here's a tip!
        //     * 
        //     * If you regularly define a PagerTemplate that you're
        //     * copy-and-pasting between pages, get rid of it and create the
        //     * controls for your pager in this method.  Voila, you get
        //     * your customised pager every time with no copy-and-paste!
        //     * 
        //     * As an example, this pager used the WebDings font to show
        //     * video player-like controls for moving between pages.  The
        //     * key is to set the CommandName and CommandArgument properties
        //     * of your button controls.  Enjoy! */

        //    // Create table cell for pager
        //    TableCell cell = new TableCell();
        //    cell.ColumnSpan = columnSpan;
        //    cell.Controls.Clear();
        //    // Create pager controls
        //    HtmlGenericControl pageControls = new HtmlGenericControl("div");
        //    pageControls.Style.Add(HtmlTextWriterStyle.FontFamily, "webdings");
        //    pageControls.Style.Add(HtmlTextWriterStyle.FontSize, "medium");
        //    pageControls.Style.Add("float", "right");
        //    LinkButton lnkFirst = new LinkButton();
        //    lnkFirst.ID = "lnkFirst";
        //    lnkFirst.ToolTip = "First page";
        //    lnkFirst.CausesValidation = false;
        //    lnkFirst.Text = "9";
        //    lnkFirst.CommandName = "Page";
        //    lnkFirst.CommandArgument = "First";
        //    lnkFirst.Enabled = (this.PageIndex != 0);
        //    pageControls.Controls.Add(lnkFirst);
        //    pageControls.Controls.Add(new LiteralControl("&nbsp;"));
        //    LinkButton lnkPrevious = new LinkButton();
        //    lnkPrevious.ID = "lnkPrevious";
        //    lnkPrevious.ToolTip = "Previous page";
        //    lnkPrevious.CausesValidation = false;
        //    lnkPrevious.Text = "3";
        //    lnkPrevious.CommandName = "Page";
        //    lnkPrevious.CommandArgument = "Prev";
        //    lnkPrevious.Enabled = lnkFirst.Enabled;
        //    pageControls.Controls.Add(lnkPrevious);
        //    pageControls.Controls.Add(new LiteralControl("&nbsp;"));
        //    LinkButton lnkNext = new LinkButton();
        //    lnkNext.ID = "lnkNext";
        //    lnkNext.ToolTip = "Next page";
        //    lnkNext.CausesValidation = false;
        //    lnkNext.Text = "4";
        //    lnkNext.CommandName = "Page";
        //    lnkNext.CommandArgument = "Next";
        //    lnkNext.Enabled = (this.PageIndex < (this.PageCount - 1));
        //    pageControls.Controls.Add(lnkNext);
        //    pageControls.Controls.Add(new LiteralControl("&nbsp;"));
        //    LinkButton lnkLast = new LinkButton();
        //    lnkLast.ID = "lnkLast";
        //    lnkLast.ToolTip = "Last page";
        //    lnkLast.CausesValidation = false;
        //    lnkLast.Text = ":";
        //    lnkLast.CommandName = "Page";
        //    lnkLast.CommandArgument = "Last";
        //    lnkLast.Enabled = lnkNext.Enabled;
        //    pageControls.Controls.Add(lnkLast);
        //    // Add pager controls to cell
        //    cell.Controls.Add(pageControls);
        //    // Add cell to pager row
        //    row.Cells.Add(cell);
        //    #endregion
        //}

        // version of insert adds a fixed number of new rows at design time
        //private GridViewRowCollection _newRows;
        //private List<int> _changedRows;

        //#region Properties

        //[Browsable( false )]
        //public GridViewRowCollection NewRows
        //{
        //    get
        //    {
        //        return ( this._newRows == null ) ? new GridViewRowCollection( new ArrayList() ) : this._newRows;
        //    }
        //}

        //public int NewRowCount
        //{
        //    get
        //    {
        //        object viewState = this.ViewState[ "NewRowCount" ];
        //        return ( viewState == null ) ? 0 : ( int )viewState;
        //    }
        //    set { this.ViewState[ "NewRowCount" ] = value; }
        //}

        //[Browsable( false )]
        //public GridViewRowCollection NewRowsChanged
        //{
        //    get
        //    {
        //        if( this._changedRows != null )
        //        {
        //            ArrayList changedRows = new ArrayList();

        //            foreach( int rowIndex in this._changedRows )
        //            {
        //                changedRows.Add( this._newRows[ rowIndex ] );
        //            }

        //            return new GridViewRowCollection( changedRows );
        //        }

        //        return new GridViewRowCollection( new ArrayList() );
        //    }
        //}

        //#endregion

        //protected override int CreateChildControls( IEnumerable dataSource, bool dataBinding )
        //{
        //    int rowCount = base.CreateChildControls( dataSource, dataBinding );

        //    this.CreateNewRows();

        //    return rowCount;
        //}

        //private void CreateNewRows()
        //{
        //    if( this.NewRowCount > 0 )
        //    {
        //        ArrayList list = new ArrayList();
        //        DataControlField[] fields = this.GetDataControlFields();
        //        for( int i = 0; i < this.NewRowCount; i++ )
        //        {
        //            GridViewRow newRow = this.CreateNewRow( i, fields );
        //            list.Add( newRow );
        //        }
        //        this._newRows = new GridViewRowCollection( list );

        //        Table grid;
        //        if( this.Rows.Count == 0 )
        //        {
        //            grid = this.CreateChildTable();
        //            this.Controls.Add( grid );
        //            if( this.ShowHeader )
        //            {
        //                GridViewRow headerRow = this.CreateHeaderRow( fields );
        //                grid.Rows.Add( headerRow );
        //            }
        //        }
        //        else
        //        {
        //            grid = ( Table )this.Rows[ 0 ].Parent;
        //        }

        //        int rowIndex = this.Rows.Count + 1;
        //        foreach( GridViewRow newRow in this._newRows )
        //        {
        //            grid.Rows.AddAt( rowIndex, newRow );
        //            rowIndex++;
        //        }
        //    }
        //}

        //private DataControlField[] GetDataControlFields()
        //{
        //    DataControlField[] fields = new DataControlField[ this.Columns.Count ];
        //    base.Columns.CopyTo( fields, 0 );
        //    return fields;
        //}

        //private GridViewRow CreateNewRow( int rowIndex, DataControlField[] fields )
        //{
        //    GridViewRow newRow = base.CreateRow( rowIndex, -1, DataControlRowType.DataRow, DataControlRowState.Insert );
        //    base.InitializeRow( newRow, fields );

        //    this.AddRowChanged( newRow );

        //    return newRow;
        //}

        //private GridViewRow CreateHeaderRow( DataControlField[] fields )
        //{
        //    GridViewRow headerRow = base.CreateRow( -1, -1, DataControlRowType.Header, DataControlRowState.Normal );
        //    base.InitializeRow( headerRow, fields );

        //    return headerRow;
        //}

        //private void AddRowChanged( Control control )
        //{
        //    foreach( Control ctr in control.Controls )
        //    {
        //        if( ctr is TextBox )
        //        {
        //            ( ( TextBox )ctr ).TextChanged += new EventHandler( this.RowChanged );
        //        }
        //        else if( ctr is ListControl )
        //        {
        //            ( ( ListControl )ctr ).SelectedIndexChanged += new EventHandler( this.RowChanged );
        //        }
        //        else if( ctr is CheckBox )
        //        {
        //            ( ( CheckBox )ctr ).CheckedChanged += new EventHandler( this.RowChanged );
        //        }

        //        if( ctr.HasControls() )
        //        {
        //            this.AddRowChanged( ctr );
        //        }
        //    }
        //}

        //private void RowChanged( object sender, EventArgs e )
        //{
        //    GridViewRow row = ( GridViewRow )( ( Control )sender ).NamingContainer;

        //    if( this._changedRows == null )
        //    {
        //        this._changedRows = new List<int>();
        //    }

        //    if( !this._changedRows.Contains( row.RowIndex ) )
        //    {
        //        this._changedRows.Add( row.RowIndex );
        //    }
        //}

        // ideas for having header appear in the event of no data
        // set ShowWhenEmpty to true to display headers and footers when empty
        //[Category( "Behaviour" )]
        //[Themeable( true )]
        //[Bindable( BindableSupport.No )]
        //[Description( "True to display headers and footers when empty." )]
        //[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
        //[NotifyParentProperty( true )]
        //[PersistenceMode( PersistenceMode.InnerProperty )]
        //public bool ShowWhenEmpty
        //{
        //    get
        //    {
        //        if( ViewState[ "ShowWhenEmpty" ] == null )
        //            ViewState[ "ShowWhenEmpty" ] = false;

        //        return ( bool )ViewState[ "ShowWhenEmpty" ];
        //    }
        //    set
        //    {
        //        ViewState[ "ShowWhenEmpty" ] = value;
        //    }
        //}

        //protected override int CreateChildControls( System.Collections.IEnumerable dataSource, bool dataBinding )
        //{
        //    int numRows = base.CreateChildControls(dataSource, dataBinding);

        //    //no data rows created, create empty table if enabled
        //    if (numRows == 0 && ShowWhenEmpty)
        //    {
        //        //create table
        //        Table table = new Table();
        //        table.ID = this.ID;

        //        //convert the exisiting columns into an array and initialize
        //        DataControlField[] fields = new DataControlField[this.Columns.Count];
        //        this.Columns.CopyTo(fields, 0);

        //        if(this.ShowHeader)
        //        {
        //            //create a new header row
        //            GridViewRow headerRow = base.CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal);

        //            this.InitializeRow(headerRow, fields);
        //            table.Rows.Add(headerRow);
        //        }

        //        //create the empty row
        //        GridViewRow emptyRow = new GridViewRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal);

        //        TableCell cell = new TableCell();
        //        cell.ColumnSpan = this.Columns.Count;
        //        cell.Width = Unit.Percentage(100);
        //        if(!String.IsNullOrEmpty(EmptyDataText))
        //            cell.Controls.Add(new LiteralControl(EmptyDataText));

        //        if(this.EmptyDataTemplate != null)
        //            EmptyDataTemplate.InstantiateIn(cell);

        //        emptyRow.Cells.Add(cell);
        //        table.Rows.Add(emptyRow);

        //        if(this.ShowFooter)
        //        {
        //            //create footer row
        //            GridViewRow footerRow = base.CreateRow(-1, -1, DataControlRowType.Footer, DataControlRowState.Normal);

        //            this.InitializeRow(footerRow, fields);
        //            table.Rows.Add(footerRow);
        //        }

        //        this.Controls.Clear();
        //        this.Controls.Add(table);
        //    }

        //    return numRows;
        //}

    }
