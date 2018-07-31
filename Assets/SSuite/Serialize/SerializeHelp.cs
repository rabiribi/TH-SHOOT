#if UNITY_5 ||  UNITY_4 ||  UNITY_3
using UnityEngine;
#endif

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using S.Serialize;
using System.IO;
using UnityEngine;

namespace S.Serialize
{

    /*------------------------------------------------------------------------------------------------
    [ENG]
    You can freely convert a list of bytes to any object, just like JSON/xml
    As is based on bytes to store data, take up less memory than the JSON or XML, not only can be used to store the object in the binary file, but also applies to network transmission.
    Of course, you want to set the member data to the public and convert only the value type.
       ------------------------------------------------------------------------------------------------
    Currently supported types of data are: int long byte enum string object float array<T> list<T> dictionary<T1 T2>
   ------------------------------------------------------------------------------------------------
    How to Using?
     for example:
         class Info
         {
               public float value;
               public string name;
         }
         Info info; 

        // If you want to store the data in a file:
            byte[] data = SerializeHelp.WriteObjectData();
            SerializeHelp.WriteFile("C:/you path/xx.file",data);

        //When you want him to read from the file:
            byte[] data =SerializeHelp.ReadFile("C:/you path/xx.file");
            Info info = SerializeHelp.ReadObjectData<Info>(data);

        //You can add the Test script to the scene camera, which has a detailed example code

        Whether it is a class or struct (like Vector2 in the Unity), or is a basic value type (int, string, List, etc.), or multiple levels of nested objects, you can meet the needs of
    ------------------------------------------------------------------------------------------------*/
    /*Version 1.0.0*/

    public static class SerializeHelp
    {

        public enum FindType
        {
            Nomal = 0,
            FirstResourceQuote,
        }

        public static void WriteInt(ref List<byte> data, object obj)
        {

            data.AddRange(BitConverter.GetBytes((int)(obj)));
        }

        public static void WriteLong(ref List<byte> data, object obj)
        {
            data.AddRange(BitConverter.GetBytes((long)(obj)));
        }

        public static void WriteFloat(ref List<byte> data, object obj)
        {
            data.AddRange(BitConverter.GetBytes((float)obj));
        }

        public static void WriteByte(ref List<byte> data, object obj)
        {
            data.Add((byte)obj);
        }

        public static void WriteByteIList(ref List<byte> data, object obj)
        {
            data.AddRange((IEnumerable<byte>)obj);
        }

        public static void WriteString(ref List<byte> data, object obj)
        {
            string str = (string)obj;
            if (str == null) str = "";
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            WriteInt(ref data, byteArray.Length);
            data.AddRange(byteArray);
        }

        public static void WriteBool(ref List<byte> data, object obj)
        {
            data.AddRange(BitConverter.GetBytes((bool)(obj)));
        }

        public static void WriteType(ref List<byte> data, object obj)
        {
            string assemblyName = "";
            string typeName = "";

            if (obj != null)
            {
                Type type = obj as Type;
                if (type != null)
                {
                    assemblyName = type.Namespace;
                    typeName = type.FullName;
                }
            }

            WriteString(ref data, assemblyName);
            WriteString(ref data, typeName);
        }

