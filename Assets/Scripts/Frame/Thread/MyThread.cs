﻿using System;
using System.Collections.Generic;
using System.Threading;

public class MyThread : FrameBase
{
	protected MyThreadCallback mCallback;
	protected ThreadTimeLock mTimeLock;
	protected Thread mThread;
	protected string mName;
	protected bool mIsBackground;       // 是否为后台线程,如果是后台线程,则在应用程序关闭时,子线程会自动强制关闭
	protected bool mRunning;
	protected bool mFinish;
	protected bool mPause;
	public MyThread(string name)
	{
		mName = name;
		mCallback = null;
		mThread = null;
		mTimeLock = null;
		mIsBackground = true;
		mRunning = false;
		mFinish = true;
		mPause = false;
	}
	public void destroy()
	{
		stop();
	}
	public void setBackground(bool background)
	{
		mIsBackground = background;
		if (mThread != null)
		{
			mThread.IsBackground = mIsBackground;
		}
	}
	public void start(MyThreadCallback callback, int frameTimeMS = 15, int forceSleep = 5)
	{
		if (mThread != null)
		{
			return;
		}
		mTimeLock = new ThreadTimeLock(frameTimeMS);
		mTimeLock.setForceSleep(forceSleep);
		mRunning = true;
		mCallback = callback;
		mThread = new Thread(run);
		mThread.Name = mName;
		mThread.Start();
		mThread.IsBackground = mIsBackground;
		logInfo("线程启动成功 : " + mName, LOG_LEVEL.FORCE);
	}
	public void pause(bool isPause) { mPause = isPause; }
	public bool isFinished() { return mFinish; }
	public void stop()
	{
		if (mThread == null)
		{
			return;
		}
		mRunning = false;
		while (!mIsBackground && !mFinish) 
		{
			Thread.Sleep(0);
		}
		mThread?.Abort();
		mThread = null;
		mCallback = null;
		mTimeLock = null;
		mPause = false;
		logInfo("线程退出完成! 线程名 : " + mName, LOG_LEVEL.FORCE);
	}
	protected void run()
	{
		mFinish = false;
		while (mRunning)
		{
			mTimeLock.update();
			try
			{
				if (mPause)
				{
					continue;
				}
				bool run = true;
				mCallback(ref run);
				if (!run)
				{
					break;
				}
			}
			catch (Exception e)
			{
				logError("捕获线程异常! 线程名 : " + mName + ", " + e.Message + ", " + e.StackTrace, false);
			}
		}
		mFinish = true;
	}
}