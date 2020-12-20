using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameKeyframe), true)]
public class GameKeyframeEditor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		GUILayout.Space(6f);
		EditorGUIUtility.labelWidth = 110.0f;
		base.OnInspectorGUI();

		if (GUILayout.Button("����"))
		{
			GameKeyframe keyframe = target as GameKeyframe;
			keyframe.CreateKeyframe();
			EditorUtility.SetDirty(target);
		}

		DrawCommonProperties();
	}

	protected void DrawCommonProperties ()
	{
		List<CurveInfo> deleteKeyList = null;
		GameKeyframe keyframe = target as GameKeyframe;
		if(keyframe.mCurveList != null)
		{
			foreach (var item in keyframe.mCurveList)
			{
				BeginContents();
				if (GUILayout.Button("X", GUILayout.Width(20)))
				{
					if (deleteKeyList == null)
					{
						deleteKeyList = new List<CurveInfo>();
					}
					deleteKeyList.Add(item);
				}
				else
				{
					GUI.changed = false;
					EditorGUILayout.CurveField(item.mID.ToString(), item.mCurve, GUILayout.Width(170f), GUILayout.Height(62f));
					// ��������иĶ�,��������Ԥ���иĶ�
					if (GUI.changed)
					{
						EditorUtility.SetDirty(target);
					}
				}
				EndContents();
			}
		}
		if(deleteKeyList != null)
		{
			foreach(var item in deleteKeyList)
			{
				keyframe.DestroyKeyframe(item);
			}
			EditorUtility.SetDirty(target);
		}
	}
}
