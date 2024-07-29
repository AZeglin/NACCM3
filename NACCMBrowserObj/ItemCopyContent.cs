using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // fields for ItemCopy related FormViews to bind to
    [Serializable]
    public class ItemCopyContent : ISerializable
    {
        private string _copyType = "";
        private int _selectedDrugItemId = -1; // id of the corresponding record
        private string _fdaAssignedLabelerCode = "";
        private string _productCode = "";
        private string _packageCode = "";
        private int _modificationStatusId = -1;
        private bool _bCopyPricing = true;
        private bool _bCopySubItems = true;
        private string _sourceContractNumber = "";
        private string _genericName = "";
        private string _tradeName = "";
        private string _destinationContractNumber = "";
        private int _destinationContractId = -1;
        private string _dispensingUnit = "";
        private string _packageDescription = "";

        private string _unitOfSale = "";
        private decimal _quantityInUnitOfSale = 0;
        private string _unitPackage = "";
        private decimal _quantityInUnitPackage = 0;
        private string _unitOfMeasure = "";


        // results of insert
        private int _newDrugItemId = -1;
        private int _newDrugItemNDCId = -1;
        private int _newDrugItemPackageId = -1;

        public ItemCopyContent()
        {
        }

        // window parms provide most of the change parameters
        public ItemCopyContent( ItemCopyWindowParms parms )
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

            _unitOfSale = parms.UnitOfSale;
            _quantityInUnitOfSale = parms.QuantityInUnitOfSale;
            _unitPackage = parms.UnitPackage;
            _quantityInUnitPackage = parms.QuantityInUnitPackage;
            _unitOfMeasure = parms.UnitOfMeasure;

            if( parms.CopyType == ItemCopyWindowParms.CopyTypes.CopyLocal )
                _copyType = "CopyToSame";
            else
                _copyType = "CopyToDestination";
        }

        // used for CopyToSame copyType
        public ItemCopyContent( string fdaAssignedLabelerCode, string productCode, string packageCode, string genericName, string tradeName, string dispensingUnit, string packageDescription, bool bCopyPricing, bool bCopySubItems,
            string unitOfSale, decimal quantityInUnitOfSale, string unitPackage, decimal quantityInUnitPackage, string unitOfMeasure )
        {
            _fdaAssignedLabelerCode = fdaAssignedLabelerCode;
            _productCode = productCode;
            _packageCode = packageCode;
            _genericName = genericName;
            _tradeName = tradeName;
            _dispensingUnit = dispensingUnit;
            _packageDescription = packageDescription;
            _bCopyPricing = bCopyPricing;
            _bCopySubItems = bCopySubItems;

            _unitOfSale = unitOfSale;
            _quantityInUnitOfSale = quantityInUnitOfSale;
            _unitPackage = unitPackage;
            _quantityInUnitPackage = quantityInUnitPackage;
            _unitOfMeasure = unitOfMeasure;

            _copyType = "CopyToSame";
        }

        // used for CopyToDestination copyType
        public ItemCopyContent( string fdaAssignedLabelerCode, string productCode, string packageCode, string destinationContractNumber, bool bCopyPricing, bool bCopySubItems )
        {
            _fdaAssignedLabelerCode = fdaAssignedLabelerCode;
            _productCode = productCode;
            _packageCode = packageCode;
            _destinationContractNumber = destinationContractNumber;
            _bCopyPricing = bCopyPricing;
            _bCopySubItems = bCopySubItems;
            _copyType = "CopyToDestination";
        }

        // supplements default constructor during automatic object creation
        public void FillItemCopyContent( ItemCopyContent source )
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

            _unitOfSale = source.UnitOfSale;
            _quantityInUnitOfSale = source.QuantityInUnitOfSale;
            _unitPackage = source.UnitPackage;
            _quantityInUnitPackage = source.QuantityInUnitPackage;
            _unitOfMeasure = source.UnitOfMeasure;

            _copyType = source.CopyType;
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

        public bool CopyPricing
        {
            get { return _bCopyPricing; }
            set { _bCopyPricing = value; }
        }

        public bool CopySubItems
        {
            get { return _bCopySubItems; }
            set { _bCopySubItems = value; }
        }

        public string SourceContractNumber
        {
          get { return _sourceContractNumber; }
          set { _sourceContractNumber = value; }
        }

        public int NewDrugItemId
        {
            get { return _newDrugItemId; }
            set { _newDrugItemId = value; }
        }

        public int NewDrugItemNDCId
        {
            get { return _newDrugItemNDCId; }
            set { _newDrugItemNDCId = value; }
        }

        public int NewDrugItemPackageId
        {
            get { return _newDrugItemPackageId; }
            set { _newDrugItemPackageId = value; }
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

        public string DestinationContractNumber
        {
            get { return _destinationContractNumber; }
            set { _destinationContractNumber = value; }
        }

        public string CopyType
        {
            get { return _copyType; }
            set { _copyType = value; }
        }

        public int DestinationContractId
        {
            get { return _destinationContractId; }
            set { _destinationContractId = value; }
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

        public string UnitOfSale
        {
            get { return _unitOfSale; }
            set { _unitOfSale = value; }
        }

        public decimal QuantityInUnitOfSale
        {
            get { return _quantityInUnitOfSale; }
            set { _quantityInUnitOfSale = value; }
        }

        public string UnitPackage
        {
            get { return _unitPackage; }
            set { _unitPackage = value; }
        }

        public decimal QuantityInUnitPackage
        {
            get { return _quantityInUnitPackage; }
            set { _quantityInUnitPackage = value; }
        }

        public string UnitOfMeasure
        {
            get { return _unitOfMeasure; }
            set { _unitOfMeasure = value; }
        }

        #region ISerializable Members
   

        public ItemCopyContent( SerializationInfo info, StreamingContext context )
        {
            _copyType = info.GetString( "CopyType" );
            _selectedDrugItemId = info.GetInt32( "SelectedDrugItemId" );
            _fdaAssignedLabelerCode = info.GetString( "FdaAssignedLabelerCode" );
            _productCode = info.GetString( "ProductCode" );
            _packageCode = info.GetString( "PackageCode" );
            _modificationStatusId = info.GetInt32( "ModificationStatusId" );
            _bCopyPricing = info.GetBoolean( "CopyPricing" );
            _bCopySubItems = info.GetBoolean( "CopySubItems" );
            _sourceContractNumber = info.GetString( "SourceContractNumber" );
            _genericName = info.GetString( "GenericName" );
            _tradeName = info.GetString( "TradeName" );
            _dispensingUnit = info.GetString( "DispensingUnit" );
            _packageDescription = info.GetString( "PackageDescription" );
            _destinationContractNumber = info.GetString( "DestinationContractNumber" );
            _destinationContractId = info.GetInt32( "DestinationContractId" );
            _newDrugItemId = info.GetInt32( "NewDrugItemId" );
            _newDrugItemNDCId = info.GetInt32( "NewDrugItemNDCId" );
            _newDrugItemPackageId = info.GetInt32( "NewDrugItemPackageId" );

            _unitOfSale = info.GetString( "UnitOfSale" );
            _quantityInUnitOfSale = info.GetDecimal( "QuantityInUnitOfSale" );
            _unitPackage = info.GetString( "UnitPackage" );
            _quantityInUnitPackage = info.GetDecimal( "QuantityInUnitPackage" );
            _unitOfMeasure = info.GetString( "UnitOfMeasure" );

        }

        public void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "CopyType", _copyType );
            info.AddValue( "SelectedDrugItemId", _selectedDrugItemId );
            info.AddValue( "FdaAssignedLabelerCode", _fdaAssignedLabelerCode );
            info.AddValue( "ProductCode", _productCode );
            info.AddValue( "PackageCode", _packageCode );
            info.AddValue( "ModificationStatusId", _modificationStatusId );
            info.AddValue( "CopyPricing", _bCopyPricing );
            info.AddValue( "CopySubItems", _bCopySubItems );
            info.AddValue( "SourceContractNumber", _sourceContractNumber );
            info.AddValue( "GenericName", _genericName );
            info.AddValue( "TradeName", _tradeName  );
            info.AddValue( "DispensingUnit", _dispensingUnit );
            info.AddValue( "PackageDescription", _packageDescription );
            info.AddValue( "DestinationContractNumber", _destinationContractNumber );
            info.AddValue( "DestinationContractId", _destinationContractId );
            info.AddValue( "NewDrugItemId", _newDrugItemId );
            info.AddValue( "NewDrugItemNDCId", _newDrugItemNDCId );
            info.AddValue( "NewDrugItemPackageId", _newDrugItemPackageId );

            info.AddValue( "UnitOfSale", _unitOfSale );
            info.AddValue( "QuantityInUnitOfSale", _quantityInUnitOfSale );
            info.AddValue( "UnitPackage", _unitPackage );
            info.AddValue( "QuantityInUnitPackage", _quantityInUnitPackage );
            info.AddValue( "UnitOfMeasure", _unitOfMeasure );
        }

        #endregion
    }
}
