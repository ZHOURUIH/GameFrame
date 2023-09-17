using UnityEditor;

[CustomEditor(typeof(GameFramework), true)]
public class EditorGameFramework : GameEditorBase
{
	protected GameFramework mGameFramework;
	public override void OnInspectorGUI()
	{
		mGameFramework = target as GameFramework;

		bool modified = false;
		modified |= displayEnum("WindowMode", "��������", ref mGameFramework.mWindowMode);
		if (mGameFramework.mWindowMode != WINDOW_MODE.FULL_SCREEN)
		{
			modified |= displayInt("ScreenWidth", "���ڿ��,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mScreenWidth);
			modified |= displayInt("ScreenHeight", "���ڸ߶�,��WindowModeΪFULL_SCREENʱ��Ч", ref mGameFramework.mScreenHeight);
		}
		modified |= toggle("PoolStackTrace", "�Ƿ����ö�����еĶ�ջ׷��,���ڶ�ջ׷�ٷǳ���ʱ,����Ĭ�Ϲر�,��ʹ��F4��̬����", ref mGameFramework.mEnablePoolStackTrace);
		modified |= toggle("ScriptDebug", "�Ƿ����õ��Խű�,Ҳ���ǹҽ���GameObject��������ʾ������Ϣ�Ľű�,��ʹ��F3��̬����", ref mGameFramework.mEnableScriptDebug);
		modified |= toggle("UseFixedTime", "�Ƿ�ÿ֡��ʱ��̶�����", ref mGameFramework.mUseFixedTime);
		modified |= toggle("ForceTop", "�����Ƿ�ʼ����ʾ�ڶ���", ref mGameFramework.mForceTop);
		modified |= displayEnum("LoadSource", "����Դ", ref mGameFramework.mLoadSource);
		modified |= displayEnum("LogLevel", "��־�ȼ�", ref mGameFramework.mLogLevel);

		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}