﻿using UnityEngine;
using System;

public class TransformableComponentMovePhysics : ComponentKeyFramePhysics, IComponentModifyPosition
{
	protected Vector3 mStartPos;    // 移动开始时的位置
	protected Vector3 mTargetPos;
	public void setTargetPos(Vector3 pos) { mTargetPos = pos; }
	public void setStartPos(Vector3 pos) { mStartPos = pos; }
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		(mComponentOwner as Transformable).setPosition(lerpSimple(mStartPos, mTargetPos, value));
	}
}