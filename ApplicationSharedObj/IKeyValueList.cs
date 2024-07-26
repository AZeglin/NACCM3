using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;

namespace VA.NAC.Application.SharedObj
{
    public interface IKeyValueList
    {
        void Add(int key, object val);
        void Add(string key, object val); 
        void Clear();
        bool ContainsKey(string key);
        bool ContainsKey(int key);
        int Count();
        void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);
        object GetValue(string key);
        object GetValue(int key);
        System.Collections.ArrayList Keys { get; set; }
        void Remove(int key);
        void Remove(string key);
        string Serialize();
        bool SetValue(int key, object val);
        bool SetValue(string key, object val);
        object this[int index] { get; set; }
        string TypeName { get; set; }
        System.Collections.ArrayList Values { get; set; }
    }
}
