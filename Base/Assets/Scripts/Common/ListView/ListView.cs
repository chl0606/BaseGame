using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * List main 클래스
 * 
 * 배경, 리스트, 스크롤 바로 이루어져 있으며, 
 * 배경과 스크롤바는 생성하지 않아도 동작에는 문제가 없다.
 */

/* EXAMPLE (GiftBox example)
    private ListView _listView;
    private void SetListView(List<MailData> mailDataList)
    {
        isUpdateGift = false;

        if (0 >= mailDataList.Count)
        {
            if (null != _listView)
            {
                _listView.RemoveAllChild();
                Destroy(_listView);
                _listView = null;
            }
            return;
        }

        List<ListItemData> itemDatas;
        if (null != _listView)
        {
            itemDatas = _listView.GetItemDatas();
            itemDatas.Clear();
            itemDatas.Capacity = mailDataList.Count;
            for (int i = 0; i < mailDataList.Count; i++)
            {
                GiftItemData data = new GiftItemData(i, mailDataList[i]);
                itemDatas.Add(data);
            }

            _listView.RefreshDatas(itemDatas);
            return;
        }

        // * Add ListView 
        _listView = listViewGameObject.AddComponent<ListView>();

        _listView.onItemAdded = OnListItemAdded;
        _listView.onItemUpdated = OnListItemUpdated;
        _listView.onItemClicked = OnListItemClicked;

        // * Property Setting *
        ListProperties listProperties = new ListProperties();
        listProperties.isVertical = true;
        listProperties.max_lineCount = 5;
        //listProperties.max_gridCount = 5; //Use Only Grid

        listProperties.listWidth = 740;
        listProperties.listHeight = 500;
        listProperties.itemWidth = 740;
        listProperties.itemHeight = 130;
        listProperties.itemPosZ = 0f;
        listProperties.itemTopPosition = 185;
        //listProperties.distanceOffset = 500 - (1 * listProperties.itemHeight);

        // * Item Data Setting *
        itemDatas = new List<ListItemData>(mailDataList.Count);
        for (int i = 0; i < mailDataList.Count; i++)
        {
            GiftItemData data = new GiftItemData(i, mailDataList[i]);
            itemDatas.Add(data);
        }

        // * Create ListView *
        //m_listView.CreateBg("prefab/bg");
        _listView.CreateList<GiftListItem>(giftItemPrefab, listProperties, itemDatas, scrollbar);
        //m_listView.CreateScrollbar("prefab/scrollbar_h"
        //    , new Rect(2, -listProperties.listHeight + 12, listProperties.listWidth - 4, 10));
        //m_listView.CreateCue("UI/Panel/Component/arrowLeft", new Vector3(-40, -28, m_zPos - 4f)
        //    , "UI/Panel/Component/arrowRight", new Vector3(listProperties.listWidth + 8, -28, m_zPos - 4f));

        // * Change Clipping Area 
        //m_listView.ChangeClippingArea(new Vector4(100, -100, 500, 500));
    }

    private void OnListItemAdded(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemAdded " + ((GiftItemData)listItem.getData()).index);
        ((GiftListItem)listItem).OnGiftOK = OnGiftOK;
    }

    private void OnListItemUpdated(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemUpdated " + ((GiftItemData)listItem.getData()).index);
        //if (_item.getData().index == initLoadDataCount - 1
        //    && m_listView.GetItemDatas().Count < m_listView.GetItemDatas().Capacity)
        //{
        //    List<ListItemData> itemDatas = m_listView.GetItemDatas();
        //    for (int i = initLoadDataCount; i < totalDataCount; i++)
        //    {
        //        GiftItemData data = new GiftItemData(i, "Item " + i);
        //        itemDatas.Add(data);
        //    }
        //}
    }

    private void OnListItemClicked(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemClicked " + ((GiftItemData)listItem.getData()).index);
    }
     */

public class ListView : MonoBehaviour
{
    protected const float FLICKTION = 0.35f; //60fps기준
    protected const float TENSION = 0.15f; //60fps기준
    protected const float DELTATIME_MIN = 0.0167f; //60fps기준값
    protected const float SCROLLBAR_SIZE_MIN = 0.1f;
    protected static float PIXEL_GAIN;
    public const float LONGPRESS_TIME_END = 1.0f;
    public const float LONGPRESS_TIME_START = 0.2f;

