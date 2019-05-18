namespace Swifter.Reflection
{
    delegate ref TValue XClassRefValueHandler<TValue>(object obj);
    delegate TValue XClassGetValueHandler<TValue>(object obj);
    delegate void XClassSetValueHandler<TValue>(object obj, TValue value);

    delegate ref TValue XStructRefValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate TValue XStructGetValueHandler<TStruct, TValue>(ref TStruct obj);
    delegate void XStructSetValueHandler<TStruct, TValue>(ref TStruct obj, TValue value);

    delegate ref TValue XStaticRefValueHandler<TValue>();
    delegate TValue XStaticGetValueHandler<TValue>();
    delegate void XStaticSetValueHandler<TValue>(TValue value);
}
