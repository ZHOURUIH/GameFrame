﻿using UnityEngine;
using System;

// 物体的抛物线移动组件
public class COMTransformableMoveParabola : ComponentKeyFrameNormal, IComponentModifyPosition
{
	protected Vector3 mStartPos;    // 移动开始时的位置
	protected Vector3 mTargetPos;   // 移动目标位置
	protected Vector3 mTempB;       // 转换到以起点为坐标原点的终点坐标,并且此坐标的Z值为0,也就是在X-Y平面上
	protected float mTopHeight;     // 抛物线的最高点高度,相对于起点
	protected float mFactorA;       // mFactorA越大,抛物线的顶点越高
	protected float mFactorB;       // 根据mFactorA和起点,终点计算出的抛物线公式中的参数
	public override void resetProperty()
	{
		base.resetProperty();
		mStartPos = Vector3.zero;
		mTargetPos = Vector3.zero;
		mTempB = Vector3.zero;
		mFactorA = 0.0f;
		mFactorB = 0.0f;
		mTopHeight = 0.0f;
	}
	public void setTargetPos(Vector3 pos) { mTargetPos = pos; }
	public void setStartPos(Vector3 pos) { mStartPos = pos; }
	public void setTopHeight(float top) { mTopHeight = abs(top); }
	public override void play(int keyframe, bool loop, float onceLength, float offset)
	{
		base.play(keyframe, loop, onceLength, offset);
		// 首先将起点和终点平移至原点
		mTempB = mTargetPos - mStartPos;
		// 即使起点和终点相同，也需要执行高度上的抛物线移动
		if (isVectorZero(mTempB))
		{
			mTempB = Vector3.forward;
		}
		// 绕A点旋转B点到X轴上
		float angle = -getAngleVectorToVector(Vector3.forward, mTempB, true, ANGLE.DEGREE) + HALF_PI_DEGREE;
		mTempB = rotateVector3(mTempB, Quaternion.AngleAxis(angle, Vector3.up));
		mFactorB = generateFactorBFromHeight(mTopHeight, mTempB);
		mFactorA = generateFactorA(mFactorB, mTempB);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Vector3 curPos = lerpSimple(mStartPos, mTargetPos, value);
		// 根据插值计算x,再代入抛物线方程计算y
		float x = value * mTempB.x;
		curPos.y = mStartPos.y + mFactorA * x * x + mFactorB * x;
		(mComponentOwner as Transformable).setPosition(curPos);
	}
}