﻿using static MathUtility;

// 指数曲线
public class CurveExpoInOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (isFloatZero(time))
		{
			return 0.0f;
		}
		if (isFloatEqual(time, 1.0f))
		{
			return 1.0f;
		}
		if (time < 2.0f)
		{
			return 0.5f * pow(2.0f, 10.0f * (time - 1.0f));
		}
		else
		{
			--time;
			return 0.5f * (-pow(2.0f, -10.0f * time) + 2.0f);
		}
	}
}