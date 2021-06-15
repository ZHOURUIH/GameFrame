﻿using System;
using System.Collections.Generic;

// 主状态反向互斥,有其他状态时,不允许添加主状态,添加其他状态时,立即将主状态移除
public class StateGroupMutexMutexInverseMain : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		// 存在主状态,则将主状态移除
		if (mGroup.mMainState == existState)
		{
			return false;
		}
		return true;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		if (mGroup.mMainState == newState)
		{
			return false;
		}
		return true;
	}
}