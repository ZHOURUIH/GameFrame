﻿using UnityEngine;
using System.Collections;

#if USE_NGUI

public class myNGUICheckBox : myNGUIObject
{
	protected UIToggle mToggle;
	public override void init()
	{
		base.init();
		mToggle = getUnityComponent<UIToggle>();
	}
	public UIToggle getToggle() {return mToggle;}
	public void setChecked(bool check){mToggle.value = check;}
	public bool getChecked(){ return mToggle.value; }
	public void setCheckChangedCallback(EventDelegate.Callback callback)
	{
		EventDelegate.Add(mToggle.onChange, callback);
	}
}

#endif