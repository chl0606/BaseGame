using UnityEngine;
using System.Collections;

public class MovingCamera : MonoBehaviour
{
    private static MovingCamera s_instance = null;

    public delegate void CameraWorkListener(int _workIndex);
    public CameraWorkListener OnFinishCameraWork;

    private float screenWidthHalf;
    private float screenHeightHalf;
    [SerializeField]    private MovingPanel m_panel;
    [SerializeField]    private Rect m_moveBoundary;    //카메라가 움직일 수 있는 Rectangle
    private float m_panelWidth;
    private float m_panelHeight;

    protected Rect m_touchArea;
    protected float m_pixelGain;

    [SerializeField]    private bool m_enablePinchZoom = false;
    [SerializeField]    private bool m_enableDragMove = false;

    private bool m_isPress = false;//single touch press
    private Vector3 m_position;
    private Vector3 m_panelScale;
    private Vector3 m_mousePosPress;
    private bool m_interceptClick = false;//현재 Drag중인 지 여부.

    private Queue m_tracker;	//position tracking Queue
    private const int COUNT_TRACKER = 10;
    private Vector3 m_accelationSpeed;
    private bool m_traking = false;	//is tracking

    private float m_touchDistInit = 0f;

    private bool m_isAnimation = false;//현재 camera animation중인지 여부
    private Vector3 m_targetPosition;//animation target position
    private Vector3 m_targetScale;//animation taget scale
    private float m_aniSpeed;
    private int m_aniFrame;
    private int m_frameCnt;

    private int m_workIndex;//CameraWork를 구분할 key

    private int m_shakeCount;
    private float m_shakeAmount;
    private Vector3 m_posOrigin;

    public static MovingCamera Instance
    {
        get
        {
            if (null == s_instance)
            {
                Debug.LogError("Fail To Get MovingCamera Instance");
            }
            return s_instance;
        }
    }

    void Awake()
    {
        s_instance = FindObjectOfType(typeof(MovingCamera)) as MovingCamera;
    }

    void OnEnable()
    {
        screenWidthHalf = RotationManager.Instance.uirootWidth * 0.5f;
        screenHeightHalf = RotationManager.Instance.uirootHeight * 0.5f;
        m_isPress = false;
        m_isAnimation = false;
        m_interceptClick = false;
        m_position = transform.localPosition;
        m_panelScale = transform.localScale;
        m_tracker = new Queue();

        //float leftWidth = PanelInfoLoader.Instance.GetFloatAttribute("width", "PanelMenuLeft") * (1 / m_pixelGain);
        //Rect area = new Rect(leftWidth
        //    , 0f
        //    , camera.pixelWidth - leftWidth - (PanelInfoLoader.Instance.GetFloatAttribute("width", "PanelMenuRight") * (1 / m_pixelGain))
        //    , camera.pixelHeight - (PanelInfoLoader.Instance.GetFloatAttribute("height", "PanelPlayer") * (1 / m_pixelGain)));
        //SetTouchArea(area);
    }

    public void EnablePinchZoom(bool _use)
    {
        m_enablePinchZoom = _use;
    }

    public void EnableDragMove(bool _use)
    {
        m_enableDragMove = _use;
    }

    public void StopAnimation()
    {
        m_interceptClick = false;
        m_isAnimation = false;
    }

    void OnStart()
    {
    }

