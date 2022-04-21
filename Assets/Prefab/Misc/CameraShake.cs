using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 打中Boss，相机震动
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    float shakeTime = 1.0f;//震动时间
    private float currentTime = 0.0f;
    private List<Vector3> gameobjpons = new List<Vector3>();
    public Camera camera;//要求震动的相机
    public float para1 = 0.04f, para2 = 1.0f, para3 = 2.0f;
    float strength = 1f;
    private void Awake()
    {
        instance = this;
    }
    public void Shake(float streng)
    {
        strength = streng;
        currentTime = shakeTime;
    }
    void LateUpdate() { UpdateShake(); }
    void UpdateShake()
    {
        if (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            camera.rect = new Rect(strength*para1 * (para2 + para3 * Random.value) * Mathf.Pow(currentTime, 2),strength * para1 * (para2 + para3 * Random.value) * Mathf.Pow(currentTime, 2), 1.0f, 1.0f);
        }
        else
        {
            currentTime = 0.0f;
        }
    }
}
