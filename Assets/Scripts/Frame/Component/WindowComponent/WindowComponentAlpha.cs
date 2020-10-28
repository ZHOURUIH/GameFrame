﻿using UnityEngine;
using System;
using System.Collections;

public class WindowComponentAlpha : ComponentKeyFrameNormal, IComponentModifyAlpha
{
	protected float mStartAlpha;
	protected float mTargetAlpha;
	public void setStartAlpha(float alpha) {mStartAlpha = alpha;}
	public void setTargetAlpha(float alpha) {mTargetAlpha = alpha;}
	//------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		myUIObject obj = mComponentOwner as myUIObject;
		float newAlpha = lerpSimple(mStartAlpha, mTargetAlpha, value);
		// 因为NGUI中透明度小于0.001时认为是将窗口隐藏,会重新构建网格顶点,所以此处最低为0.002
		if(WidgetUtility.getGUIType(obj.getObject()) == GUI_TYPE.NGUI)
		{
			clampMin(ref newAlpha, 0.002f);
		}
		obj.setAlpha(newAlpha, false);
	}
}
