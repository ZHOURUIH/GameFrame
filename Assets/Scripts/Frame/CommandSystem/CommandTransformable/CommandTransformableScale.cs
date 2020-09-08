﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandTransformableScale : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Vector3 mStartScale;
	public Vector3 mTargetScale;
	public string mName;
	public float mOnceLength;
	public float mAmplitude;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mStartScale = Vector3.one;
		mTargetScale = Vector3.one;
		mName = null;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentScale component = obj.getComponent(out component);
		// 停止其他缩放组件
		obj.breakComponent<IComponentModifyScale>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setStartScale(mStartScale);
		component.setTargetScale(mTargetScale);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PS_PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mLoop:" + mLoop +
			", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce + ", mStartScale:" + mStartScale + ", mTargetScale:" + mTargetScale;
	}
}