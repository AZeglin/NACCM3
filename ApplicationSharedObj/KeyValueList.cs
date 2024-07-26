using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;

namespace VA.NAC.Application.SharedObj
{
   	/// <summary>
	/// Acts as a simulated hash table that can be serialized.
    /// 
    /// Usage
    ///  [XmlInclude( typeof( Obj1 ) )]
    ///  [XmlInclude( typeof( VA.NAC.Application.SharedObj.KeyValueList<Obj1List> ) )]
    ///  public class Obj1List : KeyValueList<Obj1List>
    
	/// </summary>
	[ Serializable ]
	public class KeyValueList< DERIVEDTYPE > : ISerializable, IKeyValueList
	{
		[XmlArray( "KeyList" )]
		private ArrayList _keyList = null;
		[XmlArray( "ValueList" )]
		private ArrayList _valueList = null;
		private string _typeName = "";

		public string TypeName
		{
			get
			{
				return( _typeName );
			}
			set
			{
				_typeName = value;
			}
		}

		public KeyValueList( string typeName )
		{
			_typeName = typeName;
			_keyList = new ArrayList();
			_valueList = new ArrayList();
		}

		public KeyValueList()
		{
			_keyList = new ArrayList();
			_valueList = new ArrayList();
		}

		public ArrayList Keys
		{
			get
			{
				return( _keyList );
			}
			set
			{
				_keyList = value;
			}
		}

		public ArrayList Values
		{
			get
			{
				return( _valueList );
			}
			set
			{
				_valueList = value;
			}
		}

		// access values by index
		public object this [ int index ]
		{
			get
			{
				return( ( object )_valueList[ index ] );
			}
			set
			{
				_valueList[ index ] = value;
			}
		}

		public void Add( string key, object val )
		{
			lock( this )
			{
				_keyList.Add( key );
				_valueList.Add( val );
			}
		}

        public void Add( int key, object val )
        {
            lock( this )
            {
                _keyList.Add( key );
                _valueList.Add( val );
            }
        }

		public void Remove( string key )
		{
			lock( this )
			{
				int keyLoc = -1;
                object val = null;
				keyLoc = KeyLoc( key );
				if( keyLoc >= 0 )
				{
                    val = _valueList[ keyLoc ];
                    _valueList.Remove( val );
					_keyList.Remove( key );
				}
			}
		}

        public void Remove( int key )
        {
            lock( this )
            {
                int keyLoc = -1;
                object val = null;
                keyLoc = KeyLoc( key );
                if( keyLoc >= 0 )
                {
                    val = _valueList[ keyLoc ];
                    _valueList.Remove( val );
                    _keyList.Remove( key );
                }
            }
        }

        public void Clear()
        {
            _keyList.Clear();
            _valueList.Clear();
        }

		public int Count()
		{
			lock( this )
			{
				return( _keyList.Count );
			}
		}

		public object GetValue( string key )
		{
			lock( this )
			{
				int keyLoc = -1;
				keyLoc = KeyLoc( key );
				if( keyLoc >= 0 )
					return( _valueList[ keyLoc ] );
				else
					return( null );
			}
		}

        public object GetValue( int key )
        {
            lock( this )
            {
                int keyLoc = -1;
                keyLoc = KeyLoc( key );
                if( keyLoc >= 0 )
                    return ( _valueList[ keyLoc ] );
                else
                    return ( null );
            }
        }

		public bool SetValue( string key, object val )
		{
			bool bFound = false;
			lock( this )
			{
				int keyLoc = -1;
				keyLoc = KeyLoc( key );
				if( keyLoc >= 0 )
				{
					_valueList[ keyLoc ] = val;
					bFound = true;
				}
			}
			return( bFound );
		}

        public bool SetValue( int key, object val )
        {
            bool bFound = false;
            lock( this )
            {
                int keyLoc = -1;
                keyLoc = KeyLoc( key );
                if( keyLoc >= 0 )
                {
                    _valueList[ keyLoc ] = val;
                    bFound = true;
                }
            }
            return ( bFound );
        }

		public bool ContainsKey( string key )
		{
			lock( this )
			{
				if( KeyLoc( key ) >= 0 )
					return( true );
				else
					return( false );
			}
		}

        public bool ContainsKey( int key )
        {
            lock( this )
            {
                if( KeyLoc( key ) >= 0 )
                    return ( true );
                else
                    return ( false );
            }
        }
		private int KeyLoc( string key )
		{
			int keyLoc = -1;
			for( int i = 0; i < _keyList.Count; i++ )
			{
				if( (( string )_keyList[ i ]).CompareTo( key ) == 0 )
				{
					keyLoc = i;
					break;
				}
			}
			return( keyLoc );
		}

        private int KeyLoc( int key )
        {
            int keyLoc = -1;
            for( int i = 0; i < _keyList.Count; i++ )
            {
                if( ( int )_keyList[ i ] == key )
                {
                    keyLoc = i;
                    break;
                }
            }
            return ( keyLoc );
        }

        public string Serialize()
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer s = new XmlSerializer( typeof( DERIVEDTYPE ) );
            s.Serialize( ms, this );
            ms.Seek( 0, 0 );

            SerializationHelpers sh = new SerializationHelpers();
            string returnString = sh.ConvertByteToString( ms.GetBuffer() );

            ms.Close();

            return ( returnString );
        }

        public static void Deserialize( string sourceString, ref DERIVEDTYPE derivedObject )
        {
            SerializationHelpers sh = new SerializationHelpers();
            byte[] blobToRetrieve = sh.ConvertStringToByte( sourceString );

            XmlSerializer s = new XmlSerializer( typeof( DERIVEDTYPE ) );
            MemoryStream ms = new MemoryStream( blobToRetrieve, 0, blobToRetrieve.Length, true, true );
            ms.Seek( 0, 0 );

            derivedObject = ( DERIVEDTYPE )s.Deserialize( ms );

            ms.Close();
        }

        // for now save [] notation for index lookup only
        //public object this [ string key ]
        //{
        //    get
        //    {
        //        return( GetValue( key ));
        //    }
        //    set
        //    {
        //        SetValue( key, value );
        //    }
        //}

        //public object this[ int key ]
        //{
        //    get
        //    {
        //        return ( GetValue( key ) );
        //    }
        //    set
        //    {
        //        SetValue( key, value );
        //    }
        //}
       
		#region ISerializable Members

		public void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			// save a count of the keys
			info.AddValue( "Count", _keyList.Count );

			// save the type name
			info.AddValue( "TypeName", _typeName );

			// save the keys
			for( int i = 0; i < _keyList.Count; i++ )
			{
				info.AddValue( String.Format( "key_{0}", i.ToString()), _keyList[ i ] );
			}

			// save the values
			for( int i = 0; i < _keyList.Count; i++ )
			{
				info.AddValue( String.Format( "val_{0}", i.ToString()), _valueList[ i ], Type.GetType( _typeName ));
			}
		}

		public KeyValueList( SerializationInfo info, StreamingContext context)
		{
			// retrieve the count
			int count = info.GetInt32( "Count" );

			// retrieve the value type name
			_typeName = info.GetString( "TypeName" );

			// retrieve the keys
			for( int i = 0; i < count; i++ )
			{
				_keyList[ i ] = info.GetString( String.Format( "key_{0}", i.ToString()) );
			}

			// retrieve the values
			for( int i = 0; i < count; i++ )
			{
				_valueList[ i ] = info.GetValue( String.Format( "val_{0}", i.ToString()), Type.GetType( _typeName ));	
			}
		}

		#endregion
	}


}
