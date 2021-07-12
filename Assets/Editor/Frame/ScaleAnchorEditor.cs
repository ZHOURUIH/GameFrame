using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class ScaleAnchorEditor : GameEditorBase
{
	private ScaleAnchor anchor;
	public override void OnInspectorGUI()
	{
		anchor = target as ScaleAnchor;

		bool modified = false;
		displayToggle("KeepAspect", "�Ƿ񱣳ֿ�߱Ƚ�������", ref anchor.mKeepAspect, ref modified);
		displayToggle("AdjustFont", "�Ƿ�ͬʱ���������С", ref anchor.mAdjustFont, ref modified);
		displayToggle("AdjustPosition", "����ʱ�Ƿ�ͬʱ����λ��", ref anchor.mAdjustPosition, ref modified);
		displayToggle("RemoveUGUIAnchor", "�Ƿ���Ҫ�Ƴ�UGUI��ê��", ref anchor.mRemoveUGUIAnchor, ref modified);
		if (anchor.mKeepAspect)
		{
			displayEnum("AspectBase", "���ŷ�ʽ", ref anchor.mAspectBase, ref modified);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}