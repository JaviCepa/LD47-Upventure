using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace PlatformerPro.Dialog
{

    [CustomNodeEditor(typeof(EventResponseNode))]
    public class EventResponseNodeEditor : NodeEditor
    {

        /// <summary> Draws standard field editors for all public fields </summary>
        public override void OnBodyGUI()
        {
            serializedObject.Update();
            string[] excludes = { "m_Script", "graph", "position", "ports", "response"};
            
            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                NodeEditorGUILayout.PropertyField(iterator, true);
            }

            // Iterate through dynamic ports and draw them in the order in which they are serialized
            foreach (XNode.NodePort dynamicPort in target.DynamicPorts) {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }
            serializedObject.ApplyModifiedProperties();
            
            // We use the existing inspector for this
            Undo.FlushUndoRecordObjects ();
            Undo.RecordObject (target, "Dialog Response Update");
            EventResponderInspector.RenderAction(target, ((EventResponseNode)target).response);
        }

    }
}