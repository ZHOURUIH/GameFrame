﻿using System;
using System.Collections;
using System.Collections.Generic;

public class SCDemo : NetPacketTCPFrame
{
	public UINT mDemoParam = new UINT();
	protected override void fillParams()
	{
		pushParam(mDemoParam);
	}
	public override void execute()
	{
		;
	}
}