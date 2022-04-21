using UnityEngine;
using System.Collections;
using Mirror;
//和PlayerController中关于武器的代码是相同的
public class SceneWeapon : NetworkBehaviour
{
    //-----------------------------------------------------------------------------------------------------
    //变量定义
    #region
    [SerializeField]
    [SyncVar(hook = nameof(OnChangeWeaponID))]
    WeaponID weaponID;
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //初始化
    #region
    private void Start()
    {
        SetWeaponID(weaponID); 
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //实时刷新
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端操作
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //客户端操作服务器
    #region
    /// <summary>
    /// 从客户端修改服务器的WeaponID时调用此函数
    /// </summary>
    /// <param name="id"></param>
    [Command]
    public void CmdSetWeaponID(WeaponID id)
    {
        SetWeaponID(id);
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器操作客户端
    #region
    /// <summary>
    /// 服务器函数，在客户端和服务器执行自我摧毁
    /// </summary>
    [Server]
    public void DestroyOnSelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    /// <summary>
    /// 设置Weapon，请读者忽略
    /// </summary>
    /// <param name="newWeaponID"></param>
    public void CollectWeapon(WeaponID newWeaponID)
    {
        Instantiate(Data.instance.WeaponPrefabs[(int)newWeaponID], transform);
    }
    // Since Destroy is delayed to the end of the current frame, we use a coroutine
    // to clear out any child objects before instantiating the new one
    /// <summary>
    /// 清除原有武器并调用CollectWeapon设置新的武器
    /// </summary>
    /// <param name="newWeaponID"></param>
    /// <returns></returns>
    IEnumerator ChangeWeapon(WeaponID newWeaponID)
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            yield return null;
        }

        // Use the new value, not the SyncVar property value
        CollectWeapon(newWeaponID);
    }
    /// <summary>
    /// 将Server的WeaponID同步到Client，调用ChangeWeapon
    /// </summary>
    /// <param name="oldID"></param>
    /// <param name="newID"></param>
    void OnChangeWeaponID(WeaponID oldID, WeaponID newID)
    {
        StartCoroutine(ChangeWeapon(newID));
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //服务器操作
    #region
    /// <summary>
    /// 服务器调用自身设置WeaponID
    /// </summary>
    /// <param name="id"></param>
    public void SetWeaponID(WeaponID id)
    {
        weaponID = id;
        StartCoroutine(ChangeWeapon(id));
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //其他
    #region
    /// <summary>
    /// 在其他文件中，想要获取weaponID就调用这个方法
    /// </summary>
    /// <returns></returns>
    public WeaponID GetWeaponID()
    {
        return weaponID;
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deadline"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
    #endregion
}