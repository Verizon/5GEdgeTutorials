using System.Dynamic;

namespace Wavelength.Constants
{
    public static class Messages
    {
        public const string ReplicationChangeStatus = nameof(ReplicationChangeStatus);
        public const string ReplicationProgressUpdate = nameof(ReplicationProgressUpdate);
        public const string ReplicationError = nameof(ReplicationError);

        public static class ReplicationStatus
        {
            public const string Busy = "Busy";
            public const string Idle = "Idle";
            public const string Offline = "Offline";
            public const string Connecting = "Connecting";
            public const string Stopped = "Stopped";            
        }
    }
}