    void Update()
    {
        if (m_enableDragMove)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPress(true);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPress(false);
            }
        }

        if (m_enablePinchZoom)
        {
            if (Input.touchCount > 1)   //MultiTouch
            {
                m_interceptClick = true;
                if (m_isPress)
                {
                    OnPress(false);
                }

                if (0f != m_touchDistInit)
                {
                    float dist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) - m_touchDistInit;

                    float sumScale = dist / RotationManager.STANDARD_WIDTH;
                    //Debug.Log(sumScale);

                    m_panel.ChangeLocalScale(new Vector3(sumScale, sumScale, sumScale));
                }

                m_touchDistInit = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            }
            else if (m_touchDistInit != 0f && 0 == Input.touchCount)
            {
                m_interceptClick = false;
                m_touchDistInit = 0f;
            }
        }

        if (m_isAnimation)   //animation
        {
            m_frameCnt++;

            if (m_frameCnt > m_aniFrame)
            {
                m_panel.transform.localScale = m_targetScale;
                CalcMoveBoundary();
                transform.localPosition = GetCheckedPosition(m_targetPosition);
                m_position = transform.localPosition;

                m_interceptClick = false;
                m_isAnimation = false;

                if (null != OnFinishCameraWork)
                {
                    OnFinishCameraWork(m_workIndex);
                }
            }
            else
            {
                m_panelScale.x += (m_targetScale.x - m_panelScale.x) * m_aniSpeed;
                m_panelScale.y += (m_targetScale.y - m_panelScale.y) * m_aniSpeed;

                m_position.x += (m_targetPosition.x - m_position.x) * m_aniSpeed;
                m_position.y += (m_targetPosition.y - m_position.y) * m_aniSpeed;

                m_panel.transform.localScale = m_panelScale;
                CalcMoveBoundary();
                transform.localPosition = GetCheckedPosition(m_position);
                m_position = transform.localPosition;
            }

        }
        else if (m_isPress)  //Drag
        {
            Vector3 pos = Input.mousePosition;

            if (!m_interceptClick) //현재 Drag중인 지 여부. Cell의 click이벤트 호출여부를 결정한다.
            {
                Vector3 posGap = (pos - m_mousePosPress);
                //Debug.Log("posGap : " + posGap);

                if (Mathf.Abs(posGap.x) < 0.5f && Mathf.Abs(posGap.y) < 0.5f)
                {
                    return;
                }
                else
                {
                    m_interceptClick = true;
                }
            }

            CalcMoveBoundary();
            transform.localPosition = GetCheckedPosition(m_position - ((pos - m_mousePosPress) * RotationManager.Instance.PIXEL_GAIN));

            m_tracker.Enqueue(pos);

            if (m_tracker.Count > COUNT_TRACKER)
            {
                m_tracker.Dequeue();
            }
        }
        else if (m_traking) //Drag End moving
        {
            if (1 > Mathf.Abs(m_accelationSpeed.x) && 1 > Mathf.Abs(m_accelationSpeed.y))
            {
                m_traking = false;
                return;
            }

            m_accelationSpeed.x *= 0.9f;
            m_accelationSpeed.y *= 0.9f;

            CalcMoveBoundary();
            transform.localPosition = GetCheckedPosition(transform.localPosition - (m_accelationSpeed * RotationManager.Instance.PIXEL_GAIN));
            m_position = transform.localPosition;

            return;
        }

    }

    void OnPress(bool _isPress)
    {
        //Debug.Log(name + " OnPress : " + _isPress);
        if (m_enableDragMove)
        {
            if (_isPress)
            {
                m_isPress = _isPress;
                Vector3 pos = Input.mousePosition;
                if (pos.y > m_touchArea.yMax
                    || pos.x > m_touchArea.xMax
                    || pos.x < m_touchArea.xMin || pos.y < m_touchArea.yMin)
                {
                    m_isPress = false;
                    m_interceptClick = true;
                    return;
                }

                m_mousePosPress = Input.mousePosition;
                m_tracker = new Queue();
                m_traking = false;
            }
            else
            {
                m_isPress = _isPress;

                m_position = transform.localPosition;
                m_interceptClick = false;

                if (null == m_tracker || m_tracker.Count <= 0)
                {
                    return;
                }

                int cnt = m_tracker.Count;

                Vector3 prevTracker = (Vector3)m_tracker.Dequeue();
                Vector3 speedSum = new Vector3(0f, 0f, 0f);
                while (m_tracker.Count > 0)
                {
                    Vector3 currTracker = (Vector3)m_tracker.Dequeue();
                    speedSum += currTracker - prevTracker;
                    prevTracker = currTracker;
                }
                m_traking = true;

                m_accelationSpeed = speedSum / cnt;
                //			Debug.Log(name + " m_accelationSpeed : " + m_accelationSpeed);
            }
        }
    }

    public bool IsMoving
    {
        get
        {
            return m_interceptClick;
        }
    }

    public bool IsAnimation
    {
        get
        {
            return m_isAnimation;
        }
    }

    public void SetPanel(MovingPanel _panelGO, float _panelWidth, float _panelHeight)
    {
        m_panel = _panelGO;
        m_panelWidth = _panelWidth;
        m_panelHeight = _panelHeight;
        CalcMoveBoundary();
    }

    //STANDARD 해상도와 othgraphicSize에 따라서 mouse좌표에 대한 보정값(gain)을 결정.
    public void SetPixelGain()
    {
        float w;
        float h;

        if (RotationManager.Instance.isLandscape)
        {
            w = GetComponent<Camera>().pixelWidth > GetComponent<Camera>().pixelHeight ? GetComponent<Camera>().pixelWidth : GetComponent<Camera>().pixelHeight;
            h = GetComponent<Camera>().pixelWidth < GetComponent<Camera>().pixelHeight ? GetComponent<Camera>().pixelWidth : GetComponent<Camera>().pixelHeight;
            m_pixelGain = RotationManager.STANDARD_WIDTH / h * GetComponent<Camera>().orthographicSize;
        }
        else
        {
            w = GetComponent<Camera>().pixelWidth < GetComponent<Camera>().pixelHeight ? GetComponent<Camera>().pixelWidth : GetComponent<Camera>().pixelHeight;
            h = GetComponent<Camera>().pixelWidth > GetComponent<Camera>().pixelHeight ? GetComponent<Camera>().pixelWidth : GetComponent<Camera>().pixelHeight;
            m_pixelGain = RotationManager.STANDARD_WIDTH / w * GetComponent<Camera>().orthographicSize;
        }

        //m_touchRegion = new Rect(120f * (1 / m_pixelGain)
        //    , 0f + (camera.pixelHeight - (camera.pixelWidth / 16f * 9f)) * 0.5f
        //    , 1030f * (1 / m_pixelGain)
        //    , 600f * (1 / m_pixelGain));

        m_touchArea = new Rect(0, 0, w, h);
    }

    //Initial 위치로 camera이동.
    public void MoveToInitial()
    {
        transform.localPosition = new Vector3(0f, 0f, 0f);
        m_position = transform.localPosition;
    }

    public void MoveToPosition(Vector3 _position)
    {
        CalcMoveBoundary();
        transform.localPosition = GetCheckedPosition(_position);
        //transform.localPosition = _position;
        m_position = transform.localPosition;
    }

    public void PanelScaleChanged()
    {
        CalcMoveBoundary();
        transform.localPosition = GetCheckedPosition(transform.localPosition - (m_accelationSpeed * RotationManager.Instance.PIXEL_GAIN));
        m_position = transform.localPosition;
    }

    //Camera가 움직일 수 있는 baundary를 계산함.
    
    private void CalcMoveBoundary()
    {
        Vector3 panelScale = m_panel.transform.localScale;
        float panelWidthHalf = (m_panelWidth * 0.5f) * panelScale.x;
        float panelHeightHalf = (m_panelHeight * 0.5f) * panelScale.y;

        screenWidthHalf = RotationManager.Instance.uirootWidth * 0.5f;
        screenHeightHalf = RotationManager.Instance.uirootHeight * 0.5f;

        m_moveBoundary = new Rect();

        m_moveBoundary.x = -panelWidthHalf + screenWidthHalf;
        if (m_moveBoundary.x >= 0)
        {
            m_moveBoundary.x = 0;
            m_moveBoundary.width = 0;
        }
        else
        {
            m_moveBoundary.width = (panelWidthHalf - screenWidthHalf) * 2.0f;
        }

        m_moveBoundary.y = -panelHeightHalf + screenHeightHalf;
        if (m_moveBoundary.y >= 0)
        {
            m_moveBoundary.y = 0;
            m_moveBoundary.height = 0;
        }
        else
        {
            m_moveBoundary.height = (panelHeightHalf - screenHeightHalf) * 2.0f;
        }
    }

    //boundary를 벗어나지 않는지 check
    private Vector3 GetCheckedPosition(Vector3 _position)
    {
        _position.x = _position.x < m_moveBoundary.xMin ? m_moveBoundary.xMin : _position.x;
        _position.x = _position.x > m_moveBoundary.xMax ? m_moveBoundary.xMax : _position.x;

        _position.y = _position.y < m_moveBoundary.yMin ? m_moveBoundary.yMin : _position.y;
        _position.y = _position.y > m_moveBoundary.yMax ? m_moveBoundary.yMax : _position.y;

        return _position;
    }

    //Camera work 함수
    //_workName : OnFinishCameraWork 함수에서 사용할 key
    //_scale : target scale
    //_position : target position
    //_endFrame : animation frame
    //_speed : aniamtion speed
    //_finishListener : animation fisish listener
    public void CameraWork(int _workIndex, Vector3 _scale, Vector3 _position, int _endFrame = 60, float _speed = 0.1f, CameraWorkListener _finishListener = null)
    {
        m_workIndex = _workIndex;

        _position.x *= _scale.x;
        _position.y *= _scale.y;
        //_position.z *= _scale.z;

        OnFinishCameraWork = _finishListener;

        if (_scale.x != m_panel.transform.localScale.x
            || _scale.y != m_panel.transform.localScale.y
            || _position.x != transform.localPosition.x
            || _position.y != transform.localPosition.y)
        {
            m_interceptClick = true;
            m_isAnimation = true;
            m_targetPosition = _position;
            m_targetScale = _scale;
        }
        else
        {
            if (null != OnFinishCameraWork)
            {
                OnFinishCameraWork(_workIndex);
            }
        }


        m_aniFrame = _endFrame;
        m_frameCnt = 0;
        m_aniSpeed = _speed;
    }

    float m_shakeAmountStart;
    float m_shakeDelay;
    public void CameraShaking(int _shakeCount, float _shakeAmount, float _shakeDelay = 0.0f)
    {
        m_posOrigin = transform.localPosition;
        m_shakeCount = _shakeCount;
        m_shakeAmountStart = _shakeAmount;
        m_shakeAmount = _shakeAmount;
        m_shakeDelay = _shakeDelay;
        StartCoroutine("CoroutineCameraShaking");
    }

    IEnumerator CoroutineCameraShaking()
    {
        yield return new WaitForSeconds(m_shakeDelay);
        for (int i = 0; i < m_shakeCount; i++)
        {
            transform.localPosition = m_posOrigin + Random.insideUnitSphere * m_shakeAmount;
            m_shakeAmount -= m_shakeAmountStart / (float)m_shakeCount;
            //transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
            yield return new WaitForSeconds(0.042f - (0.021f * ((float)i / (float)m_shakeCount)));
        }
        transform.localPosition = m_posOrigin;
    }

    public void MoveTo(Vector3 _position,  float _speed)
    {
        _position.x *= m_panel.transform.localScale.x;
        _position.y *= m_panel.transform.localScale.y;

        m_position.x += (_position.x - m_position.x) * _speed;
        m_position.y += (_position.y - m_position.y) * _speed;

        transform.localPosition = GetCheckedPosition(m_position);
        m_position = transform.localPosition;
    }
}
