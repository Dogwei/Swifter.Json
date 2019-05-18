namespace Swifter.Tools
{
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <returns></returns>
        Out Invoke();
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Arg12"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <param name="arg12"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11, Arg12 arg12);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Arg12"></typeparam>
    /// <typeparam name="Arg13"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Arg13, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <param name="arg12"></param>
        /// <param name="arg13"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11, Arg12 arg12, Arg13 arg13);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Arg12"></typeparam>
    /// <typeparam name="Arg13"></typeparam>
    /// <typeparam name="Arg14"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Arg13, Arg14, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <param name="arg12"></param>
        /// <param name="arg13"></param>
        /// <param name="arg14"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11, Arg12 arg12, Arg13 arg13, Arg14 arg14);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Arg12"></typeparam>
    /// <typeparam name="Arg13"></typeparam>
    /// <typeparam name="Arg14"></typeparam>
    /// <typeparam name="Arg15"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Arg13, Arg14, Arg15, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <param name="arg12"></param>
        /// <param name="arg13"></param>
        /// <param name="arg14"></param>
        /// <param name="arg15"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11, Arg12 arg12, Arg13 arg13, Arg14 arg14, Arg15 arg15);
    }
    /// <summary>
    /// 有返回值并且参数数量匹配的动态委托将实现此接口。
    /// </summary>
    /// <typeparam name="Arg1"></typeparam>
    /// <typeparam name="Arg2"></typeparam>
    /// <typeparam name="Arg3"></typeparam>
    /// <typeparam name="Arg4"></typeparam>
    /// <typeparam name="Arg5"></typeparam>
    /// <typeparam name="Arg6"></typeparam>
    /// <typeparam name="Arg7"></typeparam>
    /// <typeparam name="Arg8"></typeparam>
    /// <typeparam name="Arg9"></typeparam>
    /// <typeparam name="Arg10"></typeparam>
    /// <typeparam name="Arg11"></typeparam>
    /// <typeparam name="Arg12"></typeparam>
    /// <typeparam name="Arg13"></typeparam>
    /// <typeparam name="Arg14"></typeparam>
    /// <typeparam name="Arg15"></typeparam>
    /// <typeparam name="Arg16"></typeparam>
    /// <typeparam name="Out"></typeparam>
    public interface IFunc<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8, Arg9, Arg10, Arg11, Arg12, Arg13, Arg14, Arg15, Arg16, Out>
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <param name="arg8"></param>
        /// <param name="arg9"></param>
        /// <param name="arg10"></param>
        /// <param name="arg11"></param>
        /// <param name="arg12"></param>
        /// <param name="arg13"></param>
        /// <param name="arg14"></param>
        /// <param name="arg15"></param>
        /// <param name="arg16"></param>
        /// <returns></returns>
        Out Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, Arg9 arg9, Arg10 arg10, Arg11 arg11, Arg12 arg12, Arg13 arg13, Arg14 arg14, Arg15 arg15, Arg16 arg16);
    }
}