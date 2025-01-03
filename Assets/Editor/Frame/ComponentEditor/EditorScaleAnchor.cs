using UnityEditor;

[CustomEditor(typeof(ScaleAnchor), true)]
public class EditorScaleAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		var anchor = target as ScaleAnchor;

		bool modified = false;
		modified |= toggle("���ֿ�߱�", ref anchor.mKeepAspect);
		modified |= toggle("���������С", ref anchor.mAdjustFont);
		modified |= displayInt("�������С�ߴ�", ref anchor.mMinFontSize);
		modified |= toggle("����ʱ����λ��", ref anchor.mAdjustPosition);
		modified |= toggle("�Ƴ�UGUI��ê��", ref anchor.mRemoveUGUIAnchor);
		if (anchor.mKeepAspect)
		{
			modified |= displayEnum("���ŷ�ʽ", "", ref anchor.mAspectBase);
		}
		if (modified)
		{
			EditorUtility.SetDirty(target);
		}
	}
}