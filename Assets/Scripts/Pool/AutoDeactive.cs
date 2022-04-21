using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDeactive : MonoBehaviour
{
    [SerializeField]
    bool destroyObject;
    [SerializeField]
    float lifeTime = 1f;

    WaitForSeconds waitLifeTime;
    private void Awake()
    {
        waitLifeTime = new WaitForSeconds(lifeTime);
    }
    private void Start()
    {


    }
    private void OnEnable()
    {
        StartCoroutine(DeactiveCoroutine());
    }
    IEnumerator DeactiveCoroutine()
    {
        yield return waitLifeTime;
        if (destroyObject)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
