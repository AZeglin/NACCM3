using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class EditedOfferDataSourceView : DataSourceView
    {

        public static string DefaultViewName = "EditedOfferDataSourceView";

        public EditedOfferDataSourceView( IDataSource owner, string name )
            : base( owner, DefaultViewName )
        {

        }


        // set via EditedOfferDataSource
        public EditedOfferContent EditedOfferContentFront
        {
            get
            {
                return ( ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ] );
            }
            set
            {
                HttpContext.Current.Session[ "EditedOfferContentFront" ] = value;
            }
        }


        protected override IEnumerable ExecuteSelect( DataSourceSelectArguments selectArgs )
        {
            List<EditedOfferContent> listOfOneObject = new List<EditedOfferContent>();

            listOfOneObject.Add( EditedOfferContentFront );

            return ( listOfOneObject );
        }

        public override bool CanUpdate
        {
            get
            {
                return ( true );
            }
        }

        protected override int ExecuteUpdate( IDictionary keys, IDictionary values, IDictionary oldValues )
        {
            int i = 1;

            return ( 1 );
        }


        public override bool CanDelete
        {
            get
            {
                return ( false );
            }
        }
        protected override int ExecuteDelete( IDictionary keys, IDictionary values )
        {
            throw new NotSupportedException();
        }

        public override bool CanInsert
        {
            get
            {
                return ( true );
            }
        }
        protected override int ExecuteInsert( IDictionary values )
        {
            int i = 1;

            return ( 1 );
        }
    }
}
