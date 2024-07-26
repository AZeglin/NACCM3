using System;
using System.Collections.Generic;
using System.Text;

namespace VA.NAC.Application.SharedObj
{
    // objects contained in a KeyValue list may benefit from supporting this
    // interface in list extraction and copy operations
    // although not required to be in a KeyValueList
    public interface IListObject
    {
        object GetKey();
        Type GetKeyType();
        string GetKeyName();
        string ToString(); // a formatted displayable value for the object
    }
}
