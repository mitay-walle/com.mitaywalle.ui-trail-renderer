using UnityEditor;

namespace mitaywalle
{
	[CustomEditor(typeof(UITrailRenderer))]
	[CanEditMultipleObjects]
	public class UITrailRendererEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script", "m_OnCullStateChanged", "m_RaycastPadding");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
