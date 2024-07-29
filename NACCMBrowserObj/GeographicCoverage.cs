using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using VA.NAC.NACCMBrowser.DBInterface;
using VA.NAC.NACCMBrowser.BrowserObj;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    [Serializable]
    public class GeographicCoverage
    {
        private string _errorMessage = "";

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public GeographicCoverage()
        {
        }

        public GeographicCoverage( GeographicCoverage sourceObj )
        {
            CopyFrom( sourceObj );
        }

        public bool CompleteGeographicCoverage( DataSet dsGeographicCoverage )
        {
            bool bSuccess = false;
            if( dsGeographicCoverage != null )
            {
                if( dsGeographicCoverage.Tables[ "GeographicCoverageTable" ] != null )
                {
                    if( dsGeographicCoverage.Tables[ "GeographicCoverageTable" ].Rows.Count > 0 )
                    {
                        DataRow row = dsGeographicCoverage.Tables[ "GeographicCoverageTable" ].Rows[ 0 ];

                        if( row != null )
                        {
                            try
                            {
                                CopyRowToObjectMembers( row );

                                bSuccess = true;
                            }
                            catch( Exception ex )
                            {
                                ErrorMessage = string.Format( "Exception encountered when copying row to object members: {0}", ex.Message );
                            }
                        }
                        else
                        {
                            ErrorMessage = "Row returned from geographic coverage table was null.";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Geographic coverage table had zero rows.";
                    }
                }
                else
                {
                    ErrorMessage = "Geographic coverage table was not found in the returned recordset.";
                }
            }
            else
            {
                ErrorMessage = "The recordset returned was null.";
            }

            return ( bSuccess );

        }

        private void CopyRowToObjectMembers( DataRow row )
        {
            _geographicCoverageId = int.Parse( row[ "GeographicCoverageId" ].ToString() );
            _bGroup52 = bool.Parse( row[ "Group52" ].ToString() );
            _bGroup51 = bool.Parse( row[ "Group51" ].ToString() );
            _bGroup50 = bool.Parse( row[ "Group50" ].ToString() );
            _bGroup49 = bool.Parse( row[ "Group49" ].ToString() );

            _bAL = bool.Parse( row[ "AL" ].ToString() );
            _bAK = bool.Parse( row[ "AK" ].ToString() );
            _bAZ = bool.Parse( row[ "AZ" ].ToString() );
            _bAR = bool.Parse( row[ "AR" ].ToString() );
            _bCA = bool.Parse( row[ "CA" ].ToString() );
            _bCO = bool.Parse( row[ "CO" ].ToString() );
            _bCT = bool.Parse( row[ "CT" ].ToString() );
            _bDE = bool.Parse( row[ "DE" ].ToString() );
            _bDC = bool.Parse( row[ "DC" ].ToString() );
            _bFL = bool.Parse( row[ "FL" ].ToString() );
            _bGA = bool.Parse( row[ "GA" ].ToString() );
            _bHI = bool.Parse( row[ "HI" ].ToString() );
            _bID = bool.Parse( row[ "ID" ].ToString() );
            _bIL = bool.Parse( row[ "IL" ].ToString() );
            _bIN = bool.Parse( row[ "IN" ].ToString() );
            _bIA = bool.Parse( row[ "IA" ].ToString() );
            _bKS = bool.Parse( row[ "KS" ].ToString() );
            _bKY = bool.Parse( row[ "KY" ].ToString() );
            _bLA = bool.Parse( row[ "LA" ].ToString() );
            _bME = bool.Parse( row[ "ME" ].ToString() );
            _bMD = bool.Parse( row[ "MD" ].ToString() );
            _bMA = bool.Parse( row[ "MA" ].ToString() );
            _bMI = bool.Parse( row[ "MI" ].ToString() );
            _bMN = bool.Parse( row[ "MN" ].ToString() );
            _bMS = bool.Parse( row[ "MS" ].ToString() );
            _bMO = bool.Parse( row[ "MO" ].ToString() );
            _bMT = bool.Parse( row[ "MT" ].ToString() );
            _bNE = bool.Parse( row[ "NE" ].ToString() );
            _bNV = bool.Parse( row[ "NV" ].ToString() );
            _bNH = bool.Parse( row[ "NH" ].ToString() );
            _bNJ = bool.Parse( row[ "NJ" ].ToString() );
            _bNM = bool.Parse( row[ "NM" ].ToString() );
            _bNY = bool.Parse( row[ "NY" ].ToString() );
            _bNC = bool.Parse( row[ "NC" ].ToString() );
            _bND = bool.Parse( row[ "ND" ].ToString() );
            _bOH = bool.Parse( row[ "OH" ].ToString() );
            _bOK = bool.Parse( row[ "OK" ].ToString() );
            _bOR = bool.Parse( row[ "OR" ].ToString() );
            _bPA = bool.Parse( row[ "PA" ].ToString() );
            _bRI = bool.Parse( row[ "RI" ].ToString() );
            _bSC = bool.Parse( row[ "SC" ].ToString() );
            _bSD = bool.Parse( row[ "SD" ].ToString() );
            _bTN = bool.Parse( row[ "TN" ].ToString() );
            _bTX = bool.Parse( row[ "TX" ].ToString() );
            _bUT = bool.Parse( row[ "UT" ].ToString() );
            _bVT = bool.Parse( row[ "VT" ].ToString() );
            _bVA = bool.Parse( row[ "VA" ].ToString() );
            _bWA = bool.Parse( row[ "WA" ].ToString() );
            _bWV = bool.Parse( row[ "WV" ].ToString() );
            _bWI = bool.Parse( row[ "WI" ].ToString() );
            _bWY = bool.Parse( row[ "WY" ].ToString() );

            _bPR = bool.Parse( row[ "PR" ].ToString() );

            _bAB = bool.Parse( row[ "AB" ].ToString() );
            _bBC = bool.Parse( row[ "BC" ].ToString() );
            _bMB = bool.Parse( row[ "MB" ].ToString() );
            _bNB = bool.Parse( row[ "NB" ].ToString() );
            _bNF = bool.Parse( row[ "NF" ].ToString() );
            _bNT = bool.Parse( row[ "NT" ].ToString() );
            _bNS = bool.Parse( row[ "NS" ].ToString() );
            _bON = bool.Parse( row[ "ON" ].ToString() );
            _bPE = bool.Parse( row[ "PE" ].ToString() );
            _bQC = bool.Parse( row[ "QC" ].ToString() );
            _bSK = bool.Parse( row[ "SK" ].ToString() );
            _bYT = bool.Parse( row[ "YT" ].ToString() );
        }

      // copy from sourceObj to this
        public void CopyFrom( GeographicCoverage sourceObj )
        {
            _geographicCoverageId = sourceObj.GeographicCoverageId;
            _bGroup52 = sourceObj.Group52;
            _bGroup51 = sourceObj.Group51;
            _bGroup50 = sourceObj.Group50;
            _bGroup49 = sourceObj.Group49;

            _bAL = sourceObj.AL;
            _bAK = sourceObj.AK;
            _bAZ = sourceObj.AZ;
            _bAR = sourceObj.AR;
            _bCA = sourceObj.CA;
            _bCO = sourceObj.CO;
            _bCT = sourceObj.CT;
            _bDE = sourceObj.DE;
            _bDC = sourceObj.DC;
            _bFL = sourceObj.FL;
            _bGA = sourceObj.GA;
            _bHI = sourceObj.HI;
            _bID = sourceObj.ID;
            _bIL = sourceObj.IL;
            _bIN = sourceObj.IN;
            _bIA = sourceObj.IA;
            _bKS = sourceObj.KS;
            _bKY = sourceObj.KY;
            _bLA = sourceObj.LA;
            _bME = sourceObj.ME;
            _bMD = sourceObj.MD;
            _bMA = sourceObj.MA;
            _bMI = sourceObj.MI;
            _bMN = sourceObj.MN;
            _bMS = sourceObj.MS;
            _bMO = sourceObj.MO;
            _bMT = sourceObj.MT;
            _bNE = sourceObj.NE;
            _bNV = sourceObj.NV;
            _bNH = sourceObj.NH;
            _bNJ = sourceObj.NJ;
            _bNM = sourceObj.NM;
            _bNY = sourceObj.NY;
            _bNC = sourceObj.NC;
            _bND = sourceObj.ND;
            _bOH = sourceObj.OH;
            _bOK = sourceObj.OK;
            _bOR = sourceObj.OR;
            _bPA = sourceObj.PA;
            _bRI = sourceObj.RI;
            _bSC = sourceObj.SC;
            _bSD = sourceObj.SD;
            _bTN = sourceObj.TN;
            _bTX = sourceObj.TX;
            _bUT = sourceObj.UT;
            _bVT = sourceObj.VT;
            _bVA = sourceObj.VA;
            _bWA = sourceObj.WA;
            _bWV = sourceObj.WV;
            _bWI = sourceObj.WI;
            _bWY = sourceObj.WY;

            _bPR = sourceObj.PR;

            _bAB = sourceObj.AB;
            _bBC = sourceObj.BC;
            _bMB = sourceObj.MB;
            _bNB = sourceObj.NB;
            _bNF = sourceObj.NF;
            _bNT = sourceObj.NT;
            _bNS = sourceObj.NS;
            _bON = sourceObj.ON;
            _bPE = sourceObj.PE;
            _bQC = sourceObj.QC;
            _bSK = sourceObj.SK;
            _bYT = sourceObj.YT;      
        }

        // GUI to clear groups
        public void ClearStateGroup( string groupName )
        {
            if( groupName.CompareTo( "Group52" ) == 0 )
                _bGroup52 = false;
            if( groupName.CompareTo( "Group51" ) == 0 )
                _bGroup51 = false;
            if( groupName.CompareTo( "Group50" ) == 0 )
                _bGroup50 = false;
            if( groupName.CompareTo( "Group49" ) == 0 )
                _bGroup49 = false;
        }

        // GUI call to clear/set all
        public void CheckLower48StatesPlus( bool bChecked, bool bAlsoCheckPR, bool bAlsoCheckDC, bool bAlsoCheckHIAK )
        {
            _bAL = bChecked;
            _bAZ = bChecked;
            _bAR = bChecked;
            _bCA = bChecked;
            _bCO = bChecked;
            _bCT = bChecked;
            _bDE = bChecked;
            _bFL = bChecked;
            _bGA = bChecked;
            _bID = bChecked;
            _bIL = bChecked;
            _bIN = bChecked;
            _bIA = bChecked;
            _bKS = bChecked;
            _bKY = bChecked;
            _bLA = bChecked;
            _bME = bChecked;
            _bMD = bChecked;
            _bMA = bChecked;
            _bMI = bChecked;
            _bMN = bChecked;
            _bMS = bChecked;
            _bMO = bChecked;
            _bMT = bChecked;
            _bNE = bChecked;
            _bNV = bChecked;
            _bNH = bChecked;
            _bNJ = bChecked;
            _bNM = bChecked;
            _bNY = bChecked;
            _bNC = bChecked;
            _bND = bChecked;
            _bOH = bChecked;
            _bOK = bChecked;
            _bOR = bChecked;
            _bPA = bChecked;
            _bRI = bChecked;
            _bSC = bChecked;
            _bSD = bChecked;
            _bTN = bChecked;
            _bTX = bChecked;
            _bUT = bChecked;
            _bVT = bChecked;
            _bVA = bChecked;
            _bWA = bChecked;
            _bWV = bChecked;
            _bWI = bChecked;
            _bWY = bChecked;

            if( bAlsoCheckPR == true )
                _bPR = bChecked;

            if( bAlsoCheckDC == true )
                _bDC = bChecked;

            if (bAlsoCheckHIAK == true)
            {
                _bHI = bChecked;
                _bAK = bChecked;
            }
  
        }

        // look at all states and see if they consist of one of the groups 
        // and then also check/uncheck the appropriate groups
        public void SetGroupFromStates()
        {
            bool bAllLower48 = false;

            // min criteria for checking a group is that all lower 48 are set
            if( _bAL &&
                    _bAZ &&
                    _bAR &&
                    _bCA &&
                    _bCO &&
                    _bCT &&
                    _bDE &&
                    _bFL &&
                    _bGA &&
                    _bID &&
                    _bIL &&
                    _bIN &&
                    _bIA &&
                    _bKS &&
                    _bKY &&
                    _bLA &&
                    _bME &&
                    _bMD &&
                    _bMA &&
                    _bMI &&
                    _bMN &&
                    _bMS &&
                    _bMO &&
                    _bMT &&
                    _bNE &&
                    _bNV &&
                    _bNH &&
                    _bNJ &&
                    _bNM &&
                    _bNY &&
                    _bNC &&
                    _bND &&
                    _bOH &&
                    _bOK &&
                    _bOR &&
                    _bPA &&
                    _bRI &&
                    _bSC &&
                    _bSD &&
                    _bTN &&
                    _bTX &&
                    _bUT &&
                    _bVT &&
                    _bVA &&
                    _bWA &&
                    _bWV &&
                    _bWI &&
                    _bWY )
                bAllLower48 = true;

            //Group52 = 50 States, DC, PR
            //Group51 = 50 States, DC
            //Group50 = 50 States
            //Group49 = 48 Contiguous, DC
            if( bAllLower48 == true )
            {
                if( _bHI && _bAK && _bDC && _bPR )
                {
                    _bGroup52 = true;
                    _bGroup51 = false;
                    _bGroup50 = false;
                    _bGroup49 = false;
                }
                else if( _bHI && _bAK && _bDC )
                {
                    _bGroup52 = false;
                    _bGroup51 = true;
                    _bGroup50 = false;
                    _bGroup49 = false;
                }
                else if( _bHI && _bAK )
                {
                    _bGroup52 = false;
                    _bGroup51 = false;
                    _bGroup50 = true;
                    _bGroup49 = false;
                }
                else if( _bDC )
                {
                    _bGroup52 = false;
                    _bGroup51 = false;
                    _bGroup50 = false;
                    _bGroup49 = true;
                }
                else
                {
                    _bGroup52 = false;
                    _bGroup51 = false;
                    _bGroup50 = false;
                    _bGroup49 = false;
                }
            }
            else
            {
                _bGroup52 = false;
                _bGroup51 = false;
                _bGroup50 = false;
                _bGroup49 = false;
            }
        }

        private int _geographicCoverageId = -1;

        public int GeographicCoverageId
        {
            get { return _geographicCoverageId; }
            set { _geographicCoverageId = value; }
        }

        public bool GetValueByName( string stateName )
        {
            bool bValue = false;
            switch( stateName )
            {
                case "Group52":
                    bValue = _bGroup52;
                    break;
                case "Group51":
                    bValue = _bGroup51;
                    break;
                case "Group50":
                    bValue = _bGroup50;
                    break;
                case "Group49":
                    bValue = _bGroup49;
                    break;
                case "AL":
                    bValue = _bAL;
                    break;
                case "AK":
                    bValue = _bAK;
                    break;
                case "AZ":
                    bValue = _bAZ;
                    break;
                case "AR":
                    bValue = _bAR;
                    break;
                case "CA":
                    bValue = _bCA;
                    break;
                case "CO":
                     bValue = _bCO;
                     break;
                case "CT":
                    bValue = _bCT;
                    break;
                case "DE":
                     bValue = _bDE;
                     break;
                case "DC":
                     bValue = _bDC;
                     break;
                case "FL":
                    bValue = _bFL;
                    break;
                case "GA":
                     bValue = _bGA;
                     break;
                case "HI":
                     bValue = _bHI;
                     break;
                case "ID":
                     bValue = _bID;
                    break;
                case "IL":
                      bValue = _bIL;
                    break;
                case "IN":
                     bValue = _bIN;
                    break;
                case "IA":
                     bValue = _bIA;
                    break;
                case "KS":
                      bValue = _bKS;
                    break;
                case "KY":
                     bValue = _bKY;
                    break;
                case "LA":
                     bValue = _bLA;
                    break;
                case "ME":
                      bValue = _bME;
                    break;
                case "MD":
                    bValue = _bMD;
                    break;
                case "MA":
                     bValue = _bMA;
                    break;
                case "MI":
                     bValue = _bMI;
                    break;
                case "MN":
                      bValue = _bMN;
                    break;
                case "MS":
                      bValue = _bMS;
                    break;
                case "MO":
                      bValue = _bMO;
                    break;
                case "MT":
                      bValue = _bMT;
                    break;
                case "NE":
                      bValue = _bNE;
                    break;
                case "NV":
                      bValue = _bNV;
                    break;
                case "NH":
                      bValue = _bNH;
                    break;
                case "NJ":
                      bValue = _bNJ;
                    break;
                case "NM":
                       bValue = _bNM;
                    break;
                case "NY":
                       bValue = _bNY;
                    break;
                case "NC":
                      bValue = _bNC;
                    break;
                case "ND":
                       bValue = _bND;
                    break;
                case "OH":
                       bValue = _bOH;
                    break;
                case "OK":
                      bValue = _bOK;
                    break;
                case "OR":
                       bValue = _bOR;
                    break;
                case "PA":
                       bValue = _bPA;
                    break;
                case "RI":
                       bValue = _bRI;
                    break;
                case "SC":
                      bValue = _bSC;
                    break;
                case "SD":
                       bValue = _bSD;
                    break;
                case "TN":
                       bValue = _bTN;
                    break;
                case "TX":
                       bValue = _bTX;
                    break;
                case "UT":
                       bValue = _bUT;
                    break;                                
                case "VT":
                       bValue = _bVT;
                    break;
                case "VA":
                       bValue = _bVA;
                    break;
                case "WA":
                       bValue = _bWA;
                    break;
                case "WV":
                       bValue = _bWV;
                    break;
                case "WI":
                       bValue = _bWI;
                    break;
                case "WY":
                       bValue = _bWY;
                    break;
                case "PR":
                       bValue = _bPR;
                    break;


                case "AB":
                       bValue = _bAB;
                    break;
                case "BC":
                       bValue = _bBC;
                    break;
                case "MB":
                       bValue = _bMB;
                    break;
                case "NB":
                       bValue = _bNB;
                    break;
                case "NF":
                       bValue = _bNF;
                    break;
                case "NT":
                       bValue = _bNT;
                    break;
                case "NS":
                       bValue = _bNS;
                    break;
                case "ON":
                       bValue = _bON;
                    break;
                case "PE":
                       bValue = _bPE;
                    break;
                case "QC":
                       bValue = _bQC;
                    break;
                case "SK":
                       bValue = _bSK;
                    break;
                case "YT":
                       bValue = _bYT;
                    break;
                default:
                    bValue = false;
                    break;
            }
            return ( bValue );
        }

        public void SetValueByName( string stateName, bool bValue )
        {
            switch( stateName )
            {
                case "Group52":
                    _bGroup52 = bValue;
                    break;
                case "Group51":
                    _bGroup51 = bValue;
                    break;
                case "Group50":
                    _bGroup50 = bValue;
                    break;
                case "Group49":
                    _bGroup49 = bValue;
                    break;
                case "AL":
                    _bAL = bValue;
                    break;
                case "AK":
                    _bAK = bValue;
                    break;
                case "AZ":
                    _bAZ = bValue;
                    break;
                case "AR":
                    _bAR = bValue;
                    break;
                case "CA":
                    _bCA = bValue;
                    break;
                case "CO":
                    _bCO = bValue;
                    break;
                case "CT":
                    _bCT = bValue;
                    break;
                case "DE":
                    _bDE = bValue;
                    break;
                case "DC":
                    _bDC = bValue;
                    break;
                case "FL":
                    _bFL = bValue;
                    break;
                case "GA":
                    _bGA = bValue;
                    break;
                case "HI":
                    _bHI = bValue;
                    break;
                case "ID":
                    _bID = bValue;
                    break;
                case "IL":
                    _bIL = bValue;
                    break;
                case "IN":
                    _bIN = bValue;
                    break;
                case "IA":
                    _bIA = bValue;
                    break;
                case "KS":
                    _bKS = bValue;
                    break;
                case "KY":
                    _bKY = bValue;
                    break;
                case "LA":
                    _bLA = bValue;
                    break;
                case "ME":
                    _bME = bValue;
                    break;
                case "MD":
                    _bMD = bValue;
                    break;
                case "MA":
                    _bMA = bValue;
                    break;
                case "MI":
                    _bMI = bValue;
                    break;
                case "MN":
                    _bMN = bValue;
                    break;
                case "MS":
                    _bMS = bValue;
                    break;
                case "MO":
                    _bMO = bValue;
                    break;
                case "MT":
                    _bMT = bValue;
                    break;
                case "NE":
                    _bNE = bValue;
                    break;
                case "NV":
                    _bNV = bValue;
                    break;
                case "NH":
                    _bNH = bValue;
                    break;
                case "NJ":
                    _bNJ = bValue;
                    break;
                case "NM":
                    _bNM = bValue;
                    break;
                case "NY":
                    _bNY = bValue;
                    break;
                case "NC":
                    _bNC = bValue;
                    break;
                case "ND":
                    _bND = bValue;
                    break;
                case "OH":
                    _bOH = bValue;
                    break;
                case "OK":
                    _bOK = bValue;
                    break;
                case "OR":
                    _bOR = bValue;
                    break;
                case "PA":
                    _bPA = bValue;
                    break;
                case "RI":
                    _bRI = bValue;
                    break;
                case "SC":
                    _bSC = bValue;
                    break;
                case "SD":
                    _bSD = bValue;
                    break;
                case "TN":
                    _bTN = bValue;
                    break;
                case "TX":
                    _bTX = bValue;
                    break;
                case "UT":
                    _bUT = bValue;
                    break;
                case "VT":
                    _bVT = bValue;
                    break;
                case "VA":
                    _bVA = bValue;
                    break;
                case "WA":
                    _bWA = bValue;
                    break;
                case "WV":
                    _bWV = bValue;
                    break;
                case "WI":
                    _bWI = bValue;
                    break;
                case "WY":
                    _bWY = bValue;
                    break;
                case "PR":
                    _bPR = bValue;
                    break;


                case "AB":
                    _bAB = bValue;
                    break;
                case "BC":
                    _bBC = bValue;
                    break;
                case "MB":
                    _bMB = bValue;
                    break;
                case "NB":
                    _bNB = bValue;
                    break;
                case "NF":
                    _bNF = bValue;
                    break;
                case "NT":
                    _bNT = bValue;
                    break;
                case "NS":
                    _bNS = bValue;
                    break;
                case "ON":
                    _bON = bValue;
                    break;
                case "PE":
                    _bPE = bValue;
                    break;
                case "QC":
                    _bQC = bValue;
                    break;
                case "SK":
                    _bSK = bValue;
                    break;
                case "YT":
                    _bYT = bValue;
                    break;
            }

        }

        private bool _bGroup52 = false;

        public bool Group52
        {
            get { return _bGroup52; }
            set { _bGroup52 = value; }
        }
        private bool _bGroup51 = false;

        public bool Group51
        {
            get { return _bGroup51; }
            set { _bGroup51 = value; }
        }
        private bool _bGroup50 = false;

        public bool Group50
        {
            get { return _bGroup50; }
            set { _bGroup50 = value; }
        }
        private bool _bGroup49 = false;

        public bool Group49
        {
            get { return _bGroup49; }
            set { _bGroup49 = value; }
        }

        private bool _bAL = false;

        public bool AL
        {
            get { return _bAL; }
            set { _bAL = value; }
        }
        private bool _bAK = false;

        public bool AK
        {
            get { return _bAK; }
            set { _bAK = value; }
        }
        private bool _bAZ = false;

        public bool AZ
        {
            get { return _bAZ; }
            set { _bAZ = value; }
        }
        private bool _bAR = false;

        public bool AR
        {
            get { return _bAR; }
            set { _bAR = value; }
        }
        private bool _bCA = false;

        public bool CA
        {
            get { return _bCA; }
            set { _bCA = value; }
        }
        private bool _bCO = false;

        public bool CO
        {
            get { return _bCO; }
            set { _bCO = value; }
        }
        private bool _bCT = false;

        public bool CT
        {
            get { return _bCT; }
            set { _bCT = value; }
        }
        private bool _bDE = false;

        public bool DE
        {
            get { return _bDE; }
            set { _bDE = value; }
        }
        private bool _bDC = false;

        public bool DC
        {
            get { return _bDC; }
            set { _bDC = value; }
        }
        private bool _bFL = false;

        public bool FL
        {
            get { return _bFL; }
            set { _bFL = value; }
        }
        private bool _bGA = false;

        public bool GA
        {
            get { return _bGA; }
            set { _bGA = value; }
        }
        private bool _bHI = false;

        public bool HI
        {
            get { return _bHI; }
            set { _bHI = value; }
        }
        private bool _bID = false;

        public bool ID
        {
            get { return _bID; }
            set { _bID = value; }
        }
        private bool _bIL = false;

        public bool IL
        {
            get { return _bIL; }
            set { _bIL = value; }
        }
        private bool _bIN = false;

        public bool IN
        {
            get { return _bIN; }
            set { _bIN = value; }
        }
        private bool _bIA = false;

        public bool IA
        {
            get { return _bIA; }
            set { _bIA = value; }
        }
        private bool _bKS = false;

        public bool KS
        {
            get { return _bKS; }
            set { _bKS = value; }
        }
        private bool _bKY = false;

        public bool KY
        {
            get { return _bKY; }
            set { _bKY = value; }
        }
        private bool _bLA = false;

        public bool LA
        {
            get { return _bLA; }
            set { _bLA = value; }
        }
        private bool _bME = false;

        public bool ME
        {
            get { return _bME; }
            set { _bME = value; }
        }
        private bool _bMD = false;

        public bool MD
        {
            get { return _bMD; }
            set { _bMD = value; }
        }
        private bool _bMA = false;

        public bool MA
        {
            get { return _bMA; }
            set { _bMA = value; }
        }
        private bool _bMI = false;

        public bool MI
        {
            get { return _bMI; }
            set { _bMI = value; }
        }
        private bool _bMN = false;

        public bool MN
        {
            get { return _bMN; }
            set { _bMN = value; }
        }
        private bool _bMS = false;

        public bool MS
        {
            get { return _bMS; }
            set { _bMS = value; }
        }
        private bool _bMO = false;

        public bool MO
        {
            get { return _bMO; }
            set { _bMO = value; }
        }
        private bool _bMT = false;

        public bool MT
        {
            get { return _bMT; }
            set { _bMT = value; }
        }
        private bool _bNE = false;

        public bool NE
        {
            get { return _bNE; }
            set { _bNE = value; }
        }
        private bool _bNV = false;

        public bool NV
        {
            get { return _bNV; }
            set { _bNV = value; }
        }
        private bool _bNH = false;

        public bool NH
        {
            get { return _bNH; }
            set { _bNH = value; }
        }
        private bool _bNJ = false;

        public bool NJ
        {
            get { return _bNJ; }
            set { _bNJ = value; }
        }
        private bool _bNM = false;

        public bool NM
        {
            get { return _bNM; }
            set { _bNM = value; }
        }
        private bool _bNY = false;

        public bool NY
        {
            get { return _bNY; }
            set { _bNY = value; }
        }
        private bool _bNC = false;

        public bool NC
        {
            get { return _bNC; }
            set { _bNC = value; }
        }
        private bool _bND = false;

        public bool ND
        {
            get { return _bND; }
            set { _bND = value; }
        }
        private bool _bOH = false;

        public bool OH
        {
            get { return _bOH; }
            set { _bOH = value; }
        }
        private bool _bOK = false;

        public bool OK
        {
            get { return _bOK; }
            set { _bOK = value; }
        }
        private bool _bOR = false;

        public bool OR
        {
            get { return _bOR; }
            set { _bOR = value; }
        }
        private bool _bPA = false;

        public bool PA
        {
            get { return _bPA; }
            set { _bPA = value; }
        }
        private bool _bRI = false;

        public bool RI
        {
            get { return _bRI; }
            set { _bRI = value; }
        }
        private bool _bSC = false;

        public bool SC
        {
            get { return _bSC; }
            set { _bSC = value; }
        }
        private bool _bSD = false;

        public bool SD
        {
            get { return _bSD; }
            set { _bSD = value; }
        }
        private bool _bTN = false;

        public bool TN
        {
            get { return _bTN; }
            set { _bTN = value; }
        }
        private bool _bTX = false;

        public bool TX
        {
            get { return _bTX; }
            set { _bTX = value; }
        }
        private bool _bUT = false;

        public bool UT
        {
            get { return _bUT; }
            set { _bUT = value; }
        }
        private bool _bVT = false;

        public bool VT
        {
            get { return _bVT; }
            set { _bVT = value; }
        }
        private bool _bVA = false;

        public bool VA
        {
            get { return _bVA; }
            set { _bVA = value; }
        }
        private bool _bWA = false;

        public bool WA
        {
            get { return _bWA; }
            set { _bWA = value; }
        }
        private bool _bWV = false;

        public bool WV
        {
            get { return _bWV; }
            set { _bWV = value; }
        }
        private bool _bWI = false;

        public bool WI
        {
            get { return _bWI; }
            set { _bWI = value; }
        }
        private bool _bWY = false;

        public bool WY
        {
            get { return _bWY; }
            set { _bWY = value; }
        }

        private bool _bPR = false;

        public bool PR
        {
            get { return _bPR; }
            set { _bPR = value; }
        }

        private bool _bAB = false;

        public bool AB
        {
            get { return _bAB; }
            set { _bAB = value; }
        }
        private bool _bBC = false;

        public bool BC
        {
            get { return _bBC; }
            set { _bBC = value; }
        }
        private bool _bMB = false;

        public bool MB
        {
            get { return _bMB; }
            set { _bMB = value; }
        }
        private bool _bNB = false;

        public bool NB
        {
            get { return _bNB; }
            set { _bNB = value; }
        }
        private bool _bNF = false;

        public bool NF
        {
            get { return _bNF; }
            set { _bNF = value; }
        }
        private bool _bNT = false;

        public bool NT
        {
            get { return _bNT; }
            set { _bNT = value; }
        }
        private bool _bNS = false;

        public bool NS
        {
            get { return _bNS; }
            set { _bNS = value; }
        }
        private bool _bON = false;

        public bool ON
        {
            get { return _bON; }
            set { _bON = value; }
        }
        private bool _bPE = false;

        public bool PE
        {
            get { return _bPE; }
            set { _bPE = value; }
        }
        private bool _bQC = false;

        public bool QC
        {
            get { return _bQC; }
            set { _bQC = value; }
        }
        private bool _bSK = false;

        public bool SK
        {
            get { return _bSK; }
            set { _bSK = value; }
        }
        private bool _bYT = false;
 
        public bool YT
        {
            get { return _bYT; }
            set { _bYT = value; }
        }

    }
}
