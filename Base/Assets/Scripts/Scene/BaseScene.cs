using UnityEngine;
using System.Collections;

public class BaseScene : MonoBehaviour {
	
	public virtual IEnumerator Initialize()
    {
        yield return null;
    }

    public virtual IEnumerator Terminate()
	{
        yield return null;
	}

    void OnEnable()
    {
        if (null != RotationManager.Instance)
            RotationManager.Instance.OnOrientationChanged += OrientationChanged;
    }

    void OnDiable()
    {
        if (null != RotationManager.Instance)
            RotationManager.Instance.OnOrientationChanged -= OrientationChanged;
    }

    public virtual void OrientationChanged()
    {
    }

    public virtual void OnBackButtonClick()
    {
        Application.Quit();
    }
}
