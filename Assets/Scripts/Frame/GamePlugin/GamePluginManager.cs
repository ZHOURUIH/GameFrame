using System;
using System.Collections.Generic;
using System.Reflection;

// �����׺Ϊbytes,��������Ŀ��ڱ༭��ģʽ����Ҫ�ŵ�Plugins��,�����ŵ�Managed��
public class GamePluginManager : FrameSystem
{
	protected Dictionary<string, IGamePlugin> mPluginList;
	public GamePluginManager()
	{
		mPluginList = new Dictionary<string, IGamePlugin>();
	}
	public override void init()
	{
		base.init();
		loadAllPlugin();
		foreach (var item in mPluginList)
		{
			item.Value.init();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mPluginList)
		{
			item.Value.update(elapsedTime);
		}
	}
	public override void destroy()
	{
		foreach (var item in mPluginList)
		{
			item.Value.destroy();
		}
		mPluginList.Clear();
		base.destroy();
	}
	//-----------------------------------------------------------------------------------------------------------
	protected void loadAllPlugin()
	{
#if UNITY_STANDALONE_WIN
		if(!isDirExist(FrameDefine.F_GAME_PLUGIN_PATH))
		{
			return;
		}
		LIST_MAIN(out List<string> fileList);
		findFiles(FrameDefine.F_GAME_PLUGIN_PATH, fileList, FrameDefine.DLL_PLUGIN_SUFFIX);
		int count = fileList.Count;
		for (int i = 0; i < count; ++i)
		{
			openFile(fileList[i], out byte[] fileBuffer, true);
			loadPlugin(fileBuffer, getFileName(fileList[i]));
			releaseFile(fileBuffer);
		}
		UN_LIST_MAIN(fileList);
#endif
	}
	protected bool loadPlugin(byte[] rawDll, string fileName)
	{
		try
		{
			Assembly assembly = Assembly.Load(rawDll);
			Type[] types = assembly.GetTypes();
			int count = types.Length;
			for(int i = 0; i < count; ++i)
			{
				Type type = types[i];
				if (type.GetInterfaces().Length > 0)
				{
					var instance = assembly.CreateInstance(type.FullName) as IGamePlugin;
					if (instance != null)
					{
						mPluginList.Add(instance.getPluginName(), instance);
						log("game plugin " + instance.getPluginName() + " load success!");
					}
				}
			}
		}
		catch (Exception e)
		{
			log("load game plugin failed! file name : " + fileName + ", info : " + e.Message);
			return false;
		}
		return true;
	}
}