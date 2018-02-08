using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ABManager
{
    public class DragHandler
    {
        public class DragDatas
        {
            public string[] dragPaths = null;
            public UnityEngine.Object[] dragObjects = null;
            public object customDragData = null;
        }

        public enum DragState { Receiving, Rejecting, Received, None };

        public delegate bool CanRecieveDragDelegate(DragDatas recieverData, DragDatas dragData);
        public delegate void RecieveDelegate(DragDatas recieverData, DragDatas dragData);

        public const string KEY_DRAG_IDENTIFIER = "Drag Identifier";
        public const string KEY_DRAG_DATA = "CustomDrag Data";

        public string dragIdentifier = string.Empty;
        public Rect detectRect;
        public bool dragable = true;
        public CanRecieveDragDelegate canRecieveCallBack = null;
        public RecieveDelegate reciveDragCallBack = null;

        private DragDatas dragData = new DragDatas();

        private HashSet<string> recieveIdentifiers = new HashSet<string>() { null };

        // Only called in OnGUI()
        // Return value indecate if it can recieve some thing
        public DragState GUI_DragUpdate()
        {
            if (!detectRect.Contains(Event.current.mousePosition))
                return DragState.None;
            //Debug.Log(Event.current.type);
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    StartDrage();
                    break;
                case EventType.DragPerform:
                    if (TryRecive())
                    {
                        return DragState.Received;
                    }
                    break;
                case EventType.DragUpdated:
                    if (DectectRecieve())
                    {
                        return DragState.Receiving;
                    }
                    else
                    {
                        return DragState.Rejecting;
                    }
            }

            return DragState.None;
        }

        public bool AddRecieveIdentifier(string identify)
        {
            if (recieveIdentifiers.Contains(identify))
            {
                return false;
            }
            else
            {
                recieveIdentifiers.Add(identify);
                return true;
            }
        }

        public void SetCustomDragData(object customDragData)
        {
            if (null != customDragData 
                && (null == dragData.customDragData || !string.Equals(dragData.customDragData.ToString(), customDragData.ToString())))
            {
                dragData.customDragData = customDragData;
                SetGenericData();
            }
        }

        private void SetGenericData()
        {
            if (!string.IsNullOrEmpty(dragIdentifier))
                DragAndDrop.SetGenericData(KEY_DRAG_IDENTIFIER, dragIdentifier);

            if (dragData.customDragData != null)
                DragAndDrop.SetGenericData(KEY_DRAG_DATA, dragData.customDragData);
        }

        private void StartDrage()
        {
            if (!dragable)
                return;

            DragAndDrop.PrepareStartDrag();

            DragAndDrop.paths = new string[] { };
            DragAndDrop.objectReferences = new Object[] { };

            SetGenericData();

            DragAndDrop.StartDrag((string)dragData.customDragData);

            Event.current.Use();
        }

        private bool DectectRecieve()
        {
            if (CanRecive())
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                return true;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return false;
            }
        }

        private bool TryRecive()
        {
            if (CanRecive())
            {
                reciveDragCallBack(dragData, GetCurrentDragData());
                DragAndDrop.AcceptDrag();
                return true;
            }

            return false;
        }

        private bool CanRecive()
        {
            return recieveIdentifiers.Contains((string)DragAndDrop.GetGenericData(KEY_DRAG_IDENTIFIER))
                && canRecieveCallBack(dragData, GetCurrentDragData());
        }

        private DragDatas GetCurrentDragData()
        {
            DragDatas data = new DragDatas();
            data.dragPaths = DragAndDrop.paths;
            data.dragObjects = DragAndDrop.objectReferences;
            data.customDragData = DragAndDrop.GetGenericData(KEY_DRAG_DATA);

            return data;
        }
    }
}
