using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaddingAnchor), true)]
public class PaddingAnchorEditor : GameEditorBase
{
	private PaddingAnchor anchor;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		anchor = target as PaddingAnchor;
		anchor.mAdjustFont = displayToggle("AdjustFont", anchor.mAdjustFont, out bool adjustFontModified);
		// ������EditorGUILayout.EnumPopup���ַ�ʽ��ʾ���������ڿ����ڱ༭���޸�,���Ҵ���ָ���߼�
		var anchorMode = (ANCHOR_MODE)displayEnum("AnchorMode", anchor.mAnchorMode, out bool anchorModeModified);
		if (anchorMode != anchor.mAnchorMode)
		{
			anchor.setAnchorModeInEditor(anchorMode);
		}
		var relativePos = displayToggle("RelativePosition", anchor.mRelativeDistance, out bool relativePosModified);
		if (relativePos != anchor.mRelativeDistance)
		{
			anchor.setRelativeDistanceInEditor(relativePos);
		}
		// ֻ��ͣ�������ڵ��ĳ��λ��
		bool horiPaddingModified = false;
		bool vertPaddingModified = false;
		if (anchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			var horizontalPadding = (HORIZONTAL_PADDING)displayEnum("HorizontalPaddingSide", anchor.mHorizontalNearSide, out horiPaddingModified);
			if (horizontalPadding != anchor.mHorizontalNearSide)
			{
				anchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			var verticalPadding = (VERTICAL_PADDING)displayEnum("VerticalPaddingSide", anchor.mVerticalNearSide, out vertPaddingModified);
			if (verticalPadding != anchor.mVerticalNearSide)
			{
				anchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTERģʽ�²Ż���ʾmHorizontalPosition
			if (anchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty����ֻ�Ǽ򵥵�ʹ��Ĭ�Ϸ�ʽ��ʾ����,���ڱ༭����ֱ���޸�ֵ,���ܴ����κ������߼�
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTERģʽ�²Ż���ʾmVerticalPosition
			if (anchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// ��ʾ�߾������
			if (anchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				anchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
			{
				displayProperty("mDistanceToBoard", "DistanceToBoard");
			}
		}
		else
		{
			displayProperty("mAnchorPoint", "AnchorPoint");
		}
		// ���ڲ�ȷ��hasModifiedProperties��ApplyModifiedProperties�Ժ��Ƿ��ı�,����Ԥ�Ȼ�ȡ
		bool dirty = serializedObject.hasModifiedProperties || 
					 adjustFontModified || 
					 anchorModeModified || 
					 relativePosModified || 
					 horiPaddingModified || 
					 vertPaddingModified;
		serializedObject.ApplyModifiedProperties();
		if(dirty)
		{
			EditorUtility.SetDirty(target);
		}
	}
}