using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Collections;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void ContextMenuCommandEventHandler( ContextMenu theMenu, ContextMenuCommandEventArgs args );

    [ParseChildren( true, "ContextMenuItems" )]
    [Designer( typeof( VA.NAC.NACCMBrowser.BrowserObj.ContextMenuDesigner ) )]
    [DefaultEvent( "ItemCommand" )]
    [DefaultProperty( "ContextMenuItems" )]
    public class ContextMenu : WebControl, INamingContainer
    {
        private ContextMenuItemCollection _contextMenuItems;
        private ArrayList _boundControls;

        public event ContextMenuCommandEventHandler ItemCommand;

        // property
        // gets the collection of the menu items
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
        [PersistenceMode( PersistenceMode.InnerProperty )]
        [Description( "Gets the collection of the menu items" )]
        public ContextMenuItemCollection ContextMenuItems
        {
            get
            {
                if( _contextMenuItems == null )
                    _contextMenuItems = new ContextMenuItemCollection();
                return _contextMenuItems;
            }
        }

        // property
        // gets the collection of controls for which the context menu should be displayed 
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false )]
        [Description( "Gets the collection of controls for which the context menu should be displayed" )]
        public ArrayList BoundControls
        {
            get
            {
                if( _boundControls == null )
                    _boundControls = new ArrayList();
                return _boundControls;
            }
        }

        // property to get and set the background color when the mouse hovers over the menu item
        [Description( "Gets and sets the mouse hover background color" )]
        public Color RolloverColor
        {
            get
            {
                object o = ViewState[ "RolloverColor" ];
                if( o == null )
                    return Color.Gold;
                return ( Color )o;
            }
            set { ViewState[ "RolloverColor" ] = value; }
        }

        // property to determine the pixels around each menu item 
        [Description( "The space in pixels around each menu item" )]
        public int CellPadding
        {
            get
            {
                object o = ViewState[ "CellPadding" ];
                if( o == null )
                    return 2;
                return ( int )o;
            }
            set
            {
                ViewState[ "CellPadding" ] = value;
            }
        }



        // property to indicate whether the context menu should be dismissed as the user moves out
        [Description( "Indicates whether the context menu should be dismissed as the user moves out" )]
        public bool AutoHide
        {
            get
            {
                object o = ViewState[ "AutoHide" ];
                if( o == null )
                    return true;
                return ( bool )o;
            }
            set
            {
                ViewState[ "AutoHide" ] = value;
            }
        }

        // property to indicate whether the context menu should activate on a right click instead of oncontextmenu
        [Description( "Indicates whether the context menu should activate on a right click" )]
        public bool ActivateOnRightClick
        {
            get
            {
                object o = ViewState[ "ActivateOnRightClick" ];
                if( o == null )
                    return true;
                return ( bool )o;
            }
            set
            {
                ViewState[ "ActivateOnRightClick" ] = value;
            }
        }

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
                this.ViewState[ "IsContextMenuInUpdatePanel" ] = value;
            }
        }

        private const string TrapEscKey = "__trapESC('{0}');";
        
        // Returns the Javascript code to dismiss the context menu when the user hits ESC
        public string GetEscReference()
        {
            if( this.Controls.Count == 0 )
                EnsureChildControls();

            string menuClientId = this.Controls[ 0 ].ClientID;

            return String.Format( TrapEscKey, menuClientId );
        }

        private const string HideMenuOnClick = "HideMenuOnClick('{0}');";
         
        // returns the Javascript code to dismiss the context menu when the user clicks outside the menu
        public string GetOnClickReference()
        {
            return String.Format( HideMenuOnClick, Controls[0].ClientID );
        }

 		// determines the standard style of the control
		protected override Style CreateControlStyle()
		{
			Style style = base.CreateControlStyle();
			style.BorderStyle = BorderStyle.Outset;
			style.BorderColor = Color.Snow;
			style.BorderWidth = Unit.Pixel(2);
			style.BackColor = Color.FromName("#eeeeee");
            style.Font.Name = "Arial";
			style.Font.Size = FontUnit.Point(8);
			return( style );
		}

        // script segments used by child controls
        private const string HrSeparator = "<hr style='height:1px;border:solid 1px black;' />";
        private const string OnMouseOver = "this.style.background = '{0}';";
        private const string OnMouseOut = "this.style.background = '{0}';";

        public const string ContextMenuClientIDSuffix = "Root";
        public const string ContextMenuCurrentMenuContextHiddenFieldNameSuffix = "CurrentMenuContext";

        // currently not used
        public static string GetContextMenuClientID( string contextMenuId )
        {
            return ( string.Format( "{0}_{1}", contextMenuId, ContextMenuClientIDSuffix ) );
        }

        public static string GetContextMenuCurrentMenuContextHiddenFieldName( string contextMenuId )
        {
            return ( string.Format( "{0}_{1}", contextMenuId, ContextMenuCurrentMenuContextHiddenFieldNameSuffix ) );
        }

        public string GetCurrentMenuContextValue()
        {
            string contextValue = "";

            HtmlInputHidden hf = ( HtmlInputHidden )this.Controls[ 0 ].Controls[ 1 ];
            if( hf != null )
            {
                contextValue = hf.Value.ToString();
            }
            return ( contextValue );
        }

        public HtmlInputHidden GetCurrentMenuContext()
        {
            HtmlInputHidden hf = ( HtmlInputHidden )this.Controls[ 0 ].Controls[ 1 ];
            return ( hf );
        }

		// builds the UI of the control
		protected override void CreateChildControls()
		{
			Controls.Clear();

			// A context menu is an invisible DIV that is moved around via scripting when the user
			// right-clicks on a bound HTML tag
			HtmlGenericControl div = new HtmlGenericControl( "div" );
            div.ID = ContextMenuClientIDSuffix;
			div.Style[ "Display" ] = "none";
			div.Style[ "position" ] = "absolute";
			if( AutoHide )
				div.Attributes[ "onmouseleave" ] = "this.style.display='none'";


            // explicitly disable the regular context menu
            // when using right click event, such as with gridview
            if( ActivateOnRightClick == true )
                DisableContextMenu();

			Table menu = new Table();
			menu.ApplyStyle( CreateControlStyle() );
			menu.CellSpacing = 1;
			menu.CellPadding = CellPadding;
			div.Controls.Add( menu );

            // add hidden input field to capture some context ( e.g., gridview row id )
            HtmlInputHidden currentMenuContextHiddenField = new HtmlInputHidden();
            currentMenuContextHiddenField.EnableViewState = true;
            currentMenuContextHiddenField.Name = GetContextMenuCurrentMenuContextHiddenFieldName( this.ID );
            currentMenuContextHiddenField.ID = GetContextMenuCurrentMenuContextHiddenFieldName( this.ID );
            currentMenuContextHiddenField.Attributes.Add( "runat", "server" );
   //         currentMenuContextHiddenField.Attributes.Add( "enableviewstate", "true");
            currentMenuContextHiddenField.Value = "-1";
            div.Controls.Add( currentMenuContextHiddenField );

			// loop on ContextMenuItems and add rows to the table
			foreach( ContextMenuItem item in ContextMenuItems )
			{
				// create and add a row for the menu item
				TableRow menuItemRow = new TableRow();
                menu.Rows.Add( menuItemRow );

				// configure the menu item
				TableCell menuItemCell = new TableCell();
                menuItemRow.Cells.Add( menuItemCell );

				// define the menu item's contents
				if( item.DisplayText == String.Empty || item.DisplayText == null )
				{
					// an empty item is a separator
					menuItemCell.Controls.Add( new LiteralControl( ContextMenu.HrSeparator ));
				}
				else
				{
					// mouse roll-over
					menuItemCell.Attributes[ "onmouseover" ] = String.Format( ContextMenu.OnMouseOver, ColorTranslator.ToHtml( RolloverColor ));
					menuItemCell.Attributes[ "onmouseout" ] = String.Format( ContextMenu.OnMouseOut, ColorTranslator.ToHtml( BackColor ));
                    
					// add the button and its command event handler 
					LinkButton button = new LinkButton();
					menuItemCell.Controls.Add( button );
					button.Click += new EventHandler( ButtonClicked );
                    button.ID = string.Format( "{0}MenuItem", item.CommandName );
					button.Width = Unit.Percentage( 100 );
					button.ToolTip = item.Tooltip;
					button.Text = item.DisplayText;
					button.CommandName = item.CommandName;
                    button.Enabled = item.Enabled;   // enable menu item during control creation
                    if( item.ClientOnClick.Length > 0 )
                        button.Attributes[ "onclick" ] = item.ClientOnClick;
				}
			}

			// finally, place the root div into the control array for this control
			Controls.Add( div );

			// add the supporting js into the page
			EmbedScriptCode();

			// assign click function to each control in the list
            foreach( Control c in BoundControls )
            {
                // cast to appropriate type
                WebControl ctl1 = ( c as WebControl );
                HtmlControl ctl2 = ( c as HtmlControl );
                if( ctl1 != null )
                {
                    if( ActivateOnRightClick == true )
                    {
                        ctl1.Attributes[ "onMouseDown" ] = GetRightClickMenuReference();
                    }
                    else
                    {
                        ctl1.Attributes[ "oncontextmenu" ] = GetMenuReference();
                    }
                }
                if( ctl2 != null )
                {
                    if( ActivateOnRightClick == true )
                    {
                        ctl2.Attributes[ "onMouseDown" ] = GetRightClickMenuReference();
                    }
                    else
                    {
                        ctl2.Attributes[ "oncontextmenu" ] = GetMenuReference();
                    }
                }
            }
		}

 

        private const string AttachContextMenu = "return __showContextMenu({0});";
        private const string AttachContextMenuToRightClick = "return __showContextMenuRightClick(this,'{0}',{1},'{2}');";

        // return the js to show the context menu from a participating control
        // use with oncontextmenu
        public string GetMenuReference()
        {
            if( this.Controls.Count == 0 )
                EnsureChildControls();

            string menuClientId = this.Controls[ 0 ].ClientID;
            return String.Format( AttachContextMenu, menuClientId );
        }

        // return the js to show the context menu on a right click from a participating control
        // use with onmousedown
        public string GetRightClickMenuReference( int rowIndex, string key )
        {
            if( this.Controls.Count == 0 )
                EnsureChildControls();

            return String.Format(AttachContextMenuToRightClick, this.Controls[0].ClientID, rowIndex, this.Controls[0].Controls[1].ClientID ); // was ClientID UniqueID both giving name instead of id
            //return ("return;");

        }


        public ContextMenu()
        {
            this.ClientIDMode = System.Web.UI.ClientIDMode.Predictable;
        }

        // return the js to show the context menu on a right click from a participating control
        // use with onmousedown
        public string GetRightClickMenuReference()
        {
            if( this.Controls.Count == 0 )
                EnsureChildControls();

            return String.Format( AttachContextMenuToRightClick, this.Controls[ 0 ].ClientID, "null", "null" );
           // return ("return;");
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
			HtmlGenericControl div = ( HtmlGenericControl )Controls[ 0 ];
			Table menu = ( Table )div.Controls[ 0 ];
			menu.CopyBaseAttributes( this );
			if( ControlStyleCreated )
				menu.ApplyStyle( ControlStyle );
			
			// style each menu item individually
			for( int i=0; i < menu.Rows.Count; i++ )
			{
				TableRow menuItemRow = menu.Rows[ i ];
				TableCell menuItemCell = menuItemRow.Cells[0];

				// style the link button
				LinkButton button = ( menuItemCell.Controls[ 0 ] as LinkButton );
				if( button != null )
				{
					button.ForeColor = ForeColor;
					button.Style[ "text-decoration" ] = "none";
				}
			}
		}

        // enabling of menu items after control creation
        public void Enable( string commandName, bool bEnable )
        {
            if( Controls.Count == 0 )
                EnsureChildControls();

            HtmlGenericControl div = ( HtmlGenericControl )Controls[ 0 ];
            Table menu = ( Table )div.Controls[ 0 ];

            for( int i = 0; i < menu.Rows.Count; i++ )
            {
                TableRow menuItemRow = menu.Rows[ i ];
                TableCell menuItemCell = menuItemRow.Cells[ 0 ];

                LinkButton button = ( menuItemCell.Controls[ 0 ] as LinkButton );
                if( button != null )
                {
                    if( button.CommandName.CompareTo( commandName ) == 0 )
                    {
                        button.Enabled = bEnable;

                        // block or re-establish the click script - post initialization
                        if( bEnable == false )
                        {
                            button.Attributes[ "onclick" ] = "";
                        }
                        else
                        {
                            button.Attributes[ "onclick" ] = GetClientOnClickScript( commandName );
                        }

                        // mouse roll-over
                        if( bEnable == true )
                        {
                           // button.CssClass = "ContextMenuEnabled"; //button.Attributes.Add( "class", "ContextMenuEnabled" );
                            menuItemCell.Attributes[ "onmouseover" ] = String.Format( ContextMenu.OnMouseOver, ColorTranslator.ToHtml( RolloverColor ) );
                        }
                        else
                        {
                           // button.CssClass = "ContextMenuDisabled"; 
                            menuItemCell.Attributes[ "onmouseover" ] = menuItemCell.Attributes[ "onmouseout" ];
                        }

                        break;
                    }
                }
            }
        }

        private string GetClientOnClickScript( string commandName )
        {
            string script = "";

            for( int j = 0; j < this.ContextMenuItems.Count; j++ )
            {
                ContextMenuItem contextMenuItem = this.ContextMenuItems[ j ];
                if( contextMenuItem.CommandName.CompareTo( commandName ) == 0 )
                {
                    script = contextMenuItem.ClientOnClick;
                    break;
                }
            }
            return ( script );
        }

		// insert the script code needed to refresh the UI
		private void EmbedScriptCode()
		{
			// add the script to declare the function
			string js = ReadResourceString( "ContextMenuScript.js" );

            if( !Page.ClientScript.IsStartupScriptRegistered( "ContextMenuScript" ) )
                Page.ClientScript.RegisterStartupScript( this.GetType(), "ContextMenuScript", js );
		}

        private void DisableContextMenu()
        {

            Control parentControl = null;

            if( IsContextMenuInUpdatePanel == true )
                parentControl = GetParentForm( this );
            else
                parentControl = this.Parent;

            // cast to appropriate type
            WebControl ctl1 = ( parentControl as WebControl );
            HtmlControl ctl2 = ( parentControl as HtmlControl );
            if( ctl1 != null )
            {
                ctl1.Attributes[ "oncontextmenu" ] = "return false;";
            }
            if( ctl2 != null )
            {
                ctl2.Attributes[ "oncontextmenu" ] = "return false;";
            }
        }

        private HtmlForm GetParentForm( Control aControl )
        {
            if( aControl == null )
                return ( null );
            if( aControl.GetType().Name.CompareTo( "HtmlForm" ) == 0 )
                return ( ( HtmlForm )aControl );
            return ( GetParentForm( aControl.Parent ) );
        }

		// read the specified string resource from the current assembly
		private string ReadResourceString( string resourceName )
		{
            string script = "";

            if( resourceName.Length > 0 )
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                StreamReader reader;
                string completeResourceName = string.Format( "{0}.{1}", this.GetType().Namespace, resourceName );
                
                Stream resourceStream = asm.GetManifestResourceStream( completeResourceName );
                if( resourceStream != null )
                {
                    reader = new StreamReader( resourceStream );
                    script = reader.ReadToEnd();
                    reader.Close();
                }
                else
                {
                    throw new Exception( "Failed to load ContextMenu supporting javascript resource" );
                }
            }
			return( script );
		}



	    // handle the ItemCommand event and pass it on to the host page
		private void ButtonClicked( object sender, EventArgs e )
		{
			LinkButton button = sender as LinkButton;
			if( button != null )
			{
                ContextMenuCommandEventArgs args = new ContextMenuCommandEventArgs( button.CommandName, GetAdditionalCommandEventArguments( button.CommandName ));
				OnItemCommand( args );
			}
		}


        // pass on the ItemCommand event to the host page
		protected virtual void OnItemCommand( ContextMenuCommandEventArgs e )
		{
			if( ItemCommand != null )
				ItemCommand( this, e );
		}

        public object GetAdditionalCommandEventArguments( string commandName )
        {
            object additionalCommandEventArguments = null;

            // loop on ContextMenuItems to look for the command
            foreach( ContextMenuItem item in ContextMenuItems )
            {
                if( item.CommandName.CompareTo( commandName ) == 0 )
                {
                    additionalCommandEventArguments = item.CommandEventArgs;
                    break;
                }
            }

            return ( additionalCommandEventArguments );
        }
    }
}
