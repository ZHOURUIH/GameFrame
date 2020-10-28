﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class myUGUICanvas : myUGUIObject
{
	protected Canvas mCanvas;
	protected CanvasScaler mCanvasScaler;
	protected GraphicRaycaster mGraphicRaycaster;
	protected GameObject mConnectedParent;  // 重新指定挂接到的父节点
	public override void init()
	{
		base.init();
		mCanvas = mObject.GetComponent<Canvas>();
		if (mCanvas == null)
		{
			mCanvas = mObject.AddComponent<Canvas>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mCanvas == null)
		{
			logError(Typeof(this) + " can not find " + Typeof<Canvas>() + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		mCanvas.overrideSorting = true;
		// 添加GraphicRaycaster
		getUnityComponent<GraphicRaycaster>();
	}
	public void setConnectParent(GameObject obj)
	{
		setConnectParent(obj, Vector3.zero, Vector3.zero);
	}
	public void setConnectParent(GameObject obj, Vector3 pos)
	{
		setConnectParent(obj, pos, Vector3.zero);
	}
	public void setConnectParent(GameObject obj, Vector3 pos, Vector3 rot)
	{
		mConnectedParent = obj;
		// 把窗口挂到该节点下,并且保留缩放属性
		mTransform.SetParent(mConnectedParent.transform);
		mTransform.localPosition = pos;
		mTransform.localEulerAngles = rot;
		setGameObjectLayer(mObject, mConnectedParent.layer);
	}
	public void setSortingOrder(int order) { mCanvas.sortingOrder = order; }
	public GameObject getConnectParent() { return mConnectedParent; }
	public Canvas getCanvas() { return mCanvas; }
}