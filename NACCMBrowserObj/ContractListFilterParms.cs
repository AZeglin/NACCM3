using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // user specified filter criteria for listing contract headers
    [Serializable]
    public class ContractListFilterParms
    {
        private string _filterValue = "";
        private ContractStatusFilters _contractStatusFilter; 
        private ContractOwnerFilters _contractOwnerFilter = ContractOwnerFilters.All;
        private FilterTypes _filterType = FilterTypes.None;
        private bool _bIsPrimaryFilterDirty = true;
        private bool _bIsSecondaryFilterDirty = true;
        private bool _IsDirty = false;

        private string _sortExpression = "";
        private string _sortDirection = "";
        private bool _bClearOnly = true;

        private int _version = 1;
        private string _eventTargetControlName = "";

        public string EventTargetControlName
        {
            get { return _eventTargetControlName; }
            set { _eventTargetControlName = value; }
        }

        public ContractListFilterParms()
        {
            _version = 1;
            _contractStatusFilter = ContractStatusFilters.All;
        }

        // to allow backward compatibility
        public ContractListFilterParms( int version )
        {
            _version = version;
            _contractStatusFilter = ContractStatusFilters.None; // in version 2 used to initialize results to no selection
        }

        public bool ClearOnly
        {
            get
            {
                return ( _bClearOnly );
            }
            set
            {
                _bClearOnly = value;
            }
        }

        public string SortDirection
        {
            get { return _sortDirection; }
            set { _sortDirection = value; }
        }

        public string SortExpression
        {
            get { return _sortExpression; }
            set { _sortExpression = value; }
        }


        public enum ContractStatusFilters
        {
            All,
            Active,
            Closed,
            None
        }

        public enum ContractOwnerFilters
        {
            All,
            Mine,
            None
        }

        // these match the values in the filter combo box
        public enum FilterTypes
        {
            None,
            ContractNumber,
            ContractingOfficer,
            Vendor,
            Description,
            Schedule
        }

        // N - Number, O - CO Name, V - Vendor, D - Description, S - Schedule, X = none
        public static string GetStringFromFilterType( FilterTypes filterType )
        {
            string retString = "X"; // none

            switch( filterType )
            {
                case FilterTypes.ContractingOfficer:
                    retString = "O";
                    break;
                case FilterTypes.ContractNumber:
                    retString = "N";
                    break;
                case FilterTypes.Description:
                    retString = "D";
                    break;
                case FilterTypes.None:
                    retString = "X";
                    break;
                case FilterTypes.Schedule:
                    retString = "S";
                    break;
                case FilterTypes.Vendor:
                    retString = "V";
                    break;
            }
            return ( retString );

        }

        public static string GetFieldNameFromFilterType( FilterTypes filterType )
        {
            string retString = "X"; // none

            switch( filterType )
            {
                case FilterTypes.ContractingOfficer:
                    retString = "CO_Name";
                    break;
                case FilterTypes.ContractNumber:
                    retString = "CntrctNum";
                    break;
                case FilterTypes.Description:
                    retString = "Drug_Covered";
                    break;
                case FilterTypes.None:
                    retString = "X";
                    break;
                case FilterTypes.Schedule:
                    retString = "Schedule_Name";
                    break;
                case FilterTypes.Vendor:
                    retString = "Contractor_Name";
                    break;
            }
            return ( retString );

        }
        // this corresponds to strings sent on the page query string
        public static ContractStatusFilters GetStatusFilterFromString( string statusFilterString )
        {
            ContractStatusFilters selectedContractStatusFilter = ContractStatusFilters.All;

            if( statusFilterString.CompareTo( "All" ) == 0 )
                selectedContractStatusFilter = ContractStatusFilters.All;
            else if( statusFilterString.CompareTo( "Active" ) == 0 )
                selectedContractStatusFilter = ContractStatusFilters.Active;
            else if( statusFilterString.CompareTo( "Closed" ) == 0 )
                selectedContractStatusFilter = ContractStatusFilters.Closed;
            else if( statusFilterString.CompareTo( "None" ) == 0 )
                selectedContractStatusFilter = ContractStatusFilters.None;


            return ( selectedContractStatusFilter );
        }

        // this corresponds to strings sent on the page query string
        public static ContractOwnerFilters GetOwnerFilterFromString( string ownerFilter )
        {
            ContractOwnerFilters selectedOwnerFilter = ContractOwnerFilters.All;

            if( ownerFilter.CompareTo( "All" ) == 0 )
            {
                selectedOwnerFilter = ContractOwnerFilters.All;
            }
            else if( ownerFilter.CompareTo( "Mine" ) == 0 )
            {
                selectedOwnerFilter = ContractOwnerFilters.Mine;
            }

            return ( selectedOwnerFilter );
        }

 

        public string FilterValue
        {
            get { return _filterValue; }
            set 
            {
                _filterValue = value;
 
            }
        }

        public ContractStatusFilters ContractStatusFilter
        {
            get { return _contractStatusFilter; }
            set 
            { 
                _contractStatusFilter = value;
       
            }
        }

        //  A - All, T - Active, C - Closed
        public static string GetStringFromContractStatusFilter( ContractStatusFilters contractStatusFilter )
        {
            string retString = "A"; // default to all

            switch( contractStatusFilter )
            {
                case ContractStatusFilters.Active:
                    retString = "T";
                    break;
                case ContractStatusFilters.All:
                    retString = "A";
                    break;
                case ContractStatusFilters.Closed:
                    retString = "C";
                    break;
                case ContractStatusFilters.None:
                    retString = "N";
                    break;
            }

            return ( retString );
        }

        public ContractOwnerFilters ContractOwnerFilter
        {
            get { return _contractOwnerFilter; }
            set { _contractOwnerFilter = value; }
        }

        //  A - All, M - Mine
        public static string GetStringFromContractOwnerFilter( ContractOwnerFilters contractOwnerFilter )
        {
            string retString = "A"; // default to all

            switch( contractOwnerFilter )
            {
                case ContractOwnerFilters.All:
                    retString = "A";
                    break;
                case ContractOwnerFilters.Mine:
                    retString = "M";
                    break;
            }

            return ( retString );
        }

        public FilterTypes FilterType
        {
            get { return _filterType; }
            set { _filterType = value; }
        }

        // left in for backward compatibility with version 1
        public bool IsDirty
        {
            get { return _IsDirty; }
            set { _IsDirty = value; }
        }

        public bool IsPrimaryFilterDirty
        {
            get { return _bIsPrimaryFilterDirty; }
            set { _bIsPrimaryFilterDirty = value; }
        }

        public bool IsSecondaryFilterDirty
        {
            get { return _bIsSecondaryFilterDirty; }
            set { _bIsSecondaryFilterDirty = value; }
        }
    }
}
