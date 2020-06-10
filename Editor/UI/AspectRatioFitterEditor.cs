using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(AspectRatioFitter), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the AspectRatioFitter component.
    ///   Extend this class to write a custom editor for a component derived from AspectRatioFitter.
    /// </summary>
    public class AspectRatioFitterEditor : SelfControllerEditor
    {
        SerializedProperty m_AspectMode;
        SerializedProperty m_AspectRatio;

        private AspectRatioFitter aspectRatioFitter;

        protected virtual void OnEnable()
        {
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");
            m_AspectRatio = serializedObject.FindProperty("m_AspectRatio");
            aspectRatioFitter = target as AspectRatioFitter;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_AspectMode);
            EditorGUILayout.PropertyField(m_AspectRatio);
            serializedObject.ApplyModifiedProperties();

            if (aspectRatioFitter)
            {
                if (!aspectRatioFitter.IsAspectModeValid())
                    ShowNoParentWarning();
                if (!aspectRatioFitter.IsComponentValidOnObject())
                    ShowCanvasRenderModeInvalidWarning();
            }

            base.OnInspectorGUI();
        }

        protected virtual void OnDisable()
        {
            aspectRatioFitter = null;
        }

        private static void ShowNoParentWarning()
        {
            var text = L10n.Tr("You cannot use this Aspect Mode because this Component's GameObject does not have a parent object.");
            EditorGUILayout.HelpBox(text, MessageType.Warning, true);
        }

        private static void ShowCanvasRenderModeInvalidWarning()
        {
            var text = L10n.Tr("You cannot use this Aspect Mode because this Component is attached to a Canvas with a fixed width and height.");
            EditorGUILayout.HelpBox(text, MessageType.Warning, true);
        }
    }
}
