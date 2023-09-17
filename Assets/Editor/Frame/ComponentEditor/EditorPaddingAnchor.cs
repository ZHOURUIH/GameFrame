using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaddingAnchor), true)]
public class EditorPaddingAnchor : GameEditorBase
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		var paddingAnchor = target as PaddingAnchor;

		bool modified = false;
		var rectTrans = paddingAnchor.gameObject.GetComponent<RectTransform>();
		if (rectTrans != null && !Application.isPlaying)
		{
			bool needRefresh = false;
			Vector3 curPos = rectTrans.localPosition;
			if (!MathUtility.isVectorEqual(paddingAnchor.getLastPosition(), curPos))
			{
				paddingAnchor.setLastPosition(curPos);
				needRefresh = true;
				modified = true;
			}
			Vector2 curSize = WidgetUtility.getRectSize(rectTrans);
			if (!MathUtility.isVectorEqual(paddingAnchor.getLastSize(), curSize))
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

		modified |= toggle("AdjustFont", "�Ƿ�ͬʱ���������С", ref paddingAnchor.mAdjustFont);
		modified |= displayInt("MinFontSize", "�������С�ߴ�", ref paddingAnchor.mMinFontSize);
		// ������EditorGUILayout.EnumPopup���ַ�ʽ��ʾ���������ڿ����ڱ༭���޸�,���Ҵ���ָ���߼�
		var anchorMode = displayEnum("AnchorMode", "ͣ������", paddingAnchor.mAnchorMode);
		if (anchorMode != paddingAnchor.mAnchorMode)
		{
			modified = true;
			paddingAnchor.setAnchorModeInEditor(anchorMode);
		}
		var relativePos = toggle("RelativePosition", "�洢ʹ�����ֵ���Ǿ���ֵ", paddingAnchor.mRelativeDistance);
		if (relativePos != paddingAnchor.mRelativeDistance)
		{
			modified = true;
			paddingAnchor.setRelativeDistanceInEditor(relativePos);
		}
		// ֻ��ͣ�������ڵ��ĳ��λ��
		if (paddingAnchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			HORIZONTAL_PADDING horizontalPadding = displayEnum("HorizontalPaddingSide", "ˮƽͣ������", paddingAnchor.mHorizontalNearSide);
			if (horizontalPadding != paddingAnchor.mHorizontalNearSide)
			{
				modified = true;
				paddingAnchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			VERTICAL_PADDING verticalPadding = displayEnum("VerticalPaddingSide", "��ֱͣ������", paddingAnchor.mVerticalNearSide);
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