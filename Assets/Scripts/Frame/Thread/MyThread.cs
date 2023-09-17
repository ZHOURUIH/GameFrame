﻿using System;
using System.Threading;
using static UnityUtility;

// 对线程的封装
public class MyThread
{
	protected MyThreadCallback mCallback;		// 线程执行回调
	protected ThreadTimeLock mTimeLock;			// 用于线程锁帧
	protected Thread mThread;					// 线程对象
	protected string mName;						// 线程名字
	protected volatile bool mIsBackground;		// 是否为后台线程,如果是后台线程,则在应用程序关闭时,子线程会自动强制关闭
	protected volatile bool mRunning;			// 线程是否正在执行
	protected volatile bool mFinish;			// 线程是否已经完成执行
	public MyThread(string name)
	{
		mTimeLock = new ThreadTimeLock(0);
		mName = name;
		mIsBackground = true;
		mFinish = true;
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
		logForce("线程启动成功 : " + mName);
	}
	public bool isFinished() { return mFinish; }
	public void stop()
	{
		if (mThread == null)
		{
			return;
		}
		try
		{
			mRunning = false;
			while (!mIsBackground && !mFinish)
			{
				Thread.Sleep(0);
			}
			mThread.Abort();
			mThread = null;
			mCallback = null;
			mTimeLock = null;
		}
		catch(Exception e)
		{
			logException(e, "线程退出出现异常:" + mName);
		}
		logForce("线程退出完成! 线程名 : " + mName);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void run()
	{
		mFinish = false;
		while (mRunning)
		{
			try
			{
				mTimeLock.update();
				bool run = true;
				mCallback?.Invoke(ref run);
				if (!run)
				{
					break;
				}
			}
			catch (ThreadAbortException)
			{
				// 调用Thread.Abort而正常终止线程
			}
			catch (Exception e)
			{
				logException(e, "捕获线程异常! 线程名 : " + mName);
			}
		}
		mFinish = true;
		mThread?.Abort();
	}
}