    protected ListProperties property;
    protected List<ListItemData> itemDatas;
    protected List<ListItem> listItems;
    protected UIPanel uiPanel;
    //protected BoxCollider listViewCollider;

    protected ListItem pressedItem;   //press된 item
    protected bool sendItemClick;     //clikc 이벤트를 처리할 지 여부

    protected float topPos;           //맨앞에 위치한 item의 좌표

    protected float totalDistance;    //현재 scroll된 거리
    protected float limitDistance;    //scroll될 limit 거리

    protected bool isPress;           //press 여부
    protected bool isMouseMove;       //mouses move로 인식하는지 여부
    protected float pressedPos;       //press된 좌표
    protected float movedPos;         //움직인 좌표
    protected float speedAvg;         //scroll speed

    protected Queue tracker;	//position tracking Queue
    protected const int COUNT_TRACKER = 10;
    protected bool isTraking = false; //is tracking

    protected float longpressTime;
    protected bool sendLongpressStart;

    protected float deltaTime;

    /* Legacy Scrollbar
    protected GameObject scrollbar;   
    protected Rect scrollArea;        //scrollbar영역
    protected float scrollDistance;   //scrollbar가 움직일 limit 거리
    */

    protected UIScrollBar uiScrollbar;

    protected GameObject topCue;
    protected GameObject botCue;

    //ListView Move Start
    public delegate void MoveStartListener(float _moveDistance);
    public MoveStartListener onMoveStart;

    //ListItem Init Add
    public delegate void ItemAddedListener(ListItem _item);
    public ItemAddedListener onItemAdded;

    //ListItem Update
    public delegate void ItemUpdateListener(ListItem _item);
    /// <summary>
    /// THis listener is called when an item is updated.
    /// </summary>
    public ItemUpdateListener onItemUpdated;

    //ListItem Click
    public delegate void ItemClickedListener(ListItem _item);
    public ItemClickedListener onItemClicked;

    //ListItem Press down/up
    public delegate void ItemPressedListener(bool _isPress, ListItem _item);
    public ItemPressedListener onItemPressed;

    //ListItem LongPress Start
    public delegate void ItemLongPressStartListener(ListItem _item);
    public ItemLongPressStartListener onItemLongPressStart;

    //ListItem LongPressed
    public delegate void ItemLongPressedListener(ListItem _item);
    public ItemLongPressedListener onItemLongPressed;

    public bool hold = false;

    void OnEnable()
    {
        GameObject uiRoot2d = GameObject.Find("UI Root (2D)");
        if (null != uiRoot2d)
        {
            PIXEL_GAIN = uiRoot2d.GetComponent<UIRoot>().manualHeight / (float)Screen.width;
        }
        isPress = false;
        isMouseMove = false;
        sendItemClick = false;
    }

    void Update()
    {
        deltaTime = Time.deltaTime;

        if (hold) return;

        if (isPress && LONGPRESS_TIME_END > longpressTime)
        {
            longpressTime += deltaTime;

            if (sendLongpressStart && LONGPRESS_TIME_START < longpressTime)
            {
                sendItemClick = false;
                sendLongpressStart = false;
                if (null != pressedItem && null != onItemLongPressStart)
                {
                    onItemLongPressStart(pressedItem);
                }
            }

            if (LONGPRESS_TIME_END <= longpressTime)
            {
                if (null != pressedItem && null != onItemLongPressed)
                {
                    onItemLongPressed(pressedItem);
                }
            }
        }

        if (!isMouseMove && isPress
            && 5.0f < Mathf.Abs(property.isVertical
            ? PIXEL_GAIN * (Input.mousePosition.y - pressedPos)
            : PIXEL_GAIN * (Input.mousePosition.x - pressedPos)))
        {
            isMouseMove = true;
            sendItemClick = false;
            longpressTime = LONGPRESS_TIME_END;
            sendLongpressStart = false;

            if (null != pressedItem && null != onItemUpdated)
            {
                onItemUpdated(pressedItem);
            }

            if (null != onMoveStart)
                onMoveStart(totalDistance);
        }

        Jump();
    }

    void OnPress(bool _isPress)
    {
        OnPressProcess(_isPress, null);
    }

