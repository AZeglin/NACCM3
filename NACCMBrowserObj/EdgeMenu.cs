using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void EdgeMenuCommandEventHandler( EdgeMenu theMenu, EdgeMenuCommandEventArgs args );

    [Designer( typeof( VA.NAC.NACCMBrowser.BrowserObj.EdgeMenuDesigner ) )]
    [DefaultEvent( "EdgeMenuCommand" )]
    [DefaultProperty( "EdgeMenuItems" )]
    public class EdgeMenu : WebControl, INamingContainer, IPostBackEventHandler
    {
      
        public event EdgeMenuCommandEventHandler EdgeMenuCommand;

        public static string MenuClickCommand = "MenuClick";

        public EdgeMenuItemCollection MenuItems
        {
            get
            {
                if( this.ViewState[ "MenuItems" ] == null )                    
                {
                    EdgeMenuItemCollection menuItems = new EdgeMenuItemCollection();
                    this.ViewState[ "MenuItems" ] = menuItems;
                    ItemCount = 0;
                }
                return ( ( EdgeMenuItemCollection )this.ViewState[ "MenuItems" ] );
            }
            set
            {
                this.ViewState[ "MenuItems" ] = value;
                ItemCount = (( EdgeMenuItemCollection )this.ViewState[ "MenuItems" ] ).Count;
            }
        }

        // presumes all items are created in an initialization phase/ no removal
        public void AddItem( EdgeMenuItem item )
        {
            MenuItems.Add( item );
            ItemCount = MenuItems.Count;
        }

        public void AddItem( string displayText, string itemValue )
        {
            MenuItems.Add( displayText, itemValue );
            ItemCount = MenuItems.Count;
        }

        public void AddItem( string displayText, string itemValue, string target, string itemUrl )
        {
            MenuItems.Add( displayText, itemValue, target, itemUrl );
            ItemCount = MenuItems.Count;
        }

        public void RemoveItem( EdgeMenuItem item )
        {
            int indexToRemove = GetItemIndexFromValue( item.ItemValue );
            MenuItems.Remove( indexToRemove );
            ItemCount = MenuItems.Count;
        }

        private EdgeMenuItem GetItem( int itemIndex )
        {
            return ( MenuItems[ itemIndex ] );
        }

        private EdgeMenuItem GetItem( string itemValue )
        {
            return ( MenuItems.GetEdgeMenuItem( itemValue ) );
        }

        private int GetItemIndexFromValue( string itemValue )
        {
            return ( MenuItems.GetEdgeMenuItemIndexFromValue( itemValue ) );
        }

        public void EnableItem( string itemValue, bool bEnabled )
        {
            MenuItems.Enable( itemValue, bEnabled );
        }

        private void HighlightItem( string itemValue )
        {
            MenuItems.Highlight( itemValue );
        }

        public void SelectItem( string itemValue )
        {
            if( CurrentItemValue.Length > 0 )
                PreviousItemValue = CurrentItemValue;
            CurrentItemValue = itemValue;

            EdgeMenuItem item = GetItem( itemValue );
            CurrentItemIndex = GetItemIndexFromValue( itemValue );

            HighlightItem( itemValue );
        }

        #region Properties

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "Container script to run on menu item click events." )]
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


        [DefaultValue( 0 )]
        [Category( "Behavior" )]
        [Description( "The current selected item index." )]
        public int CurrentItemIndex
        {
            get
            {
                if( this.ViewState[ "CurrentItemIndex" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "CurrentItemIndex" ];
            }
            set
            {
                this.ViewState[ "CurrentItemIndex" ] = value;
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "The value of the current selected item." )]
        public string CurrentItemValue
        {
            get
            {
                if( this.ViewState[ "CurrentItemValue" ] == null )
                    return "";
                else
                    return ( string )this.ViewState[ "CurrentItemValue" ];
            }
            set
            {
                this.ViewState[ "CurrentItemValue" ] = value;
            }
        }

        [DefaultValue( "" )]
        [Category( "Behavior" )]
        [Description( "The value of the previous selected item." )]
        public string PreviousItemValue
        {
            get
            {
                if( this.ViewState[ "PreviousItemValue" ] == null )
                    return "";
                else
                    return ( string )this.ViewState[ "PreviousItemValue" ];
            }
            set
            {
                this.ViewState[ "PreviousItemValue" ] = value;
            }
        }


        [DefaultValue( "#CCCCCC" )]
        [Category( "Behavior" )]
        [Description( "The color of a menu item when it is disabled." )]
        public string MenuDisabledColor
        {
            get
            {
                if( this.ViewState[ "MenuDisabledColor" ] == null )
                    return "";
                else
                    return ( string )this.ViewState[ "MenuDisabledColor" ];
            }
            set
            {
                this.ViewState[ "MenuDisabledColor" ] = value;
            }
        }

        [DefaultValue( "#FFFFFF" )]
        [Category( "Behavior" )]
        [Description( "The color of a menu item when it is selected." )]
        public string MenuSelectedColor
        {
            get
            {
                if( this.ViewState[ "MenuSelectedColor" ] == null )
                    return "";
                else
                    return ( string )this.ViewState[ "MenuSelectedColor" ];
            }
            set
            {
                this.ViewState[ "MenuSelectedColor" ] = value;
            }
        }

        [DefaultValue( "#E6E6E6" )]
        [Category( "Behavior" )]
        [Description( "The color of a menu item when it is moused over." )]
        public string MenuMouseOverColor
        {
            get
            {
                if( this.ViewState[ "MenuMouseOverColor" ] == null )
                    return "";
                else
                    return ( string )this.ViewState[ "MenuMouseOverColor" ];
            }
            set
            {
                this.ViewState[ "MenuMouseOverColor" ] = value;
            }
        }

        [DefaultValue( 0 )]
        [Category( "Behavior" )]
        [Description( "Count of items in the menu." )]
        public int ItemCount
        {
            get
            {
                if( this.ViewState[ "ItemCount" ] == null )
                    return 0;
                else
                    return ( int )this.ViewState[ "ItemCount" ];
            }
            set
            {
                this.ViewState[ "ItemCount" ] = value;
            }
        }

        #endregion Properties

        public EdgeMenu()
        {
            MenuItems = new EdgeMenuItemCollection();
            ItemCount = MenuItems.Count;

            this.ClientIDMode = System.Web.UI.ClientIDMode.Predictable;

            this.Load += new EventHandler( Page_Load );

            _currentSelectedMenuItemHiddenFieldName = string.Format( "Menu{0}CurrentSelectedItemHiddenField", ( base.ID != null ) ? base.ID : "_" );
            _previousSelectedMenuItemHiddenFieldName = string.Format( "Menu{0}PreviousSelectedItemHiddenField", ( base.ID != null ) ? base.ID : "_" );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            // important setup for server side events
            ScriptManager.GetCurrent( Page ).RegisterAsyncPostBackControl( this );
        }



        void IPostBackEventHandler.RaisePostBackEvent( string eventArgument )
        {
            // select the clicked item
            this.SelectItem( eventArgument );

            RaiseMenuCommandEvent( MenuClickCommand, eventArgument );

        }

        private void RaiseMenuCommandEvent( string commandName, string selectedMenuItemValue )
        {
            if( EdgeMenuCommand != null )
            {
                EdgeMenuCommand( this, new EdgeMenuCommandEventArgs( commandName, selectedMenuItemValue ) );
            }
        }


        // inline style added by standard menu
        //# MainMenu { background-color:Transparent;height:100%; }
        //# MainMenu img.icon { border-style:none;vertical-align:middle; }
        //# MainMenu img.separator { border-style:none;display:block; }
        //# MainMenu img.horizontal-separator { border-style:none;vertical-align:middle; }
        //# MainMenu ul { list-style:none;margin:0;padding:0;width:auto; }
        //# MainMenu ul.dynamic { z-index:1; }
        //# MainMenu a { text-decoration:none;white-space:nowrap;display:block; }
        //# MainMenu a.static { text-decoration:none;border-style:none;padding-left:0.15em;padding-right:0.15em; }
        //# MainMenu a.popout { background-image:url("/WebResource.axd?d=YAYach_zykzn7tRotFpEUmZpXms8LBJgsFsGAmZzYjJfH0RVvr5CU2lmrbLNUzyDbYJb4bu6Td5lGppLtV1lzgLGmirBBm7FrlMWKsmqzgg1&t=637453852754849868");background-repeat:no-repeat;background-position:right center;padding-right:14px; }
        //# MainMenu a.static.selected { color:White;text-decoration:none;border-style:none; }
        //# MainMenu a.static.highlighted { color:White; }

        // determines the standard style of the control
        protected override Style CreateControlStyle()
        {
            Style style = base.CreateControlStyle();

            style.BorderStyle = BorderStyle.None;
            style.BorderColor = Color.Transparent;
            style.BorderWidth = Unit.Pixel( 1 );
            style.BackColor = Color.Transparent;
            style.Font.Name = "Arial";
            style.Font.Size = FontUnit.Point( 10 );
            style.ForeColor = Color.Black;
            return ( style );
        }


        private string _currentSelectedMenuItemHiddenFieldName = "";

        public string CurrentSelectedMenuItemHiddenFieldName
        {
            get
            {
                return _currentSelectedMenuItemHiddenFieldName;
            }

            set
            {
                _currentSelectedMenuItemHiddenFieldName = value;
            }
        }

        private string _previousSelectedMenuItemHiddenFieldName = "";

        public string PreviousSelectedMenuItemHiddenFieldName
        {
            get
            {
                return _previousSelectedMenuItemHiddenFieldName;
            }

            set
            {
                _previousSelectedMenuItemHiddenFieldName = value;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return ( base.Controls );
            }
        }


        private const string OnMouseOver = "this.style.color = '{0}';";
        private const string OnMouseOut = "this.style.color = '{0}';";

        // builds the UI of the control
        // ECMS version div/ul/li/a
        protected override void CreateChildControls()
        {
            Controls.Clear();

            // the menu is a panel/list with each list item containing an anchor

            Panel panel = new Panel();
            panel.ID = base.ID;
            panel.TabIndex = -1;
            panel.Attributes.Add( "role", "navigation" );
            panel.Attributes.Add( "name", base.ID );
            panel.ApplyStyle( CreateControlStyle() );


            HtmlGenericControl list = new HtmlGenericControl( "ul" );

            for( int i = 0; i < ItemCount; i++ )
            {
                EdgeMenuItem item = GetItem( i );

                HtmlGenericControl listItem = new HtmlGenericControl( "li" );
                listItem.Attributes.Add( "value", item.ItemValue );

                listItem.Style.Add( "display", "inline" );
                listItem.Style.Add( "padding", "10px 10px" );
                listItem.Style.Add( "margin", "5px" );

                HtmlGenericControl anchor = new HtmlGenericControl( "a" );
                anchor.Attributes.Add( "id", item.ItemValue );
                anchor.Attributes.Add( "name", item.ItemValue );
                anchor.InnerText = item.ItemText; // "CCST"

                if( item.HasTarget == true )
                {
                    anchor.Attributes.Add( "target", item.Target );  // _new
                }

                string onClickScript = "";               
                string onClickPostbackScript = "";
                
                if( item.HasUrl == true )
                {
                    anchor.Attributes.Add( "href", item.ItemUrl );  // "http://www.va.gov/nac/"
                }
                else
                {
                    // alternate dummy href for non-url callback
                    anchor.Attributes.Add( "href", "javascript:void(0);" );

                    onClickPostbackScript = Page.ClientScript.GetPostBackEventReference( this, item.ItemValue );
                                      
                    onClickScript += onClickPostbackScript;
                }

                if( OnClientClickScript.Trim().CompareTo( String.Empty ) != 0 )
                {
                    if( onClickPostbackScript.Trim().CompareTo( String.Empty ) != 0 )
                        onClickScript += "; ";
                    else
                        onClickScript += " ";

                    onClickScript += OnClientClickScript.Trim();
                }

                if( onClickScript.Trim().CompareTo( String.Empty ) != 0 )
                {
                    anchor.Attributes.Add( "onClick", onClickScript.Trim() );
                }

                anchor.Attributes[ "onmouseover" ] = String.Format( EdgeMenu.OnMouseOver, MenuMouseOverColor );
      
                listItem.Controls.Add( anchor );

                list.Controls.Add( listItem );
            }

            panel.Controls.Add( list );

            Controls.Add( panel );
        }




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
            // abort if the root panel was not correctly added
            if( Controls.Count != 1 )
                return;

            // style each menu item individually
            for( int i = 0; i < ItemCount; i++ )
            {
                EdgeMenuItem item = GetItem( i );

                Panel panel = ( Panel )Controls[ 0 ];

                HtmlGenericControl list = ( HtmlGenericControl )panel.Controls[ 0 ];

                if( list != null )
                {
                    HtmlGenericControl listItem = ( HtmlGenericControl )list.Controls[ i ];

                    if( listItem != null )
                    {
                        HtmlGenericControl anchor = ( HtmlGenericControl )listItem.Controls[ 0 ];


                        anchor.Style.Add( HtmlTextWriterStyle.TextDecoration, "none" );

                        if( item.Enabled != true )
                        {
                            anchor.Style.Add( HtmlTextWriterStyle.Color, MenuDisabledColor );
                            anchor.Attributes[ "onmouseout" ] = String.Format( EdgeMenu.OnMouseOut, MenuDisabledColor );
                        }
                        else // enabled
                        {
                            if( item.Highlighted == true )
                            {
                                anchor.Style.Add( HtmlTextWriterStyle.Color, MenuSelectedColor );
                                anchor.Attributes[ "onmouseout" ] = String.Format( EdgeMenu.OnMouseOut, MenuSelectedColor );
                            }
                            else
                            {
                                anchor.Style.Add( HtmlTextWriterStyle.Color, "Black" );
                                anchor.Attributes[ "onmouseout" ] = String.Format( EdgeMenu.OnMouseOut, "#000000" ); // black
                            }
                        }                    
                    }
                }
            }

        }




        //// builds the UI of the control
        //protected override void CreateChildControls()
        //{
        //    Controls.Clear();

        //    // the menu is a panel/table with one row filled with buttons

        //    Panel panel = new Panel();
        //    panel.ID = base.ID;
        //    panel.TabIndex = -1;
        //    panel.Attributes.Add( "role", "navigation" );

        //    Table menuTable = new Table();

        //    menuTable.ApplyStyle( CreateControlStyle() );



        //    menuTable.Style[ "table-layout" ] = "fixed";
        //    menuTable.Style[ "align" ] = "center";
        //    menuTable.Style[ "BackColor" ] = Color.Transparent.ToString();
        //    menuTable.Style[ "border" ] = "none";
        //    menuTable.Style[ "border-color" ] = Color.Transparent.ToString();
        //    menuTable.Style[ "Height" ] = "100%";
        //    menuTable.Style[ "Width" ] = "100%";
        //    menuTable.Style[ "display" ] = "block";
        //    menuTable.CellSpacing = 1;
        //    menuTable.CellPadding = 10;
        //    menuTable.TabIndex = -1;
        //    panel.Controls.Add( menuTable );


        //    // create and add a row 
        //    TableRow menuTableRow = new TableRow();
        //    menuTableRow.Style[ "background-color" ] = Color.Transparent.ToString();
        //    menuTableRow.Style[ "width" ] = "100%";
        //    menuTableRow.Style[ "Height" ] = "50%";
        //    menuTableRow.Attributes.Add( "role", "menubar" );
        //    menuTable.Rows.Add( menuTableRow );

        //    //string widthPercentage = string.Format( "{0}%", Math.Ceiling( ( decimal )( 100 / ItemCount ) ) );


        //    for( int i = 0; i < ItemCount; i++ )
        //    {
        //        TableCell c = new TableCell();
        //        c.Style[ "width" ] = "auto";
        //        c.Style[ "display" ] = "inline-block";
        //        menuTableRow.Cells.Add( c );

        //        EdgeMenuItem item = GetItem( i );

        //        HyperLink menuButton = new HyperLink();   // tried button, linkbutton, hyperlink, HtmlAnchor
        //        menuButton.ID = item.ItemValue;
        //        menuButton.Text = item.ItemText;

        //        //   if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
        //        //     {
        //        //enuButton.OnClientClick = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );
        //        // menuButton.Attributes[ "onclick" ] = OnClientClickScript;   // $$$ add this back - modelled after contextmenu

        //        //    }

        //        //menuButton.Command += new CommandEventHandler( MenuButton_Command );   is this the culprit?


        //        // mouse roll-over
        //        menuButton.Attributes[ "onmouseover" ] = String.Format( EdgeMenu.OnMouseOver, MenuMouseOverColor );
        //        menuButton.Attributes[ "onmouseout" ] = String.Format( EdgeMenu.OnMouseOut, ColorTranslator.ToHtml( BackColor ) );

        //        menuButton.Attributes.Add( "value", item.ItemValue );   // adding just to override default value 
        //        menuButton.Attributes.Add( "runat", "server" );
        //        menuButton.Attributes.Add( "role", "menuitem" );

        //        short tabIndex = ( short )( i + 1 );
        //        menuButton.TabIndex = tabIndex;

        //        menuButton.BorderStyle = BorderStyle.None;
        //        menuButton.BorderWidth = 0;
        //        menuButton.BorderColor = Color.Transparent;

        //        menuButton.BackColor = Color.Transparent;              

        //        if( item.Target.Length > 0 )
        //        {
        //            //menuButton.Attributes.Add( "target", item.Target );                   
        //            menuButton.Target = item.Target;
        //        }

        //        if( item.ItemUrl.Length > 0 )
        //        {
        //            //menuButton.PostBackUrl = item.ItemUrl;
        //            //menuButton.Attributes.Remove( "href" );
        //            //menuButton.Attributes[ "href" ] = item.ItemUrl;
        //            menuButton.NavigateUrl = item.ItemUrl;
        //        }
        //        else
        //        {
        //            //menuButton.Attributes[ "href" ] = "#";
        //            menuButton.NavigateUrl = "#";
        //            //    menuButton.OnClientClick = string.Format( "__doPostback( '{0}', '{1}' );", base.ID, menuButton.ID );
        //            //menuButton.Click += new EventHandler( MenuButtonClick );    // doing it this way places postback into href instead of click
        //            //menuButton.OnClientClick = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );
        //            menuButton.Attributes[ "onclick" ] = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );

        //        }


        //        c.Controls.Add( menuButton );
        //    }

        //    // consider placing hidden fields in a hidden div
        //    //<div id="HiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >

        //    // add hidden input fields to hold current and previous selections
        //    HtmlInputHidden currentSelectedMenuItemHiddenField = new HtmlInputHidden();
        //    currentSelectedMenuItemHiddenField.EnableViewState = true;
        //    currentSelectedMenuItemHiddenField.Name = CurrentSelectedMenuItemHiddenFieldName;
        //    currentSelectedMenuItemHiddenField.ID = CurrentSelectedMenuItemHiddenFieldName;
        //    currentSelectedMenuItemHiddenField.Attributes.Add( "runat", "server" );

        //    currentSelectedMenuItemHiddenField.Value = CurrentItemValue;
        //    panel.Controls.Add( currentSelectedMenuItemHiddenField );

        //    HtmlInputHidden previousSelectedMenuItemHiddenField = new HtmlInputHidden();
        //    previousSelectedMenuItemHiddenField.EnableViewState = true;
        //    previousSelectedMenuItemHiddenField.Name = PreviousSelectedMenuItemHiddenFieldName;
        //    previousSelectedMenuItemHiddenField.ID = PreviousSelectedMenuItemHiddenFieldName;
        //    previousSelectedMenuItemHiddenField.Attributes.Add( "runat", "server" );

        //    previousSelectedMenuItemHiddenField.Value = PreviousItemValue;
        //    panel.Controls.Add( previousSelectedMenuItemHiddenField );


        //    // finally, place the root panel into the control array for this control
        //    Controls.Add( panel );

        //    // add the supporting js into the page
        //    //  EmbedScriptCode();

        //}

        //private const string OnMouseOver = "this.style.forecolor = '{0}';";
        //private const string OnMouseOut = "this.style.forecolor = '{0}';";



        //            So for a custom button, render to the output stream:

        //< a id = "someclientid" name = "someuniqueid" href = "javascript:void(0);" onclick = "__doPostBack('someuniqueid', '');" > val </ a >
        //In your custom button, add the IPostBackEventHandler, and this __doPostBack statement will fire its RaisePostBackEvent method automatically for you.

        //  anchor2.Attributes.Add( "onclick", string.Format( "__doPostBack('{0}', '{1}');" , base.ID, "TEST" ));


        //// create and add a row 
        //TableRow menuTableRow = new TableRow();
        //menuTableRow.Style[ "background-color" ] = Color.Transparent.ToString();
        //menuTableRow.Style[ "width" ] = "100%";
        //menuTableRow.Style[ "Height" ] = "50%";
        //menuTableRow.Attributes.Add( "role", "menubar" );
        //menuTable.Rows.Add( menuTableRow );

        ////string widthPercentage = string.Format( "{0}%", Math.Ceiling( ( decimal )( 100 / ItemCount ) ) );


        //for( int i = 0; i < ItemCount; i++ )
        //{
        //    TableCell c = new TableCell();
        //    c.Style[ "width" ] = "auto";
        //    c.Style[ "display" ] = "inline-block";
        //    menuTableRow.Cells.Add( c );

        //    EdgeMenuItem item = GetItem( i );

        //    HyperLink menuButton = new HyperLink();   // tried button, linkbutton, hyperlink, HtmlAnchor
        //    menuButton.ID = item.ItemValue;
        //    menuButton.Text = item.ItemText;

        //   if( OnClientClickScript.Trim().CompareTo( string.Empty ) != 0 )
        //     {
        //enuButton.OnClientClick = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );
        // menuButton.Attributes[ "onclick" ] = OnClientClickScript;   // $$$ add this back - modelled after contextmenu

        //    }

        //menuButton.Command += new CommandEventHandler( MenuButton_Command );   is this the culprit?


        // mouse roll-over
        //menuButton.Attributes[ "onmouseover" ] = String.Format( EdgeMenu.OnMouseOver, MenuMouseOverColor );
        //menuButton.Attributes[ "onmouseout" ] = String.Format( EdgeMenu.OnMouseOut, ColorTranslator.ToHtml( BackColor ) );

        //menuButton.Attributes.Add( "value", item.ItemValue );   // adding just to override default value 
        //menuButton.Attributes.Add( "runat", "server" );
        //menuButton.Attributes.Add( "role", "menuitem" );

        //short tabIndex = ( short )( i + 1 );
        //menuButton.TabIndex = tabIndex;

        //menuButton.BorderStyle = BorderStyle.None;
        //menuButton.BorderWidth = 0;
        //menuButton.BorderColor = Color.Transparent;

        //menuButton.BackColor = Color.Transparent;

        //if( item.Target.Length > 0 )
        //{
        //    //menuButton.Attributes.Add( "target", item.Target );                   
        //    menuButton.Target = item.Target;
        //}

        //if( item.ItemUrl.Length > 0 )
        //{
        //    //menuButton.PostBackUrl = item.ItemUrl;
        //    //menuButton.Attributes.Remove( "href" );
        //    //menuButton.Attributes[ "href" ] = item.ItemUrl;
        //    menuButton.NavigateUrl = item.ItemUrl;
        //}
        //else
        //{
        //    //menuButton.Attributes[ "href" ] = "#";
        //    menuButton.NavigateUrl = "#";
        //    //    menuButton.OnClientClick = string.Format( "__doPostback( '{0}', '{1}' );", base.ID, menuButton.ID );
        //    //menuButton.Click += new EventHandler( MenuButtonClick );    // doing it this way places postback into href instead of click
        //    //menuButton.OnClientClick = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );
        //    menuButton.Attributes[ "onclick" ] = string.Format( "__doPostback( ''{0}'', ''{1}'' );", base.ID, menuButton.ID );

        //}


        //c.Controls.Add( menuButton );
        //}

        //// consider placing hidden fields in a hidden div
        ////<div id="HiddenDiv"  style="width:0px; height:0px; border:none; background-color:White; margin:0px;" >

        //// add hidden input fields to hold current and previous selections
        //HtmlInputHidden currentSelectedMenuItemHiddenField = new HtmlInputHidden();
        //currentSelectedMenuItemHiddenField.EnableViewState = true;
        //currentSelectedMenuItemHiddenField.Name = CurrentSelectedMenuItemHiddenFieldName;
        //currentSelectedMenuItemHiddenField.ID = CurrentSelectedMenuItemHiddenFieldName;
        //currentSelectedMenuItemHiddenField.Attributes.Add( "runat", "server" );

        //currentSelectedMenuItemHiddenField.Value = CurrentItemValue;
        //panel.Controls.Add( currentSelectedMenuItemHiddenField );

        //HtmlInputHidden previousSelectedMenuItemHiddenField = new HtmlInputHidden();
        //previousSelectedMenuItemHiddenField.EnableViewState = true;
        //previousSelectedMenuItemHiddenField.Name = PreviousSelectedMenuItemHiddenFieldName;
        //previousSelectedMenuItemHiddenField.ID = PreviousSelectedMenuItemHiddenFieldName;
        //previousSelectedMenuItemHiddenField.Attributes.Add( "runat", "server" );

        //previousSelectedMenuItemHiddenField.Value = PreviousItemValue;
        //panel.Controls.Add( previousSelectedMenuItemHiddenField );


        // finally, place the root panel into the control array for this control



        // apply styles to the control components immediately before rendering - old version used with table
        // protected virtual void ApplyStylesToEmbeddedControls()
        // {
        // abort if the root panel was not correctly added
        //   if( Controls.Count != 1 )
        //       return;

        //apply the table style
        //Panel menuPanel = ( Panel )Controls[ 0 ];
        //Table menuTable = ( Table )menuPanel.Controls[ 0 ];

        //menuTable.CopyBaseAttributes( this );
        //if( ControlStyleCreated )
        //    menuTable.ApplyStyle( ControlStyle );

        //TableRow menuTableRow = menuTable.Rows[ 0 ];

        //// style each menu item individually
        //for( int i = 0; i < ItemCount; i++ )
        //{                
        //    TableCell menuTableCell = menuTableRow.Cells[ i ];
        //    EdgeMenuItem item = GetItem( i );

        //    // style the link button
        //    //LinkButton button = ( menuTableCell.Controls[ 0 ] as LinkButton );
        //    HyperLink button = ( menuTableCell.Controls[ 0 ] as HyperLink );
        //    if( button != null )
        //    {
        //        button.Style[ "text-decoration" ] = "none";
        //        button.Style[ "outline" ] = "none";

        //        button.ForeColor = System.Drawing.ColorTranslator.FromHtml( "#000000" );

        //        if( item.Enabled != true )
        //        {
        //            button.ForeColor = System.Drawing.ColorTranslator.FromHtml( MenuDisabledColor );
        //        }
        //        else // enabled
        //        {
        //            if( item.Highlighted == true )
        //            {
        //                button.ForeColor = System.Drawing.ColorTranslator.FromHtml( MenuSelectedColor );
        //            }                      
        //        }

        //    }
        //}
        //}
    }
}


