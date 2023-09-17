﻿using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 字典池的调试信息
public class DictionaryPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();	// 持久使用的对象列表
	public List<string> InuseList = new List<string>();				// 单帧使用的对象列表
	public List<string> UnuseList = new List<string>();				// 未使用的对象列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = mDictionaryPool.getPersistentInusedList();
		foreach (var item in persistentInuse)
		{
			PersistentInuseList.Add(item.Key + ":" + IToS(item.Value.Count));
		}

		InuseList.Clear();
		var inuse = mDictionaryPool.getInusedList();
		foreach(var item in inuse)
		{
			if(item.Value.Count > 0)
			{
				InuseList.Add(item.Key + ":" + IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = mDictionaryPool.getUnusedList();
		foreach (var item in unuse)
		{
			if(item.Value.Count > 0)
			{
				UnuseList.Add(item.Key + ":" + IToS(item.Value.Count));
			}
		}
	}
}