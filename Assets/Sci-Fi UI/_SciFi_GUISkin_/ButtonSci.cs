using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonSci : MonoBehaviour
{
    public Sprite ButtonSprite;
    public Sprite OnHoverSprite;
    public Sprite OnClickSprite;
    Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void OnHoverEnter()
    {
        image.sprite = OnHoverSprite;
    }
    public void OnHoverExit()
    {
        image.sprite = ButtonSprite;
    }
    public void OnClickStart()
    {
        image.sprite = OnClickSprite;
    }
    public void OnClickEnd()
    {
        image.sprite = ButtonSprite;
    }
}
