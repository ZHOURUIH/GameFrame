using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaddingAnchor), true)]
public class PaddingAnchorEditor : GameEditorBase
{
	protected PaddingAnchor mPaddingAnchor;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		mPaddingAnchor = target as PaddingAnchor;

		bool modified = false;
		displayToggle("AdjustFont", "�Ƿ�ͬʱ���������С", ref mPaddingAnchor.mAdjustFont, ref modified);
		// ������EditorGUILayout.EnumPopup���ַ�ʽ��ʾ���������ڿ����ڱ༭���޸�,���Ҵ���ָ���߼�
		var anchorMode = displayEnum("AnchorMode", "ͣ������", mPaddingAnchor.mAnchorMode, ref modified);
		if (anchorMode != mPaddingAnchor.mAnchorMode)
		{
			mPaddingAnchor.setAnchorModeInEditor(anchorMode);
		}
		var relativePos = displayToggle("RelativePosition", "�洢ʹ�����ֵ���Ǿ���ֵ", mPaddingAnchor.mRelativeDistance, ref modified);
		if (relativePos != mPaddingAnchor.mRelativeDistance)
		{
			mPaddingAnchor.setRelativeDistanceInEditor(relativePos);
		}
		// ֻ��ͣ�������ڵ��ĳ��λ��
		if (mPaddingAnchor.mAnchorMode == ANCHOR_MODE.PADDING_PARENT_SIDE)
		{
			HORIZONTAL_PADDING horizontalPadding = displayEnum("HorizontalPaddingSide", "ˮƽͣ������", mPaddingAnchor.mHorizontalNearSide, ref modified);
			if (horizontalPadding != mPaddingAnchor.mHorizontalNearSide)
			{
				mPaddingAnchor.setHorizontalNearSideInEditor(horizontalPadding);
			}
			VERTICAL_PADDING verticalPadding = displayEnum("VerticalPaddingSide", "��ֱͣ������", mPaddingAnchor.mVerticalNearSide, ref modified);
			if (verticalPadding != mPaddingAnchor.mVerticalNearSide)
			{
				mPaddingAnchor.setVerticalNearSideInEditor(verticalPadding);
			}
			// HPS_CENTERģʽ�²Ż���ʾmHorizontalPosition
			if (mPaddingAnchor.mHorizontalNearSide == HORIZONTAL_PADDING.CENTER)
			{
				// displayProperty����ֻ�Ǽ򵥵�ʹ��Ĭ�Ϸ�ʽ��ʾ����,���ڱ༭����ֱ���޸�ֵ,���ܴ����κ������߼�
				displayProperty("mHorizontalPositionRelative", "HorizontalPositionRelative");
				displayProperty("mHorizontalPositionAbsolute", "HorizontalPositionAbsolute");
			}
			// VPS_CENTERģʽ�²Ż���ʾmVerticalPosition
			if (mPaddingAnchor.mVerticalNearSide == VERTICAL_PADDING.CENTER)
			{
				displayProperty("mVerticalPositionRelative", "VerticalPositionRelative");
				displayProperty("mVerticalPositionAbsolute", "VerticalPositionAbsolute");
			}
			// ��ʾ�߾������
			if (mPaddingAnchor.mHorizontalNearSide != HORIZONTAL_PADDING.CENTER ||
				mPaddingAnchor.mVerticalNearSide != VERTICAL_PADDING.CENTER)
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