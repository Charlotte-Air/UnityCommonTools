using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor.Animations;
#endif

// 此工具应用于3D美术动作文件，自动修改动作名
public class ReNameAnimationFile
{
    private static string baseName = "";
    private static int _splitCount = 0;
    private static bool isHaveSkin = false;
    private static string basePath = "";

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/ReNameAnimationFile")]
    public static void Rename()
    {
        UnityEngine.Object[] m_objects = UnityEditor.Selection.GetFiltered(typeof(UnityEngine.Object), UnityEditor.SelectionMode.Assets);//选择资源对象  

        // 检出skin文件 确定basename
        for (int i = 0; i < m_objects.Length; i++)
        {
            if (m_objects[i].name.Contains("_skin"))
            {
                var str = m_objects[i].name.Split('_');
                baseName = str[0];
                _splitCount = str.Length;
                for (int s = 1; s < str.Length - 1; s++)
                {
                    baseName = baseName + "_" + str[s];
                }
                basePath = UnityEditor.AssetDatabase.GetAssetPath(m_objects[i]).Replace(m_objects[i].name + ".FBX", "");
                isHaveSkin = true;
                break;
            }
        }

        // 若无skin文件 则return
        if (!isHaveSkin)
        {
            Debug.Log("资源选择错误，未找到skin文件。");
            return;
        }


        for (int i = 0; i < m_objects.Length; i++)
        {
            if (System.IO.Path.GetExtension(UnityEditor.AssetDatabase.GetAssetPath(m_objects[i])) != "")//判断路径是否为空
            {
                // 排除baseName同名文件、排除模型文件、排除@ 已经修改过的文件、选中skin文件基础名相同的文件
                if (m_objects[i].name != baseName && !m_objects[i].name.Contains("_skin") && !m_objects[i].name.Contains("@") && m_objects[i].name.Contains(baseName))
                {
                    var curNameSplit = m_objects[i].name.Split('_');

                    string newName = baseName + "@";
                    for (int s = 0; s < curNameSplit.Length; s++)
                    {
                        if (s > _splitCount - 2)
                        {
                            if (s == _splitCount - 1)
                            {
                                newName = newName + curNameSplit[s];
                            }
                            else
                            {
                                newName = newName + "_" + curNameSplit[s];
                            }
                        }
                    }

                    Debug.Log("name : " + m_objects[i].name + "  to  " + newName);

                    // 修改文件名
                    string path = UnityEditor.AssetDatabase.GetAssetPath(m_objects[i]);
                    UnityEditor.AssetDatabase.RenameAsset(path, newName);
                }
            }
        }

        CreateAnimatorController(m_objects);

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }

    private static AnimatorController CreateAnimatorController(UnityEngine.Object[] objects)
    {
        //生成动画控制器（AnimatorController）
        AnimatorController _animatorController = AnimatorController.CreateAnimatorControllerAtPath(basePath + "/" + baseName + ".controller");

        //得到它的Layer， 默认layer为base,可以拓展
        AnimatorControllerLayer _layer = _animatorController.layers[0];

        //把动画文件保存在我们创建的AnimatorController中
        AddStateTransition(objects, _layer);
        return _animatorController;
    }

    private static void AddStateTransition(UnityEngine.Object[] objects, AnimatorControllerLayer _layer)
    {
        //添加动画状态机（这里只是通过层得到根状态机，并未添加）
        AnimatorStateMachine _stateMachine = _layer.stateMachine;

        // 根据动画文件读取它的AnimationClip对象
        if (objects.Length == 0)
        {
            Debug.Log(string.Format("Can't find clip objects"));
            return;
        }

        // 遍历读取模型中包含的动画片段
        for (int i = 0; i < objects.Length; i++)
        {
            var o = objects[i];
            if (o.name != baseName && !o.name.Contains("_skin") && o.name.Contains(baseName))
            {
                if (!(o is GameObject)) continue;
                if (!o.name.Contains("@")) continue;
                AnimationClip srcclip = UnityEditor.AssetDatabase.LoadAssetAtPath(basePath + o.name + ".FBX", typeof(AnimationClip)) as AnimationClip;
                if (srcclip == null)
                    continue;

                var _nameStrs = o.name.Split('@');
                AnimatorState _state = _stateMachine.AddState(_nameStrs[1], new Vector3(
                    _stateMachine.entryPosition.x + 200,
                    _stateMachine.entryPosition.y + 50 * i,
                    0));
                _state.motion = srcclip;
                if (o.name.Contains("Idle01"))
                {
                    _stateMachine.defaultState = _state;
                }
            }
        }
    }
#endif
}