        //---------------------------------------------------------------------------------------------------
        public static void WriteForChose(ref List<byte> data, FieldObjInfo foi, FindType findType = FindType.Nomal)
        {
            Type type = foi.type;
            object obj = foi.obj;
            object baseObj = foi.baseObj;

            //基本类型构造出来
            if (findType == FindType.Nomal)
            {
                if (obj == null)
                {
                    obj = CreateInstanceObject(type);
                }
            }

            if (type == typeof(int))
            {
                WriteInt(ref data, obj);
            }
            else if (type == typeof(long))
            {
                WriteLong(ref data, obj);
            }
            else if (type == typeof(System.Byte))
            {
                WriteByte(ref data, obj);
            }
            else if (type == typeof(float))
            {
                WriteFloat(ref data, obj);
            }
            else if (type == typeof(System.String))
            {
                WriteString(ref data, obj);
            }
            else if (type == typeof(System.Boolean))
            {
                WriteBool(ref data, obj);
            }
            else if (type.BaseType != null && type.BaseType == typeof(System.Enum))
            {
                WriteType(ref data, type);
                WriteInt(ref data, (int)obj);
            }
            else if (type == typeof(System.Type))
            {
                WriteType(ref data, obj);
            }
            else if (type == typeof(System.Object))
            {
                if (obj != null && obj.GetType() != typeof(System.Object))
                {
                    foi.type = obj.GetType();
                    WriteType(ref data, foi.type);
                    WriteForChose(ref data, foi);
                }
                else WriteType(ref data, null);
            }
            else if ((type.IsGenericType && type.Name == "Dictionary`2"))
            {
                Type _keyType = type.GetGenericArguments()[0];
                Type _valueType = type.GetGenericArguments()[1];

                //object genericList = CreateDictionaryGeneric(typeof(Dictionary<,>), _keyType, _valueType);
                IDictionary dic = (IDictionary)(obj);

                Array keyArray = Array.CreateInstance(_keyType, dic.Count);
                dic.Keys.CopyTo(keyArray, 0);

                Array valueArray = Array.CreateInstance(_valueType, dic.Count);
                dic.Values.CopyTo(valueArray, 0);

                WriteInt(ref data, dic.Count);

                for (int i = 0; i < dic.Count; i++)
                {
                    foi.obj = keyArray.GetValue(i);
                    foi.type = _keyType;
                    WriteForChose(ref data, foi);

                    foi.obj = valueArray.GetValue(i);
                    foi.type = _valueType;
                    WriteForChose(ref data, foi);
                }
            }
            else if ((type.IsGenericType && type.Name == "List`1") || type.IsArray)
            {
                Type _objType = (type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType());

                IList list = (IList)(obj);
                WriteInt(ref data, list.Count);

                if (_objType == typeof(System.Byte))
                {
                    WriteByteIList(ref data, obj);
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        foi.obj = list[i];
                        foi.type = _objType;
                        WriteForChose(ref data, foi);
                    }
                }
            }
            else
            {
                //       if (obj == null) Debug.Log(type);
                FieldInfo[] fields = obj.GetType().GetFields();
                if (fields != null)
                {
                    foreach (FieldInfo _field in fields)
                    {

                        if (!_field.IsStatic && !HasRootType(_field.FieldType, typeof(Delegate)))
                        {
                            object _obj = _field.GetValue(obj);

                            Type fieldType = _field.FieldType;
                            foi.obj = _obj;
                            foi.type = fieldType;
                            WriteForChose(ref data, foi);
                        }
                    }
                }


            }
        }

        //---------------------------------------------------------------------------------------------------
        public static int ReadInt(ref List<byte> data)
        {
            byte[] byteArray = new byte[sizeof(int)];
            byteArray = data.GetRange(0, byteArray.Length).ToArray();
            data.RemoveRange(0, byteArray.Length);
            return BitConverter.ToInt32(byteArray, 0);
        }

        public static long ReadLong(ref List<byte> data)
        {
            byte[] byteArray = new byte[sizeof(long)];
            byteArray = data.GetRange(0, byteArray.Length).ToArray();
            data.RemoveRange(0, byteArray.Length);

            return BitConverter.ToInt64(byteArray, 0);
        }

        public static byte ReadByte(ref List<byte> data)
        {
            byte byteOne = data[0];
            data.RemoveAt(0);
            return byteOne;
        }

        public static byte[] ReadByteArray(ref List<byte> data, int length)
        {
            byte[] bytes = data.GetRange(0, length).ToArray();
            data.RemoveRange(0, length);
            return bytes;
        }

        public static List<byte> ReadByteList(ref List<byte> data, int length)
        {
            List<byte> byteList = data.GetRange(0, length);
            data.RemoveRange(0, length);
            return byteList;
        }


