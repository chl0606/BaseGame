using UnityEngine;
using System.Collections;

public class ButtonListener : MonoBehaviour 
{
    //public bool playSound = true;
    //public bool isCancelButton = false;
    public int index;
    public string btnName;
    public delegate void PressListener(ButtonListener _btn, bool isPress);
    public PressListener OnPressed;

    public delegate void ClickListener(ButtonListener _btn);
    public ClickListener OnClicked;

    public delegate void DragListener(ButtonListener _btn, Vector2 delta);
    public DragListener OnDraged;


    void OnPress(bool isPress)
    {
        if (null != OnPressed)
        {
            OnPressed(this, isPress);
        }
    }

    void OnClick()
    {
        if (null != OnClicked)
        {
            //if (playSound)
            //{
                //SoundManager.Instance.PlaySfx(isCancelButton ? eSfxType.ButtonCancel : eSfxType.ButtonOk);
            //}

            OnClicked(this);
            //SoundManager.Instance.PlaySound("button_click", SoundManager.SoundType.sfx);
        }
    }

    void OnDrag(Vector2 delta)
    {
        if (null != OnDraged)
        {
            OnDraged(this, delta);
        }
    }
}
