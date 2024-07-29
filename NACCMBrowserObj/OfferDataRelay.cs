using System;
using System.Collections;
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
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public delegate void OfferUpdateSuccessEventHandler( DataRelayUpdateSuccessEventArgs args );
    public delegate void OfferInsertSuccessEventHandler( DataRelayInsertSuccessEventArgs args );
    // not part of ObjectDataSource
    public delegate void CustomOfferInsertingEventHandler( CustomOfferInsertingEventArgs args );

    [Serializable]
    public class OfferDataRelay : IDataRelay, ISerializable
    {
        public event OfferUpdateSuccessEventHandler UpdateSuccessEvent;
        public event OfferInsertSuccessEventHandler InsertSuccessEvent;
        protected event CustomOfferInsertingEventHandler CustomInsertingEvent;

        public OfferDataRelay( DocumentTypes documentType )
        {
            _documentType = documentType;
            Init();
        }

        private DocumentTypes _documentType = DocumentTypes.Undefined;

        public DocumentTypes DocumentType
        {
            get { return _documentType; }
            set { _documentType = value; }
        }

        private Page _currentPage;

        public void SetCurrentPage( Page currentPage )
        {
            _currentPage = currentPage;
        }

        public void SetInternalEventHandlers()
        {          
            this.CustomInsertingEvent += new CustomOfferInsertingEventHandler( _editedOfferDataSourceBack_CustomInsertingEvent );
        }

        public Page GetCurrentPage()
        {
            return ( _currentPage );
        }

        private EditedOfferDataSource _editedOfferDataSourceFront = null;

        public EditedOfferDataSource EditedOfferDataSourceFront
        {
            get { return _editedOfferDataSourceFront; }
            set { _editedOfferDataSourceFront = value; }
        }

        // used when edit in progress is complete and user has called save
        private SerializableObjectDataSource _editedOfferDataSourceBack = null;

        public SerializableObjectDataSource EditedOfferDataSourceBack
        {
            get { return _editedOfferDataSourceBack; }
            set { _editedOfferDataSourceBack = value; }
        }

        private void Init()
        {
            ClearSessionVariables();

            _editedOfferDataSourceBack = null;
            _editedOfferDataSourceBack = new SerializableObjectDataSource();
            _editedOfferDataSourceBack.ID = "EditedOfferDataSourceBack";
            _editedOfferDataSourceBack.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditedOfferContent";
            _editedOfferDataSourceBack.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditedOfferManagerBack";
            _editedOfferDataSourceBack.SelectMethod = "GetEditedOfferContent";
            _editedOfferDataSourceBack.UpdateMethod = "SaveChanges";
          // _editedOfferDataSourceBack.InsertMethod = "CreateOffer";
            _editedOfferDataSourceBack.SetEventOwnerName( "OfferDataRelay" );
            _editedOfferDataSourceBack.Updating += new ObjectDataSourceMethodEventHandler( _editedOfferDataSourceBack_Updating );
            _editedOfferDataSourceBack.Updated += new ObjectDataSourceStatusEventHandler( _editedOfferDataSourceBack_Updated );
            _editedOfferDataSourceBack.Selected += new ObjectDataSourceStatusEventHandler( _editedOfferDataSourceBack_Selected );
            //_editedOfferDataSourceBack.Inserting += new ObjectDataSourceMethodEventHandler( _editedOfferDataSourceBack_Inserting );
            //_editedOfferDataSourceBack.Inserted += new ObjectDataSourceStatusEventHandler( _editedOfferDataSourceBack_Inserted );

            HttpContext.Current.Session[ "EditedOfferDataSourceBack" ] = _editedOfferDataSourceBack;

            _editedOfferDataSourceFront = null;
            _editedOfferDataSourceFront = new EditedOfferDataSource();
            _editedOfferDataSourceFront.ID = "EditedOfferDataSourceFront";

            EditedOfferContent editedOfferContentFront = new EditedOfferContent();
            _editedOfferDataSourceFront.EditedOfferContentFront = editedOfferContentFront; // saved to session by view

            HttpContext.Current.Session[ "EditedOfferDataSourceFront" ] = _editedOfferDataSourceFront;
        }

        private void ClearSessionVariables()
        {
            HttpContext.Current.Session[ "EditedOfferContentFront" ] = null;
            HttpContext.Current.Session[ "EditedOfferDataSourceFront" ] = null;
            HttpContext.Current.Session[ "EditedOfferContentBack" ] = null;
            HttpContext.Current.Session[ "EditedOfferDataSourceBack" ] = null;
            HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = null;
            HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = null;
        }

        public void ClearError()
        {
            HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] = "";
            HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] = "";
        }

        public void Load()
        {
            _editedOfferDataSourceBack.Select();

            _editedOfferDataSourceFront.EditedOfferContentFront.CopyFrom( ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentBack" ] );

            HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = false.ToString();  // load complete, reset flag
        }

        public void _editedOfferDataSourceBack_Selected( object sender, ObjectDataSourceStatusEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ].ToString().CompareTo( EditedOfferManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ].ToString();
                        e.ExceptionHandled = true;
                        throw new Exception( string.Format( "Exception encountered during select: {0}", msg ) );
                    }
                }
            }
        }

        // call update on the back object
        public void Update()
        {
            ClearError();
            _editedOfferDataSourceBack.Update();
        }

        // call update on the front object only, 
        // used to save a screen when tabbing
        public void UpdateFront( EditedOfferContent editedOfferContentFront )
        {
            _editedOfferDataSourceFront.UpdateFront( editedOfferContentFront );

        }

        // when update is called on the back object, the front object is copied onto the back
        public void _editedOfferDataSourceBack_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            // get the back object
            EditedOfferContent editedOfferContentBack = ( EditedOfferContent )e.InputParameters[ "editedOfferContentBack" ];  // no: HttpContext.Current.Session[ "EditedOfferContentBack" ];
            // get the front object
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            // execute the copy
            editedOfferContentBack.CopyFrom( editedOfferContentFront );
        }

        // database exceptions on update are handled here
        public void _editedOfferDataSourceBack_Updated( object sender, ObjectDataSourceStatusEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ].ToString().CompareTo( EditedOfferManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ].ToString();
                        //     e.ExceptionHandled = true;
                        throw new Exception( string.Format( "Exception encountered during update: {0}", msg ) );
                    }
                }
                else
                {
                    OnUpdateSuccessEvent();
                }
            }
            else
            {
                OnUpdateSuccessEvent();
            }
        }

        protected virtual void OnUpdateSuccessEvent()
        {
            if( UpdateSuccessEvent != null )
            {
                DataRelayUpdateSuccessEventArgs args = new DataRelayUpdateSuccessEventArgs( GetCurrentPage() );
                UpdateSuccessEvent( args );
            }
        }

        public EditedOfferContent EditedOfferContentFront
        {
            get
            {
                if( _editedOfferDataSourceFront != null )
                {
                    return ( _editedOfferDataSourceFront.EditedOfferContentFront );
                }
                else
                {
                    return ( null );
                }
            }
        }

        // prepare a new front content object for an insert operation
        // $$$ may send null first, then set to insure GC. then would/may have to take out update event when setting null to avoid 2 events firing
        public void Clear()
        {
            EditedOfferContent editedOfferContentFront = new EditedOfferContent();
            _editedOfferDataSourceFront.EditedOfferContentFront = editedOfferContentFront; // saved to session by view

            HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = false.ToString();  // clear complete, reset flag
        }

        // call insert on the back object
        public void Insert()
        {
            ClearError();
            
            // get the back object
            EditedOfferContent editedOfferContentBack = ( EditedOfferContent )new EditedOfferContent();
            // get the front object
            EditedOfferContent editedOfferContentFront = ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentFront" ];
            // execute the copy
            editedOfferContentBack.CopyFrom( editedOfferContentFront );

            // insert using the back object - would have liked the ObjectDataSource model to call this, but had parameter errors
            // so this direct call is a workaround.
            EditedOfferManagerBack.CreateOffer( editedOfferContentBack, CustomInsertingEvent );     
        }


        // database exceptions on custom insert are handled here
        // custom inserting just means that the createcontract function was called directly
        void _editedOfferDataSourceBack_CustomInsertingEvent( CustomOfferInsertingEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedOfferDataSourceBackHasError" ].ToString().CompareTo( EditedDocumentManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedOfferDataSourceBackErrorMessage" ].ToString();
                        //     e.ExceptionHandled = true;
                        throw new Exception( string.Format( "Exception encountered during insert: {0}", msg ) );
                    }
                }
                else
                {
                    OnInsertSuccessEvent();
                }
            }
            else
            {
                OnInsertSuccessEvent();
            }
        }

        protected virtual void OnInsertSuccessEvent()
        {
            // recopy the front object to get the newly inserted id's
            _editedOfferDataSourceFront.EditedOfferContentFront.CopyFrom( ( EditedOfferContent )HttpContext.Current.Session[ "EditedOfferContentBack" ] );

            if( InsertSuccessEvent != null )
            {
                DataRelayInsertSuccessEventArgs args = new DataRelayInsertSuccessEventArgs( GetCurrentPage() );
                InsertSuccessEvent( args );
            }
        }



        #region ISerializable Members

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "DocumentTypeString", Enum.GetName( typeof( DocumentTypes ), _documentType ) );
            info.AddValue( "EditedOfferDataSourceFront", _editedOfferDataSourceFront );
            info.AddValue( "EditedOfferDataSourceBack", _editedOfferDataSourceBack );
        }

        protected OfferDataRelay( SerializationInfo info, StreamingContext context )
        {
            _documentType = ( DocumentTypes )Enum.Parse( typeof( DocumentTypes ), info.GetString( "DocumentTypeString" ), false );
            _editedOfferDataSourceFront = ( EditedOfferDataSource )info.GetValue( "EditedOfferDataSourceFront", typeof( EditedOfferDataSource ) );
            _editedOfferDataSourceBack = ( SerializableObjectDataSource )info.GetValue( "EditedOfferDataSourceBack", typeof( SerializableObjectDataSource ) );
          //  _editedOfferDataSourceBack.RestoreDelegatesAfterDeserialization( this );
        }

        // $$$ need to add calls to this anywhere OfferDataRelay object is deserialized 10/21/2015
        public void RestoreDelegatesAfterDeserialization()
        {
             _editedOfferDataSourceBack.RestoreDelegatesAfterDeserialization( this, "OfferDataRelay" );
        }

        #endregion

    }
}
