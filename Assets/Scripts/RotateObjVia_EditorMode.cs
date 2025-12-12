/*SHU QIYU 
 * 该脚本用于在UnityEditor模式下
 * 将一个物体沿着一个另一个物体
 * 的本地x，y或z轴旋转一定的度数
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class RotateObjVia_EditorMode : MonoBehaviour
{
    public Transform target;           // 旋转轴参考的物体
    public Transform objectToRotate;   // 要旋转的物体
    public float angle = 30f;          // 旋转角度
    public Axis axis = Axis.X;         // 指定绕哪条轴旋转

    public enum Axis { X, Y, Z }

    private void Reset()
    {
        // 默认 objectToRotate 为挂载脚本的物体
        objectToRotate = transform;
    }

    public void RotateNow()
    {
        if (target == null || objectToRotate == null)
        {
            Debug.LogWarning("请设置 target 与 objectToRotate！");
            return;
        }

        // 选择轴
        Vector3 localAxis = axis switch
        {
            Axis.X => target.transform.right,
            Axis.Y => target.transform.up,
            Axis.Z => target.transform.forward,
            _ => target.transform.right
        };

        // 在编辑模式下使用 Rotate
        objectToRotate.RotateAround(target.position, localAxis, angle);

#if UNITY_EDITOR
        // 标记为已修改，支持撤销和保存
        Undo.RecordObject(objectToRotate, "Rotate Along Local Axis");
        EditorUtility.SetDirty(objectToRotate);
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RotateObjVia_EditorMode))]
public class RotateAlongLocalAxisEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RotateObjVia_EditorMode script = (RotateObjVia_EditorMode)target;
        if (GUILayout.Button("Rotate"))
        {
            script.RotateNow();
        }
    }
}
#endif
