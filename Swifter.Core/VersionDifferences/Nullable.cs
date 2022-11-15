using System;

namespace System.Diagnostics.CodeAnalysis
{
#if NET5_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

#else

    /// <summary>
    ///     Specifies that <see langword="null"/> is allowed as an input even if the
    ///     corresponding type disallows it.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property,
        Inherited = false
    )]
    public sealed class AllowNullAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AllowNullAttribute"/> class.
        /// </summary>
        public AllowNullAttribute() { }
    }

    /// <summary>
    ///     Specifies that <see langword="null"/> is disallowed as an input even if the
    ///     corresponding type allows it.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property,
        Inherited = false
    )]
    public sealed class DisallowNullAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisallowNullAttribute"/> class.
        /// </summary>
        public DisallowNullAttribute() { }
    }

    /// <summary>
    ///     Specifies that a method that will never return under any circumstance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DoesNotReturnAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoesNotReturnAttribute"/> class.
        /// </summary>
        ///
        public DoesNotReturnAttribute() { }
    }

    /// <summary>
    ///     Specifies that the method will not return if the associated <see cref="Boolean"/>
    ///     parameter is passed the specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class DoesNotReturnIfAttribute : Attribute
    {
        /// <summary>
        ///     Gets the condition parameter value.
        ///     Code after the method is considered unreachable by diagnostics if the argument
        ///     to the associated parameter matches this value.
        /// </summary>
        public bool ParameterValue { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoesNotReturnIfAttribute"/>
        ///     class with the specified parameter value.
        /// </summary>
        /// <param name="parameterValue">
        ///     The condition parameter value.
        ///     Code after the method is considered unreachable by diagnostics if the argument
        ///     to the associated parameter matches this value.
        /// </param>
        public DoesNotReturnIfAttribute(bool parameterValue)
        {
            ParameterValue = parameterValue;
        }
    }

    /// <summary>
    ///     Specifies that an output may be <see langword="null"/> even if the
    ///     corresponding type disallows it.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Parameter |
        AttributeTargets.Property | AttributeTargets.ReturnValue,
        Inherited = false
    )]
    public sealed class MaybeNullAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MaybeNullAttribute"/> class.
        /// </summary>
        public MaybeNullAttribute() { }
    }

    /// <summary>
    ///     Specifies that when a method returns <see cref="ReturnValue"/>, 
    ///     the parameter may be <see langword="null"/> even if the corresponding type disallows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class MaybeNullWhenAttribute : Attribute
    {
        /// <summary>
        ///     Gets the return value condition.
        ///     If the method returns this value, the associated parameter may be <see langword="null"/>.
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        ///      Initializes the attribute with the specified return value condition.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition.
        ///     If the method returns this value, the associated parameter may be <see langword="null"/>.
        /// </param>
        public MaybeNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }
    }

    /// <summary>
    ///     Specifies that an output is not <see langword="null"/> even if the
    ///     corresponding type allows it.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Parameter |
        AttributeTargets.Property | AttributeTargets.ReturnValue,
        Inherited = false
    )]
    public sealed class NotNullAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotNullAttribute"/> class.
        /// </summary>
        public NotNullAttribute() { }
    }

    /// <summary>
    ///     Specifies that the output will be non-<see langword="null"/> if the
    ///     named parameter is non-<see langword="null"/>.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue,
        AllowMultiple = true,
        Inherited = false
    )]
    public sealed class NotNullIfNotNullAttribute : Attribute
    {
        /// <summary>
        ///     Gets the associated parameter name.
        ///     The output will be non-<see langword="null"/> if the argument to the
        ///     parameter specified is non-<see langword="null"/>.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        ///     Initializes the attribute with the associated parameter name.
        /// </summary>
        /// <param name="parameterName">
        ///     The associated parameter name.
        ///     The output will be non-<see langword="null"/> if the argument to the
        ///     parameter specified is non-<see langword="null"/>.
        /// </param>
        public NotNullIfNotNullAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
    }

    /// <summary>
    ///     Specifies that when a method returns <see cref="ReturnValue"/>,
    ///     the parameter will not be <see langword="null"/> even if the corresponding type allows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>
        ///     Gets the return value condition.
        ///     If the method returns this value, the associated parameter will not be <see langword="null"/>.
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        ///     Initializes the attribute with the specified return value condition.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition.
        ///     If the method returns this value, the associated parameter will not be <see langword="null"/>.
        /// </param>
        public NotNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }
    }
#endif

#if NET5_0_OR_GREATER

#else

    /// <summary>
    ///     Specifies that the method or property will ensure that the listed field and property members have
    ///     not-<see langword="null"/> values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class MemberNotNullAttribute : Attribute
    {
        /// <summary>
        ///     Gets field or property member names.
        /// </summary>
        public string[] Members { get; }

        /// <summary>
        ///     Initializes the attribute with a field or property member.
        /// </summary>
        /// <param name="member">
        ///     The field or property member that is promised to be not-null.
        /// </param>
        public MemberNotNullAttribute(string member)
        {
            Members = new[] { member };
        }

        /// <summary>
        ///     Initializes the attribute with the list of field and property members.
        /// </summary>
        /// <param name="members">
        ///     The list of field and property members that are promised to be not-null.
        /// </param>
        public MemberNotNullAttribute(params string[] members)
        {
            Members = members;
        }
    }

    /// <summary>
    ///     Specifies that the method or property will ensure that the listed field and property members have
    ///     non-<see langword="null"/> values when returning with the specified return value condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class MemberNotNullWhenAttribute : Attribute
    {
        /// <summary>
        ///     Gets the return value condition.
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        ///     Gets field or property member names.
        /// </summary>
        public string[] Members { get; }

        /// <summary>
        ///     Initializes the attribute with the specified return value condition and a field or property member.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition. If the method returns this value,
        ///     the associated parameter will not be <see langword="null"/>.
        /// </param>
        /// <param name="member">
        ///     The field or property member that is promised to be not-<see langword="null"/>.
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, string member)
        {
            ReturnValue = returnValue;
            Members = new[] { member };
        }

        /// <summary>
        ///     Initializes the attribute with the specified return value condition and list
        ///     of field and property members.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition. If the method returns this value,
        ///     the associated parameter will not be <see langword="null"/>.
        /// </param>
        /// <param name="members">
        ///     The list of field and property members that are promised to be not-null.
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            ReturnValue = returnValue;
            Members = members;
        }
    }


#endif
}