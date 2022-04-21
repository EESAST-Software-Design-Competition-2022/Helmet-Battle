using UnityEngine;
using System.Collections;
using Mirror;
//��PlayerController�й��������Ĵ�������ͬ��
public class SceneWeapon : NetworkBehaviour
{
    //-----------------------------------------------------------------------------------------------------
    //��������
    #region
    [SerializeField]
    [SyncVar(hook = nameof(OnChangeWeaponID))]
    WeaponID weaponID;
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //��ʼ��
    #region
    private void Start()
    {
        SetWeaponID(weaponID); 
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //ʵʱˢ��
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //�ͻ��˲���
    #region
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //�ͻ��˲���������
    #region
    /// <summary>
    /// �ӿͻ����޸ķ�������WeaponIDʱ���ô˺���
    /// </summary>
    /// <param name="id"></param>
    [Command]
    public void CmdSetWeaponID(WeaponID id)
    {
        SetWeaponID(id);
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //�����������ͻ���
    #region
    /// <summary>
    /// �������������ڿͻ��˺ͷ�����ִ�����Ҵݻ�
    /// </summary>
    [Server]
    public void DestroyOnSelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    /// <summary>
    /// ����Weapon������ߺ���
    /// </summary>
    /// <param name="newWeaponID"></param>
    public void CollectWeapon(WeaponID newWeaponID)
    {
        Instantiate(Data.instance.WeaponPrefabs[(int)newWeaponID], transform);
    }
    // Since Destroy is delayed to the end of the current frame, we use a coroutine
    // to clear out any child objects before instantiating the new one
    /// <summary>
    /// ���ԭ������������CollectWeapon�����µ�����
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
    /// ��Server��WeaponIDͬ����Client������ChangeWeapon
    /// </summary>
    /// <param name="oldID"></param>
    /// <param name="newID"></param>
    void OnChangeWeaponID(WeaponID oldID, WeaponID newID)
    {
        StartCoroutine(ChangeWeapon(newID));
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //����������
    #region
    /// <summary>
    /// ������������������WeaponID
    /// </summary>
    /// <param name="id"></param>
    public void SetWeaponID(WeaponID id)
    {
        weaponID = id;
        StartCoroutine(ChangeWeapon(id));
    }
    #endregion
    //-----------------------------------------------------------------------------------------------------
    //����
    #region
    /// <summary>
    /// �������ļ��У���Ҫ��ȡweaponID�͵����������
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