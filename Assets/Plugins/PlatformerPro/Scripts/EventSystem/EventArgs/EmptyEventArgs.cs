using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
    /// <summary>
    /// Generic event used for events tht have no args.
    /// </summary>
    public class EmptyEventArgs : System.EventArgs
    {
	    public static EmptyEventArgs Instance = new EmptyEventArgs();
    }
	
}