using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Threading;
using AjaxControlToolkit;
using System.Web.Script.Serialization;
using System.IO;

using VA.NAC.NACCMBrowser.BrowserObj;
using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.ReportManager;

namespace VA.NAC.CM.ApplicationStartup
{
    /// <summary>
    /// Capture the window size on startup.
    /// </summary>
    public class WindowSizeHandler : IHttpHandler, IRequiresSessionState

    {

        public class WindowSize
        {
            public string Height;
            public string Width;
        }

        public class ResponseMessage
        {
            public string Status;
            public string Message;
        }

        public void ProcessRequest( HttpContext context )
        {
            context.Response.ContentType = "text/plain";

            string strJson = new StreamReader( context.Request.InputStream ).ReadToEnd();

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            WindowSize windowSize = ( WindowSize )serializer.Deserialize( strJson, typeof( WindowSize ) );

            string screenHeight = windowSize.Height;
            string screenWidth = windowSize.Width;

            CMGlobals cmGlobals = ( CMGlobals )context.Session[ "CMGlobals" ];
            if( cmGlobals != null )
            {
                cmGlobals.ClientScreenHeight = int.Parse( screenHeight );
                cmGlobals.ClientScreenWidth = int.Parse( screenWidth );

                context.Response.ContentType = "text/plain";

                ResponseMessage responseMessage = new ResponseMessage
                {
                    Status = "success",
                    Message = "OK"
                };

                context.Response.Write( serializer.Serialize( responseMessage ) );  
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}