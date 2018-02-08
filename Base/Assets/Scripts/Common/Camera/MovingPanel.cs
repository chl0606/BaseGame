using UnityEngine;
using System.Collections;

public class MovingPanel : MonoBehaviour
{
    protected float m_limitScaleMin = 0.50f;  //minimun Scale Limit
    protected float m_limitScaleMax = 1.70f;  //maximun Scale Limit
    protected MovingCamera m_movingCamera;
    //public static MovingPanel Instance;

    //LocalScale을 변경.
    public void ChangeLocalScale(Vector3 _sumScale)
    {
        if (null == m_movingCamera) return;

        if (0f == _sumScale.y)
        {
            return;
        }

        Vector3 targetScale = transform.localScale + _sumScale;

        if (targetScale.y < m_limitScaleMin)
        {
            if (transform.localScale.y == m_limitScaleMin)
            {
                return;
            }
            transform.localScale = new Vector3(m_limitScaleMin, m_limitScaleMin, m_limitScaleMin);

        }
        else if (targetScale.y > m_limitScaleMax)
        {
            if (transform.localScale.y == m_limitScaleMax)
            {
                return;
            }
            transform.localScale = new Vector3(m_limitScaleMax, m_limitScaleMax, m_limitScaleMax); ;
        }
        else
        {
            transform.localScale += _sumScale;
        }

        m_movingCamera.PanelScaleChanged();
    }

    public void SetLocalScale(Vector3 v3ChangeScale)
    {
        if( v3ChangeScale.x <= m_limitScaleMin )
        {
            v3ChangeScale = Vector3.one * m_limitScaleMin;
        }
        else if (v3ChangeScale.x >= m_limitScaleMax)
        {
            v3ChangeScale = Vector3.one * m_limitScaleMax;
        }
        transform.localScale = v3ChangeScale;

        m_movingCamera.PanelScaleChanged();
    } 

}
