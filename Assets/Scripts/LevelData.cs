using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LevelData_01", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("基础信息")]
    public string levelName = "关卡1";          // 关卡显示名称
  //  public Sprite levelIcon;                    // 关卡预览图标
   // public bool isUnlocked = true;              // 是否解锁
    public int levelIndex;  //关卡顺序
    [Header("Addressable 配置")]
    public AssetReference sceneReference;       // 关卡场景的 Addressable 引用
}