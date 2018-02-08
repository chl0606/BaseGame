using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * StackList main 클래스
 * 
 * 배경, 리스트, 스크롤 바로 이루어져 있으며, 
 * 배경과 스크롤바는 생성하지 않아도 동작에는 문제가 없다.
 * StackList는 크기가 정해지지 않아서 무한 리스트 사용시에 itemData의 Size는 모두 값이 세팅되어야 하며, Count와 Capacity비교나 null체크가 아닌 다른 방법을 사용하여 데이터 유효성체크를해야 한다.
 */

/* EXAMPLE
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
                data.size = 130 + (i * 10);
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
            data.size = 130 + (i * 10);
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
    }

    private void OnListItemUpdated(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemUpdated " + ((GiftItemData)listItem.getData()).index);
    }

    private void OnListItemClicked(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemClicked " + ((GiftItemData)listItem.getData()).index);
    }
     */
public class StackListView : ListView
{
    protected float[] sizeSumArray;
    protected int topIndex;

    protected override void CalcLimitDistance()
    {
        limitDistance = 0f;
        sizeSumArray = new float[itemDatas.Capacity];

        for (int i = 0; i < itemDatas.Capacity; i++)
        {
            sizeSumArray[i] = limitDistance;
            limitDistance += itemDatas[i].size;
        }

        FindTopIndex();

        if (property.isVertical)
        {
            limitDistance -= property.listHeight;
        }
        else
        {
            limitDistance -= property.listWidth;
        }
        limitDistance = limitDistance < 0 ? 0 : limitDistance;
    }

    public override void SetTotalDistance(float value)
    {
        totalDistance = value;
    }

    protected void FindTopIndex()
    {
        //Find topIndex : 최상단 ListItem의 index를 찾는다
        for (int i = 1; i < sizeSumArray.Length; i++)
        {
            if (totalDistance <= sizeSumArray[i])
            {
                topIndex = i - 1;
                break;
            }
        }
    }

    public override void CreateList<T>(GameObject _resPrefab, ListProperties _listProperties, List<ListItemData> _itemDatas, UIScrollBar _uiScrollbar = null) 
    {
        SetListProperties(_listProperties);

        SetItemDatas(_itemDatas);

        //posx = 0.0f;
        topPos = property.itemTopPosition - property.distanceOffset;
        totalDistance = -property.distanceOffset;
        topIndex = 0;

        listItems = new List<ListItem>();
        ListItem item;
        Vector3 localScale;
        for (int i = 0; i < property.max_lineCount; i++)
        {
            GameObject goItem = Instantiate(_resPrefab);
            item = goItem.GetComponent<T>();
            if (null == item)
                item = goItem.AddComponent<T>();

            listItems.Add(item);
            if (i < itemDatas.Count)
            {
                item.OnItemAdd(itemDatas[i], this);
                if (null != onItemAdded)
                {
                    onItemAdded(item);
                }

                localScale = item.transform.localScale;
                item.transform.parent = transform;

                item.transform.localScale = localScale;
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

    public override void Redraw()
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
        topIndex = 0;
        UpdateItemPosition();

        MoveToDistance(prevTotalDistance);

        //ListView와 ListItem간의 Collider Depth가 꼬이는 현상방지
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    protected override void Jump()
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
                if (topPos > itemDatas[topIndex].size + property.itemTopPosition && speedAvg > 0.0f)
                {
                    while (topPos > itemDatas[topIndex].size + property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(true, itemDatas[topIndex].size);
                        if (doRenew)
                        {
                            IncreaseTopIndex();
                        }
                    }
                }
                else if (topPos < property.itemTopPosition && speedAvg < 0.0f)
                {
                    while (0 < topIndex && topPos < property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(false, itemDatas[topIndex - 1].size);
                        if (doRenew)
                        {
                            DecreaseTopIndex();
                        }
                    }
                }
            }
            else    //Horizontal List
            {
                if (topPos < -itemDatas[topIndex].size + property.itemTopPosition && speedAvg < 0.0f)
                {
                    while (topPos < -itemDatas[topIndex].size + property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(true, -itemDatas[topIndex].size);
                        if (doRenew)
                        {
                            IncreaseTopIndex();
                        }
                    }
                }
                else if (topPos > property.itemTopPosition && speedAvg > 0.0f)
                {
                    while (0 < topIndex && topPos > property.itemTopPosition && doRenew)
                    {
                        doRenew = RenewItem(false, -itemDatas[topIndex - 1].size);
                        if (doRenew)
                        {
                            DecreaseTopIndex();
                        }
                    }
                }
            }

            //FindTopIndex();

            UpdateItemPosition();

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

    protected void IncreaseTopIndex()
    {
        topIndex++;
        if (itemDatas.Count <= topIndex)
            topIndex = itemDatas.Count - 1;
    }

    protected void DecreaseTopIndex()
    {
        topIndex--;
        if (0 > topIndex)
            topIndex = 0;
    }

    protected override void UpdateItemPosition()
    {
        ListItem item;
        float itemPos = topPos;
        for (int i = 0; i < listItems.Count; i++)
        {
            item = listItems[i];

            float dis = 0;
            if (0 < i && null != item.GetData())
            {
                dis = (listItems[i - 1].GetData().size + item.GetData().size) * 0.5f;
            }

            Vector3 localPos = new Vector3(0, 0, property.itemPosZ);
            if (property.isVertical)
            {
                itemPos -= dis;
                localPos.y = itemPos;
            }
            else
            {
                itemPos += dis;
                localPos.x = itemPos;
            }
            item.transform.localPosition = localPos;
        }
    }

}

