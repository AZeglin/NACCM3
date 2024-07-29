using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // fields for ItemDiscontinue related FormViews to bind to
    [Serializable]
    public class ItemDiscontinueContent : ISerializable
    {
        private int _selectedDrugItemId = -1; // id of the corresponding record
        private string _fdaAssignedLabelerCode = "";
        private string _productCode = "";
        private string _packageCode = "";
        private int _modificationStatusId = -1;
        private string _sourceContractNumber = "";
        private string _genericName = "";
        private string _tradeName = "";
        private string _dispensingUnit = "";
        private string _packageDescription = "";

        private DateTime _discontinuationDate = DateTime.Today;

        private string _discontinuationReasonString = "";

 
        public ItemDiscontinueContent()
        {
        }

        // window parms provide most of the change parameters
        public ItemDiscontinueContent( ItemDiscontinueWindowParms parms )
        {
            _fdaAssignedLabelerCode = parms.FdaAssignedLabelerCode.Trim();
            _productCode = parms.ProductCode.Trim();
            _packageCode = parms.PackageCode.Trim();
            _modificationStatusId = parms.ModificationStatusId;
            _selectedDrugItemId = parms.SelectedDrugItemId;
            _sourceContractNumber = parms.ContractNumber.Trim();
            _genericName = parms.GenericName.Trim();
            _tradeName = parms.TradeName.Trim();
            _dispensingUnit = parms.DispensingUnit.Trim();
            _packageDescription = parms.PackageDescription.Trim();
            if( parms.DiscontinuationDateString.Length > 0 )
            {
                DateTime testDate;
                if( DateTime.TryParse( parms.DiscontinuationDateString, out testDate ) == true )
                {
                    _discontinuationDate = testDate;
                }

            }
            _discontinuationReasonString = parms.DiscontinuationReasonString;
        }

        // supplements default constructor during automatic object creation
        public void FillItemDiscontinueContent( ItemDiscontinueContent source )
        {
            _fdaAssignedLabelerCode = source.FdaAssignedLabelerCode;
            _productCode = source.ProductCode;
            _packageCode = source.PackageCode;
            _modificationStatusId = source.ModificationStatusId;
            _selectedDrugItemId = source.SelectedDrugItemId;
            _sourceContractNumber = source.SourceContractNumber;
            _genericName = source.GenericName;
            _tradeName = source.TradeName;
            _dispensingUnit = source.DispensingUnit;
            _packageDescription = source.PackageDescription;

            _discontinuationReasonString = source.DiscontinuationReasonString;
            _discontinuationDate = source.DiscontinuationDate;
        }


        public int SelectedDrugItemId
        {
            get { return _selectedDrugItemId; }
            set { _selectedDrugItemId = value; }
        }

        public string FdaAssignedLabelerCode
        {
            get { return _fdaAssignedLabelerCode; }
            set { _fdaAssignedLabelerCode = value; }
        }

        public string ProductCode
        {
            get { return _productCode; }
            set { _productCode = value; }
        }

        public string PackageCode
        {
            get { return _packageCode; }
            set { _packageCode = value; }
        }

        public int ModificationStatusId
        {
            get { return _modificationStatusId; }
            set { _modificationStatusId = value; }
        }

        public string SourceContractNumber
        {
          get { return _sourceContractNumber; }
          set { _sourceContractNumber = value; }
        }

        public string GenericName
        {
            get { return _genericName; }
            set { _genericName = value; }
        }

        public string TradeName
        {
            get { return _tradeName; }
            set { _tradeName = value; }
        }

        public string DispensingUnit
        {
            get { return _dispensingUnit; }
            set { _dispensingUnit = value; }
        }

        public string PackageDescription
        {
            get { return _packageDescription; }
            set { _packageDescription = value; }
        }

        public string DiscontinuationReasonString
        {
            get { return _discontinuationReasonString; }
            set { _discontinuationReasonString = value; }
        }


        public DateTime DiscontinuationDate
        {
            get { return _discontinuationDate; }
            set { _discontinuationDate = value; }
        }

        

        #region ISerializable Members
   

        public ItemDiscontinueContent( SerializationInfo info, StreamingContext context )
        {
            _selectedDrugItemId = info.GetInt32( "SelectedDrugItemId" );
            _fdaAssignedLabelerCode = info.GetString( "FdaAssignedLabelerCode" );
            _productCode = info.GetString( "ProductCode" );
            _packageCode = info.GetString( "PackageCode" );
            _modificationStatusId = info.GetInt32( "ModificationStatusId" );
            _sourceContractNumber = info.GetString( "SourceContractNumber" );
            _genericName = info.GetString( "GenericName" );
            _tradeName = info.GetString( "TradeName" );
            _dispensingUnit = info.GetString( "DispensingUnit" );
            _packageDescription = info.GetString( "PackageDescription" );

            _discontinuationReasonString = info.GetString( "DiscontinuationReason" );
            _discontinuationDate = info.GetDateTime( "ItemDiscontinuationDate" );

        }

        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "SelectedDrugItemId", _selectedDrugItemId );
            info.AddValue( "FdaAssignedLabelerCode", _fdaAssignedLabelerCode );
            info.AddValue( "ProductCode", _productCode );
            info.AddValue( "PackageCode", _packageCode );
            info.AddValue( "ModificationStatusId", _modificationStatusId );
             info.AddValue( "SourceContractNumber", _sourceContractNumber );
            info.AddValue( "GenericName", _genericName );
            info.AddValue( "TradeName", _tradeName  );
            info.AddValue( "DispensingUnit", _dispensingUnit );
            info.AddValue( "PackageDescription", _packageDescription );

            info.AddValue( "DiscontinuationReason", _discontinuationReasonString );
            info.AddValue( "ItemDiscontinuationDate", _discontinuationDate );

        }

        #endregion
    }
}
