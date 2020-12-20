﻿using UnityEngine;
using System.Collections;

public class CommandTransformableRotatePhysics : Command
{
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public Vector3 mStartRotation;
	public Vector3 mTargetRotation;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mAmplitude;
	public float mOffset;
	public bool mRandomOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mStartRotation = Vector3.zero;
		mTargetRotation = Vector3.zero;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mRandomOffset = false;
		mFullOnce = true;
		mLoop = false;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentRotatePhysics component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(Typeof(component));
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		if (mRandomOffset)
		{
			mOffset = randomFloat(0.0f, mOnceLength);
		}
		component.setTargetRotation(mTargetRotation);
		component.setStartRotation(mStartRotation);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mKeyframe:" + mKeyframe + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + ", mLoop:" + mLoop + ", mAmplitude:" +
			mAmplitude + ", mFullOnce:" + mFullOnce + ", mRandomOffset:" + mRandomOffset + ", mStartRotation:" + mStartRotation + ", mTargetRotation:" + mTargetRotation;
	}
}