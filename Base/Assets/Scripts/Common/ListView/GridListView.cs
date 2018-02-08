using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * GridList main 클래스
 * 
 * 배경, 리스트, 스크롤 바로 이루어져 있으며, 
 * 배경과 스크롤바는 생성하지 않아도 동작에는 문제가 없다.
 */

/* EXAMPLE (ItemInventory example)
    private GridListView _listView;

    private void SetListView(List<ItemData> itemDataList)
    {
        if (null != _listView)
        {
            _listView.RemoveAllChild();
            Destroy(_listView);
            _listView = null;
        }

        if (0 >= itemDataList.Count)
        {
            return;
        }

        List<ListItemData> itemDatas;
        if (null != _listView)
        {
            itemDatas = _listView.GetItemDatas();
            itemDatas.Clear();
            itemDatas.Capacity = itemDataList.Count;
            for (int i = 0; i < itemDataList.Count; i++)
            {
                EtcItemData data = new EtcItemData(i, itemDataList[i]);
                //data.isSelected = _selectItemList.Contains(itemDataList[i].guid);
                itemDatas.Add(data);
            }

            _listView.RefreshDatas(itemDatas);
            return;
        }

        // * Add ListView 
        _listView = _listViewGameObject.AddComponent<GridListView>();
        _listView.onItemAdded = OnListItemAdded;
        _listView.onItemUpdated = OnListItemUpdated;
        _listView.onMoveStart = OnListMoveStart;
        _listView.onItemPressed = OnListItemPressed;
        _listView.onItemLongPressStart = OnListItemLongPressStart;
        _listView.onItemLongPressed = OnListItemLongPressed;
        _listView.onItemClicked = OnListItemClicked;

        // * Property Setting *
        ListProperties listProperties = new ListProperties();
        listProperties.isVertical = true;
        listProperties.max_lineCount = 5;
        listProperties.max_gridCount = 6; //Use Only Grid

        listProperties.listWidth = 1054;
        listProperties.listHeight = 555;
        listProperties.itemWidth = 1054 / 6;
        listProperties.itemHeight = 1054 / 6;
        listProperties.itemPosZ = 0f;
        listProperties.itemTopPosition = (555 * 0.5f) - (listProperties.itemHeight * 0.5f);
        listProperties.offset_gridPosition = -(listProperties.listWidth * 0.5f) + (listProperties.itemWidth * 0.5f);

        // * Item Prefab Setting *
        GameObject itemPrefab = jRes.LoadData<GameObject>(StaticFunction.ETCITEM_PREFAB_PATH);
        //if (null != itemPrefab.GetComponent<UIDragScrollView>())
        //    DestroyImmediate(itemPrefab.GetComponent<UIDragScrollView>(), true);

        // * Item Data Setting *
        itemDatas = new List<ListItemData>(itemDataList.Count);
        for (int i = 0; i < itemDataList.Count; i++)
        {
            EtcItemData data = new EtcItemData(i, itemDataList[i]);
            //data.isSelected = _selectItemList.Contains(equipDataList[i].guid);
            itemDatas.Add(data);
        }

        // * Create ListView *
        _listView.CreateList<EtcListItem>(itemPrefab, listProperties, itemDatas, null);
    }

    private void OnListItemAdded(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemAdded " + ((EtcItemData)listItem.GetData()).index);
        listItem.GetComponent<BoxCollider>().size = new Vector3(_listView.GetListProperty().itemWidth, _listView.GetListProperty().itemHeight, 1);
        OnListItemUpdated(listItem);
    }

    private void OnListItemUpdated(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemUpdatedEtc" + ((EtcItemData)listItem.GetData()).index);
        EtcListItem etcListItem = (EtcListItem)listItem;
        EtcItemData etcItemData = (EtcItemData)etcListItem.GetData();
        UIEtcItemIcon etcIcon = etcListItem.etcIcon;
        ItemData itemData = etcItemData.info;

        etcIcon.itemTexture.mainTexture = null;
        etcIcon.UpdateItemData(itemData);

        etcListItem.iconSelectObject.SetActive(etcItemData.isSelected);

        if (0 < etcItemData.sellCount)
        {
            etcListItem.iconCountLabel.text = etcItemData.sellCount.ToString();
            etcListItem.iconCount.SetActive(true);
        }
        else
        {
            etcListItem.iconCount.SetActive(false);
        }
    }

    private void OnListMoveStart(float moveDistance)
    {
        //GLog.Debug.Log("OnListMoveStart " + moveDistance);
        if (null != ScreenTouchEffect.Instance)
            ScreenTouchEffect.Instance.HideLoaderEffect();
    }

    private void OnListItemPressed(bool isPress, ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemPress " + isPress + " " + ((EtcItemData)listItem.GetData()).index);
        if (!isPress && null != ScreenTouchEffect.Instance)
            ScreenTouchEffect.Instance.HideLoaderEffect();
    }

    private void OnListItemLongPressStart(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemLongPressStart " + ((EtcItemData)listItem.GetData()).index);
        if (null != ScreenTouchEffect.Instance)
            ScreenTouchEffect.Instance.ShowLoaderEffect(listItem.transform.position, ListView.LONGPRESS_TIME_END - ListView.LONGPRESS_TIME_START);
    }

    private void OnListItemLongPressed(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemLongPressed " + ((EtcItemData)listItem.GetData()).index);
        if (null != ScreenTouchEffect.Instance)
            ScreenTouchEffect.Instance.HideLoaderEffect();
        ((EtcListItem)listItem).etcIcon.OnHoldBtn(listItem.gameObject);
    }

    private void OnListItemClicked(ListItem listItem)
    {
        //GLog.Debug.Log("OnListItemClicked " + ((EtcItemData)listItem.GetData()).index);
        EtcListItem etcListItem = (EtcListItem)listItem;
        GameObject blindObj = etcListItem.blindGameObject;
        EtcItemData etcItemData = (EtcItemData)etcListItem.GetData();
        //UIEtcItemIcon etcIcon = etcListItem.etcIcon;
        ItemData itemData = etcItemData.info;

        if (blindObj.activeSelf) return;

        if (_itemMenuType == ItemMenuType.SELECT_SOCKET1 || 
            _itemMenuType == ItemMenuType.SELECT_SOCKET2 ||
            _itemMenuType == ItemMenuType.SELECT_SOCKET3 ||
            _itemMenuType == ItemMenuType.SELECT_SOCKET4 || _itemMenuType == ItemMenuType.SELECT_ETC || _itemMenuType == ItemMenuType.SELECT_BATTLEPOTION)
        {
            SelectItem(listItem);
            return;
        }

        if (itemData.itemCount <= etcItemData.sellCount) return;

        etcItemData.sellCount++;
        _listView.onItemUpdated(listItem);

        if (_selectItemList.ContainsKey(itemData))
            _selectItemList[itemData] = etcItemData.sellCount;
        else
            _selectItemList.Add(itemData, etcItemData.sellCount);

        if (_itemMenuType == ItemMenuType.ETC_SELL)
        {
            WindowEtcInventory windowEtcInventory = WindowManager.instance.GetWindow<WindowEtcInventory>();
            windowEtcInventory.UpdateSellPrice(itemData.GetItemTable().sellPrice_Gold);
        }
    }
     */

