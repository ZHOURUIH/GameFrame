using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameFramework), true)]
public class GameFrameworkEditor : GameEditorBase
{
	protected GameFramework mGameFramework;
	public override void OnInspectorGUI()
	{
		mGameFramework = target as GameFramework;

		bool modified = false;
		displayEnum("WindowMode", "��������", ref mGameFramework.mWindowMode, ref modified);
		if (mGameFramework.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			displayInt("ScreenWidth", "���ڿ��,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mScreenWidth, ref modified);
			displayInt("ScreenHeight", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mScreenHeight, ref modified);
		}
		displayToggle("PoolStackTrace", "�Ƿ����ö�����еĶ�ջ׷��,���ڶ�ջ׷�ٷǳ���ʱ,����Ĭ�Ϲر�,��ʹ��F4��̬����", ref mGameFramework.mEnablePoolStackTrace, ref modified);
		displayToggle("ScriptDebug", "�Ƿ����õ��Խű�,Ҳ���ǹҽ���GameObject��������ʾ������Ϣ�Ľű�,��ʹ��F3��̬����", ref mGameFramework.mEnableScriptDebug, ref modified);
		displayToggle("UseFixedTime", "�Ƿ�ÿ֡��ʱ��̶�����", ref mGameFramework.mUseFixedTime, ref modified);
		displayToggle("ForceTop", "�����Ƿ�ʼ����ʾ�ڶ���", ref mGameFramework.mForceTop, ref modified);
		displayEnum("LoadSource", "����Դ", ref mGameFramework.mLoadSource, ref modified);
		displayEnum("LogLevel", "��־�ȼ�", ref mGameFramework.mLogLevel, ref modified);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}