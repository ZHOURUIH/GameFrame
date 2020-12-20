﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderCriticalMaskFadeOutLinearDodge : WindowShaderCriticalMask
{
	protected float mFadeOutCriticalValue;
	protected int mFadeOutCriticalValueID;
	protected WindowShaderCriticalMaskFadeOutLinearDodge()
	{
		mFadeOutCriticalValueID = Shader.PropertyToID("_FadeOutCriticalValue");
	}
	public void setFadeOutCriticalValue(float value) { mFadeOutCriticalValue = value; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mFadeOutCriticalValueID, mFadeOutCriticalValue);
		}
	}
}