public class GridListView : ListView
{
    protected override void CalcLimitDistance()
    {
        if (property.isVertical)
        {
            limitDistance = property.itemHeight * Mathf.Ceil((float)itemDatas.Capacity / (float)property.max_gridCount) - property.listHeight;
        }
        else
        {
            limitDistance = property.itemWidth * Mathf.Ceil((float)itemDatas.Capacity / (float)property.max_gridCount) - property.listWidth;
        }
        limitDistance = limitDistance < 0 ? 0 : limitDistance;
        //Debug.Log("limitDistance ::: " + limitDistance);
    }

    protected override void SetItemDatas(List<ListItemData> _itemDatas)
    {
        itemDatas = _itemDatas;

        CalcLimitDistance();
    }

    public override void CreateList<T>(GameObject _resPrefab, ListProperties _listProperties, List<ListItemData> _itemDatas, UIScrollBar _uiScrollbar = null)
    {
        SetListProperties(_listProperties);

        SetItemDatas(_itemDatas);

        //posx = 0.0f;
        topPos = property.itemTopPosition;
        totalDistance = 0.0f;

        listItems = new List<ListItem>();
        ListItem item;
        float itemDis = property.isVertical ? property.itemWidth : -property.itemHeight;
        float itemSize = property.isVertical ? -property.itemHeight : property.itemWidth;
        Vector3 localPos, localScale;
        for (int i = 0; i < (property.max_lineCount * property.max_gridCount); i++)
        {
            GameObject goItem = Instantiate(_resPrefab);
            item = goItem.GetComponent<T>();
            if (null == item)
                item = goItem.AddComponent<T>();

            if (property.isVertical)
            {
                localPos = new Vector3(itemDis * (i % property.max_gridCount) + property.offset_gridPosition
                    , (itemSize * (i / property.max_gridCount)) + property.itemTopPosition, property.itemPosZ);
            }
            else
            {
                localPos = new Vector3((itemSize * (i / property.max_gridCount)) + property.itemTopPosition
                    , itemDis * (i % property.max_gridCount) + +property.offset_gridPosition, property.itemPosZ);
            }
            localScale = item.transform.localScale;

            item.transform.parent = transform;

            item.transform.localPosition = localPos;
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

        if (null != _uiScrollbar)
        {
            uiScrollbar = _uiScrollbar;
            uiScrollbar.value = 0;
            float screenSize = property.isVertical ? property.listHeight : property.listWidth;
            uiScrollbar.barSize = screenSize / (screenSize + limitDistance);
            uiScrollbar.barSize = SCROLLBAR_SIZE_MIN > uiScrollbar.barSize ? SCROLLBAR_SIZE_MIN : uiScrollbar.barSize;
        }

        //ChangeClippingArea(new Vector4(property.listWidth * 0.5f, -property.listHeight * 0.5f, property.listWidth, property.listHeight));
    }

    protected override bool RenewItem(bool _flickForward, float _itemDitance)
    {
        if (_flickForward)
        {
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
                topPos -= _itemDitance;

                for (int i = 0; i < property.max_gridCount; i++)
                {
                    ListItem item = listItems[0];

                    int itemIdx = targetIdx + i;
                    if (itemIdx < itemDatas.Count)
                    {
                        ListItemData data = itemDatas[itemIdx];
                        item.OnItemUpdate(data);
                        if (null != onItemUpdated)
                        {
                            onItemUpdated(item);
                        }

                        Active(item.gameObject, true);
                        //Debug.Log(item.GetData().index);
                    }
                    else
                    {
                        item.SetData(null);
                        Active(item.gameObject, false);
                    }

                    listItems.Add(item);
                    listItems.RemoveAt(0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            int targetIdx = listItems[0].GetData().index - 1;
            if (targetIdx > -1)
            {
                topPos += _itemDitance;

                for (int i = 0; i < property.max_gridCount; i++)
                {
                    ListItem item = listItems[listItems.Count - 1];

                    int itemIdx = targetIdx - i;
                    ListItemData data = itemDatas[itemIdx];
                    item.OnItemUpdate(data);
                    if (null != onItemUpdated)
                    {
                        onItemUpdated(item);
                    }

                    Active(item.gameObject, true);
                    //Debug.Log(item.GetData().index);

                    listItems.RemoveAt(listItems.Count - 1);
                    listItems.Insert(0, item);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    protected override void UpdateItemPosition()
    {
        ListItem item;
        float itemSize = property.isVertical ? -property.itemHeight : property.itemWidth;
        for (int i = 0; i < listItems.Count; i++)
        {
            item = listItems[i];
            float itemPos = topPos + (itemSize * (i / property.max_gridCount));

            Vector3 localPos = item.transform.localPosition;
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
}

