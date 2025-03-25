using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FixAnimationCycle))]
public class FixAnimationCycleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FixAnimationCycle script = (FixAnimationCycle)target;
        if (GUILayout.Button("Fix Animation Cycle"))
        {
            script.FixAnimationCycleMethod();
        }

        if (GUILayout.Button("Clip Keyframes"))
        {
            script.ClipKeyframesMethod();
        }
    }
}