        public static float ReadFloat(ref List<byte> data)
        {
            byte[] byteArray = new byte[sizeof(float)];
            byteArray = data.GetRange(0, byteArray.Length).ToArray();
            data.RemoveRange(0, byteArray.Length);
            return BitConverter.ToSingle(byteArray, 0);
        }

        public static string ReadString(ref List<byte> data)
        {
            byte[] byteArray = new byte[sizeof(int)];

            byteArray = data.GetRange(0, byteArray.Length).ToArray();
            int length = BitConverter.ToInt32(byteArray, 0);
            data.RemoveRange(0, sizeof(int));

            byteArray = data.GetRange(0, length).ToArray();
            string str = Encoding.UTF8.GetString(byteArray);
            data.RemoveRange(0, byteArray.Length);

            return str;
        }

        public static bool ReadBool(ref List<byte> data)
        {
            byte[] byteArray = new byte[sizeof(bool)];
            byteArray = data.GetRange(0, byteArray.Length).ToArray();
            data.RemoveRange(0, byteArray.Length);
            return BitConverter.ToBoolean(byteArray, 0);
        }

        public static Type ReadType(ref List<byte> data)
        {
            string assemblyName = ReadString(ref data);
            string typeName = ReadString(ref data);
            return SerializeHelp.GetAssemblyType(assemblyName, typeName);
        }

        public static object CreateGeneric(Type generic, Type innerType, params object[] args)
        {
            Type specificType = generic.MakeGenericType(new System.Type[] { innerType });
            return Activator.CreateInstance(specificType, args);
        }

        public static object CreateDictionaryGeneric(Type generic, Type keyType, Type valueType, params object[] args)
        {
            Type specificType = generic.MakeGenericType(new System.Type[] { keyType, valueType });
            return Activator.CreateInstance(specificType, args);
        }

        public static Type CreateGenericType(Type generic, Type innerType)
        {
            return generic.MakeGenericType(new System.Type[] { innerType });
        }

        public struct FieldObjInfo
        {
            public object obj;
            public Type type;
            public object baseObj;
            public FieldInfo fieldInfo;
            public object fieldObj;
            public PropertyInfo propertyInfo;
            public object propertyObj;

            public FieldObjInfo(object baseObj, Type type, object obj)
            {
                this.baseObj = baseObj;
                this.type = type;
                this.obj = obj;
                this.fieldInfo = null;
                this.fieldObj = null;
                this.propertyInfo = null;
                this.propertyObj = null;
            }
        }

        //---------------------------------------------------------------------------------------------------
        public static object ReadForChose(ref List<byte> data, FieldObjInfo foi, FindType findType = FindType.Nomal)
        {
            Type type = foi.type;
            object obj = foi.obj;
            object baseObj = foi.baseObj;

            if (findType == FindType.Nomal)
            {
                if (obj == null)
                {
                    obj = CreateInstanceObject(type);
                }
            }

