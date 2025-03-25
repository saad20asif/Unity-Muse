using UnityEngine;
using UnityEditor;

public class FixAnimationCycle : MonoBehaviour
{
    public AnimationClip originalClip;
    public int blendKeyframes = 5; // Number of keyframes to blend at the end
    public float easingInfluence = 1.0f; // Influence of the easing function
    public int keyframesToClip = 1; // Number of keyframes to clip from the end
    public string newClipName = "NewAnimationClip"; // Name for the new animation clip
    public bool removeRootTx = false; // Tickbox to remove curves containing "RootT.x"
    public bool removeRootTy = false; // Tickbox to remove curves containing "RootT.y"
    public bool removeRootTz = false; // Tickbox to remove curves containing "RootT.z"
    public bool removeRootQx = false; // Tickbox to remove curves containing "RootQ.x"
    public bool removeRootQy = false; // Tickbox to remove curves containing "RootQ.y"
    public bool removeRootQz = false; // Tickbox to remove curves containing "RootQ.z"

    public void FixAnimationCycleMethod()
    {
        if (originalClip == null)
        {
            Debug.LogError("Original AnimationClip is not assigned.");
            return;
        }

        AnimationClip newClip = DuplicateAnimationClip(originalClip, newClipName);

        // Retrieve all curves from the new animation clip
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(newClip);

        foreach (var binding in curveBindings)
        {
            // Optionally remove curves containing "RootT" or "RootQ"
            if ((removeRootTx && binding.propertyName == "RootT.x") ||
                (removeRootTy && binding.propertyName == "RootT.y") ||
                (removeRootTz && binding.propertyName == "RootT.z") ||
                (removeRootQx && binding.propertyName == "RootQ.x") ||
                (removeRootQy && binding.propertyName == "RootQ.y") ||
                (removeRootQz && binding.propertyName == "RootQ.z"))
            {
                AnimationUtility.SetEditorCurve(newClip, binding, null);
                continue;
            }

            // Ignore specific curves
            if (binding.propertyName == "RootT.z")
            {
                continue;
            }

            // Get the animation curve
            AnimationCurve curve = AnimationUtility.GetEditorCurve(newClip, binding);

            if (curve.length > 1)
            {
                // Copy the value of the first keyframe
                float firstKeyframeValue = curve.keys[0].value;

                // Create a new keyframe with the value of the first keyframe at the time of the last keyframe
                Keyframe lastKeyframe = curve.keys[curve.length - 1];
                lastKeyframe.value = firstKeyframeValue;

                // Replace the last keyframe with the new keyframe
                curve.MoveKey(curve.length - 1, lastKeyframe);

                // Blend the end of the animation back to the start with easing
                int startBlendingIndex = Mathf.Max(0, curve.length - blendKeyframes - 1);
                for (int i = startBlendingIndex; i < curve.length - 1; i++)
                {
                    float t = (float)(i - startBlendingIndex) / (blendKeyframes - 1);
                    t = EaseInOutQuad(t, easingInfluence); // Apply easing function with influence
                    Keyframe key = curve.keys[i];
                    key.value = Mathf.Lerp(key.value, curve.keys[0].value, t);
                    curve.MoveKey(i, key);
                }

                // Smooth the tangents for all keyframes in the curve
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                }

                // Set the modified curve back to the new animation clip
                AnimationUtility.SetEditorCurve(newClip, binding, curve);
            }
        }

        Debug.Log("New animation clip created with first keyframe values copied to the last keyframe, tangents smoothed, and end blended back to start with easing.");
    }

    public void ClipKeyframesMethod()
    {
        if (originalClip == null)
        {
            Debug.LogError("Original AnimationClip is not assigned.");
            return;
        }

        AnimationClip newClip = DuplicateAnimationClip(originalClip, newClipName);

        // Retrieve all curves from the new animation clip
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(newClip);

        foreach (var binding in curveBindings)
        {
            // Get the animation curve
            AnimationCurve curve = AnimationUtility.GetEditorCurve(newClip, binding);

            if (curve.length > keyframesToClip)
            {
                // Create a new array for the modified keyframes
                Keyframe[] newKeyframes = new Keyframe[curve.length - keyframesToClip];
                System.Array.Copy(curve.keys, newKeyframes, curve.length - keyframesToClip);

                // Create a new animation curve with the clipped keyframes
                AnimationCurve newCurve = new AnimationCurve(newKeyframes);

                // Set the modified curve back to the new animation clip
                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            }
        }

        Debug.Log($"{keyframesToClip} keyframes clipped from the end of the new animation.");
    }

    private AnimationClip DuplicateAnimationClip(AnimationClip original, string newName)
    {
        AnimationClip newClip = new AnimationClip
        {
            name = newName,
            frameRate = original.frameRate,
            wrapMode = original.wrapMode
        };

        foreach (var binding in AnimationUtility.GetCurveBindings(original))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(original, binding);
            AnimationUtility.SetEditorCurve(newClip, binding, curve);
        }

        AssetDatabase.CreateAsset(newClip, $"Assets/{newName}.anim");
        AssetDatabase.SaveAssets();

        return newClip;
    }

    private float EaseInOutQuad(float t, float influence)
    {
        if (t < 0.5f)
        {
            return 2 * Mathf.Pow(t, 2) * influence;
        }
        else
        {
            return 1 - Mathf.Pow(-2 * t + 2, 2) / 2 * influence;
        }
    }
}
