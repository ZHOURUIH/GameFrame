using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class EditorScaleAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		var anchor = target as ScaleAnchor;

		bool modified = false;
		modified |= toggle("KeepAspect", "�Ƿ񱣳ֿ�߱Ƚ�������", ref anchor.mKeepAspect);
		modified |= toggle("AdjustFont", "�Ƿ�ͬʱ���������С", ref anchor.mAdjustFont);
		modified |= displayInt("MinFontSize", "�������С�ߴ�", ref anchor.mMinFontSize);
		modified |= toggle("AdjustPosition", "����ʱ�Ƿ�ͬʱ����λ��", ref anchor.mAdjustPosition);
		modified |= toggle("RemoveUGUIAnchor", "�Ƿ���Ҫ�Ƴ�UGUI��ê��", ref anchor.mRemoveUGUIAnchor);
		if (anchor.mKeepAspect)
		{
			modified |= displayEnum("AspectBase", "���ŷ�ʽ", ref anchor.mAspectBase);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}