            if (type == typeof(int))
            {
                object value = ReadInt(ref data);
                return value;
            }
            else if (type == typeof(long))
            {
                object value = ReadLong(ref data);
                return value;
            }
            else if (type == typeof(System.Byte))
            {
                object value = ReadByte(ref data);
                return value;
            }
            else if (type == typeof(float))
            {
                object value = ReadFloat(ref data);
                return value;
            }
            else if (type == typeof(System.String))
            {
                object value = ReadString(ref data);
                return value;
            }
            else if (type == typeof(System.Boolean))
            {
                object value = ReadBool(ref data);
                return value;
            }
            else if (type.BaseType != null && type.BaseType == typeof(System.Enum))
            {
                Type enumType = ReadType(ref data);
                int value = ReadInt(ref data);
                return Enum.ToObject(enumType, value);
            }
            else if (type == typeof(System.Type))
            {
                return ReadType(ref data);
            }
            else if (type == typeof(System.Object))
            {
                Type objType = ReadType(ref data);

                if (objType != null)
                {
                    foi.type = objType;
                    return ReadForChose(ref data, foi);
                }
                else return null;
            }
            else if ((type.IsGenericType && type.Name == "Dictionary`2"))
            {
                Type _keyType = type.GetGenericArguments()[0];
                Type _valueType = type.GetGenericArguments()[1];

             //genericList = CreateDictionaryGeneric(typeof(Dictionary<,>), _keyType, _valueType);
                IDictionary dic = (IDictionary)(obj);

                int count = ReadInt(ref data);

                for (int i = 0; i < count; i++)
                {
                    foi.obj = null;
                    foi.type = _keyType;
                    object keyObj = ReadForChose(ref data, foi);

                    foi.obj = null;
                    foi.type = _valueType;
                    object valueObj = ReadForChose(ref data, foi);

                    dic.Add(keyObj, valueObj);
                }

                return dic;
            }
            else if ((type.IsGenericType && type.Name == "List`1") || type.IsArray)
            {
                int length = ReadInt(ref data);

                if (type.IsGenericType)
                {
                    Type _type = type.GetGenericArguments()[0];
               //     object genericList = CreateGeneric(typeof(List<>), _type);

                    if (_type == typeof(System.Byte))
                    {
                        return ReadByteList(ref data, length);
                    }
                    else
                    {
                        IList list = (IList)(obj);
                        for (int i = 0; i < length; i++)
                        {
                            foi.obj = null;
                            foi.type = _type;
                            list.Add(ReadForChose(ref data, foi));
                        }
                        return list;
                    }

                }
                else
                {
                    Type _type = type.GetElementType();
                 //   obj = Array.CreateInstance(_type, length);

                    if (_type == typeof(System.Byte))
                    {
                        return ReadByteArray(ref data, length);
                    }
                    else
                    {
                        IList list = (IList)(obj);
                        for (int i = 0; i < length; i++)
                        {
                            foi.obj = list[i];

                            foi.type = _type;
                            list[i] = ReadForChose(ref data, foi);
                        }

                        return list;
                    }
                }
            }
            else
            {
                 if (obj == null) Debug.LogError(type + " is Null");
                FieldInfo[] fields = obj.GetType().GetFields();
                if (fields != null)
                {
                    foreach (FieldInfo _field in fields)
                    {
                        if (!_field.IsStatic && !HasRootType(_field.FieldType, typeof(Delegate)))
                        {
                            object _obj = _field.GetValue(obj);
                            Type fieldType = _field.FieldType;

                            foi.obj = _obj;
                            foi.type = _field.FieldType;

                            foi.fieldInfo = _field;
                            foi.fieldObj = obj;

                            object value = ReadForChose(ref data, foi);
                            _field.SetValue(obj, value);
                        }
                    }
                }



                return obj;

            }
        }


        public static object CreateInstanceObject(Type type)
        {
            if (type == typeof(System.String))
            {
                return "";
            }
            else if (type == typeof(System.Type))
            {
                return null;
            }
            else
            {
                if (type.IsGenericType && type.Name == "List`1")
                {
                    Type _type = type.GetGenericArguments()[0];
                    return CreateGeneric(typeof(List<>), _type);
                }
                else if ((type.IsGenericType && type.Name == "Dictionary`2"))
                {
                    Type _keyType = type.GetGenericArguments()[0];
                    Type _valueType = type.GetGenericArguments()[1];
                    return CreateDictionaryGeneric(typeof(Dictionary<,>), _keyType, _valueType);
                }
                else if (type.IsArray)
                {
                    return Array.CreateInstance(type.GetElementType(), 0);
                }
#if UNITY_5 || UNITY_4 || UNITY_3
                else if (HasRootType(type, typeof(UnityEngine.Object)))
                {
                    return null;
                }
#endif
            }

            ConstructorInfo[] conInfoArray = type.GetConstructors();
            if (conInfoArray.Length==0  && type.IsClass) return null;
            else return Activator.CreateInstance(type);
        }

