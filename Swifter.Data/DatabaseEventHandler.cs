namespace Swifter.Data
{
    /// <summary>
    /// Database 的事件委托。
    /// </summary>
    /// <typeparam name="TEventArgs">事件参数类型</typeparam>
    /// <param name="sender">事件触发者</param>
    /// <param name="args">事件参数</param>
    public delegate void DatabaseEventHandler<TEventArgs>(Database sender, TEventArgs args);

}
