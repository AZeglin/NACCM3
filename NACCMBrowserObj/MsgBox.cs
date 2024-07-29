using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void MsgBoxPageAlertEventHandler( Page callingPage, string msg );
    public delegate void MsgBoxControlAlertEventHandler( Control callingControl, string msg );
    public delegate void MsgBoxConfirmEventHandler( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName );
    public delegate void MsgBoxTriStateConfirmEventHandler( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName );
    public delegate void MsgBoxPromptEventHandler( Page callingPage, string msg, string nameOfHiddenField );
    public delegate void MsgBoxPageShowErrorEventHandler( Page callingPage, Exception ex );
    public delegate void MsgBoxPopupEditWindowEventHandler( Page callingPage, int buttonCount, string msg, string yesText, string noText, string cancelText, string nameOfHiddenField, string clientIdOfHiddenField );
 
	/// <summary>
	/// MessageBox/confirmation box 
	/// </summary>
    [DefaultProperty( "Text" ),
        ToolboxData( "<{0}:MsgBox runat=server></{0}:MsgBox>" )]
	public  class MsgBox : System.Web.UI.WebControls.WebControl
	{
		private string _content;
        private const string _clearAlertFlagScript = @"<script language='javascript'> alertDone = false; </script>";

        public event MsgBoxPageAlertEventHandler PageAlertEvent;
        public event MsgBoxControlAlertEventHandler ControlAlertEvent;
        public event MsgBoxConfirmEventHandler ConfirmEvent;
        public event MsgBoxTriStateConfirmEventHandler TriStateConfirmEvent;
        public event MsgBoxPromptEventHandler PromptEvent;
        public event MsgBoxPageShowErrorEventHandler PageShowErrorEvent;
        public event MsgBoxPopupEditWindowEventHandler PopupWindowEvent;


        [DefaultValue("Undefined")]
        [Category("Appearance")]
        [Description("Name of msgbox.")]
        public string NameofMsgBox
        {
            get
            {
                if( this.ViewState[ "NameofMsgBox" ] == null )
                {
                    return ( "Undefined" ); // default
                }
                else
                {
                    return (( string )this.ViewState[ "NameofMsgBox" ]);
                }
            }
            set
            {
                if( this.NameofMsgBox != value )
                {
                    this.ViewState[ "NameofMsgBox" ] = value;
                }
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            PageAlertEvent += new MsgBoxPageAlertEventHandler( MsgBox_PageAlertEvent );
            ControlAlertEvent += new MsgBoxControlAlertEventHandler( MsgBox_ControlAlertEvent );
            ConfirmEvent += new MsgBoxConfirmEventHandler( MsgBox_ConfirmEvent );
            TriStateConfirmEvent += new MsgBoxTriStateConfirmEventHandler(MsgBox_TriStateConfirmEvent);
            PromptEvent += new MsgBoxPromptEventHandler( MsgBox_PromptEvent );
            PageShowErrorEvent += new MsgBoxPageShowErrorEventHandler( MsgBox_PageShowErrorEvent );
            PopupWindowEvent += new MsgBoxPopupEditWindowEventHandler( MsgBox_PopupWindowEvent );
        }

 


        [Bindable( false ),
       Category( "Appearance" ),
       DefaultValue( "" )]
        public void Alert( string msg )
		{
            //string messageString = msg.Replace( "\"", "'" );
            //messageString = messageString.Replace( "\n", "\\n" );
            //messageString = messageString.Replace( "\r", "" );

            string messageString = CleanseForJava( msg );

			StringBuilder sb = new StringBuilder();
			
			sb.Append( @"<script language='javascript'>" );

            sb.Append( @"var alertDone;" );

            sb.Append( @"if( alertDone == null || alertDone == false )" );

			sb.Append( @"alert( "" " + messageString + @""" );" );

            sb.Append( @"alertDone = true;" );

			sb.Append( @"</script>" );	

			_content = sb.ToString();

		}

        private string CleanseForJava( string msg )
        {
            string cleansedString = "";

            if( msg.Length > 0 )
            {
                cleansedString = msg.Replace( "\\", "\\\\" );
                cleansedString = cleansedString.Replace( "'", "\\'" );
                cleansedString = cleansedString.Replace( "&", "\\&" );
                cleansedString = cleansedString.Replace( "%", "\\%" );
                cleansedString = cleansedString.Replace( "\"", "'" );
                cleansedString = cleansedString.Replace( "+", "\\+" );
                cleansedString = cleansedString.Replace( "<", "\\<" );
                cleansedString = cleansedString.Replace( ">", "\\>" );
                cleansedString = cleansedString.Replace( "\n", "\\n" );
                cleansedString = cleansedString.Replace( "\r", "" );
            }
            return ( cleansedString );
        }

        [Bindable( true ),
       Category( "Appearance" ),
       DefaultValue( "" )]
        public void Confirm( string msg, string nameOfHiddenField, string confirmationKeyName )
		{

            string messageString = CleanseForJava( msg );

			StringBuilder sb = new StringBuilder();
		
            sb.Append( @"<script language='javascript'>" );

            sb.Append( @" if(confirm( "" " + messageString + @""" ))" );
            sb.Append( @" { " );
            sb.Append( "document.forms[0]." + nameOfHiddenField + ".value='" + CreateConfirmationResult( confirmationKeyName, true ) + "';" + "document.forms[0].submit(); }" );
            sb.Append( @" else { " );
            sb.Append( "document.forms[0]." + nameOfHiddenField + ".value='" + CreateConfirmationResult( confirmationKeyName, false ) + "';" + "document.forms[0].submit(); }" );

            sb.Append( @"</script>" );	

			_content = sb.ToString();

        }


        [Bindable( true ),
       Category( "Appearance" ),
       DefaultValue( "" )]
        public void TriStateConfirm( string msg, string nameOfHiddenField, string confirmationKeyName )
        {

            string messageString = CleanseForJava( msg );

            StringBuilder sb = new StringBuilder();

//$(function() {
//            $('.checked').click(function(e) {
//                e.preventDefault();
//                var dialog = $('<p>Are you sure?</p>').dialog({
//                    buttons: {
//                        "Yes": function() {alert('you chose yes');},
//                        "No":  function() {alert('you chose no');},
//                        "Cancel":  function() {
//                            alert('you chose cancel');
//                            dialog.dialog('close');
//                        }
//                    }
//                });
//            });
//        });


            sb.Append( @"<script type='text/javascript'>" );
       //     sb.Append( @" alert('howdy!'); " );   // $$$ howdy works.
       //     sb.Append( @"$(document).ready(function(){" );
            sb.Append( @"function ShowTriStatePopup() { " );
            sb.Append( @"$(function () { " );

            sb.Append( @"$('<div id=""messageDiv"" style=""display: none"">').dialog({" );
            sb.Append( @"buttons: {" );
            sb.Append( @"    ""Yes"": function() {alert('you chose yes');}," );
            sb.Append( @"    ""No"":  function() {alert('you chose no');}," );
            sb.Append( @"    ""Cancel"":  function() {" );
            sb.Append( @"alert('you chose cancel');" );
            sb.Append( @"$(this).dialog('close');" );
            sb.Append( @" } " );
            sb.Append( @" }, " );
            sb.Append( @" modal: true " );
            sb.Append( @" }); " );

            sb.Append( @" }; " );
            sb.Append( @" };" );
            sb.Append( @"ShowTriStatePopup();" );
            sb.Append( @"</script>" );

            _content = sb.ToString();

        }
        // confirmationKeyName cannot have :: in it
        private string CreateConfirmationResult( string confirmationKeyName, bool bGetTrueKey )
        {
            string returnKey = "";
            returnKey = string.Format( "{0}::{1}", confirmationKeyName, ( bGetTrueKey ) ? "True" : "False" );
            return ( returnKey );
        }

        public enum ConfirmationResults
        {
            TrueResult,
            FalseResult,
            CancelResult,
            NotSubmitted,    // indicates submit had nothing to do with the current confirmation, if any
            NotDefined
        }


        public ConfirmationResults GetConfirmationResult( string confirmationKeyName, string confirmationResultString )
        {
            ConfirmationResults confirmationResults = ConfirmationResults.NotDefined;
            if( confirmationResultString.Contains( confirmationKeyName ) == true )
            {
                int resultPosition = confirmationResultString.LastIndexOf( "::" );
                string stringResult = confirmationResultString.Substring( resultPosition + 2 );
                bool bResult = bool.Parse( stringResult );
                if( bResult == true )
                    confirmationResults = ConfirmationResults.TrueResult;
                else
                    confirmationResults = ConfirmationResults.FalseResult;
            }
            else
            {
                confirmationResults = ConfirmationResults.NotSubmitted;
            }

            return ( confirmationResults );
        }

        [Bindable( true ),
            Category( "Appearance" ),
            DefaultValue( "" )]
        public void Prompt( string msg, string nameOfHiddenField )
        {
            string messageString = CleanseForJava( msg );

            StringBuilder sb = new StringBuilder();

            sb.Append( @"<INPUT type=hidden value='0' name='" + nameOfHiddenField + "'>" );

            sb.Append( @"<script language='javascript'>" );

            sb.Append( @" x = prompt( "" " + messageString + @""" );" );

            sb.Append( "document.forms[0]." + nameOfHiddenField + ".value=x;" + "document.forms[0].submit(); }" );

            sb.Append( @"</script>" );

            _content = sb.ToString();

        }

        public void PopupWindowMessage( int buttonCount, string msg, string yesText, string noText, string cancelText, string nameOfHiddenField, string clientId )
        {
            string messageString = CleanseForJava( msg );

            StringBuilder sb = new StringBuilder();

// $$$ for now presume hidden field is already on the calling page
 //           sb.Append( @"<INPUT type=hidden value='0' name='" + nameOfHiddenField + "'>" );

            sb.Append( @"<script>" );

            sb.Append( string.Format( @"window.open('PopupEditWindow.aspx?Message={0}&ButtonCount={1}&YesText={2}&NoText={3}&CancelText={4}&NameOfHiddenField={5}&ClientIdOfHiddenField={6}','PopupEditWindow','toolbar=no,menubar=no,resizable=no,scrollbars=no,width=400,height=200,left=410,top=300');", msg, buttonCount, yesText, noText, cancelText, nameOfHiddenField, clientId ) );

            sb.Append( @"</script>" );

            _content = sb.ToString();
        }

      
        protected override void Render( HtmlTextWriter output )
        {
            output.Write( this._content );
        }


        public void ShowError( Exception ex )
        {
            ArrayList messageArray = SplitStringOnLineAndWord( ex.Message, 60, 100 );
            if( ex.InnerException != null )
            {
                ArrayList innerMessageArray = SplitStringOnLineAndWord( ex.InnerException.Message, 60, 100 );
                messageArray.AddRange( ( ICollection )innerMessageArray );
            }
            string tmpMsg = AssembleMessage( messageArray );
            Alert( tmpMsg );
        }

        // not used
        private ArrayList SplitString( string theString, int subStringLength, int maxSubStrings )
        {
            string tmpString = new String( theString.ToCharArray(), 0, theString.Length );

            ArrayList sa = new ArrayList( maxSubStrings );

            while( tmpString.Length > 0 )
            {
                if( tmpString.Length < subStringLength )
                {
                    sa.Add( tmpString );
                    tmpString = "";
                }
                else
                {
                    sa.Add( tmpString.Substring( 0, subStringLength ) );
                    tmpString = tmpString.Substring( subStringLength );
                }
            }

            return ( sa );
        }

        private ArrayList SplitStringOnLineAndWord( string theString, int subStringLength, int maxSubStrings )
        {
            ArrayList sa = new ArrayList( maxSubStrings );
            string[] lines = theString.Split( new[] { "\n\n" }, StringSplitOptions.None );

            for( int j = 0; j < lines.Length; j++ )
            {
                string[] words = lines[ j ].Split( ' ' );

                StringBuilder sb = new StringBuilder( subStringLength );
                int currentLength = 0;
                int wordLength = 0;

                for( int i = 0; i < words.Length; i++ )
                {
                    currentLength = sb.Length;
                    wordLength = words[ i ].Length;
                    if( currentLength + wordLength <= subStringLength )
                    {
                        sb.AppendFormat( "{0} ", words[ i ] );
                    }
                    else
                    {
                        sa.Add( sb.ToString() );
                        sb.Clear();
                        sb.AppendFormat( "{0} ", words[ i ] );
                    }
                }
                sa.Add( sb.ToString() );
                sb.Clear();
            }
            return ( sa );
        }

        private ArrayList SplitStringOnWord( string theString, int subStringLength, int maxSubStrings )
        {
            string[] words = theString.Split( ' ' );
  
            StringBuilder sb = new StringBuilder( subStringLength );
            int currentLength = 0;
            int wordLength = 0;
            ArrayList sa = new ArrayList( maxSubStrings );

            for( int i = 0; i < words.Length; i++ )
            {
                currentLength = sb.Length;
                wordLength = words[ i ].Length;
                if( currentLength + wordLength <= subStringLength )
                {
                    sb.AppendFormat( "{0} ", words[ i ] );
                }
                else
                {
                    sa.Add( sb.ToString() );
                    sb.Clear();
                    sb.AppendFormat( "{0} ", words[ i ] );
                }
            }
            sa.Add( sb.ToString() );
            sb.Clear();

            return ( sa );
        }

        private string AssembleMessage( ArrayList messageArray )
        {
            StringBuilder sbMessage = new StringBuilder();

            // gather message
            for( int i = 0; i < messageArray.Count - 1; i++ )
                sbMessage.AppendLine( ( string )messageArray[ i ] );
            sbMessage.Append( ( string )messageArray[ messageArray.Count - 1 ] );

            return ( sbMessage.ToString() );
        }

        // not used
        private string GetNewScriptKey( string keyString )
        {
            string returnString = "";

            returnString = string.Format( "{0}{1}", keyString, DateTime.Now.Ticks.ToString() );

            return ( returnString );
        }

        public void ShowErrorFromUpdatePanel( Page callingPage, Exception ex )
        {
            ShowError( ex ); // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "errmsg", _content, false );
        }

        public void ShowErrorFromUpdatePanelAsync( Page callingPage, Exception ex )
        {
            if( PageShowErrorEvent != null )
                PageShowErrorEvent( callingPage, ex );
        }

        void MsgBox_PageShowErrorEvent( Page callingPage, Exception ex )
        {
            ShowErrorFromUpdatePanelHelper( callingPage, ex );
        }


        internal void ShowErrorFromUpdatePanelHelper( Page callingPage, Exception ex )
        {
            ShowError( ex ); // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "ClearAlertFlagScript", _clearAlertFlagScript, false );
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "errmsg", _content, false );
        }

        public void AlertFromUpdatePanel( Page callingPage, string msg )
        {
            if( PageAlertEvent != null )
            {
                PageAlertEvent( callingPage, msg );
            }
        }

        void MsgBox_PageAlertEvent( Page callingPage, string msg )
        {
            AlertFromUpdatePanelHelper( callingPage, msg );
        }

        internal void AlertFromUpdatePanelHelper( Page callingPage, string msg )
        {
            Alert( msg ); // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "ClearAlertFlagScript", _clearAlertFlagScript, false );
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "alertmsg", _content, false );
        }

        public void AlertFromUpdatePanel( Control callingControl, string msg )
        {
            if( ControlAlertEvent != null )
            {
                ControlAlertEvent( callingControl, msg );
            }
        }

        void MsgBox_ControlAlertEvent( Control callingControl, string msg )
        {
            AlertFromUpdatePanelHelper( callingControl, msg );
        }

        // for use with inherited controls
        internal void AlertFromUpdatePanelHelper( Control callingControl, string msg )
        {
            Alert( msg ); // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingControl, callingControl.GetType(), "ClearAlertFlagScript", _clearAlertFlagScript, false );
            ScriptManager.RegisterClientScriptBlock( callingControl, callingControl.GetType(), "alertmsg", _content, false );
        }

        public void ConfirmFromUpdatePanel( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            if( ConfirmEvent != null )
            {
                ConfirmEvent( callingPage, msg, nameOfHiddenField, confirmationKeyName );
            }
        }


        void MsgBox_ConfirmEvent( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            ConfirmFromUpdatePanelHelper( callingPage, msg, nameOfHiddenField, confirmationKeyName );
        }

        internal void ConfirmFromUpdatePanelHelper( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            Confirm( msg, nameOfHiddenField, confirmationKeyName );  // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "confirmation", _content, false );
        }


        public void TriStateConfirmFromUpdatePanel( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            if( TriStateConfirmEvent != null )
            {
                TriStateConfirmEvent( callingPage, msg, nameOfHiddenField, confirmationKeyName );
            }
        }


        void MsgBox_TriStateConfirmEvent( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            TriStateConfirmFromUpdatePanelHelper( callingPage, msg, nameOfHiddenField, confirmationKeyName );
        }

        internal void TriStateConfirmFromUpdatePanelHelper( Page callingPage, string msg, string nameOfHiddenField, string confirmationKeyName )
        {
            TriStateConfirm( msg, nameOfHiddenField, confirmationKeyName );  // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "confirmation", _content, false );
        }


        public void PromptFromUpdatePanel( Page callingPage, string msg, string nameOfHiddenField )
        {
            if( PromptEvent != null )
            {
                PromptEvent( callingPage, msg, nameOfHiddenField );
            }
        }

        void MsgBox_PromptEvent( Page callingPage, string msg, string nameOfHiddenField )
        {
            PromptFromUpdatePanelHelper( callingPage, msg, nameOfHiddenField );
        }

        public void PromptFromUpdatePanelHelper( Page callingPage, string msg, string nameOfHiddenField )
        {
            Prompt( msg, nameOfHiddenField );  // sets up content member
            ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), "prompt", _content, false );
        }

        private Page _callingPage = null;

        // three choice popup
        public void PopupMessageFromUpdatePanel( Page callingPage, string msg, string yesText, string noText, string cancelText, string nameOfHiddenField, string clientIdOfHiddenField )
        {
            if( PopupWindowEvent != null )
            {
                _callingPage = callingPage;

                SetStringInSession( "PopupWindowYesResult", yesText );
                SetStringInSession( "PopupWindowNoResult", noText );
                SetStringInSession( "PopupWindowCancelResult", cancelText );

                PopupWindowEvent( callingPage, 3, msg, yesText, noText, cancelText, nameOfHiddenField, clientIdOfHiddenField );
            }
        }

        // two choice popup
        public void PopupMessageFromUpdatePanel( Page callingPage, string msg, string noText, string cancelText, string nameOfHiddenField, string clientIdOfHiddenField )
        {
            if( PopupWindowEvent != null )
            {
                _callingPage = callingPage;

                SetStringInSession( "PopupWindowYesResult", "" );
                SetStringInSession( "PopupWindowNoResult", noText );
                SetStringInSession( "PopupWindowCancelResult", cancelText );

                PopupWindowEvent( callingPage, 2, msg, "", noText, cancelText, nameOfHiddenField, clientIdOfHiddenField );
            }
        }

        private static string GetStringFromSession( string keyName )
        {
            string value = "";
            if( HttpContext.Current != null )
            {
                if( HttpContext.Current.Session[ keyName ] != null )
                {
                    value = HttpContext.Current.Session[ keyName ].ToString();
                }
            }
            return ( value );
        }

        private static void SetStringInSession( string keyName, string value )
        {
            if( HttpContext.Current != null )
            {
                HttpContext.Current.Session[ keyName ] = value;
            }
        }

        public static string GetPopupWindowYesResult()
        {
            return( GetStringFromSession( "PopupWindowYesResult" )); 
        }

        public static string GetPopupWindowNoResult()
        {
            return ( GetStringFromSession( "PopupWindowNoResult" ) );
        }
        
        public static string GetPopupWindowCancelResult()
        {
            return ( GetStringFromSession( "PopupWindowCancelResult" ) );
        }

        void MsgBox_PopupWindowEvent( Page callingPage, int buttonCount, string msg, string yesText, string noText, string cancelText, string nameOfHiddenField, string clientIdOfHiddenField )
        {
            PopupWindowEventHelper( callingPage, buttonCount, msg, yesText, noText, cancelText, nameOfHiddenField, clientIdOfHiddenField );
        }

        public void PopupWindowEventHelper( Page callingPage, int buttonCount, string msg, string yesText, string noText, string cancelText, string nameOfHiddenField, string clientIdOfHiddenField )
        {
            PopupWindowMessage( buttonCount, msg, yesText, noText, cancelText, nameOfHiddenField, clientIdOfHiddenField ); // sets up content member
           ScriptManager.RegisterClientScriptBlock( callingPage, callingPage.GetType(), GetNewScriptKey( "popupWindow" ), _content, false );
    //        ScriptManager.RegisterStartupScript( callingPage, callingPage.GetType(), "popupWindow", _content, false );
        }

        public string GetClientId()
        {
            return ( this.ClientID );
        }
    }
}
