using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class UserActivity
    {

        public UserActivity( ActionTypes actionType, string actionDetails, ActionDetailsTypes actionDetailsType  )
        {
            _actionType = actionType;
            _actionDetailsString = actionDetails;
            _actionDetailsType = actionDetailsType;
        }

        public UserActivity( ActionTypes actionType, string actionDetailsString, int actionDetailsNumeric, ActionDetailsTypes actionDetailsType )
        {
            _actionType = actionType;
            _actionDetailsString = actionDetailsString;
            _actionDetailsNumeric = actionDetailsNumeric;
            _actionDetailsType = actionDetailsType;
        }

        public void LogUserActivity()
        {
            ContractDB contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];
            if( contractDB != null )
            {
                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();

                bool bSuccess = contractDB.LogUserActivity( ActionTypeString, _actionDetailsString, ActionDetailsTypeString );
                if( bSuccess == false )
                {
                    throw new Exception( string.Format( "The following error was encountered when attempting to log user activity: {0}", contractDB.ErrorMessage ) );
                }
            }
        }

        public void LogDocumentAccess()
        {
            if( _actionType == ActionTypes.ViewDocument )
            {
                ContractDB contractDB = ( ContractDB )HttpContext.Current.Session[ "ContractDB" ];
                contractDB.TargetDatabase = DBCommon.TargetDatabases.NACCMCommonUser;
                contractDB.MakeConnectionString();
                bool bSuccess = true;

                if( _actionDetailsType == ActionDetailsTypes.ViewContract )
                {
                    bSuccess = contractDB.LogUserContractAccess( _actionDetailsString, _actionDetailsNumeric );

                }
                else if( _actionDetailsType == ActionDetailsTypes.ViewOffer )
                {
                    bSuccess = contractDB.LogUserOfferAccess( _actionDetailsString, _actionDetailsNumeric );
                }

                if( bSuccess == false )
                {
                    throw new Exception( string.Format( "The following error was encountered when attempting to log user activity: {0}", contractDB.ErrorMessage ) );
                }
            }
        }

        private string _actionDetailsString = "";

        public string ActionDetailsString
        {
            get { return _actionDetailsString; }
            set { _actionDetailsString = value; }
        }

        private int _actionDetailsNumeric = -1;

        public int ActionDetailsNumeric
        {
            get { return _actionDetailsNumeric; }
            set { _actionDetailsNumeric = value; }
        }

        private ActionTypes _actionType = ActionTypes.Undefined;

        public ActionTypes ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }
        
        private ActionDetailsTypes _actionDetailsType = ActionDetailsTypes.Undefined;

        public ActionDetailsTypes ActionDetailsType
        {
            get { return _actionDetailsType; }
            set { _actionDetailsType = value; }
        }

        // enums of user activity types for loggin
        public enum ActionTypes
        {
            Undefined,
            Login,
            ScreenResolution,
            Report,
            ViewPersonalNotificationScreen,
            ViewDocument,
            MenuSelect,
            Exception
        }

        public enum ActionDetailsTypes
        {
            Undefined,
            ReportType,
            ScreenResolutionValues,
            ViewPersonalNotificationScreen,
            ViewContract,
            ViewOffer,
            MainMenuSelect,
            ExceptionDetect,
            ExceptionHandler
        }

        public string ActionDetailsTypeString
        {
            get
            {
                string actionDetailsType = "U";
                switch( _actionDetailsType )
                {
                    case ActionDetailsTypes.ReportType:
                        actionDetailsType = "R";
                        break;
                    case ActionDetailsTypes.ScreenResolutionValues:
                        actionDetailsType = "S";
                        break;
                    case ActionDetailsTypes.ViewPersonalNotificationScreen:
                        actionDetailsType = "P";
                        break;
                    case ActionDetailsTypes.ViewContract:
                        actionDetailsType = "C";
                        break;
                    case ActionDetailsTypes.ViewOffer:
                        actionDetailsType = "O";
                        break;
                    case ActionDetailsTypes.MainMenuSelect:
                        actionDetailsType = "M";
                        break;
                    case ActionDetailsTypes.ExceptionDetect:
                        actionDetailsType = "X";
                        break;
                    case ActionDetailsTypes.ExceptionHandler:
                        actionDetailsType = "H";
                        break;
                    default:
                        actionDetailsType = "U";
                        break;
                }
                return ( actionDetailsType );
            }
        }

        public string ActionTypeString
        {
            get
            {
                string actionType = "U";
                switch( _actionType )
                {
                    case ActionTypes.Login:
                        actionType = "L";
                        break;
                    case ActionTypes.Report:
                        actionType = "R";
                        break;
                    case ActionTypes.ScreenResolution:
                        actionType = "S";
                        break;       
                    case ActionTypes.ViewPersonalNotificationScreen:
                        actionType = "P";
                        break;
                    case ActionTypes.ViewDocument:
                        actionType = "D";
                        break;
                    case ActionTypes.MenuSelect:
                        actionType = "M";
                        break;
                    case ActionTypes.Exception:
                        actionType = "X";
                        break;
                    default:
                        actionType = "U";
                        break;
                }
                return( actionType );
            }
        }
    }
}
