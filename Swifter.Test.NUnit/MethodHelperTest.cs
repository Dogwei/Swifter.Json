using NUnit.Framework;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;
using static Swifter.Tools.MethodHelper;

namespace Swifter.Test
{
    public class MethodHelperTest
    {
        public delegate Span<T>.Enumerator GetSpanEnumeratorFunc<T>(Delegate @delegate, ref Span<T> span);

        [Test]
        public void SpanTest()
        {
            var methodInfo = typeof(Span<int>).GetMethod(nameof(Span<int>.GetEnumerator));

            var @delegate = CreateDelegate(methodInfo);

            var @delagate2 = CreateDelegate<GetSpanEnumeratorFunc<int>>(@delegate.GetType().GetMethod(nameof(Action.Invoke)), false);

            Span<int> span = stackalloc int[] { 12, 18 };

            var enumerator = delagate2(@delegate, ref span);

            Assert.AreEqual(true, enumerator.MoveNext());
            Assert.AreEqual(12, enumerator.Current);
            Assert.AreEqual(true, enumerator.MoveNext());
            Assert.AreEqual(18, enumerator.Current);
            Assert.AreEqual(false, enumerator.MoveNext());
        }

        public delegate int TypedReferenceGetHashCodeFunc(Delegate @delegate, IntPtr pTypedReference);

        [Test]
        public unsafe void TypedReferenceTest()
        {
            var methodInfo = typeof(TypedReference).GetMethod(nameof(TypedReference.GetHashCode));

            var @delegate = CreateDelegate(methodInfo, false);

            var @delagate2 = CreateDelegate<TypedReferenceGetHashCodeFunc>(@delegate.GetType().GetMethod(nameof(Action.Invoke)), false);

            var value = 999;

            var typedReference = __makeref(value);

            Assert.AreEqual(typedReference.GetHashCode(), @delagate2(@delegate, (IntPtr)(&typedReference)));
        }

        public delegate int ValueTypeGetHashCodeFunc<T>(ref T obj) where T : struct;

        [Test]
        public unsafe void Int32Test()
        {
            var methodInfo = typeof(int).GetMethod(nameof(int.GetHashCode));

            var @delegate = CreateDelegate<Func<IntPtr, int>>(methodInfo, false);

            var @delegate2 = CreateDelegate<ValueTypeGetHashCodeFunc<int>>(methodInfo);


            var val = 9999;

            Assert.AreEqual(val.GetHashCode(), @delegate((IntPtr)(&val)));

            Assert.AreEqual(val.GetHashCode(), @delegate2(ref val));
        }

        [Test]
        public void OverrideTest()
        {
            var @delegate = DynamicAssembly.DefineDynamicMethod<Func<int, int, int>>((dynamicMethod, ilGenerator) =>
            {
                ilGenerator.LoadArgument(0);
                ilGenerator.LoadArgument(1);
                ilGenerator.Multiply();
                ilGenerator.Return();
            });

            Assert.AreEqual(12 * 18, @delegate(12, 18));

            Override(MethodOf<int, int, int>(OverrideTestTest), @delegate.GetFunctionPointer());

            Assert.AreEqual(@delegate(12, 18), OverrideTestTest(12, 18));
        }

        [Test]
        public void GetParameterTypesTest()
        {
            GetParametersTypes(MethodOf<int, int, int>(OverrideTestTest), out var parameterTypes, out var returnType);

            Assert.AreEqual(new Type[] { typeof(int), typeof(int) }, parameterTypes);
            Assert.AreEqual(typeof(int), returnType);


            GetParametersTypes(MethodOf(GetParameterTypesTest), out parameterTypes, out returnType);

            Assert.AreEqual(new Type[] { typeof(MethodHelperTest) }, parameterTypes);
            Assert.AreEqual(typeof(void), returnType);


            GetParametersTypes(typeof(string).GetConstructor(new Type[] {typeof(char[]) }), out parameterTypes, out returnType);

            Assert.AreEqual(new Type[] { typeof(string), typeof(char[]) }, parameterTypes);
            Assert.AreEqual(typeof(void), returnType);

            GetParametersTypes(typeof(Func<int, int>), out parameterTypes, out returnType);

            Assert.AreEqual(new Type[] { typeof(int) }, parameterTypes);
            Assert.AreEqual(typeof(int), returnType);

            GetParametersTypes(typeof(Action<int, int>), out parameterTypes, out returnType);

            Assert.AreEqual(new Type[] { typeof(int), typeof(int) }, parameterTypes);
            Assert.AreEqual(typeof(void), returnType);

            GetParametersTypes(typeof(Action), out parameterTypes, out returnType);

            Assert.AreEqual(new Type[] {  }, parameterTypes);
            Assert.AreEqual(typeof(void), returnType);

            //GetParametersTypes(typeof(RefReturnFunc<int, int>), out parameterTypes, out returnType);

            //Assert.AreEqual(new Type[] { typeof(int) }, parameterTypes);
            //Assert.AreEqual(typeof(int).MakeByRefType(), returnType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int OverrideTestTest(int left, int right)
        {
            return left + right;
        }
    }
}