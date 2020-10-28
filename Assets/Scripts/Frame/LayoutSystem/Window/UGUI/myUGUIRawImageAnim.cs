﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class myUGUIRawImageAnim : myUGUIRawImage, IUIAnimation
{
	protected List<TextureAnimCallBack> mPlayEndCallback;  // 一个序列播放完时的回调函数,只在非循环播放状态下有效
	protected List<TextureAnimCallBack> mPlayingCallback;  // 一个序列正在播放时的回调函数
	protected List<Texture> mTextureNameList;
	protected List<Vector2> mTexturePosList;
	protected AnimControl mControl;
	protected string mTextureSetName;
	protected string mSubPath;
	protected bool mUseTextureSize;
	public myUGUIRawImageAnim()
	{
		mTextureNameList = new List<Texture>();
		mControl = new AnimControl();
		mUseTextureSize = false;
		mPlayEndCallback = new List<TextureAnimCallBack>();
		mPlayingCallback = new List<TextureAnimCallBack>();
		mTextureSetName = EMPTY_STRING;
		mSubPath = EMPTY_STRING;
		mEnable = true;
	}
	public override void init()
	{
		base.init();
		string textureName = getTextureName();
		if (!isEmpty(textureName))
		{
			int index = textureName.LastIndexOf('_');
			if (index >= 0)
			{
				setTextureSet(textureName.Substring(0, index));
			}
		}
		mControl.setObject(this);
		mControl.setPlayEndCallback(onPlayEnd);
		mControl.setPlayingCallback(onPlaying);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTextureNameList.Count == 0)
		{
			setTexture(null, false);
		}
		mControl.update(elapsedTime);
	}
	public string getTextureSet() { return mTextureSetName; }
	public int getTextureFrameCount() { return mTextureNameList.Count; }
	public void setUseTextureSize(bool useSize){mUseTextureSize = useSize;}
	public void setSubPath(string subPath) { mSubPath = subPath; }
	public string getSubPath() { return mSubPath; }
	public void setTexturePosList(List<Vector2> posList) { mTexturePosList = posList; }
	public List<Vector2> getTexturePosList() { return mTexturePosList; }
	public void setTextureSet(string textureSetName)
	{
		setTextureSet(textureSetName, mSubPath);
	}
	public void setTextureSet(string textureSetName, string subPath)
	{
		if (!isEmpty(subPath) && !subPath.EndsWith("/"))
		{
			subPath += "/";
		}
		if (mTextureSetName == textureSetName && mSubPath == subPath)
		{
			return;
		}
		mTextureNameList.Clear();
		mTextureSetName = textureSetName;
		mSubPath = subPath;
		string path = FrameDefine.R_TEXTURE_ANIM_PATH + mSubPath + mTextureSetName;
		string preName = path + "/" + mTextureSetName + "_";
		for (int i = 0; ; ++i)
		{
			string name = preName + intToString(i);
			Texture tex = mResourceManager.loadResource<Texture>(name, false);
			if (tex == null)
			{
				break;
			}
			mTextureNameList.Add(tex);
		}
		mControl.setFrameCount(getTextureFrameCount());
		if(mTextureNameList.Count == 0)
		{
			logError("invalid texture set! " + textureSetName + ", subPath : " + subPath);
		}
	}
	public LOOP_MODE getLoop() { return mControl.getLoop(); }
	public float getInterval() { return mControl.getInterval(); }
	public float getSpeed() { return mControl.getSpeed(); }
	public int getStartIndex() { return mControl.getStartIndex(); }
	public PLAY_STATE getPlayState() { return mControl.getPlayState(); }
	public bool getPlayDirection() { return mControl.getPlayDirection(); }
	public int getEndIndex() { return mControl.getEndIndex(); }
	public bool isAutoHide() { return mControl.isAutoResetIndex(); }
	// 获得实际的终止下标,如果是自动获得,则返回最后一张的下标
	public int getRealEndIndex() { return mControl.getRealEndIndex(); }
	public void setLoop(LOOP_MODE loop) { mControl.setLoop(loop); }
	public void setInterval(float interval) { mControl.setInterval(interval); }
	public void setSpeed(float speed) { mControl.setSpeed(speed); }
	public void setPlayDirection(bool direction) { mControl.setPlayDirection(direction); }
	public void setAutoHide(bool autoHide) { mControl.setAutoHide(autoHide); }
	public void setStartIndex(int startIndex) { mControl.setStartIndex(startIndex); }
	public void setEndIndex(int endIndex) { mControl.setEndIndex(endIndex); }
	public void stop(bool resetStartIndex = true, bool callback = true, bool isBreak = true) { mControl.stop(resetStartIndex, callback, isBreak); }
	public void play() { mControl.play(); }
	public void pause() { mControl.pause(); }
	public int getCurFrameIndex() { return mControl.getCurFrameIndex(); }
	public void setCurFrameIndex(int index) { mControl.setCurFrameIndex(index); }
	public void addPlayEndCallback(TextureAnimCallBack callback, bool clear = true)
	{
		if(clear)
		{
			List<TextureAnimCallBack> tempList = mListPool.newList(out tempList);
			tempList.AddRange(mPlayEndCallback);
			mPlayEndCallback.Clear();
			// 如果回调函数当前不为空,则是中断了更新
			foreach(var item in tempList)
			{
				item(this, true);
			}
			mListPool.destroyList(tempList);
		}
		mPlayEndCallback.Add(callback);
	}
	public void addPlayingCallback(TextureAnimCallBack callback, bool clear = true)
	{
		if (clear)
		{
			mPlayingCallback.Clear();
		}
		if (callback != null)
		{
			mPlayingCallback.Add(callback);
		}
	}
	//---------------------------------------------------------------------------------------------------------------------------------------------------
	protected void onPlaying(AnimControl control, int frame, bool isPlaying)
	{
		if (mControl.getCurFrameIndex() >= mTextureNameList.Count)
		{
			return;
		}
		setTexture(mTextureNameList[mControl.getCurFrameIndex()], mUseTextureSize);
		if(mTexturePosList != null)
		{
			int positionIndex = (int)(frame / (float)mTextureNameList.Count * mTexturePosList.Count + 0.5f);
			setPosition(mTexturePosList[positionIndex]);
		}
		foreach (var item in mPlayingCallback)
		{
			item(this, false);
		}
	}
	protected void onPlayEnd(AnimControl control, bool callback, bool isBreak)
	{
		// 正常播放完毕后根据是否重置下标来判断是否自动隐藏
		if (!isBreak && mControl.isAutoResetIndex())
		{
			setActive(false);
		}
		if(callback)
		{
			List<TextureAnimCallBack> tempList = mListPool.newList(out tempList);
			tempList.AddRange(mPlayEndCallback);
			mPlayEndCallback.Clear();
			foreach (var item in tempList)
			{
				item(this, isBreak);
			}
			mListPool.destroyList(tempList);
		}
		else
		{
			mPlayEndCallback.Clear();
		}
	}
}