    protected void OnPressProcess(bool _isPress, ListItem _item)
    {
        if (_isPress)
        {
            //Debug.Log("MouseDown");
            pressedItem = _item;
            isPress = true;
            isMouseMove = false;
            sendItemClick = true;

            pressedPos = 0.0f;
            movedPos = 0.0f;
            speedAvg = 0.0f;

            tracker = new Queue();
            isTraking = false;

            pressedPos = property.isVertical ? Input.mousePosition.y : Input.mousePosition.x;
            movedPos = pressedPos;

            longpressTime = 0f;
            sendLongpressStart = true;
        }
        else
        {
            //Debug.Log("MouseUp");
            pressedItem = null;
            isPress = false;
            isMouseMove = false;
            longpressTime = LONGPRESS_TIME_END;
            sendLongpressStart = false;

            if (null != tracker && tracker.Count > 0)
            {
                isTraking = true;

                int cnt = tracker.Count;
                float speedSum = 0.0f;
                while (tracker.Count > 0)
                {
                    speedSum += (float)tracker.Dequeue();
                }

                speedAvg = speedSum / (float)cnt;
            }
            else if (0.0f > totalDistance || limitDistance < totalDistance)
            {
                isTraking = true;
            }
        }
    }

    public void OnItemPress(bool _isPress, ListItem _item)
    {
        OnPressProcess(_isPress, _item);

        if (null != onItemPressed)
        {
            onItemPressed(_isPress, _item);
        }
    }

    public void OnItemClick(ListItem _item)
    {
        //Debug.Log(sendItemClick);
        if (sendItemClick)
        {
            _item.OnItemClick();

            if (null != onItemClicked)
            {
                onItemClicked(_item);
            }
        }
    }

    /// <summary>
    /// Set Essential properties for ListView.
    /// </summary>
    protected void SetListProperties(ListProperties _listProperties)
    {
        property = _listProperties;
    }

    public ListProperties GetListProperty()
    {
        return property;
    }

    protected virtual void CalcLimitDistance()
    {
        if (property.isVertical)
        {
            limitDistance = property.itemHeight * itemDatas.Capacity - property.listHeight;
        }
        else
        {
            limitDistance = property.itemWidth * itemDatas.Capacity - property.listWidth;
        }
        limitDistance = limitDistance < 0 ? 0 : limitDistance;
        //Debug.Log("limitDistance ::: " + limitDistance);
    }

    protected virtual void SetItemDatas(List<ListItemData> _itemDatas)
    {
        itemDatas = _itemDatas;

        CalcLimitDistance();
    }

    public void SetItemData(ListItemData _item)
    {
        itemDatas.Add(_item);
    }

    public List<ListItemData> GetItemDatas()
    {
        return itemDatas;
    }

    public ListItemData GetItemData(int _index)
    {
        if (_index >= 0 && _index < itemDatas.Count)
            return itemDatas[_index];
        else
            return null;
    }

    public List<ListItem> GetItems()
    {
        return listItems;
    }

    public ListItem GetItem(int _index)
    {
        ListItem item = null;

        for (int i = 0; i < listItems.Count; i++)
        {
            if (listItems[i].GetData() == null) continue;

            if (listItems[i].GetData().index == _index)
            {
                item = listItems[i];
                break;
            }
        }
        return item;
    }

    public float GetTotalDistance()
    {
        return totalDistance;
    }

    public virtual void SetTotalDistance(float value)
    {
        totalDistance = value;
    }

    public float GetLimitDistance()
    {
        return limitDistance;
    }

