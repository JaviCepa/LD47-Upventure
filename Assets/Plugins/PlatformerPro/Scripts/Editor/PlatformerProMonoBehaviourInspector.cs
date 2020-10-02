using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PlatformerPro
{
	/// <summary>
	/// Base editor for paltformer pro monobehaviours.
	/// </summary>
	[CustomEditor (typeof(PlatformerProMonoBehaviour), true)]
	[CanEditMultipleObjects]
	public class PlatformerProMonoBehaviourInspector : Editor 
	{
		/// <summary>
		/// Holds the platformer pro icon texture
		/// </summary>
		public static Texture2D iconTexture;

		/// <summary>
		/// Unity OnEnable hook.
		/// </summary>
		virtual protected void OnEnable() 
		{
			if (iconTexture == null) iconTexture = Resources.Load <Texture2D> ("Platformer_Icon");
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawHeader((PlatformerProMonoBehaviour) target);
			GUILayout.Space (5);
			DrawDefaultInspector ();
			GUILayout.Space (5);
			DrawFooter((PlatformerProMonoBehaviour) target);
		}

		public static void DrawHeaderStatic(PlatformerProMonoBehaviour myTarget)
		{
			GUILayout.BeginHorizontal ();
			if (iconTexture == null) iconTexture = Resources.Load <Texture2D> ("Platformer_Icon");
			if (GUILayout.Button (iconTexture, GUILayout.Width (48), GUILayout.Height (48)))
			{
				PlatformerProWelcomePopUp.ShowWelcomeScreen ();
			}
			GUILayout.BeginVertical ();
			if (myTarget.Header != null)
			{
				EditorGUILayout.HelpBox (myTarget.Header, MessageType.None);
			} 

			GUILayout.BeginHorizontal ();
			if (myTarget.DocLink != null)
			{
				if (GUILayout.Button ("Go to Doc", EditorStyles.miniButton))
				{
					Application.OpenURL (myTarget.DocLink);
				}
			}
			if (myTarget.VideoLink != null)
			{
				if (GUILayout.Button ("Go to Video", EditorStyles.miniButton))
				{
					Application.OpenURL(myTarget.VideoLink);
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
			DrawHorizontalRule();
		}

		public static void DrawHorizontalRule(bool fillWindow = false)
		{
			EditorGUILayout.Space();
			var rect = EditorGUILayout.BeginHorizontal();
			Handles.color = Color.gray;
			Handles.DrawLine(new Vector2(rect.x - (fillWindow ? 15 : 0), rect.y), new Vector2(rect.width +  (fillWindow ? 15 : 0), rect.y));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
		
		/// <summary>
		/// Draws the header.
		/// </summary>
		/// <param name="myTarget">My target.</param>
		virtual protected void DrawHeader(PlatformerProMonoBehaviour myTarget)
		{
			DrawHeaderStatic(myTarget);
		}

		/// <summary>
		/// Draws the footer.
		/// </summary>
		virtual protected void DrawFooter(PlatformerProMonoBehaviour myTarget)
		{
			myTarget.Validate(myTarget);
		}

	}
}