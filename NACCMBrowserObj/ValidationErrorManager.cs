using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Configuration;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml.Serialization;
using System.Reflection;

using VA.NAC.Application.SharedObj;
using VA.NAC.NACCMBrowser.DBInterface;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class ValidationErrorManager : ISerializable
    {
        public DocumentTypes _documentType = DocumentTypes.Undefined;

        public DocumentTypes DocumentType
        {
            get
            {
                return _documentType;
            }
            set
            {
                _documentType = value;
            }
        }

        

        public string _currentValidationGroupName = "";

        public ArrayList _validationErrorList = null;

        public StringBuilder _validationMessageStringBuilder = null;

        private int _outputSpacing = 4;

        public int OutputSpacing
        {
            get
            {
                return _outputSpacing;
            }

            set
            {
                _outputSpacing = value;
            }
        }

        public ValidationErrorManager( DocumentTypes documentType )
        {
            _documentType = documentType;

            _validationErrorList = new ArrayList( 200 );   // new List<ValidationError>();
        }

        [Serializable]
        public class ValidationError : IComparable, ISerializable
        {
            public string ValidationGroupName;
            public string ErrorMessage;

            public ValidationError( string validationGroupName, string errorMessage )
            {
                ValidationGroupName = validationGroupName;
                ErrorMessage = errorMessage;
            }

            public int CompareTo( object obj )
            {
                if( obj == null )
                    return 1;

                ValidationError otherValidationError = obj as ValidationError;
                if( otherValidationError != null )
                    return this.ValidationGroupName.CompareTo( otherValidationError.ValidationGroupName );
                else
                    throw new ArgumentException( "Object is not a ValidationError" );
            }

            [SecurityCritical]
            void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
            {
                info.AddValue( "ValidationGroupName", ValidationGroupName );
                info.AddValue( "ErrorMessage", ErrorMessage );
            }

            protected ValidationError( SerializationInfo info, StreamingContext context )
            {
                ValidationGroupName = info.GetString( "ValidationGroupName" );
                ErrorMessage = info.GetString( "ErrorMessage" );
            }

        }

        public override string ToString()
        {
            _validationMessageStringBuilder = null;
            _validationMessageStringBuilder = new StringBuilder( 200 );

            int newLineCount = ( int )Math.Round( Math.Ceiling( ( decimal )( OutputSpacing / 2 ) ), 0 );
            string newLineString = new string( '\n', newLineCount );

            _validationMessageStringBuilder.AppendFormat( string.Format( "Please correct the following: {0}", newLineString ));

            _validationErrorList.Sort();
          
            string formatString = string.Format( "{0}{1}{2}", newLineString, "{0}", newLineString );

            foreach( ValidationError validationError in _validationErrorList )
            {
                _validationMessageStringBuilder.AppendFormat( formatString, validationError.ErrorMessage );
            }

            return ( _validationMessageStringBuilder.ToString() );
        }

        public void ResetValidationGroup( string validationGroupName )
        {
            _currentValidationGroupName = validationGroupName;

            ArrayList removeList = new ArrayList( 200 );

            for( int i = 0; i < _validationErrorList.Count; i++ )
            {
                ValidationError validationError = ( ValidationError )_validationErrorList[ i ];
                if( validationError != null )
                {
                    if( validationError.ValidationGroupName.CompareTo( validationGroupName ) == 0 )
                        removeList.Add( validationError );
                }
            }

            for( int i = 0; i < removeList.Count; i++ )
            {
                ValidationError validationError = ( ValidationError )removeList[ i ];
                if( validationError != null )
                {                    
                    _validationErrorList.Remove( validationError );
                }
            }            
        }

        public void AppendValidationError( string message, bool bIsShortSave )
        {
            _validationErrorList.Add( new ValidationError( _currentValidationGroupName, message ) );            
        }

        public bool HasErrors()
        {
            bool bHasErrors = false;

            if( _validationErrorList.Count > 0 )
                bHasErrors = true;

            return ( bHasErrors );
        }

        #region ISerializable Members

        public int _errorCount = 0;

        [SecurityCritical]
        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            _errorCount = _validationErrorList.Count;

            info.AddValue( "ErrorCount", _errorCount );
            info.AddValue( "DocumentTypeString", Enum.GetName( typeof( DocumentTypes ), _documentType ) );
            info.AddValue( "ValidationMessageStringBuilder", _validationMessageStringBuilder );

            for( int i = 0; i < _validationErrorList.Count; i++ )
            {
                info.AddValue( String.Format( "val_{0}", i.ToString() ), _validationErrorList[ i ], typeof( ValidationError ) );
            }
        }

        protected ValidationErrorManager( SerializationInfo info, StreamingContext context )
        {
            _errorCount = ( int )info.GetValue( "ErrorCount", typeof( int ) );
            _documentType = ( DocumentTypes )Enum.Parse( typeof( DocumentTypes ), info.GetString( "DocumentTypeString" ), false );
            _validationMessageStringBuilder = ( StringBuilder )info.GetValue( "ValidationMessageStringBuilder" , typeof( StringBuilder ) );

            _validationErrorList = new ArrayList( 200 );

            for( int i = 0; i < _errorCount; i++ )
            {
                _validationErrorList.Add( ( ValidationError )info.GetValue( String.Format( "val_{0}", i.ToString() ), typeof( ValidationError ) ) );
            }
        }

      

        //public string Serialize()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    XmlSerializer s = new XmlSerializer( typeof( ValidationError ) );
        //    s.Serialize( ms, this );
        //    ms.Seek( 0, 0 );

        //    SerializationHelpers sh = new SerializationHelpers();
        //    string returnString = sh.ConvertByteToString( ms.GetBuffer() );

        //    ms.Close();

        //    return ( returnString );
        //}

        //public static void Deserialize( string sourceString, ref ValidationError derivedObject )
        //{
        //    SerializationHelpers sh = new SerializationHelpers();
        //    byte[] blobToRetrieve = sh.ConvertStringToByte( sourceString );

        //    XmlSerializer s = new XmlSerializer( typeof( ValidationError ) );
        //    MemoryStream ms = new MemoryStream( blobToRetrieve, 0, blobToRetrieve.Length, true, true );
        //    ms.Seek( 0, 0 );

        //    derivedObject = ( ValidationError )s.Deserialize( ms );

        //    ms.Close();
        //}
        #endregion
    }
}
