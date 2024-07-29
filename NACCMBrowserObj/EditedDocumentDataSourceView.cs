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
    public class EditedDocumentDataSourceView : DataSourceView  
    {

        public static string DefaultViewName = "EditedDocumentDataSourceView";

        private EditedDocumentDataSource _editedDocumentDataSource = null;

        public EditedDocumentDataSourceView( IDataSource owner, string name )
            : base( owner, DefaultViewName )
        {
            _editedDocumentDataSource = ( EditedDocumentDataSource )owner;
        }


        // set via EditedDocumentDataSource
        public EditedDocumentContent EditedDocumentContentFront
        {
            get 
            { 
                return ( ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ] ); 
            }
            set 
            {
                HttpContext.Current.Session[ "EditedDocumentContentFront" ] = value; 
            }
        }


        protected override IEnumerable ExecuteSelect( DataSourceSelectArguments selectArgs )
        {
            List<EditedDocumentContent> listOfOneObject = new List<EditedDocumentContent>();

            listOfOneObject.Add( EditedDocumentContentFront );

            return ( listOfOneObject );
        }

        public override bool CanUpdate
        {
            get
            {
                return( true );
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
                return( false );
            }
        }
        protected override int ExecuteDelete( IDictionary keys, IDictionary values )
        {
            throw new NotSupportedException();
        }

        // note the ObjectDataSource way of calling insert never worked due to parameter error, would have to not use object for update
        // and implement an insert that took each value as an individual parameter according to posted solutions.  Opted to call
        // create contract function directly instead.
        public override bool CanInsert
        {
            get
            {
                return( true );
            }
        }

        //public int ExecuteInsert( EditedDocumentContent editedDocumentContentFront )
        //{
        //    int i = 1;

        //    return ( 1 );
        //}

        protected override int ExecuteInsert( IDictionary values )
        {
            int i = 1;

            return ( 1 );
        }
    }
}
