using System;
using System.Globalization;
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

namespace VA.NAC.CM.ApplicationStartup
{
    public partial class PopupEditWindow : System.Web.UI.Page
    {
        private int _buttonCount = 3;
        private string _message = "Continue ?";
        private string _yesButtonText = "Yes";
        private string _noButtonText = "No";
        private string _cancelButtonText = "Cancel";
        private string _nameOfHiddenField = "PopupWindowResult";
        private string _clientIdOfHiddenField = "Fred";


        protected void Page_Load( object sender, EventArgs e )
        {
            if( Request.QueryString[ "Message" ] != null )
            {
                _message = Request.QueryString[ "Message" ].ToString();
            }

            if( Request.QueryString[ "ButtonCount" ] != null )
            {
                _buttonCount = int.Parse( Request.QueryString[ "ButtonCount" ].ToString() );
            }

            if( Request.QueryString[ "YesText" ] != null )
            {
                _yesButtonText = Request.QueryString[ "YesText" ].ToString();
            }

            if( Request.QueryString[ "NoText" ] != null )
            {
                _noButtonText = Request.QueryString[ "NoText" ].ToString();
            }

            if( Request.QueryString[ "CancelText" ] != null )
            {
                _cancelButtonText = Request.QueryString[ "CancelText" ].ToString();
            }
            
            if( Request.QueryString[ "NameOfHiddenField" ] != null )
            {
                _nameOfHiddenField = Request.QueryString[ "NameOfHiddenField" ].ToString();
            }

            if( Request.QueryString[ "ClientIdOfHiddenField" ] != null )
            {
                _clientIdOfHiddenField = Request.QueryString[ "ClientIdOfHiddenField" ].ToString();
            } 
            
        }

        private string GetCloseWindowFunctionCall( string response, string nameOfHiddenField, string clientIdOfHiddenField )
        {
            return ( string.Format( "CloseWindow( \"{0}\", \"{1}\", \"{2}\" )", response, nameOfHiddenField, clientIdOfHiddenField ) );
        }

        protected void TwoButtonPanel_OnPreRender( object sender, EventArgs e )
        {
            Panel twoButtonPanel = ( Panel )sender;

            if( _buttonCount == 2 )
                twoButtonPanel.Visible = true;
            else
                twoButtonPanel.Visible = false;
        }

        protected void ThreeButtonPanel_OnPreRender( object sender, EventArgs e )
        {
            Panel threeButtonPanel = ( Panel )sender;

            if( _buttonCount == 3 )
                threeButtonPanel.Visible = true;
            else
                threeButtonPanel.Visible = false;
        }

        protected void MessageLabel_OnPreRender( object sender, EventArgs e )
        {
            Label messageLabel = ( Label )sender;
            messageLabel.Text = _message;
        }

        protected void YesButton_OnPreRender( object sender, EventArgs e )
        {
            Button yesButton = ( Button )sender;
            yesButton.Text = _yesButtonText;
            yesButton.Attributes.Add( "onclick", GetCloseWindowFunctionCall( _yesButtonText, _nameOfHiddenField, _clientIdOfHiddenField ));
        }

        protected void NoButton_OnPreRender( object sender, EventArgs e )
        {
            Button noButton = ( Button )sender;
            noButton.Text = _noButtonText;
            noButton.Attributes.Add( "onclick", GetCloseWindowFunctionCall( _noButtonText, _nameOfHiddenField, _clientIdOfHiddenField ) );
        }

        protected void CancelButton_OnPreRender( object sender, EventArgs e )
        {
            Button cancelButton = ( Button )sender;
            cancelButton.Text = _cancelButtonText;
            cancelButton.Attributes.Add( "onclick", GetCloseWindowFunctionCall( _cancelButtonText, _nameOfHiddenField, _clientIdOfHiddenField ) );
        }

        protected void PopupEditWindowScriptManager_OnAsyncPostBackError( object sender, AsyncPostBackErrorEventArgs e )
        {
            string errorMsg = "";

            if( e.Exception.Data[ "PopupEditWindowErrorMessage" ] != null )
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0} /nDetails: {1}", e.Exception.Message, e.Exception.Data[ "PopupEditWindowErrorMessage" ] );
            }
            else
            {
                errorMsg = string.Format( "The following error was encountered during async postback: {0}", e.Exception.Message );
            }

            PopupEditWindowScriptManager.AsyncPostBackErrorMessage = errorMsg;
        }

    }
}
