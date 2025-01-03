﻿using System.Collections.Generic;
using static FrameUtility;
using static UnityUtility;
using static FrameEditorUtility;

// 用于UGUI的multi sprite管理
public class TPSpriteManager : FrameSystem
{
	protected AtlasLoaderAssetBundle mAssetBundleAtlasManager = new();	// 从AssetBundle中加载
	protected AtlasLoaderResources mResourcesAtlasManager = new();		// 从Resources中加载
	public TPSpriteManager()
	{
		if (isEditor())
		{
			mCreateObject = true;
		}
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<TPSpriteManagerDebug>();
		}
	}
	public void destroyAll()
	{
		mAssetBundleAtlasManager.destroyAll();
		mResourcesAtlasManager.destroyAll();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAssetBundleAtlasManager.update();
		mResourcesAtlasManager.update();
	}
	public void addDontUnloadAtlas(string fileName) { mAssetBundleAtlasManager.addDontUnloadAtlas(fileName); }
	public SafeDictionary<string, UGUIAtlas> getAtlasList() { return mAssetBundleAtlasManager.getAtlasList(); }
	public SafeDictionary<string, UGUIAtlas> getAtlasListInResources() { return mResourcesAtlasManager.getAtlasList(); }
	// 获取位于GameResources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlas(string atlasName, bool errorInNull, bool loadIfNull)
	{
		return mAssetBundleAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 获取位于Resources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlasInResources(string atlasName, bool errorInNull, bool loadIfNull)
	{
		return mResourcesAtlasManager.getAtlas(atlasName, errorInNull, loadIfNull);
	}
	// 异步加载位于GameResources中的图集
	public CustomAsyncOperation getAtlasAsync(string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull, bool loadIfNull)
	{
		return mAssetBundleAtlasManager.getAtlasAsync(atlasName, callback, errorInNull, loadIfNull);
	}
	// 异步加载位于Resources中的图集
	public CustomAsyncOperation getAtlasInResourcesAsync(string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull, bool loadIfNull)
	{
		return mResourcesAtlasManager.getAtlasAsync(atlasName, callback, errorInNull, loadIfNull);
	}
	// 卸载图集
	public void unloadAtlas(ref UGUIAtlasPtr atlasPtr)
	{
		mAssetBundleAtlasManager.unloadAtlas(atlasPtr);
		UN_CLASS(ref atlasPtr);
	}
	public void unloadAtlas(List<UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr)
		{
			mAssetBundleAtlasManager.unloadAtlas(item);
		}
		UN_CLASS_LIST(atlasPtr);
	}
	public void unloadAtlas<Key>(Dictionary<Key, UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr.Values)
		{
			mAssetBundleAtlasManager.unloadAtlas(item);
		}
		UN_CLASS_LIST(atlasPtr);
	}
	public void unloadAtlasInResourcecs(ref UGUIAtlasPtr atlasPtr)
	{
		mResourcesAtlasManager.unloadAtlas(atlasPtr);
		UN_CLASS(ref atlasPtr);
	}
	public void unloadAtlasInResourcecs(List<UGUIAtlasPtr> atlasPtr)
	{
		foreach (UGUIAtlasPtr item in atlasPtr)
		{
			mResourcesAtlasManager.unloadAtlas(item);
		}
		UN_CLASS_LIST(atlasPtr);
	}
}