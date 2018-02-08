using UnityEngine;
using System.Collections;

/*
 * List의 각 Item 클래스
 * 
 * OnPress와  OnClick을 바로 처리하지 않고, ListView에 전달하여 처리한다.
 */

public class ListItem : MonoBehaviour
{
#if UNITY_EDITOR
    //ListItem으로 이름을 변경하기때문에 어떤 프리팹을 사용하는지 알게하기 위한 변수
    public string prefabName;
    void Awake()
    {
        prefabName = gameObject.name;
    }
#endif
    protected ListView listView;
    protected ListItemData data;

    void OnPress(bool _isPress)
    {
        if(listView != null)
            listView.OnItemPress(_isPress, this);
    }

    void OnClick()
    {
        if (listView != null)
            listView.OnItemClick(this);
    }

    public ListItemData GetData()
    {
        return data;
    }

    public void SetData(ListItemData _data)
    {
        data = _data;
    }

    //최초 item이 생성될 때 호출된다.
    public virtual void OnItemAdd(ListItemData _data, ListView _listView)
    {
        listView = _listView;

        if (null != _data)
            OnItemUpdate(_data);
        else
            data = null;
    }

    //item이 Update 될 때 호출된다. 
    public virtual void OnItemUpdate(ListItemData _data)
    {
        data = _data;
        gameObject.name = "ListItem" + _data.index;
    }

    //item이 Click될 때 호출된다.
    public virtual void OnItemClick()
    {
        //TutorialManager.Instance.OnClickTrigger(gameObject.name);
    }
}
