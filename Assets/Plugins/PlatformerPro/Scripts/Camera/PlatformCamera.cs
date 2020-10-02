using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlatformerPro
{
	/// <summary>
	/// Base class for all platformer cameras.
	/// </summary>
	public abstract class PlatformCamera : PlatformerProMonoBehaviour
	{
		/// <summary>
		/// Set to true if this is the default camera.
		/// </summary>
		public bool isDefaultCamera;

		[Header ("Camera Zones")]
		/// <summary>
		/// The starting zone.
		/// </summary>
		public CameraZone startingZone;

		/// <summary>
		/// The current zone.
		/// </summary>
		protected CameraZone currentZone;

		/// <summary>
		/// Cached reference to the associated Unity camera.
		/// </summary>
		protected Camera myCamera;

		/// <summary>
		/// Gets the current zone.
		/// </summary>
		public CameraZone CurrentZone 
		{
			get
			{
				return currentZone;
			}
			set
			{
				currentZone = value;
			}
		}

		#region events

		/// <summary>
		/// Event sent when the camera zone changes. This event is sent when the zone change starts.
		/// Add a handler to the tweener to listen to the zone change end for a change that is not instantaneous
		/// (or delay your event response).
		/// </summary>
		public System.EventHandler<CameraZoneChangeEventArgs> CameraZoneChanged;

		/// <summary>
		/// Send the Camera Zone changed event.
		/// </summary>
		/// <param name="oldZone"></param>
		/// <param name="newZone"></param>
		virtual protected void OnCameraZoneChanged(CameraZone oldZone, CameraZone newZone)
		{
			if (CameraZoneChanged != null) CameraZoneChanged(this, new CameraZoneChangeEventArgs(oldZone, newZone));
		}
        
		#endregion
		
		#region Unity Hooks

		/// <summary>
		/// Unity Destory hook.
		/// </summary>
		void OnDestroy()
		{
			DoDestroy ();
		}

		/// <summary>
		/// Unity Awake hook.
		/// </summary>
		void Awake()
		{
			AddCamera (this);
		}

		/// <summary>
		/// Unity start hook.
		/// </summary>
		void Start()
		{
			SetDefaultCamera ();
			Init ();
		}

		#endregion

		#region public methods
		
		/// <summary>
		/// Initialise this instance.
		/// </summary>
		virtual public void Init()
		{
			myCamera = GetComponent<Camera> ();
			if (myCamera == null) myCamera = GetComponentInParent<Camera> ();
			if (currentZone == null) currentZone = startingZone;
		}

		/// <summary>
		/// Changes the zone by immediately snapping to the new zones position.
		/// </summary>
		/// <param name="newZone">The zone to move to.</param>
		/// <param name="triggeringCharacter">Character that triggered the transition./param> 
		virtual public void ChangeZone(CameraZone newZone, Character triggeringCharacter = null)
		{
			OnCameraZoneChanged(currentZone, newZone);
			myCamera.transform.position = newZone.GetBestPositionForCharacter (myCamera, triggeringCharacter);
			currentZone = newZone;
		}

		#endregion

		/// <summary>
		/// Do the destroy actions.
		/// </summary>
		virtual protected void DoDestroy()
		{
			RemoveCamera (this);
		}

		#region static methods and members

		/// <summary>
		/// A list of all camera.
		/// </summary>
		static protected List<PlatformCamera> allCameras;

		/// <summary>
		/// The main or default camera.
		/// </summary>
		static protected PlatformCamera defaultCamera;

		/// <summary>
		/// Gets the default camera.
		/// </summary>
		static public PlatformCamera DefaultCamera
		{
			get 
			{
				return defaultCamera;
			}
		}

		/// <summary>
		/// Gets the default camera.
		/// </summary>
		static public PlatformCamera[]  AllCameras
		{
			get 
			{
				return allCameras.ToArray();
			}
		}

		/// <summary>
		/// Adds a camera to the global list.
		/// </summary>
		/// <param name="platformCamera">Platform camera.</param>
		static protected void AddCamera(PlatformCamera platformCamera)
		{
			if (allCameras == null) allCameras = new List<PlatformCamera>();
			allCameras.Add(platformCamera);
		}

		/// <summary>
		/// Removes a camera from the global list.
		/// </summary>
		/// <param name="platformCamera">Platform camera.</param>
		static protected void RemoveCamera(PlatformCamera platformCamera)
		{
			if (defaultCamera == platformCamera) defaultCamera = null;
			if (allCameras != null) 
			{
				if (allCameras.Contains(platformCamera))
				{
					allCameras.Remove (platformCamera);
				}
			}
		}

		/// <summary>
		/// Sets the main camera.
		/// </summary>
		static protected void SetDefaultCamera()
		{
			if (allCameras == null || allCameras.Count == 0)
			{
				Debug.LogError ("No platform cameras found");
			}
			// If there's only one camera assume it is the main regardless of settings.
			else if (allCameras.Count == 1)
			{
				defaultCamera = allCameras[0];
			}
			else 
			{
				int defaultCameraCount = 0;
				foreach (PlatformCamera camera in allCameras)
				{
					if (camera.isDefaultCamera)
					{
						defaultCamera = camera;
						defaultCameraCount++;
					}
				}
				if (defaultCameraCount != 1)
				{
					Debug.LogError ("Expected 1 Default Camera but there were: " + defaultCameraCount);
				}
			}
		}

		#endregion
	}

    public static class CameraExtensions
    {
        public static Bounds OrthographicBounds(this Camera camera)
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
    }

}