    public virtual void CreateList<T>(GameObject _resPrefab, ListProperties _listProperties, List<ListItemData> _itemDatas, UIScrollBar _uiScrollbar = null) where T : ListItem
    {
        SetListProperties(_listProperties);

        SetItemDatas(_itemDatas);

        //posx = 0.0f;
        topPos = property.itemTopPosition - property.distanceOffset;
        totalDistance = -property.distanceOffset;

        listItems = new List<ListItem>();
        ListItem item;
        Vector3 localScale;
        for (int i = 0; i < property.max_lineCount; i++)
        {
            GameObject goItem = Instantiate(_resPrefab);
            item = goItem.GetComponent<T>();
            if (null == item)
                item = goItem.AddComponent<T>();

            localScale = item.transform.localScale;

            item.transform.parent = transform;

            item.transform.localScale = localScale;

            listItems.Add(item);
            if (i < itemDatas.Count)
            {
                item.OnItemAdd(itemDatas[i], this);
                if (null != onItemAdded)
                {
                    onItemAdded(item);
                }
            }
            else
            {
                item.OnItemAdd(null, this);
                Active(item.gameObject, false);
            }
        }

        UpdateItemPosition();

        if (null != _uiScrollbar)
        {
            uiScrollbar = _uiScrollbar;
            uiScrollbar.value = 0;
            float screenSize = property.isVertical ? property.listHeight : property.listWidth;
            uiScrollbar.barSize = screenSize / (screenSize + limitDistance);
            uiScrollbar.barSize = SCROLLBAR_SIZE_MIN > uiScrollbar.barSize ? SCROLLBAR_SIZE_MIN : uiScrollbar.barSize;
        }

        //ChangeClippingArea(new Vector4(property.listWidth * 0.5f, -property.listHeight * 0.5f, property.listWidth, property.listHeight));

        //ListView와 ListItem간의 Collider Depth가 꼬이는 현상방지
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    /* Legacy Scrollbar
    public void CreateScrollbar(GameObject _resPrefab, Rect _area)
    {
        scrollArea = _area;
        GameObject go = Instantiate(_resPrefab) as GameObject;
        go.transform.parent = transform;

        scrollbar = go.transform.FindChild("scroll").gameObject;
        //Vector3 localPos = new Vector3(_area.x, _area.y, 0);
        Vector3 localScale = new Vector3(1, 1, 1);

        scrollbar.transform.localPosition = Vector3.up * _area.y;
        go.transform.localPosition = Vector3.right * _area.x;
        go.transform.localScale = localScale;


        Transform bodyT = scrollbar.transform.FindChild("body");
        UISprite sprBack = go.transform.FindChild("back").GetComponent<UISprite>();

        int scrollbarSize;
        if (property.isVertical)
        {
            scrollbarSize = (int)(property.listHeight / (limitDistance + property.listHeight) * _area.height);
            //minimum size
            scrollbarSize = scrollbarSize < 20 ? 20 : scrollbarSize;

            sprBack.transform.localPosition = Vector3.down * _area.height / 2;
            sprBack.height = (int)_area.height;
            scrollDistance = _area.height;
        }
        else
        {
            scrollbarSize = (int)(property.listWidth / (limitDistance + property.listWidth) * _area.width);
            //minimum size
            scrollbarSize = scrollbarSize < 20 ? 20 : scrollbarSize;

            Vector3 bodyScale = bodyT.localScale;
            bodyScale.x = scrollbarSize;// +topT.localScale.x + botT.localScale.x;
            bodyT.localScale = bodyScale;

            scrollDistance = _area.width - bodyT.localScale.x;
        }

        if (limitDistance <= 0)
        {
            go.SetActive(false);
        }
    }
    */

    /// <summary>
    /// Create Scroll cue.
    /// [_topPath : Top cue prefab path,
    /// _topPos : Top cue localPosition,
    /// _botPath : Bottom cue prefab path,
    /// _botPos : Bottom cue localPosition]
    /// </summary>
    public void CreateCue(GameObject _resPrefabTop, Vector3 _topPos, GameObject _resPrefabBot, Vector3 _botPos, Transform _parent = null)
    {
        //topCue
        topCue = Instantiate(_resPrefabTop) as GameObject;
        Vector3 localScale = topCue.transform.localScale;

        topCue.transform.parent = null == _parent ? transform : _parent;

        topCue.transform.localPosition = _topPos;
        topCue.transform.localScale = localScale;

        //botCue
        botCue = Instantiate(_resPrefabBot) as GameObject;
        localScale = botCue.transform.localScale;

        botCue.transform.parent = null == _parent ? transform : _parent;

        botCue.transform.localPosition = _botPos;
        botCue.transform.localScale = localScale;

        SetCueAlpha();
    }

    public void RemoveCue()
    {
        GameObject.DestroyImmediate(topCue);
        topCue = null;
        GameObject.DestroyImmediate(botCue);
        botCue = null;
    }

    /// <summary>
    /// Change Clipping area to _area.
    /// [Vector4 _area : (centerX, centerY, with, height)]
    /// </summary>
    public void ChangeClippingArea(Vector4 _area, int _uiPanelDepth)
    {
        if (null == uiPanel)
        {
            uiPanel = GetComponent<UIPanel>();
            if (null == uiPanel)
            {
                uiPanel = gameObject.AddComponent<UIPanel>();
            }
        }

        uiPanel.clipping = UIDrawCall.Clipping.SoftClip;
        uiPanel.baseClipRegion = _area;
        uiPanel.depth = _uiPanelDepth;

        //if (!listViewCollider) listViewCollider = gameObject.AddComponent<BoxCollider>();
        //listViewCollider.center = new Vector3(_area.x, _area.y, 100);
        //listViewCollider.size = new Vector3(_area.z, _area.w, 1);
        //listViewCollider.gameObject.SetActive(false);
        //listViewCollider.gameObject.SetActive(true);
        //uiPanel.clipRange = _area;
    }

    public void RemoveItemDataAt(int _index)
    {
        itemDatas.RemoveAt(_index);
        Redraw();
    }

    public void RefreshDatas(List<ListItemData> _itemDatas)
    {
        SetItemDatas(_itemDatas);
        Redraw();
    }

    public virtual void Redraw()
    {
        CalcLimitDistance();

        float prevTotalDistance = totalDistance;

        for (int i = 0; i < listItems.Count; i++)
        {
            ListItem item = listItems[i];
            if (i < itemDatas.Count)
            {
                Active(item.gameObject, true);
                item.OnItemUpdate(itemDatas[i]);

                if (null != onItemUpdated)
                {
                    onItemUpdated(item);
                }
            }
            else
            {
                item.SetData(null);
                Active(item.gameObject, false);
            }
        }

        topPos = property.itemTopPosition - property.distanceOffset;
        totalDistance = -property.distanceOffset;
        MoveToDistance(prevTotalDistance);

        //ListView와 ListItem간의 Collider Depth가 꼬이는 현상방지
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void MoveToDistance(float _distance)
    {
        speedAvg = _distance;
        CalcTotalDistance();
        //Jump(true);

        speedAvg = 0.11f;
        isTraking = true;
        Jump();
    }

    public void RemoveAllChild()
    {
        for (int i = 0; i < listItems.Count; i++)
        {
            if (null != listItems[i])
            {
                // RegisterUIObject 호출 할 때 객체가 바로 않 지워 지기 때문에 transform.Find로 이 지워질 객체가 찾아지는 경우가 있어서 이름을 바꿔 놓는다.
                listItems[i].gameObject.name += "_Deleted";
                Destroy(listItems[i].gameObject);
            }
        }
        listItems.Clear();
    }

    protected virtual void Jump()
    {
        //if (GameSystem.Instance.IsTutorial()) return;

        bool posChanged = false;
        if (isMouseMove)
        {
            //if (limitDistance > 0)
            //{
                posChanged = true;
                movedPos = property.isVertical ? Input.mousePosition.y : Input.mousePosition.x;
                speedAvg = movedPos - pressedPos;
                speedAvg *= PIXEL_GAIN;

                if (null != tracker)
                {
                    tracker.Enqueue(speedAvg);
                    while (tracker.Count > COUNT_TRACKER)
                    {
                        tracker.Dequeue();
                    }
                }

                CalcTotalDistance();

                pressedPos = movedPos;
            //}
        }
        else if (isTraking)
        {
            if (0.1f < Mathf.Abs(speedAvg) || 0.0f > totalDistance || limitDistance < totalDistance)
            {
                posChanged = true;
                float accel = FLICKTION * (DELTATIME_MIN / deltaTime);
                speedAvg *= 0.5f + (FLICKTION > accel ? accel : FLICKTION);

                CalcTotalDistance();

                if (0.0f > totalDistance && -0.1f < totalDistance)
                {
                    totalDistance = 0.0f;
                    isTraking = false;
                }
                else if (limitDistance < totalDistance && limitDistance + 0.1f > totalDistance)
                {
                    totalDistance = limitDistance;
                    isTraking = false;
                }
            }
            else
            {
                isTraking = false;
            }
        }

        if (posChanged)
        {
            //next item 처리
            bool doRenew = true;
            if (property.isVertical)    //Vertical List
            {
                if (topPos > property.itemHeight + property.itemTopPosition && speedAvg > 0.0f)
                {
                    while (topPos > property.itemHeight + property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(true, property.itemHeight);
                    }
                }
                else if (topPos < property.itemTopPosition && speedAvg < 0.0f)
                {
                    while (topPos < property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(false, property.itemHeight);
                    }
                }
            }
            else    //Horizontal List
            {
                if (topPos < -property.itemWidth + property.itemTopPosition && speedAvg < 0.0f)
                {
                    while (topPos < -property.itemWidth + property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(true, -property.itemWidth);
                    }
                }
                else if (topPos > property.itemTopPosition && speedAvg > 0.0f)
                {
                    while (topPos > property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(false, -property.itemWidth);
                    }
                }
            }

            UpdateItemPosition();

            /* Legacy Scrollbar
            if (null != scrollbar && scrollDistance > 0)
            {
                Vector3 localPos = scrollbar.transform.localPosition;

                if (property.isVertical)
                {
                    localPos.y = scrollArea.y - (totalDistance / limitDistance * scrollDistance);
                    localPos.y = localPos.y > scrollArea.y ? scrollArea.y : localPos.y;
                    localPos.y = localPos.y < scrollArea.y - scrollDistance ? scrollArea.y - scrollDistance : localPos.y;
                    scrollbar.transform.localPosition = localPos;
                }
                else
                {
                    localPos.x = scrollArea.x + (totalDistance / limitDistance * scrollDistance);
                    localPos.x = localPos.x < scrollArea.x ? scrollArea.x : localPos.x;
                    localPos.x = localPos.x > scrollArea.x + scrollDistance ? scrollArea.x + scrollDistance : localPos.x;
                    scrollbar.transform.localPosition = localPos;
                }
            }
            */

            if (null != uiScrollbar)
            {
                float screenSize = property.isVertical ? property.listHeight : property.listWidth;
                uiScrollbar.barSize = screenSize / (screenSize + limitDistance);
                uiScrollbar.barSize = SCROLLBAR_SIZE_MIN > uiScrollbar.barSize ? SCROLLBAR_SIZE_MIN : uiScrollbar.barSize;

                float scrollValue;

                if (0 > totalDistance)
                {
                    scrollValue = 0;
                    uiScrollbar.barSize *= 1f - (Mathf.Abs(totalDistance) / screenSize);
                }
                else if (limitDistance < totalDistance)
                {
                    scrollValue = 1;
                    uiScrollbar.barSize *= 1f - (Mathf.Abs(totalDistance - limitDistance) / screenSize);
                }
                else
                {
                    scrollValue = totalDistance / limitDistance;
                }

                uiScrollbar.value = scrollValue;
            }

            if (null != topCue && null != botCue)
            {
                SetCueAlpha();
            }
        }
    }

    protected virtual bool RenewItem(bool _flickForward, float _itemDitance)
    {
        if (_flickForward)
        {
            ListItem item = listItems[0];
            int targetIdx = itemDatas.Count;
            int idx = listItems.Count - 1;
            while (0 <= idx)
            {
                if (null != listItems[idx].GetData())
                {
                    targetIdx = listItems[idx].GetData().index + 1;
                    idx = 0;
                }
                idx--;
            }

            if (targetIdx < itemDatas.Count)
            {
                //Debug.Log("targetIndex : " + targetIdx);
                topPos -= _itemDitance;
                ListItemData data = itemDatas[targetIdx];
                item.OnItemUpdate(data);

                if (null != onItemUpdated)
                {
                    onItemUpdated(item);
                }
                listItems.Add(item);
                listItems.RemoveAt(0);

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            ListItem item = listItems[listItems.Count - 1];
            int targetIdx = listItems[0].GetData().index - 1;
            if (targetIdx > -1)
            {
                //Debug.Log("targetIndex : " + targetIdx);
                topPos += _itemDitance;
                ListItemData data = itemDatas[targetIdx];
                item.OnItemUpdate(data);

                if (null != onItemUpdated)
                {
                    onItemUpdated(item);
                }
                listItems.RemoveAt(listItems.Count - 1);
                listItems.Insert(0, item);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    protected virtual void UpdateItemPosition()
    {
        ListItem item;
        float itemSize = property.isVertical ? -property.itemHeight : property.itemWidth; ;
        for (int i = 0; i < listItems.Count; i++)
        {
            item = listItems[i];
            float itemPos = topPos + (itemSize * i);

            Vector3 localPos = new Vector3(0, 0, property.itemPosZ);
            if (property.isVertical)
            {
                localPos.y = itemPos;
            }
            else
            {
                localPos.x = itemPos;
            }
            item.transform.localPosition = localPos;
        }
    }

    protected void CalcTotalDistance()
    {
        topPos += speedAvg;

        float currTension = TENSION * (deltaTime / DELTATIME_MIN);
        currTension = TENSION > currTension ? TENSION : (0.5f < currTension ? 0.5f : currTension);

        if (property.isVertical)
        {
            totalDistance += speedAvg;

            if (totalDistance < -property.distanceOffset)
            {
                if (isMouseMove)
                {
                    topPos -= speedAvg * 0.5f;
                    totalDistance -= speedAvg * 0.5f;
                }
                else
                {
                    float tensionDis = (-property.distanceOffset - totalDistance) * currTension;
                    totalDistance += tensionDis;
                    topPos += tensionDis;
                }
            }
            else if (totalDistance > limitDistance)
            {
                if (isMouseMove)
                {
                    topPos -= speedAvg * 0.5f;
                    totalDistance -= speedAvg * 0.5f;
                }
                else
                {
                    float tensionDis = (limitDistance - totalDistance) * currTension;
                    totalDistance += tensionDis;
                    topPos += tensionDis;
                }
            }
        }
        else
        {
            totalDistance -= speedAvg;

            if (totalDistance < -property.distanceOffset)
            {
                if (isMouseMove)
                {
                    topPos -= speedAvg * 0.5f;
                    totalDistance += speedAvg * 0.5f;
                }
                else
                {
                    float tensionDis = (-property.distanceOffset - totalDistance) * currTension;
                    totalDistance += tensionDis;
                    topPos -= tensionDis;
                }
            }
            else if (totalDistance > limitDistance)
            {
                if (isMouseMove)
                {
                    topPos -= speedAvg * 0.5f;
                    totalDistance += speedAvg * 0.5f;
                }
                else
                {
                    float tensionDis = (limitDistance - totalDistance) * currTension;
                    totalDistance += tensionDis;
                    topPos -= tensionDis;
                }
            }
        }
    }

    protected void SetCueAlpha()
    {
        //Debug.Log(limitDistance + " " + totalDistance);
        if (limitDistance <= 0)
        {
            topCue.SetActive(false);
            botCue.SetActive(false);
            return;
        }

        if (totalDistance < property.itemHeight)
        {
            float alpha = totalDistance / (property.itemHeight);
            alpha = alpha < 0 ? 0 : alpha;
            alpha = alpha > 1 ? 1 : alpha;
            //TweenAlpha.Begin(topCue, 0.0f, alpha);
            ChangeAlpha(topCue.transform, alpha);

            if (limitDistance - totalDistance < property.itemHeight)
            {
                alpha = (limitDistance - totalDistance) / (property.itemHeight);
                alpha = alpha < 0 ? 0 : alpha;
                alpha = alpha > 1 ? 1 : alpha;
                //TweenAlpha.Begin(botCue, 0.0f, alpha);
                ChangeAlpha(botCue.transform, alpha);
            }
            else
            {
                //TweenAlpha.Begin(botCue, 0.0f, 1);
                ChangeAlpha(botCue.transform, 1f);
            }
        }
        else if (limitDistance - totalDistance < property.itemHeight)
        {
            float alpha = (limitDistance - totalDistance) / (property.itemHeight);
            alpha = alpha < 0 ? 0 : alpha;
            alpha = alpha > 1 ? 1 : alpha;
            //TweenAlpha.Begin(botCue, 0.0f, alpha);
            ChangeAlpha(botCue.transform, alpha);

            //TweenAlpha.Begin(topCue, 0.0f, 1);
            ChangeAlpha(topCue.transform, 1f);
        }
        else
        {
            //TweenAlpha.Begin(topCue, 0.0f, 1);
            //TweenAlpha.Begin(botCue, 0.0f, 1);
            ChangeAlpha(topCue.transform, 1f);
            ChangeAlpha(botCue.transform, 1f);
        }

        //Debug.Log((limitDistance - totalDistance) + "   " + property.itemHeight);
    }

    #region Functions
    protected void ChangeAlpha(Transform _target, float _alpha, bool _applyImage = true, bool _applyLabel = true)
    {
        if (_applyImage)
        {
            UISprite[] sprites = _target.GetComponentsInChildren<UISprite>();

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].alpha = _alpha;
            }

            UITexture[] textures = _target.GetComponentsInChildren<UITexture>();

            for (int i = 0; i < textures.Length; i++)
            {
                textures[i].alpha = _alpha;
            }
        }

        if (_applyLabel)
        {
            UILabel[] labels = _target.GetComponentsInChildren<UILabel>();

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].alpha = _alpha;
            }
        }
    }

    protected bool Active(GameObject _go, bool _active)
    {
        if (_active != _go.activeSelf)
        {
            _go.SetActive(_active);
            return true;
        }
        return false;
    }
    #endregion

}

