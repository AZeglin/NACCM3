using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace VA.NAC.Application.SharedObj
{
	/// <summary>
	/// Groups common functions required for serialization.
	/// </summary>
	public class SerializationHelpers
	{
		public SerializationHelpers()
		{
		}

		// convert byte array to string
		public string ConvertByteToString( byte[] bytesIn )
		{
			Encoding ascii = Encoding.ASCII;

			char[] asciiChars = new char[ ascii.GetCharCount( bytesIn, 0, bytesIn.Length ) ];
			
			ascii.GetChars( bytesIn, 0, bytesIn.Length, asciiChars, 0 );

			// byte array may contain trailing nulls which must be removed
			string temp = new string( asciiChars );
			char[] trimChars = { '\0' };
			string trimmedString = temp.TrimEnd( trimChars );

			return( trimmedString );
		}

		// convert string to byte array
		public byte[] ConvertStringToByte( string sIn )
		{
			Encoding ascii = Encoding.ASCII;

			byte[] bytes = new byte[ ascii.GetByteCount( sIn ) ];

			ascii.GetBytes( sIn, 0, bytes.Length, bytes, 0 );

			return( bytes );
		}

        public int GetByteCount( string sIn )
        {
            Encoding ascii = Encoding.ASCII;

            return( ascii.GetByteCount( sIn ) );
        }
	}
}
