using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // user specified filter criteria for listing offer headers
    [Serializable]
    public class OfferListFilterParms
    {
        private string _filterValue = "";
        private OfferStatusFilters _offerStatusFilter = OfferStatusFilters.All;
        private OfferOwnerFilters _offerOwnerFilter = OfferOwnerFilters.All;
        private FilterTypes _filterType = FilterTypes.None;
        private bool _bIsPrimaryFilterDirty = true;
        private bool _bIsSecondaryFilterDirty = true;
        private bool _isDirty = false;
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

        
        public OfferListFilterParms()
        {
            _version = 1;
            _offerStatusFilter = OfferStatusFilters.All;
        }

        // to allow backward compatibility
        public OfferListFilterParms( int version )
        {
            _version = version;
            _offerStatusFilter = OfferStatusFilters.None; // in version 2 used to initialize results to no selection
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


        public enum OfferStatusFilters
        {
            All,
            Open,
            Completed,
            None
        }

        public enum OfferOwnerFilters
        {
            All,
            Mine,
            None
        }

        // these match the values in the filter combo box
        public enum FilterTypes
        {
            None,
            ContractingOfficer,          
            Vendor,
            Status,
            Schedule,
            OfferNumber,
            ExtendsContractNumber
        }

        // O - CO Name, V - Vendor, T - Status, S - Schedule, X = none, B = offer number, E = Extends Contract Number
        public static string GetStringFromFilterType( FilterTypes filterType )
        {
            string retString = "X"; // none

            switch( filterType )
            {
                case FilterTypes.ContractingOfficer:
                    retString = "O";
                    break;
                case FilterTypes.Vendor:
                    retString = "V";
                    break;
                case FilterTypes.Status:
                    retString = "T";
                    break;
                case FilterTypes.None:
                    retString = "X";
                    break;
                case FilterTypes.Schedule:
                    retString = "S";
                    break;
                case FilterTypes.OfferNumber:
                    retString = "B";
                    break;
                case FilterTypes.ExtendsContractNumber:
                    retString = "E";
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
                    retString = "FullName";
                    break;
                case FilterTypes.Vendor:
                    retString = "Contractor_Name";
                    break;
                case FilterTypes.Status:
                    retString = "Action_Description";
                    break;
                case FilterTypes.None:
                    retString = "X";
                    break;
                case FilterTypes.Schedule:
                    retString = "Schedule_Name";
                    break;
                case FilterTypes.OfferNumber:
                    retString = "OfferNumber";
                    break;
                case FilterTypes.ExtendsContractNumber:
                    retString = "ExtendsContractNumber";
                    break;

            }
            return ( retString );

        }
        // this corresponds to strings sent on the page query string
        public static OfferStatusFilters GetStatusFilterFromString( string statusFilterString )
        {
            OfferStatusFilters selectedOfferStatusFilter = OfferStatusFilters.All;

            if( statusFilterString.CompareTo( "All" ) == 0 )
                selectedOfferStatusFilter = OfferStatusFilters.All;
            else if( statusFilterString.CompareTo( "Open" ) == 0 )
                selectedOfferStatusFilter = OfferStatusFilters.Open;
            else if( statusFilterString.CompareTo( "Completed" ) == 0 )
                selectedOfferStatusFilter = OfferStatusFilters.Completed;
            else if( statusFilterString.CompareTo( "None" ) == 0 )
                selectedOfferStatusFilter = OfferStatusFilters.None;

            return ( selectedOfferStatusFilter );
        }

        // this corresponds to strings sent on the page query string
        public static OfferOwnerFilters GetOwnerFilterFromString( string ownerFilter )
        {
            OfferOwnerFilters selectedOwnerFilter = OfferOwnerFilters.All;

            if( ownerFilter.CompareTo( "All" ) == 0 )
            {
                selectedOwnerFilter = OfferOwnerFilters.All;
            }
            else if( ownerFilter.CompareTo( "Mine" ) == 0 )
            {
                selectedOwnerFilter = OfferOwnerFilters.Mine;
            }

            return ( selectedOwnerFilter );
        }

        public string FilterValue
        {
            get { return _filterValue; }
            set { _filterValue = value; }
        }

        public OfferStatusFilters OfferStatusFilter
        {
            get { return _offerStatusFilter; }
            set { _offerStatusFilter = value; }
        }

        //  A - All, O - Open, C - Completed
        public static string GetStringFromOfferStatusFilter( OfferStatusFilters offerStatusFilter )
        {
            string retString = "A"; // default to all

            switch( offerStatusFilter )
            {
                case OfferStatusFilters.Open:
                    retString = "O";
                    break;
                case OfferStatusFilters.All:
                    retString = "A";
                    break;
                case OfferStatusFilters.Completed:
                    retString = "C";
                    break;
                case OfferStatusFilters.None:
                    retString = "N";
                    break;
            }

            return ( retString );
        }

        public OfferOwnerFilters OfferOwnerFilter
        {
            get { return _offerOwnerFilter; }
            set { _offerOwnerFilter = value; }
        }

        //  A - All, M - Mine
        public static string GetStringFromOfferOwnerFilter( OfferOwnerFilters offerOwnerFilter )
        {
            string retString = "A"; // default to all

            switch( offerOwnerFilter )
            {
                case OfferOwnerFilters.All:
                    retString = "A";
                    break;
                case OfferOwnerFilters.Mine:
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

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
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
