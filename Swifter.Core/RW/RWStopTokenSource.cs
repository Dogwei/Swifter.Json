namespace Swifter.RW
{
    /// <summary>
    /// 停止令牌源。
    /// </summary>
    public class RWStopTokenSource
    {
        private bool isStopRequested;
        internal object? state;

        /// <summary>
        /// 是否已请求停止。
        /// </summary>
        public bool IsStopRequested
        {
            get
            {
                return isStopRequested;
            }
        }

        /// <summary>
        /// 能否继续。
        /// </summary>
        public bool CanToContinue
        {
            get
            {
                return state is not null;
            }
        }

        /// <summary>
        /// 获取停止令牌。
        /// </summary>
        public RWStopToken Token
        {
            get
            {
                return new RWStopToken(this);
            }
        }

        internal bool CanBeStopped
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 准备继续。
        /// </summary>
        public void PrepareContinue()
        {
            isStopRequested = false;
        }

        /// <summary>
        /// 请求停止。
        /// </summary>
        public void Stop()
        {
            isStopRequested = true;
        }
    }
}