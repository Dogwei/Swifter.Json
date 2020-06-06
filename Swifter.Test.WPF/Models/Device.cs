using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Test.WPF.Models
{
    [MessagePackObject(true)]
    public sealed class Device
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string SystemNo { get; set; }
        public string IMEI { get; set; }
        public int MainRoleId { get; set; }
        public int DeviceType { get; set; }
        public int DeviceState { get; set; }
        public DateTime InstallDate { get; set; }
        public DateTime InsertTime { get; set; }
        public int MediaChannels { get; set; }
        public int SimId { get; set; }
        public DateTime LastHeartbeatTime { get; set; }
    }
}