        public static Type GetRootType(Type type, Type endType)
        {
            if (type == null)
            {
                return null;
            }
            if (type.BaseType == null)
            {
                return type;
            }
            else if (type.BaseType == endType)
            {
                return endType;
            }
            else return GetRootType(type.BaseType, endType);
        }

        public static bool HasRootType(Type type, Type endType)
        {
            return GetRootType(type, endType) == endType;
        }

        public static byte[] WriteObjectData(object obj)
        {
            List<byte> data = new List<byte>();
            WriteForChose(ref data, new FieldObjInfo(obj, obj.GetType(), obj));
            return data.ToArray();
        }

        public static T ReadObjectData<T>(byte[] data)
        {
            List<byte> byteList = new List<byte>(data);
            Type type = typeof(T);
            T obj = (T)CreateInstanceObject(type);
            obj = (T)ReadForChose(ref byteList, new FieldObjInfo(obj, type, obj));
            return obj;
        }

        public class AssemblyTypeInfo
        {
            public Dictionary<string, Type> typeDic = new Dictionary<string, Type>();

            public AssemblyTypeInfo(Assembly assembly)
            {
                AddAssemblyTypes(assembly);
            }

            public void AddAssemblyTypes(Assembly assembly)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        typeDic[types[i].FullName] = types[i];
                    }
                }
                catch
                {

                }
            }

            public Type GetType(string name)
            {
                Type type = null;
                typeDic.TryGetValue(name, out type);
                return type;
            }
        }

        #region Assembly

        public static Dictionary<string, AssemblyTypeInfo> assemblyDic = new Dictionary<string, AssemblyTypeInfo>();

        public static void LoadAppAssembly()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                AddAssembly(assemblies[i]);
            }
        }

        public static void AddAssembly(Assembly assembly)
        {
            string moduleName = assembly.ManifestModule.Name;
            int ptr = moduleName.LastIndexOf(".");
            string nameSpace = moduleName.Remove(ptr, moduleName.Length - ptr);
            CheckAssemblyDic(nameSpace, assembly);
        }

        private static void CheckAssemblyDic(string name, Assembly assembly)
        {
            AssemblyTypeInfo assemblyTypeInfo = null;
            if (SerializeHelp.assemblyDic.TryGetValue(name, out assemblyTypeInfo))
            {
                assemblyTypeInfo.AddAssemblyTypes(assembly);
            }
            else
            {
                assemblyTypeInfo = new AssemblyTypeInfo(assembly);
                SerializeHelp.assemblyDic.Add(name, assemblyTypeInfo);
            }
        }

        public static Type GetAssemblyType(string assemblyName, string typeName)
        {
            if (assemblyName == "" || assemblyName == "System" || assemblyName == "S.Serialize" || assemblyName == "System.Collections.Generic")
            {
                return Type.GetType(typeName);
            }

            AssemblyTypeInfo assembly = null;
            if (assemblyDic.TryGetValue(assemblyName, out assembly))
            {
                if (assembly == null)
                {
                    return null;
                }
                return assembly.GetType(typeName);
            }
            else
            {
                Debug.Log("Not pre loaded assembly:" + assemblyName + "\n" + "You can use the SerializeHelp.AddAssembly() to load the assembly .");
            }
            return null;
        }

#endregion

        #region File operation

        public static string GetFileDirectoryPath(string path)
        {
            int ptr = path.LastIndexOf("/");
            return path.Substring(0, ptr + 1);
        }


        public static byte[] ReadFile(string path, FileMode mode = FileMode.Open)
        {
            if (!File.Exists(path)) return new byte[0];
            FileStream stream = new FileStream(path, mode);
            byte[] byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, byteArray.Length);
            stream.Close();
            return byteArray;
        }

        public static void WriteFile(string path, byte[] byteArray, FileMode mode = FileMode.Create)
        {
            string dir = GetFileDirectoryPath(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            FileStream stream = new FileStream(path, mode);
            stream.Write(byteArray, 0, byteArray.Length);
            stream.Close();
        }

        #endregion

    }



}