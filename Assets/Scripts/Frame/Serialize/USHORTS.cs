﻿using System;
using System.Collections.Generic;

// 自定义的对ushort[]的封装,可用于序列化
public class USHORTS : SerializableBit
{
	public List<ushort> mValue = new List<ushort>();
	public ushort this[int index]
	{
		get { return mValue[index]; }
		set { mValue[index] = value; }
	}
	public int Count { get { return mValue.Count; } }
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue.Clear(); 
	}
	public override bool read(SerializerBitRead reader)
	{
		return reader.readList(mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.writeList(mValue);
	}
	public void Add(ushort value)
	{
		mValue.Add(value);
	}
}