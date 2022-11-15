
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 停止令牌。支持继续读写。
    /// </summary>
    public struct RWStopToken
    {
        private readonly RWStopTokenSource? source;

        internal RWStopToken(RWStopTokenSource? source)
        {
            this.source = source;
        }

        /// <summary>
        /// 是否已请求停止。
        /// </summary>
        public bool IsStopRequested
        {
            get
            {
                return source is not null && source.IsStopRequested;
            }
        }

        /// <summary>
        /// 能否被停止。
        /// </summary>
        public bool CanBeStopped
        {
            get
            {
                return source is not null && source.CanBeStopped;
            }
        }

        /// <summary>
        /// 在停止前设置当前状态。
        /// </summary>
        public void SetState(object state)
        {
            if (source is not null && source.IsStopRequested)
            {
                source.state = state;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 弹出上次停止前的状态。
        /// </summary>
        public object? PopState()
        {
            if (source is not null)
            {
                var result = source.state;

                source.state = null;

                return result;
            }

            return null;
        }
    }
}