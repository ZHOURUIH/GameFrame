﻿using UnityEngine;
using static UnityUtility;
using static FrameEditorUtility;

// 用于加载Android平台下的资源
public class AndroidMainClass : FrameSystem
{
	protected static AndroidJavaClass mMainClass;    // Java中加载类的实例
	public static void initJava(string classPath)
	{
		if (!isEditor() && isAndroid())
		{
			if (classPath.isEmpty())
			{
				logError("initJava failed! classPath not valid");
				return;
			}
			mMainClass = new(classPath);
		}
	}
	public static AndroidJavaClass getMainClass() { return mMainClass; }
	public override void destroy()
	{
		mMainClass?.Dispose();
		mMainClass = null;
		base.destroy();
	}
	// 获取当前的电流大小,单位微安
	public static int getBatteryEnergy()
	{
		return mMainClass?.CallStatic<int>("getBatteryEnergy") ?? 0;
	}
	// 安装一个apk文件
	public static void installAPK(string filePath)
	{
		mMainClass?.CallStatic("installAPK", filePath);
	}
	public static void gameStart()
	{
		if (isEditor() || !isAndroid())
		{
			return;
		}
		if (mMainClass == null)
		{
			logError("MainClass is null");
			return;
		}
		mMainClass.CallStatic("gameStart", AndroidPluginManager.getMainActivity());
	}
}