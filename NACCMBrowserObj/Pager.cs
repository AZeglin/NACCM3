using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Globalization;
using System.Web.Resources;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Collections;

using AjaxControlToolkit;
using DropDownList = System.Web.UI.WebControls.DropDownList;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void PagerCommandEventHandler( Pager thePager, PagerCommandEventArgs args );

    [Designer( typeof( VA.NAC.NACCMBrowser.BrowserObj.PagerDesigner ) )]
    [DefaultEvent( "PagerCommand" )]
    [DefaultProperty( "RowsPerPage" )]
    public class Pager : WebControl, INamingContainer
    {
        private ArrayList _boundControls;

        public event PagerCommandEventHandler PagerCommand;

        public static string CurrentPageUpdateCommand = "CurrentPageUpdate";
        public static string RowsPerPageUpdateCommand = "RowsPerPageUpdate";

 
        #region Properties

      
        [DefaultValue( false )]
        [Category( "Behavior" )]
        [Description( "Is pager in an update panel?" )]
        public bool IsPagerInUpdatePanel
        {
            get
            {
                bool bIsPagerInUpdatePanel = false;
                if( this.ViewState[ "IsPagerInUpdatePanel" ] == null )
                {
                    return ( false );
                }
                else
                {
                    bIsPagerInUpdatePanel = ( bool )this.ViewState[ "IsPagerInUpdatePanel" ];
                    return ( bIsPagerInUpdatePanel );
                }
            }
            set
            {
                this.ViewState[ "IsPagerInUpdatePanel" ] = value;
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "The image used by the pager to display the next page button." )]
        public string NextPageImageUrl
        {
            get
            {
                string url = string.Empty;
                if( this.ViewState[ "NextPageImageUrl" ] == null )
                {
                    return ( string.Empty );
                }
                else
                {
                    url = ( string )ViewState[ "NextPageImageUrl" ];
                    return ( url );
                }
            }
            set
            {
                if( this.NextPageImageUrl != value )
                {
                    base.ViewState[ "NextPageImageUrl" ] = value;
                }
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "The image used by the pager to display the previous page button." )]
        public string PreviousPageImageUrl
        {
            get
            {
                string url = string.Empty;
                if( this.ViewState[ "PreviousPageImageUrl" ] == null )             
                {
                    return ( string.Empty );
                }
                else
                {
                    url = ( string )ViewState[ "PreviousPageImageUrl" ];
                    return ( url );
                }
            }
            set
            {
                if ( this.PreviousPageImageUrl != value )
                {
                    base.ViewState[ "PreviousPageImageUrl" ] = value;
                }
            }
        }


        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "Container script to run on pager control click events." )]
        public string OnClientClickScript
        {
            get
            {
                string script = string.Empty;
                if( this.ViewState[ "OnClientClickScript" ] == null )
                {
                    return ( string.Empty );
                }
                else
                {
                    script = ( string )ViewState[ "OnClientClickScript" ];
                    return ( script );
                }
            }
            set
            {
                if( this.OnClientClickScript != value )
                {
                    base.ViewState[ "OnClientClickScript" ] = value;
                }
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "Associated control that will be paged." )]
        public string AssociatedControlID
        {
            get
            {
                if( this.ViewState[ "AssociatedControlID" ] == null )
                    return ( "" );
                else
                {
                    string associatedControlID = ( string )this.ViewState[ "AssociatedControlID" ];
                    return ( associatedControlID );
                }
            }
            set
            {              
                this.ViewState[ "AssociatedControlID" ] = value;
            }
        }

        public bool RenderNonBreakingSpacesBetweenControls
        {
            get
            {
                bool bRender = true;             
                if( this.ViewState[ "RenderNonBreakingSpacesBetweenControls" ] == null )
                {
                    return ( true );
                }
                else
                {
                    bRender = ( bool )base.ViewState[ "RenderNonBreakingSpacesBetweenControls" ];
                    return ( bRender );
                }
            }
            set
            {
                if ( this.RenderNonBreakingSpacesBetweenControls != value )
                {
                    base.ViewState[ "RenderNonBreakingSpacesBetweenControls" ] = value;
                }
            }
        }

        public bool IsPagerInitializing
        {
            get
            {
                bool bInitializing = true;
                if( this.ViewState[ "IsPagerInitializing" ] == null )
                {
                    return ( true );
                }               
                else
                {
                    bInitializing = ( bool )base.ViewState[ "IsPagerInitializing" ];
                    return ( bInitializing );
                }
            }
            set
            {
                if( this.IsPagerInitializing != value )
                {
                    base.ViewState[ "IsPagerInitializing" ] = value;
                }
            }
        }

        [DefaultValue( "" )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false )]
        public string GotoTextBoxContents
        {
            get
            {
                if( this.ViewState[ "GotoTextBoxContents" ] == null )
                    return ( "" );
                else
                {
                    string gotoTextBoxContents = ( string )this.ViewState[ "GotoTextBoxContents" ];
                    return ( gotoTextBoxContents );
                }
            }
            set
            {
                this.ViewState[ "GotoTextBoxContents" ] = value;
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
                this.ViewState[ "TotalRowsForPaging" ] = value;
                PageCount = CalculatePageCount( RowsPerPage, value );
                RowsPerPageIndex = CalculateRowsPerPageToUse( value );
          //      GotoPage( 1 );  // for the most part, the count will move by 1 and there is no need to jump
            }
        }

        [DefaultValue( 5 )]
        [Category( "Behavior" )]
        [Description( "Number of rows to be displayed when paging." )]
        public int RowsPerPage
        {
            get
            {
                if( this.ViewState[ "RowsPerPage" ] == null )
                    return 5;
                else
                    return ( int )this.ViewState[ "RowsPerPage" ];
            }
            set
            {
                if( this.RowsPerPage != value )
                {
                    this.ViewState[ "RowsPerPage" ] = value;
                    PageCount = CalculatePageCount( value, TotalRowsForPaging );
                    GotoPage( 1 );
                }
            }
        }

        private int[] _rowsPerPageChoices = { 5, 10, 20, 50, 100, 1000 };    //, 5000 };
        private int RowsPerPageIndex
        {
            get
            {
                if( this.ViewState[ "RowsPerPageIndex" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "RowsPerPageIndex" ];
            }
            set
            {
                if( this.RowsPerPageIndex != value )
                {
                    this.ViewState[ "RowsPerPageIndex" ] = value;
                }
            }

        }

        private int CalculateRowsPerPageToUse( int totalRowsForPaging )
        {
            if( totalRowsForPaging <= 5 )
                return ( 0 );
            else if( totalRowsForPaging > 5 && totalRowsForPaging <= 10 )
                return ( 1 );
            else if( totalRowsForPaging > 10 && totalRowsForPaging <= 20 )
                return ( 2 );
            else if( totalRowsForPaging > 20 && totalRowsForPaging <= 50 )
                return ( 3 );
            else if( totalRowsForPaging > 50 && totalRowsForPaging <= 100 )
                return ( 4 );
            else if( totalRowsForPaging > 100 && totalRowsForPaging <= 1000 )
                return ( 5 );
            //  else if( totalRowsForPaging > 1000 && totalRowsForPaging <= 5000 )
            //      return ( 6 );
            //  else
            //      return ( 6 );
            else
                return ( 5 );
        }

        [DefaultValue( 1 )]
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
                this.ViewState[ "CurrentStartingRow" ] = value;
            }
        }

        [DefaultValue( 1 )]
        [Category( "Behavior" )]
        [Description( "The current selected page." )]
        public int CurrentPage
        {
            get
            {
                if( this.ViewState[ "CurrentPage" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "CurrentPage" ];
            }
            set
            {
                this.ViewState[ "CurrentPage" ] = value;
                CurrentStartingRow = CalculateCurrentStartingRow( value );
            }
        }

        [DefaultValue( 1 )]
        [Category( "Behavior" )]
        [Description( "The row number used to select the current page." )]
        public int PageToRowNumber
        {
            get
            {
                if( this.ViewState[ "PageToRowNumber" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "PageToRowNumber" ];
            }
            set
            {
                this.ViewState[ "PageToRowNumber" ] = value;
                GotoRowNumber( value ); // sets current page to the page containing the row               
            }
        }

        #endregion Properties


        private int PageCount
        {
            get
            {
                if( this.ViewState[ "PageCount" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "PageCount" ];
            }
            set
            {
                if( this.PageCount != value )
                {
                    this.ViewState[ "PageCount" ] = value;
                }
            }

        }

        private int CalculatePageCount( int rowsPerPage, int totalRowsForPaging )
        {
            if( totalRowsForPaging == 0 || rowsPerPage == 0 )
                return ( 0 );
            else
                return ( ( int )Math.Ceiling( ( double )totalRowsForPaging / rowsPerPage ) );
        }

        private int CalculateCurrentStartingRow( int currentPage )
        {            
            return ((( currentPage - 1 ) * RowsPerPage ) + 1 );
        }

        private void IncrementCurrentPage()
        {
            if( CurrentPage < PageCount )
                CurrentPage += 1;
        }

        private void DecrementCurrentPage()
        {
            if( CurrentPage >= 2 )
                CurrentPage -= 1;
        }

        private void GotoRowNumber( int rowNumber )
        {
            if( rowNumber <= 0 | RowsPerPage <= 0 )
                CurrentPage = 1;
            else
                CurrentPage = ( int )Math.Ceiling( ( double )rowNumber / RowsPerPage );
        }

        private void GotoPage( int targetPage )
        {
            int correctedPage = ( targetPage <= 0 ) ? 1 : ( targetPage > PageCount ) ? PageCount : targetPage;
            CurrentPage = correctedPage;
        }

        private void ReflectUserActionInTextBox()
        {
            TextBox gotoTextBox = ( TextBox )this.FindControl( "PagerGotoTextBox" );
            if( gotoTextBox != null )
            {
                gotoTextBox.Text = CurrentPage.ToString();
            }
        }

        private void RaisePagerCommandEvent( string commandName, int currentPage, int rowsPerPage, int totalRowsForPaging )
        {
            if( PagerCommand != null )
            {
                PagerCommand( this, new PagerCommandEventArgs( commandName, currentPage, rowsPerPage, totalRowsForPaging ));
            }
        }

        protected void AssociatedGridView_TotalRowsForPagingChangedEvent( GridView theGridView, PagerCommandEventArgs args )
        {
            int newTotalRowsForPaging = args.TotalRowsInDataSet;

            if( newTotalRowsForPaging != TotalRowsForPaging )
            {               
                TotalRowsForPaging = newTotalRowsForPaging;
                
                // update the display to reflect the change              
                ReflectUserActionInTextBox();
                EnableArrows();
                RefreshStatus();
                RefreshDropDownList();
            }
        }
       
        protected void gotoButton_Command( object sender, CommandEventArgs e )
        {
            Button gotoButton = ( Button )sender;
            Pager thisPager = ( Pager )gotoButton.NamingContainer;
            TextBox gotoTextBox = ( TextBox )thisPager.FindControl( "PagerGotoTextBox" );
            string gotoPageNumberString = gotoTextBox.Text;
            int gotoPageNumber = 1;
            if( int.TryParse( gotoPageNumberString, out gotoPageNumber ) != true )
                return;
            GotoTextBoxContents = gotoPageNumberString;
            GotoPage( gotoPageNumber );
            EnableArrows();
            RaisePagerCommandEvent( CurrentPageUpdateCommand, CurrentPage, RowsPerPage, TotalRowsForPaging );
        }

        protected void previousPage_Command( object sender, CommandEventArgs e )
        {
            DecrementCurrentPage();
            ReflectUserActionInTextBox();
            EnableArrows();
            RaisePagerCommandEvent( CurrentPageUpdateCommand, CurrentPage, RowsPerPage, TotalRowsForPaging );
        }

        protected void nextPage_Command( object sender, CommandEventArgs e )
        {
            IncrementCurrentPage();
            ReflectUserActionInTextBox();
            EnableArrows();
            RaisePagerCommandEvent( CurrentPageUpdateCommand, CurrentPage, RowsPerPage, TotalRowsForPaging );
        }

        protected void rowsPerPageDropDownList_SelectedIndexChanged( object sender, EventArgs e )
        {
            DropDownList rowsPerPageDropDownList = ( DropDownList )sender;
            ListItem selectedItem = rowsPerPageDropDownList.SelectedItem;
            RowsPerPage = int.Parse( selectedItem.Value );
            CurrentPage = 1;
            ReflectUserActionInTextBox();
            EnableArrows();
            RaisePagerCommandEvent( RowsPerPageUpdateCommand, CurrentPage, RowsPerPage, TotalRowsForPaging );
        }

        public Pager()
        {
            this.ClientIDMode = System.Web.UI.ClientIDMode.Predictable;
        }


        private GridView _associatedGridView = null;

        public void SetGridView( GridView associatedGridView )
        {
            _associatedGridView = associatedGridView;
            if( _associatedGridView != null )
            {
                _associatedGridView.TotalRowsForPagingChangedEvent += new TotalRowsForPagingChangedEventHandler( AssociatedGridView_TotalRowsForPagingChangedEvent );
            }
        }

        private void EnableArrows()
        {
            ImageButton previousPageImageButton = ( ImageButton )this.FindControl( "PreviousPageImageButton" );
            if( previousPageImageButton != null )
            {
                previousPageImageButton.Enabled = PreviousPageEnabled;
            }

            ImageButton nextPageImageButton = ( ImageButton )this.FindControl( "NextPageImageButton" );
            if( nextPageImageButton != null )
            {
                nextPageImageButton.Enabled = NextPageEnabled;
            }
        }

        private bool PreviousPageEnabled
        {
            get
            {
                return ( CurrentPage >= 2 );
            }
        }

        private bool NextPageEnabled
        {
            get
            {
                return ( CurrentPage < ( CalculatePageCount( RowsPerPage, TotalRowsForPaging ) ) );
            }
        }


        // determines the standard style of the control
        protected override Style CreateControlStyle()
        {
            Style style = base.CreateControlStyle();
            style.BorderStyle = BorderStyle.Outset;
            style.BorderColor = Color.Snow;
            style.BorderWidth = Unit.Pixel( 1 );
            style.BackColor = Color.FromName( "#eeeeee" );
            style.Font.Name = "Tahoma";
            style.Font.Size = FontUnit.Point( 8 );
            return ( style );
        }

        public const string PagerRowsPerPageHiddenFieldName = "PagerRowsPerPageHiddenField";
        public const string PagerTopRowIndexHiddenFieldName = "PagerTopRowIndexHiddenField";

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return( base.Controls );
            }
        }

        // builds the UI of the control
        protected override void CreateChildControls()
        {
            Controls.Clear();

            // pager is a panel/table with one row and a few controls
            Panel panel = new Panel();
            panel.ID = "PagerPanel";

            Table pagerTable = new Table();
            pagerTable.ApplyStyle( CreateControlStyle() );
            pagerTable.Style[ "table-layout" ] = "fixed";
            pagerTable.Style[ "align" ] = "center";
            pagerTable.Style[ "background-color" ] = "#E3E3E3";
            pagerTable.Style[ "border" ] = "solid 1px";
            pagerTable.Style[ "border-color" ] = "Black"; 
            pagerTable.Style[ "Width" ] = "600px";
            pagerTable.CellSpacing = 1;
            pagerTable.CellPadding = 2;
            panel.Controls.Add( pagerTable );

            // add hidden input fields to hold top row index and rows per page
            HtmlInputHidden pagerRowsPerPageHiddenField = new HtmlInputHidden();
            pagerRowsPerPageHiddenField.EnableViewState = true;
            pagerRowsPerPageHiddenField.Name = PagerRowsPerPageHiddenFieldName;
            pagerRowsPerPageHiddenField.ID = PagerRowsPerPageHiddenFieldName;
            pagerRowsPerPageHiddenField.Attributes.Add( "runat", "server" );

            pagerRowsPerPageHiddenField.Value = RowsPerPage.ToString();
            panel.Controls.Add( pagerRowsPerPageHiddenField );

            HtmlInputHidden pagerTopRowIndexHiddenField = new HtmlInputHidden();
            pagerTopRowIndexHiddenField.EnableViewState = true;
            pagerTopRowIndexHiddenField.Name = PagerTopRowIndexHiddenFieldName;
            pagerTopRowIndexHiddenField.ID = PagerTopRowIndexHiddenFieldName;
            pagerTopRowIndexHiddenField.Attributes.Add( "runat", "server" );

            pagerTopRowIndexHiddenField.Value = CurrentStartingRow.ToString();
            panel.Controls.Add( pagerTopRowIndexHiddenField );

         
            // create and add a row 
            TableRow pagerTableRow = new TableRow();
            pagerTableRow.Style[ "background-color" ] = "#E3E3E3";
            pagerTable.Rows.Add( pagerTableRow );

            // configure the row with 5 cells
            TableCell gotoCell = new TableCell();
            gotoCell.Style[ "width" ] = "28%";          
            pagerTableRow.Cells.Add( gotoCell );
            TableCell prevCell = new TableCell();
            prevCell.Style[ "width" ] = "10%";
            pagerTableRow.Cells.Add( prevCell );
            TableCell statusCell = new TableCell();
            statusCell.Style[ "width" ] = "25%";
            pagerTableRow.Cells.Add( statusCell );
            TableCell nextCell = new TableCell();
            nextCell.Style[ "width" ] = "10%";
            pagerTableRow.Cells.Add( nextCell );
            TableCell rowsPerPageCell = new TableCell();
            rowsPerPageCell.Style[ "width" ] = "27%";
            pagerTableRow.Cells.Add( rowsPerPageCell );

            // goto text box
            Label gotoLabel = new Label();
            gotoLabel.ID = "PagerGotoLabel";
            gotoLabel.Text = "Go To Page: ";
            gotoLabel.Attributes.Add( "runat", "server" );
            gotoCell.Controls.Add( gotoLabel );

            TextBox gotoTextBox = new TextBox();
            
            gotoTextBox.ID = "PagerGotoTextBox";
            gotoTextBox.Width = new Unit( "54px" );
            gotoTextBox.Text = GotoTextBoxContents;
            gotoTextBox.Attributes.Add( "runat", "server" );
            gotoTextBox.Attributes.Add( "MaxLength", "5" );
            gotoCell.Controls.Add( gotoTextBox );

            Button gotoButton = new Button();
            gotoButton.ID = "PagerGotoButton";
            gotoButton.Text = "Go";
            gotoButton.Attributes.Add( "runat", "server" );
            gotoButton.ToolTip = "Go to page entered in textbox";
            if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
            {
                gotoButton.OnClientClick = OnClientClickScript;
            }
            gotoButton.Command += new CommandEventHandler( gotoButton_Command );
            gotoCell.Controls.Add( gotoButton );


            // previous button
            ImageButton previous = new ImageButton();
            ( ( ImageButton )previous ).ID = "PreviousPageImageButton";
            ( ( ImageButton )previous ).ImageUrl = PreviousPageImageUrl;
            ( ( ImageButton )previous ).Enabled = PreviousPageEnabled;
            ( ( ImageButton )previous ).AlternateText = "Previous page";
            ( ( ImageButton )previous ).Attributes.Add( "runat", "server" );
            if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
            {
                ( ( ImageButton )previous ).OnClientClick = OnClientClickScript;
            }
            ( ( ImageButton )previous ).Attributes.Add( "onmouseover", string.Format( "this.src='{0}/{1}Highlighted{2}'", Path.GetDirectoryName( PreviousPageImageUrl ).Replace( "~\\", "" ), Path.GetFileNameWithoutExtension( PreviousPageImageUrl ), Path.GetExtension( PreviousPageImageUrl ) ) );
            ( ( ImageButton )previous ).Attributes.Add( "onmouseout", string.Format( "this.src='{0}'", PreviousPageImageUrl.Replace( "~/", "" ) ));
            ( ( ImageButton )previous ).Command += new CommandEventHandler( previousPage_Command );
            prevCell.Controls.Add( previous );

            // next button
            ImageButton next = new ImageButton();
            ( ( ImageButton )next ).ID = "NextPageImageButton";
            ( ( ImageButton )next ).ImageUrl = NextPageImageUrl;
            ( ( ImageButton )next ).Enabled = NextPageEnabled;
            ( ( ImageButton )next ).AlternateText = "Next page";
            ( ( ImageButton )next ).Attributes.Add( "runat", "server" );
            if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
            {
                ( ( ImageButton )next ).OnClientClick = OnClientClickScript;
            } 
            ( ( ImageButton )next ).Attributes.Add( "onmouseover", string.Format( "this.src='{0}/{1}Highlighted{2}'", Path.GetDirectoryName( NextPageImageUrl ).Replace( "~\\", "" ), Path.GetFileNameWithoutExtension( NextPageImageUrl ), Path.GetExtension( NextPageImageUrl ) ) );
            ( ( ImageButton )next ).Attributes.Add( "onmouseout", string.Format( "this.src='{0}'", NextPageImageUrl.Replace( "~/", "" ) ) );
            ( ( ImageButton )next ).Command += new CommandEventHandler( nextPage_Command );
            nextCell.Controls.Add( next );

            // status
            LiteralControl statusLiteralControl = new LiteralControl();
            statusLiteralControl.ID = "StatusLiteralControl";
            statusLiteralControl.Text = GetStatusString();
            statusCell.Controls.Add( statusLiteralControl );

            // rows per page drop down list
            Label rowsPerPageLabel = new Label();
            rowsPerPageLabel.ID = "RowsPerPageLabel";
            rowsPerPageLabel.Text = "Rows Per Page: ";
            rowsPerPageLabel.Attributes.Add( "runat", "server" );
            rowsPerPageCell.Controls.Add( rowsPerPageLabel );

            DropDownList rowsPerPageDropDownList = new DropDownList();
            for( int i = 0; i <= RowsPerPageIndex; i++ )
            {
                rowsPerPageDropDownList.Items.Add( new ListItem( _rowsPerPageChoices[ i ].ToString(), _rowsPerPageChoices[ i ].ToString() ) );
            }
            rowsPerPageDropDownList.ID = "RowsPerPageDropDownList";
            rowsPerPageDropDownList.ToolTip = "Rows Per Page";
            rowsPerPageDropDownList.Style[ "width" ] = "58px";
            rowsPerPageDropDownList.Attributes.Add( "runat", "server" );
            rowsPerPageDropDownList.ViewStateMode = System.Web.UI.ViewStateMode.Enabled;
            if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
            {
                string showProgressIndicator = string.Format( "__doPostBack('{0}','');{1}", rowsPerPageDropDownList.UniqueID, OnClientClickScript );
                rowsPerPageDropDownList.Attributes.Add( "onChange", showProgressIndicator );
            }
            rowsPerPageDropDownList.AutoPostBack = true;
            rowsPerPageDropDownList.SelectedIndexChanged += new EventHandler( rowsPerPageDropDownList_SelectedIndexChanged );

            // select the current value
            ListItem rowsPerPageItem = rowsPerPageDropDownList.Items.FindByValue( RowsPerPage.ToString() );
            if( rowsPerPageItem != null )
            {
                rowsPerPageItem.Selected = true;
            }
          
            rowsPerPageCell.Controls.Add( rowsPerPageDropDownList );

            // finally, place the root panel into the control array for this control
            Controls.Add( panel );

            // add the supporting js into the page
          //  EmbedScriptCode();

        }

        private void RefreshDropDownList()
        {
            DropDownList rowsPerPageDropDownList = ( DropDownList )this.FindControl( "RowsPerPageDropDownList" );
            if( rowsPerPageDropDownList != null )
            {
                rowsPerPageDropDownList.Items.Clear();

                for( int i = 0; i <= RowsPerPageIndex; i++ )
                {
                    rowsPerPageDropDownList.Items.Add( new ListItem( _rowsPerPageChoices[ i ].ToString(), _rowsPerPageChoices[ i ].ToString() ) );
                }
            }
        }

        // pager status
        private string GetStatusString()
        {
            int currentEndingRow = ( CurrentStartingRow + RowsPerPage ) - 1;

            if( currentEndingRow > TotalRowsForPaging )
                currentEndingRow = TotalRowsForPaging;

            return( string.Format( "{0} - {1} of {2}", CurrentStartingRow, currentEndingRow, TotalRowsForPaging ));
        }

        // force redraw
        public void RefreshStatus()
        {
            LiteralControl statusLiteralControl = ( LiteralControl )( ( Pager )this ).FindControl( "StatusLiteralControl" );
            if( statusLiteralControl != null )
            {
                statusLiteralControl.Text = GetStatusString();
            }
        }

        public void RefreshHiddenValues()
        {
            HtmlInputHidden pagerRowsPerPageHiddenField = ( HtmlInputHidden )( ( Pager )this ).FindControl( PagerRowsPerPageHiddenFieldName );
            if( pagerRowsPerPageHiddenField != null )
            {
                pagerRowsPerPageHiddenField.Value = RowsPerPage.ToString();
            }

            HtmlInputHidden pagerTopRowIndexHiddenField = ( HtmlInputHidden )( ( Pager )this ).FindControl( PagerTopRowIndexHiddenFieldName );
            if( pagerTopRowIndexHiddenField != null )
            {
                pagerTopRowIndexHiddenField.Value = CurrentStartingRow.ToString();
            }
        }

        // render the UI of this control 
        protected override void Render( HtmlTextWriter writer )
        {
            // ensures the control behaves well at design-time 
            // this will not be required if the control supports data-binding because it will get called in DataBind()
            EnsureChildControls();

            // set the style of embedded controls before rendering
            ApplyStylesToEmbeddedControls();

            // avoids a surrounding <span> tag
            RenderContents( writer );
        }

        // apply styles to the control components immediately before rendering
        protected virtual void ApplyStylesToEmbeddedControls()
        {
            // abort if the root div was not correctly added
            if( Controls.Count != 1 )
                return;

            // apply the table style
            Panel pagerPanel = ( Panel )Controls[ 0 ];
            Table pagerTable = ( Table )pagerPanel.Controls[ 0 ];
            pagerTable.CopyBaseAttributes( this );
            if( ControlStyleCreated )
                pagerTable.ApplyStyle( ControlStyle );
            
        }

        // currently not used
        private void AddNonBreakingSpace( Control container)
        {
            if( this.RenderNonBreakingSpacesBetweenControls )
            {
                container.Controls.Add( new LiteralControl( "&nbsp;" ));
            }
        }

    }
}