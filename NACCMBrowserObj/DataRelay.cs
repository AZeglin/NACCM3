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
    public delegate void UpdateSuccessEventHandler( DataRelayUpdateSuccessEventArgs args );
    public delegate void InsertSuccessEventHandler( DataRelayInsertSuccessEventArgs args );
    // not part of ObjectDataSource
    public delegate void CustomDocumentInsertingEventHandler( CustomDocumentInsertingEventArgs args );

    public enum DocumentTypes
    {
        Undefined,
        Contract,
        Offer
    };

    public interface IDataRelay
    {
        DocumentTypes DocumentType { get; set; }
    }

    [Serializable]
    public class DataRelay : IDataRelay, ISerializable
    {
        public event UpdateSuccessEventHandler UpdateSuccessEvent;
        public event InsertSuccessEventHandler InsertSuccessEvent;
        protected event CustomDocumentInsertingEventHandler CustomInsertingEvent;

        public DataRelay( DocumentTypes documentType )
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
            this.CustomInsertingEvent += new CustomDocumentInsertingEventHandler( _editedDocumentDataSourceBack_CustomInsertingEvent );
        }

        public Page GetCurrentPage()
        {
            return ( _currentPage );
        }

        private EditedDocumentDataSource _editedDocumentDataSourceFront = null;

        public EditedDocumentDataSource EditedDocumentDataSourceFront
        {
            get { return _editedDocumentDataSourceFront; }
            set { _editedDocumentDataSourceFront = value; }
        }

        // used when edit in progress is complete and user has called save
        private SerializableObjectDataSource _editedDocumentDataSourceBack = null;

        public SerializableObjectDataSource EditedDocumentDataSourceBack
        {
            get { return _editedDocumentDataSourceBack; }
            set { _editedDocumentDataSourceBack = value; }
        }

        private void Init()
        {
            ClearSessionVariables();

            _editedDocumentDataSourceBack = null;
            _editedDocumentDataSourceBack = new SerializableObjectDataSource();
            _editedDocumentDataSourceBack.ID = "EditedDocumentDataSourceBack";
            _editedDocumentDataSourceBack.DataObjectTypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditedDocumentContent";
            _editedDocumentDataSourceBack.TypeName = "VA.NAC.NACCMBrowser.BrowserObj.EditedDocumentManagerBack";
            _editedDocumentDataSourceBack.SelectMethod = "GetEditedDocumentContent";
            _editedDocumentDataSourceBack.UpdateMethod = "SaveChanges";
            _editedDocumentDataSourceBack.SetEventOwnerName( "DataRelay" );
         // _editedDocumentDataSourceBack.InsertMethod = "CreateDocument";
            _editedDocumentDataSourceBack.Updating += new ObjectDataSourceMethodEventHandler( _editedDocumentDataSourceBack_Updating );
            _editedDocumentDataSourceBack.Updated += new ObjectDataSourceStatusEventHandler( _editedDocumentDataSourceBack_Updated );
            _editedDocumentDataSourceBack.Selected += new ObjectDataSourceStatusEventHandler(_editedDocumentDataSourceBack_Selected);
         // _editedDocumentDataSourceBack.Inserting += new ObjectDataSourceMethodEventHandler( _editedDocumentDataSourceBack_Inserting );
         // _editedDocumentDataSourceBack.Inserted += new ObjectDataSourceStatusEventHandler( _editedDocumentDataSourceBack_Inserted );


            HttpContext.Current.Session[ "EditedDocumentDataSourceBack" ] = _editedDocumentDataSourceBack;

            _editedDocumentDataSourceFront = null;
            _editedDocumentDataSourceFront = new EditedDocumentDataSource();
            _editedDocumentDataSourceFront.ID = "EditedDocumentDataSourceFront";

            EditedDocumentContent editedDocumentContentFront = new EditedDocumentContent();
            _editedDocumentDataSourceFront.EditedDocumentContentFront = editedDocumentContentFront; // saved to session by view

            HttpContext.Current.Session[ "EditedDocumentDataSourceFront" ] = _editedDocumentDataSourceFront;
        }


        private void ClearSessionVariables()
        {
            HttpContext.Current.Session[ "EditedDocumentContentFront" ] = null;
            HttpContext.Current.Session[ "EditedDocumentDataSourceFront" ] = null;
            HttpContext.Current.Session[ "EditedDocumentContentBack" ] = null;
            HttpContext.Current.Session[ "EditedDocumentDataSourceBack" ] = null;
            HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = null;
            HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = null;
        }

        public void ClearError()
        {
            HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] = "";
            HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] = "";
        }

        public void Load()
        {
            _editedDocumentDataSourceBack.Select();

            _editedDocumentDataSourceFront.EditedDocumentContentFront.CopyFrom( ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentBack" ] );

            HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = false.ToString();  // load complete, reset flag
        }

        public void _editedDocumentDataSourceBack_Selected( object sender, ObjectDataSourceStatusEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ].ToString().CompareTo( EditedDocumentManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ].ToString();
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
            _editedDocumentDataSourceBack.Update();
        }

        // call update on the front object only, 
        // used to save a screen when tabbing
        public void UpdateFront( EditedDocumentContent editedDocumentContentFront )
        {
            _editedDocumentDataSourceFront.UpdateFront( editedDocumentContentFront );

        }

        // when update is called on the back object, the front object is copied onto the back
        public void _editedDocumentDataSourceBack_Updating( object sender, ObjectDataSourceMethodEventArgs e )
        {
            // get the back object
            EditedDocumentContent editedDocumentContentBack = ( EditedDocumentContent )e.InputParameters[ "editedDocumentContentBack" ];  // no: HttpContext.Current.Session[ "EditedDocumentContentBack" ];
            // get the front object
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            // execute the copy
            editedDocumentContentBack.CopyFrom( editedDocumentContentFront );
        }

        // database exceptions on update are handled here
        public void _editedDocumentDataSourceBack_Updated( object sender, ObjectDataSourceStatusEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ].ToString().CompareTo( EditedDocumentManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ].ToString();
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


        public EditedDocumentContent EditedDocumentContentFront
        {
            get 
            {
                if( _editedDocumentDataSourceFront != null )
                {
                    return ( _editedDocumentDataSourceFront.EditedDocumentContentFront );
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
            EditedDocumentContent editedDocumentContentFront = new EditedDocumentContent();
            _editedDocumentDataSourceFront.EditedDocumentContentFront = editedDocumentContentFront; // saved to session by view

            HttpContext.Current.Session[ "CurrentDocumentIsChanging" ] = false.ToString();  // clear complete, reset flag
        }

        // call insert on the back object
        public void Insert()
        {
            ClearError();

            // get the back object
            EditedDocumentContent editedDocumentContentBack = ( EditedDocumentContent )new EditedDocumentContent(); 
            // get the front object
            EditedDocumentContent editedDocumentContentFront = ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentFront" ];
            // execute the copy
            editedDocumentContentBack.CopyFrom( editedDocumentContentFront );

            // insert using the back object - would have liked the ObjectDataSource model to call this, but had parameter errors
            // so this direct call is a workaround.
            EditedDocumentManagerBack.CreateDocument( editedDocumentContentBack, CustomInsertingEvent );     
        }


        // database exceptions on custom insert are handled here
        // custom inserting just means that the createcontract function was called directly
        void _editedDocumentDataSourceBack_CustomInsertingEvent( CustomDocumentInsertingEventArgs e )
        {
            if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ] != null )
            {
                if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackHasError" ].ToString().CompareTo( EditedDocumentManagerBack.ErrorIndicator ) == 0 )
                {
                    if( HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ] != null )
                    {
                        string msg = HttpContext.Current.Session[ "EditedDocumentDataSourceBackErrorMessage" ].ToString();
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
            _editedDocumentDataSourceFront.EditedDocumentContentFront.CopyFrom( ( EditedDocumentContent )HttpContext.Current.Session[ "EditedDocumentContentBack" ] );

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
            info.AddValue( "DocumentTypeString", Enum.GetName( typeof( DocumentTypes ), _documentType ));
            info.AddValue( "EditedDocumentDataSourceFront", _editedDocumentDataSourceFront );
            info.AddValue( "EditedDocumentDataSourceBack", _editedDocumentDataSourceBack );
        }

        protected DataRelay( SerializationInfo info, StreamingContext context )
        {
            _documentType = ( DocumentTypes )Enum.Parse( typeof( DocumentTypes ), info.GetString( "DocumentTypeString" ), false );
            _editedDocumentDataSourceFront = ( EditedDocumentDataSource )info.GetValue( "EditedDocumentDataSourceFront", typeof( EditedDocumentDataSource ) );
            _editedDocumentDataSourceBack = ( SerializableObjectDataSource )info.GetValue( "EditedDocumentDataSourceBack", typeof( SerializableObjectDataSource ) );
        //    _editedDocumentDataSourceBack.RestoreDelegatesAfterDeserialization( this );
        }

        // $$$ need to add calls to this anywhere DataRelay object is deserialized 10/16/2015
        public void RestoreDelegatesAfterDeserialization()
        {
            // _editedDocumentDataSourceFront.RestoreDelegatesAfterDeserialization( this );  not possible or needed since no events in the front object
            _editedDocumentDataSourceBack.RestoreDelegatesAfterDeserialization( this, "DataRelay" );    
        }

        #endregion

    }
}
