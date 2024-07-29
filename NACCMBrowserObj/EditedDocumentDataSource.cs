using System;
using System.Collections;
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

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // a bindable data source that works with the EditedDocumentContent object and the asp.net controls
    // the EditedDocumentContent object, is then passed to the ObjectDataSource backed by the EditedDocumentManager
    // for the initial select and final save to the database
    [Serializable]
    public class EditedDocumentDataSource : DataSourceControl, ISerializable
    {
        public EditedDocumentDataSource()
            : base()
        {
        }

        public static string NameOfObjectInSession = "EditedDocumentContentFront";

        public EditedDocumentContent EditedDocumentContentFront
        {
            get
            {
                return( ( ( EditedDocumentDataSourceView )this.GetView( String.Empty ) ).EditedDocumentContentFront );
            }
            set  
            {
                EditedDocumentContent existingEditedDocumentContent = ( EditedDocumentContent )( ( EditedDocumentDataSourceView )this.GetView( String.Empty ) ).EditedDocumentContentFront;
                EditedDocumentContent newEditedDocumentContent = value;
                if( existingEditedDocumentContent != null && newEditedDocumentContent != null )
                {
                    if( newEditedDocumentContent.ContractId != existingEditedDocumentContent.ContractId )
                    {
                        ( ( EditedDocumentDataSourceView )this.GetView( String.Empty ) ).EditedDocumentContentFront = newEditedDocumentContent;
                        RaiseDataSourceChangedEvent( EventArgs.Empty );
                    }
                    else  // $$$ do this anyway to raise event from the front -- may refine it later
                    {
                        ( ( EditedDocumentDataSourceView )this.GetView( String.Empty ) ).EditedDocumentContentFront = newEditedDocumentContent;
                        RaiseDataSourceChangedEvent( EventArgs.Empty );
                    }
                }
                else
                {
                    ( ( EditedDocumentDataSourceView )this.GetView( String.Empty ) ).EditedDocumentContentFront = newEditedDocumentContent;
                    RaiseDataSourceChangedEvent( EventArgs.Empty );
                }
            }
        }

        public void UpdateFront( EditedDocumentContent editedDocumentContentFront )
        {
            EditedDocumentContentFront = editedDocumentContentFront;
        }


        private EditedDocumentDataSourceView _theView = null;

        protected override DataSourceView GetView( string viewName )
        {
            if( _theView == null )
            {
                _theView = new EditedDocumentDataSourceView( this, String.Empty );
            }
            return ( _theView );
        }

        protected override ICollection GetViewNames()
        {
            ArrayList viewList = new ArrayList( 1 );
            viewList.Add( EditedDocumentDataSourceView.DefaultViewName );
            return ( viewList as ICollection );
        }

        // not used
        public void InsertFront( EditedDocumentContent editedDocumentContentFront )
        {
            EditedDocumentContentFront = editedDocumentContentFront;
        }

        #region ISerializable Members

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
           
        }

        public EditedDocumentDataSource( SerializationInfo info, StreamingContext context )
            : base()
        {
        }

        #endregion
    }
}
