using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // parms set when opening the edit sba plan details window
    [Serializable]
    public class EditSBAPlanDetailsWindowParms
    {

        public enum EditSBAPlanActions
        {
            CreateNewPlan,
            SelectDifferentPlan,
            EditCurrentPlan,
            Undefined
        }   

        private EditSBAPlanActions _editSBAPlanAction = EditSBAPlanActions.Undefined;

        public EditSBAPlanActions EditSBAPlanAction
        {
            get { return _editSBAPlanAction; }
            set { _editSBAPlanAction = value; }
        }  

         // editing existing plan
        public EditSBAPlanDetailsWindowParms( int SBAPlanId, string contractNumber, bool bEditCurrentPlan, bool bIsContractExpired )
        {
            if( bEditCurrentPlan == true )
            {
                _editSBAPlanAction = EditSBAPlanActions.EditCurrentPlan;
            }
            else
            {
                _editSBAPlanAction = EditSBAPlanActions.SelectDifferentPlan;
            }

            _sbaPlanId = SBAPlanId;
            _contractNumber = contractNumber;
            _bIsContractExpired = bIsContractExpired;
        }

        // adding new plan
        public EditSBAPlanDetailsWindowParms( string contractNumber )
        {
            _editSBAPlanAction = EditSBAPlanActions.CreateNewPlan;
            _contractNumber = contractNumber;
        }

        private string _contractNumber = "";

        public string ContractNumber
        {
            get { return _contractNumber; }
            set { _contractNumber = value; }
        }

        private int _sbaPlanId = -1;

        public int SBAPlanId
        {
          get { return _sbaPlanId; }
          set { _sbaPlanId = value; }
        }
      
        private bool _bIsContractExpired = false;

        public bool IsContractExpired
        {
            get
            {
                return _bIsContractExpired;
            }

            set
            {
                _bIsContractExpired = value;
            }
        }

    }
}
