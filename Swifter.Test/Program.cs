using Swifter.Json;
using Swifter.RW;
using System;
using System.Collections.Generic;

namespace Swifter.Test
{
    [FieldsFilter]
    public class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ReadOnlySpan<char> FirstName => Name.AsSpan().Slice(0, 3);

        public static void Main()
        {
            var json = JsonFormatter.SerializeObject(new Demo() { Id = 1, Name = "Dogwei" });

            Console.WriteLine(json); // {"Id":1,"Name":"Dogwei"}
        }
    }

    public class FieldsFilterAttribute : RWObjectAttribute
    {
        public override void OnCreate(Type type, ref List<IObjectField> fields)
        {
            fields.RemoveAll(item =>
            {
                // 移除所有非基础类型的属性或字段。
                // Removes all non-base type properties or fields.
                switch (Type.GetTypeCode(item.AfterType))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Char:
                    case TypeCode.DateTime:
                    case TypeCode.DBNull:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.Single:
                    case TypeCode.String:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return false;
                }

                return true;
            });
        }
    }
}