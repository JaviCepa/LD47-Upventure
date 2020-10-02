namespace PlatformerPro
{
    /// <summary>
    /// Arguments passed when a camera zone change event is raised.
    /// </summary>
    public class CameraZoneChangeEventArgs : System.EventArgs
    {
        /// <summary>
        /// The new zone (zone camera ended in ).
        /// </summary>
        public CameraZone NewZone {
            get;
            protected set;
        }

        /// <summary>
        /// The old zone (zone camera started in).
        /// </summary>
        public CameraZone OldZone {
            get;
            protected set;
        }

        /// <summary>
        /// Create a new instance of CameraZoneEventArgs.
        /// </summary>
        public CameraZoneChangeEventArgs(CameraZone oldZone, CameraZone newZone)
        {
            OldZone = oldZone;
            NewZone = newZone;
        }
    }
}
