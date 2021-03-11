﻿using System;
using System.Collections.Generic;

public class UINT : OBJECT
{
	protected const int TYPE_SIZE = sizeof(uint);
	public uint mValue;
	public UINT()
	{
		mType = Typeof<uint>();
		mSize = TYPE_SIZE;
	}
	public UINT(uint value)
	{
		mValue = value;
		mType = Typeof<uint>();
		mSize = TYPE_SIZE;
	}
	public override void zero() { mValue = 0; }
	public void set(uint value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readUInt(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeUInt(buffer, ref index, mValue);
	}
}