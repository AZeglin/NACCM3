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
    public class EditedOfferDataSource : DataSourceControl, ISerializable
    {
        public EditedOfferDataSource()
            : base()
        {
        }

        public static string NameOfObjectInSession = "EditedOfferContentFront";

        public EditedOfferContent EditedOfferContentFront
        {
            get
            {
                return ( ( ( EditedOfferDataSourceView )this.GetView( String.Empty ) ).EditedOfferContentFront );
            }
            set
            {
                EditedOfferContent existingEditedOfferContent = ( EditedOfferContent )( ( EditedOfferDataSourceView )this.GetView( String.Empty ) ).EditedOfferContentFront;
                EditedOfferContent newEditedOfferContent = value;
                if( existingEditedOfferContent != null && newEditedOfferContent != null )
                {
                    if( newEditedOfferContent.OfferId != existingEditedOfferContent.OfferId )
                    {
                        ( ( EditedOfferDataSourceView )this.GetView( String.Empty ) ).EditedOfferContentFront = newEditedOfferContent;
                        RaiseDataSourceChangedEvent( EventArgs.Empty );
                    }
                    else  // $$$ do this anyway to raise event from the front -- may refine it later
                    {
                        ( ( EditedOfferDataSourceView )this.GetView( String.Empty ) ).EditedOfferContentFront = newEditedOfferContent;
                        RaiseDataSourceChangedEvent( EventArgs.Empty );
                    }
                }
                else
                {
                    ( ( EditedOfferDataSourceView )this.GetView( String.Empty ) ).EditedOfferContentFront = newEditedOfferContent;
                    RaiseDataSourceChangedEvent( EventArgs.Empty );
                }
            }
        }

        public void UpdateFront( EditedOfferContent editedOfferContentFront )
        {
            EditedOfferContentFront = editedOfferContentFront;
        }


        private EditedOfferDataSourceView _theView = null;

        protected override DataSourceView GetView( string viewName )
        {
            if( _theView == null )
            {
                _theView = new EditedOfferDataSourceView( this, String.Empty );
            }
            return ( _theView );
        }

        protected override ICollection GetViewNames()
        {
            ArrayList viewList = new ArrayList( 1 );
            viewList.Add( EditedOfferDataSourceView.DefaultViewName );
            return ( viewList as ICollection );
        }

        #region ISerializable Members

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {

        }

        public EditedOfferDataSource( SerializationInfo info, StreamingContext context )
            : base()
        {
        }

        #endregion
    }
}
