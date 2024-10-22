﻿using UnityEditor;
using UnityEngine;
using System.Collections;

namespace PlatformerPro
{
	[CustomEditor (typeof(GrappleAimer), false)]
	public class GrappleAimerInspector : Editor
	{
		override public void OnInspectorGUI()
		{
			// Unity says we don't need to do this, but if we don't do this then the automatic serialised object updates get the same name as the last object recorded
			Undo.FlushUndoRecordObjects ();
			Undo.RecordObject (target, "Grapple Aimer Update");
			GrappleAimer aimer = target as GrappleAimer;
			Vector2 offset = EditorGUILayout.Vector2Field (new GUIContent("Offset (UP)", "How far offset is the bullet from the character position when shotting UP."),
			                                           aimer.offset);
			if (offset != aimer.offset)
			{
				aimer.offset = offset;
			}
			Vector2 offsetFortyFive = EditorGUILayout.Vector2Field (new GUIContent ("Offset (45)", "How far offset is the bullet from the character position when shotting UP."),
			                                                    aimer.offsetFortyFive);
			if (offsetFortyFive != aimer.offsetFortyFive)
			{
				aimer.offsetFortyFive = offsetFortyFive;
			}
			float speedAffectsAngleFactor = EditorGUILayout.FloatField (new GUIContent("X Speed Affects Angle", "How much the character X speed affects the grapple angle when throwing at 45 degrees WHILE IN THE AIR."),
			                                                            aimer.speedAffectsAngleFactor);
			if (speedAffectsAngleFactor != aimer.speedAffectsAngleFactor)
			{
				aimer.speedAffectsAngleFactor = speedAffectsAngleFactor;
			}
		}
	}
}