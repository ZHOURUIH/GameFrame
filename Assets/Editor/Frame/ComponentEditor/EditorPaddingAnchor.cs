using UnityEditor;
using UnityEngine;
using static MathUtility;

[CustomEditor(typeof(PaddingAnchor), true)]
public class EditorPaddingAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		var paddingAnchor = target as PaddingAnchor;

		bool modified = false;
		paddingAnchor.gameObject.TryGetComponent<RectTransform>(out var rectTrans);
		if (rectTrans != null && !Application.isPlaying)
		{
			bool needRefresh = false;
			Vector3 curPos = rectTrans.localPosition;
			if (!isVectorEqual(paddingAnchor.getLastPosition(), curPos))
			{
				paddingAnchor.setLastPosition(curPos);
				needRefresh = true;
				modified = true;
			}
			Vector2 curSize = rectTrans.rect.size;
			if (!isVectorEqual(paddingAnchor.getLastSize(), curSize))
			{
				paddingAnchor.setLastSize(curSize);
				needRefresh = true;
				modified = true;
			}
			if (needRefresh)
			{
				paddingAnchor.setAnchorMode(paddingAnchor.getAnchorMode());
			}
		}

		modified |= toggle("���������С", ref paddingAnchor.mAdjustFont);
		modified |= displayInt("�������С�ߴ�", ref paddingAnchor.mMinFontSize);
		// ������EditorGUILayout.EnumPopup���ַ�ʽ��ʾ���������ڿ����ڱ༭���޸�,���Ҵ���ָ���߼�
		ANCHOR_MODE anchorMode = displayEnum("ͣ������", paddingAnchor.mAnchorMode);
		if (anchorMode != paddingAnchor.mAnchorMode)
		{
			modified = true;
			paddingAnchor.setAnchorModeInEditor(anchorMode);
		}
		bool relativePos = toggle("�Ƿ�洢���ֵ", paddingAnchor.mRelativeDistance);
		if (relativePos != paddingAnchor.mRelativeDistance)
		{
			modified = true;
			paddingAnchor.setRelativeDistanceInEditor(relativePos);
		}
		// ֻ��ͣ�������ڵ��ĳ��λ��
		if (paddingAnchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			HORIZONTAL_PADDING horizontalPadding = displayEnum("ˮƽͣ������", "", paddingAnchor.mHorizontalNearSide);
			if (horizontalPadding != paddingAnchor.mHorizontalNearSide)
			{
				modified = true;
				paddingAnchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			VERTICAL_PADDING verticalPadding = displayEnum("��ֱͣ������", "", paddingAnchor.mVerticalNearSide);
			if (verticalPadding != paddingAnchor.mVerticalNearSide)
			{
				modified = true;
				paddingAnchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTERģʽ�²Ż���ʾmHorizontalPosition
			if (paddingAnchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty����ֻ�Ǽ򵥵�ʹ��Ĭ�Ϸ�ʽ��ʾ����,���ڱ༭����ֱ���޸�ֵ,���ܴ����κ������߼�
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTERģʽ�²Ż���ʾmVerticalPosition
			if (paddingAnchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// ��ʾ�߾������
			if (paddingAnchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				paddingAnchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
			{
				displayProperty("mDistanceToBoard", "DistanceToBoard");
			}
		}
		else
		{
			displayProperty("mAnchorPoint", "AnchorPoint");
		}

		// ���ڲ�ȷ��hasModifiedProperties��ApplyModifiedProperties�Ժ��Ƿ��ı�,����Ԥ�Ȼ�ȡ
		bool dirty = serializedObject.hasModifiedProperties || modified;
		serializedObject.ApplyModifiedProperties();
		if(dirty)
		{
			EditorUtility.SetDirty(target);
		}
	}
}