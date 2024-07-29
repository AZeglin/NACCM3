using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.ComponentModel.Design;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    public class EdgeMenuItemCollectionEditor : CollectionEditor
    {
        public EdgeMenuItemCollectionEditor( Type type )
            : base( type )
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof( EdgeMenuItem );
        }
    }
}
