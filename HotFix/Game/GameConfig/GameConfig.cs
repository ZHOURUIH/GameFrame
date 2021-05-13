﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameConfig : ConfigBase
{
	public override void writeConfig()
	{
		writeTxtFile(FrameDefine.F_CONFIG_PATH + "GameFloatConfig.txt", generateFloatFile());
		writeTxtFile(FrameDefine.F_CONFIG_PATH + "GameStringConfig.txt", generateStringFile());
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override void addFloat(){}
	protected override void addString(){}
	protected override void readConfig()
	{
		readFile(FrameDefine.F_CONFIG_PATH + "GameFloatConfig.txt", true);
		readFile(FrameDefine.F_CONFIG_PATH + "GameStringConfig.txt", false);
	}
}