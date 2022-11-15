using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    sealed class TargetableSetOptionsHelper<TOptions> : IValueInterface<TargetableSetOptionsHelper<TOptions>>
    {
        public static void SetOptions(ITargetableValueRWSource targetable, TOptions options)
        {
            targetable.SetValueInterface(new TargetableSetOptionsHelper<TOptions>(options));
        }

        public static bool TryGetOptions(ITargetableValueRWSource targetable, [MaybeNullWhen(false)] out TOptions options)
        {
            var valueInterface = targetable.GetValueInterface<TargetableSetOptionsHelper<TOptions>>();

            if (valueInterface is TargetableSetOptionsHelper<TOptions> helper)
            {
                options = helper.Options;

                return true;
            }

            options = default;

            return false;
        }

        public static bool TryGetOptions(ITargetableValueRW targetable, [MaybeNullWhen(false)] out TOptions options)
        {
            var valueInterface = targetable.GetValueInterface<TargetableSetOptionsHelper<TOptions>>();

            if (valueInterface is TargetableSetOptionsHelper<TOptions> helper)
            {
                options = helper.Options;

                return true;
            }

            options = default;

            return false;
        }

        public readonly TOptions Options;

        private TargetableSetOptionsHelper(TOptions options)
        {
            Options = options;
        }

        TargetableSetOptionsHelper<TOptions>? IValueInterface<TargetableSetOptionsHelper<TOptions>>.ReadValue(IValueReader valueReader) 
            => throw new InvalidOperationException();

        void IValueInterface<TargetableSetOptionsHelper<TOptions>>.WriteValue(IValueWriter valueWriter, TargetableSetOptionsHelper<TOptions>? value) 
            => throw new InvalidOperationException();
    }
}