using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public GameObject ProgressPrefab;
    public float leftPoint;
    public float rightPoint;
    public List<GameObject> Bars;
    public int Size => Bars.Count;
    // Start is called before the first frame update
    void Start()
    {
        CreateProgressBars();
        SetValue(0.5f);
    }

    void CreateProgressBars()
    {
        var p = leftPoint;
        while (p < rightPoint)
        {
            var bar = Instantiate(ProgressPrefab,transform);
            bar.transform.localPosition = Vector3.right * p;
            Bars.Add(bar);
            p += 0.5f;
        }
    }

    public void SetValue(float percent)
    {
        int num = (int)(percent * Size);
        for(var i = 0;i < Size; i++)
        {
           if(i < num)
            {
                Bars[i].SetActive(true);
            }
            else
            {
                Bars[i].SetActive(false);
            }
        }
    }
}
