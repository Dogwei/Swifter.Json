namespace Swifter.RW
{
    /// <summary>
    /// 读写路径节点访问器接口。
    /// </summary>
    public interface IRWPathNodeVisitor
    {
        /// <summary>
        /// 访问一个常量节点。
        /// </summary>
        void VisitConstant<TKey>(RWPathConstantNode<TKey> node) where TKey : notnull;
    }
}