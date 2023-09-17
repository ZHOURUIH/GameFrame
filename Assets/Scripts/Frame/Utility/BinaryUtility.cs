﻿using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using static StringUtility;
using static MathUtility;
using static CSharpUtility;

public class BinaryUtility
{
	protected static Encoding ENCODING_GB2312;
	protected static Encoding ENCODING_GBK;
	// CRC table for the CRC-16. The poly is 0x8005 (x^16 + x^15 + x^2 + 1)
	protected static ushort[] mCRC16Table;
	protected static ThreadLock mCRC16TableLock = new ThreadLock();
	// 下标是sizeof(T),value是此T类型的bit长度所需要的长度表示
	// 比如1个char的取值范围是-127~127,去除符号以后是0~127,127占7个bit,可以使用3个bit存储这个7,所以长度位就是用3个bit来表示,所以下标1的值是3
	protected static int[] SIGNED_LENGTH_MAX_BIT = new int[9] { 0, 3, 4, 0, 5, 0, 0, 0, 6 };
	// 比如1个byte的取值范围是8~255,255占8个bit,可以使用4个bit存储这个8,所以长度位就是用4个bit来表示,所以下标1的值是4
	// 其作用于mBitCountTable类似,只不过不需要计算
	protected static int[] UNSIGNED_LENGTH_MAX_BIT = new int[9] { 0, 4, 5, 0, 6, 0, 0, 0, 7 };
	// 0到65535的每个数中的最高位1的下标,也就是需要用几个bit来表示,1可以用1个bit表示,5可以使用3个bit表示,8可以使用5个bit表示
	protected static byte[] mBitCountTable;
	protected static ThreadLock mBitCountTableLock = new ThreadLock();
	public static Encoding getGB2312()
	{
		if (ENCODING_GB2312 == null)
		{
			ENCODING_GB2312 = Encoding.GetEncoding("gb2312");
		}
		return ENCODING_GB2312;
	}
	public static Encoding getGBK()
	{
		if (ENCODING_GBK == null)
		{
			ENCODING_GBK = Encoding.GetEncoding("GBK");
		}
		return ENCODING_GBK;
	}
	// 计算 16进制的c中1的个数
	public static int bitCount1(byte c)
	{
		int count = 0;
		int bitCount = sizeof(char) * 8;
		for (int i = 0; i < bitCount; ++i)
		{
			if ((c & (1 << i)) > 0)
			{
				++count;
			}
		}
		return count;
	}
	// 字节数组buffer中是否包含字节数组key,类似于string.Contains
	public static bool contains(byte[] buffer, byte[] key)
	{
		if (buffer == null || key == null)
		{
			return false;
		}
		int keyLength = key.Length;
		int bufferLength = buffer.Length - keyLength + 1;
		for (int i = 0; i < bufferLength; ++i)
		{
			bool find = true;
			for (int j = 0; j < keyLength; ++j)
			{
				if (buffer[i + j] != key[j])
				{
					find = false;
					break;
				}
			}
			if (find)
			{
				return true;
			}
		}
		return false;
	}
	public static ushort crc16(ushort crc, byte[] buffer, int len, int bufferOffset = 0)
	{
		for (int i = 0; i < len; ++i)
		{
			crc = crc16_byte(crc, buffer[bufferOffset + i]);
		}
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0)
	{
		crc = crc16_byte(crc, byte0);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1, byte byte2, byte byte3)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		crc = crc16_byte(crc, byte2);
		crc = crc16_byte(crc, byte3);
		return crc;
	}
	public static ushort crc16(ushort crc, byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)
	{
		crc = crc16_byte(crc, byte0);
		crc = crc16_byte(crc, byte1);
		crc = crc16_byte(crc, byte2);
		crc = crc16_byte(crc, byte3);
		crc = crc16_byte(crc, byte4);
		crc = crc16_byte(crc, byte5);
		crc = crc16_byte(crc, byte6);
		crc = crc16_byte(crc, byte7);
		return crc;
	}
	public static ushort crc16_byte(ushort crc, byte data)
	{
		if (mCRC16Table == null)
		{
			initCRC16();
		}
		return (ushort)((crc >> 8) ^ mCRC16Table[(crc ^ data) & 0xFF]);
	}
	public static bool readBool(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(bool);
		if (!success)
		{
			return false;
		}
		return buffer[index++] != 0;
	}
	public static sbyte readSByte(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(sbyte);
		if (!success)
		{
			return 0;
		}
		return (sbyte)buffer[index++];
	}
	public static byte readByte(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(byte);
		if (!success)
		{
			return 0;
		}
		return buffer[index++];
	}
	public static short readShort(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static short readShortBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(short);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(short);
		return bytesToShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShort(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShort(buffer[pre + 0], buffer[pre + 1]);
	}
	public static ushort readUShortBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ushort);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ushort);
		return bytesToUShortBigEndian(buffer[pre + 0], buffer[pre + 1]);
	}
	public static int readInt(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static int readIntBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(int);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(int);
		return bytesToIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUInt(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUInt(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static uint readUIntBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(uint);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(uint);
		return bytesToUIntBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3]);
	}
	public static long readLong(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
							buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static long readLongBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(long);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(long);
		return bytesToLongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
									buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULong(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULong(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
							buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static ulong readULongBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success)
	{
		success = bufferSize >= index + sizeof(ulong);
		if (!success)
		{
			return 0;
		}
		int pre = index;
		index += sizeof(ulong);
		return bytesToULongBigEndian(buffer[pre + 0], buffer[pre + 1], buffer[pre + 2], buffer[pre + 3],
									buffer[pre + 4], buffer[pre + 5], buffer[pre + 6], buffer[pre + 7]);
	}
	public static float readFloat(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		byte[] floatBuffer = helpBuffer;
		if (floatBuffer == null)
		{
			floatBuffer = new byte[sizeof(float)];
		}
		floatBuffer[0] = buffer[index + 0];
		floatBuffer[1] = buffer[index + 1];
		floatBuffer[2] = buffer[index + 2];
		floatBuffer[3] = buffer[index + 3];
		index += sizeof(float);
		return bytesToFloat(floatBuffer);
	}
	public static float readFloatBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(float);
		if (!success)
		{
			return 0.0f;
		}
		byte[] floatBuffer = helpBuffer;
		if (floatBuffer == null)
		{
			floatBuffer = new byte[sizeof(float)];
		}
		floatBuffer[0] = buffer[index + 3];
		floatBuffer[1] = buffer[index + 2];
		floatBuffer[2] = buffer[index + 1];
		floatBuffer[3] = buffer[index + 0];
		index += sizeof(float);
		return bytesToFloat(floatBuffer);
	}
	public static double readDouble(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(double);
		if (!success)
		{
			return 0.0f;
		}
		byte[] doubleBuffer = helpBuffer;
		if (doubleBuffer == null)
		{
			doubleBuffer = new byte[sizeof(double)];
		}
		doubleBuffer[0] = buffer[index + 0];
		doubleBuffer[1] = buffer[index + 1];
		doubleBuffer[2] = buffer[index + 2];
		doubleBuffer[3] = buffer[index + 3];
		doubleBuffer[4] = buffer[index + 4];
		doubleBuffer[5] = buffer[index + 5];
		doubleBuffer[6] = buffer[index + 6];
		doubleBuffer[7] = buffer[index + 7];
		index += sizeof(double);
		return bytesToDouble(doubleBuffer);
	}
	public static double readDoubleBigEndian(byte[] buffer, int bufferSize, ref int index, out bool success, byte[] helpBuffer = null)
	{
		success = bufferSize >= index + sizeof(double);
		if (!success)
		{
			return 0.0f;
		}
		byte[] doubleBuffer = helpBuffer;
		if (doubleBuffer == null)
		{
			doubleBuffer = new byte[sizeof(double)];
		}
		doubleBuffer[0] = buffer[index + 7];
		doubleBuffer[1] = buffer[index + 6];
		doubleBuffer[2] = buffer[index + 5];
		doubleBuffer[3] = buffer[index + 4];
		doubleBuffer[4] = buffer[index + 3];
		doubleBuffer[5] = buffer[index + 2];
		doubleBuffer[6] = buffer[index + 1];
		doubleBuffer[7] = buffer[index + 0];
		index += sizeof(double);
		return bytesToDouble(doubleBuffer);
	}
	// readCount表示读取的bool的个数,小于0表示按照destBuffer数组长度读取
	public static bool readBools(byte[] buffer, int bufferSize, ref int index, bool[] destBuffer, int readCount = -1)
	{
		int boolCount = readCount < 0 ? destBuffer.Length : readCount;
		if (boolCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < boolCount; ++i)
		{
			destBuffer[i] = readBool(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool value)
	{
		// bool固定读取1位
		if (bitCountToByteCount(bitIndex + 1) > bufferSize)
		{
			value = false;
			return false;
		}
		value = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
		++bitIndex;
		return true;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out sbyte value)
	{
		value = (sbyte)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(sbyte));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out byte value)
	{
		value = (byte)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(byte));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out short value)
	{
		value = (short)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(short));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out ushort value)
	{
		value = (ushort)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(ushort));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out int value)
	{
		value = (int)readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(int));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out uint value)
	{
		value = (uint)readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(uint));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out long value)
	{
		value = readSignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(long));
		return success;
	}
	public static bool readBit(byte[] buffer, int bufferSize, ref int bitIndex, out ulong value)
	{
		value = readUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, out bool success, sizeof(ulong));
		return success;
	}
	// 因为long是最大的带符号整型类型,所以可以使用long传递任何带符号整数类型的值
	public static long readSignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool success, int typeSize)
	{
		success = false;
		if (!readSignedBitLength(buffer, bufferSize, ref bitIndex, typeSize, out int bitCount))
		{
			return 0;
		}
		if (bitCount == 0)
		{
			success = true;
			return 0;
		}
		
		if (bitCountToByteCount(bitIndex + 1 + bitCount) > bufferSize)
		{
			return 0;
		}

		// 读符号位
		bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
		++bitIndex;

		// 读取值
		long value = 0;
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref value, i);
			}
		}
		bitIndex += bitCount;
		if (isNegative)
		{
			value = -value;
		}
		success = true;
		return value;
	}
	// 因为long是最大的无符号整型类型,所以可以使用long传递任何无符号整数类型的值
	public static ulong readUnsignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, out bool success, int typeSize)
	{
		success = false;
		if (!readUnsignedBitLength(buffer, bufferSize, ref bitIndex, typeSize, out int bitCount))
		{
			return 0;
		}
		if (bitCount == 0)
		{
			success = true;
			return 0;
		}
		if (bitCountToByteCount(bitIndex + bitCount) > bufferSize)
		{
			return 0;
		}

		// 读取值
		ulong value = 0;
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref value, i);
			}
		}
		bitIndex += bitCount;
		success = true;
		return value;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<sbyte> list)
	{
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(sbyte), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			// 读符号位
			bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
			++bitIndex;
			sbyte tempValue = 0;
			for (int j = 0; j < bitCount; ++j)
			{
				if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
				{
					setBitOne(ref tempValue, j);
				}
			}
			bitIndex += bitCount;
			if (isNegative)
			{
				tempValue = (sbyte)-tempValue;
			}
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<byte> list)
	{
		if (!readUnsignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(byte), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			byte tempValue = 0;
			for (int j = 0; j < bitCount; ++j)
			{
				if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
				{
					setBitOne(ref tempValue, j);
				}
			}
			bitIndex += bitCount;
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<short> list)
	{
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(short), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			short tempValue = 0;
			if (bitCount > 0)
			{
				// 读符号位
				bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
				++bitIndex;
				for (int j = 0; j < bitCount; ++j)
				{
					if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
					{
						setBitOne(ref tempValue, j);
					}
				}
				bitIndex += bitCount;
				if (isNegative)
				{
					tempValue = (short)-tempValue;
				}
			}
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<ushort> list)
	{
		if (!readUnsignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(ushort), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			ushort tempValue = 0;
			for (int j = 0; j < bitCount; ++j)
			{
				if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
				{
					setBitOne(ref tempValue, j);
				}
			}
			bitIndex += bitCount;
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<int> list)
	{
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(int), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			int tempValue = 0;
			if (bitCount > 0)
			{
				// 读符号位
				bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
				++bitIndex;
				for (int j = 0; j < bitCount; ++j)
				{
					if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
					{
						setBitOne(ref tempValue, j);
					}
				}
				bitIndex += bitCount;
				if (isNegative)
				{
					tempValue = -tempValue;
				}
			}
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<uint> list)
	{
		if (!readUnsignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(uint), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			uint tempValue = 0;
			for (int j = 0; j < bitCount; ++j)
			{
				if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
				{
					setBitOne(ref tempValue, j);
				}
			}
			bitIndex += bitCount;
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<long> list)
	{
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(long), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			long tempValue = 0;
			if (bitCount > 0)
			{
				// 读符号位
				bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
				++bitIndex;
				for (int j = 0; j < bitCount; ++j)
				{
					if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
					{
						setBitOne(ref tempValue, j);
					}
				}
				bitIndex += bitCount;
				if (isNegative)
				{
					tempValue = -tempValue;
				}
			}
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<ulong> list)
	{
		if (!readUnsignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(ulong), out int count, out int bitCount))
		{
			return false;
		}

		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			ulong tempValue = 0;
			for (int j = 0; j < bitCount; ++j)
			{
				if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
				{
					setBitOne(ref tempValue, j);
				}
			}
			bitIndex += bitCount;
			list.Add(tempValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<float> list, int precision = 3)
	{
		// 这里本来应该是sizeof(int),因为float最终会转换为int存储,只不过sizeof(int)跟sizeof(float)一样,所以就写成与参数类型一致的float,下面的double同理
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(float), out int count, out int bitCount))
		{
			return false;
		}

		float powValue = 1.0f / pow10(precision);
		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			int tempValue = 0;
			if (bitCount > 0)
			{
				// 读符号位
				bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
				++bitIndex;
				for (int j = 0; j < bitCount; ++j)
				{
					if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
					{
						setBitOne(ref tempValue, j);
					}
				}
				bitIndex += bitCount;
				if (isNegative)
				{
					tempValue = -tempValue;
				}
			}
			list.Add(tempValue * powValue);
		}
		return true;
	}
	public static bool readBitList(byte[] buffer, int bufferSize, ref int bitIndex, List<double> list, int precision = 4)
	{
		if (!readSignedListBitLength(buffer, bufferSize, ref bitIndex, sizeof(double), out int count, out int bitCount))
		{
			return false;
		}

		double powValue = 1.0 / pow10Long(precision);
		// 读取所有元素,每个元素占的bit数量固定
		for (int i = 0; i < count; ++i)
		{
			long tempValue = 0;
			if (bitCount > 0)
			{
				// 读符号位
				bool isNegative = hasBit(buffer[bitIndex >> 3], bitIndex & 7);
				++bitIndex;
				for (int j = 0; j < bitCount; ++j)
				{
					if (hasBit(buffer[(bitIndex + j) >> 3], (bitIndex + j) & 7))
					{
						setBitOne(ref tempValue, j);
					}
				}
				bitIndex += bitCount;
				if (isNegative)
				{
					tempValue = -tempValue;
				}
			}
			list.Add(tempValue * powValue);
		}
		return true;
	}
	public static bool readBytes(byte[] buffer, ref int index, byte[] destBuffer, int bufferSize = -1, int destBufferSize = -1, int readSize = -1)
	{
		if (bufferSize == -1)
		{
			bufferSize = buffer.Length;
		}
		if (destBufferSize == -1)
		{
			destBufferSize = destBuffer.Length;
		}
		if (readSize == -1)
		{
			readSize = destBuffer.Length;
		}
		if (destBufferSize < readSize || readSize + index > bufferSize)
		{
			return false;
		}
		readSize = getMin(readSize, destBufferSize);
		if (readSize > 0)
		{
			memcpy(destBuffer, buffer, 0, index, readSize);
			index += readSize;
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShorts(byte[] buffer, int bufferSize, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShort(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的short个数,小于0表示使用数组长度
	public static bool readShortsBigEndian(byte[] buffer, int bufferSize, ref int index, short[] destBuffer, int readCount = -1)
	{
		int shortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (shortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < shortCount; ++i)
		{
			destBuffer[i] = readShortBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShorts(byte[] buffer, int bufferSize, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShort(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ushort个数,小于0表示使用数组长度
	public static bool readUShortsBigEndian(byte[] buffer, int bufferSize, ref int index, ushort[] destBuffer, int readCount = -1)
	{
		int ushortCount = readCount < 0 ? destBuffer.Length : readCount;
		if (ushortCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < ushortCount; ++i)
		{
			destBuffer[i] = readUShortBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readInts(byte[] buffer, int bufferSize, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readInt(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的int个数,小于0表示使用destBuffer数组长度
	public static bool readIntsBigEndian(byte[] buffer, int bufferSize, ref int index, int[] destBuffer, int readCount = -1)
	{
		int intCount = readCount < 0 ? destBuffer.Length : readCount;
		if (intCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < intCount; ++i)
		{
			destBuffer[i] = readIntBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUInts(byte[] buffer, int bufferSize, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUInt(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的uint个数,小于0表示使用数组长度
	public static bool readUIntsBigEndian(byte[] buffer, int bufferSize, ref int index, uint[] destBuffer, int readCount = -1)
	{
		int uintCount = readCount < 0 ? destBuffer.Length : readCount;
		if (uintCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < uintCount; ++i)
		{
			destBuffer[i] = readUIntBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongs(byte[] buffer, int bufferSize, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLong(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的long个数,小于0表示使用数组长度
	public static bool readLongsBigEndian(byte[] buffer, int bufferSize, ref int index, long[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readLongBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongs(byte[] buffer, int bufferSize, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULong(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示要读取的ulong个数,小于0表示使用数组长度
	public static bool readULongsBigEndian(byte[] buffer, int bufferSize, ref int index, ulong[] destBuffer, int readCount = -1)
	{
		int longCount = readCount < 0 ? destBuffer.Length : readCount;
		if (longCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < longCount; ++i)
		{
			destBuffer[i] = readULongBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloats(byte[] buffer, int bufferSize, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloat(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的float的数量,小于0表示使用数组的长度
	public static bool readFloatsBigEndian(byte[] buffer, int bufferSize, ref int index, float[] destBuffer, int readCount = -1)
	{
		int floatCount = readCount < 0 ? destBuffer.Length : readCount;
		if (floatCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < floatCount; ++i)
		{
			destBuffer[i] = readFloatBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的double的数量,小于0表示使用数组的长度
	public static bool readDoubles(byte[] buffer, int bufferSize, ref int index, double[] destBuffer, int readCount = -1)
	{
		int doubleCount = readCount < 0 ? destBuffer.Length : readCount;
		if (doubleCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < doubleCount; ++i)
		{
			destBuffer[i] = readDouble(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	// readCount表示读取的double的数量,小于0表示使用数组的长度
	public static bool readDoublesBigEndian(byte[] buffer, int bufferSize, ref int index, double[] destBuffer, int readCount = -1)
	{
		int doubleCount = readCount < 0 ? destBuffer.Length : readCount;
		if (doubleCount > destBuffer.Length)
		{
			return false;
		}
		for (int i = 0; i < doubleCount; ++i)
		{
			destBuffer[i] = readDoubleBigEndian(buffer, bufferSize, ref index, out bool success);
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, bool value)
	{
		if (bitCountToByteCount(bitIndex + 1) > bufferSize)
		{
			return false;
		}
		// 固定只写入1位
		if (value)
		{
			setBitOne(ref buffer[bitIndex >> 3], bitIndex & 7);
		}
		++bitIndex;
		return true;
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, byte value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(byte), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, sbyte value)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(sbyte), generateBitCount(abs(value)), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, ushort value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(ushort), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, short value)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(short), generateBitCount(abs(value)), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, uint value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(uint), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, int value)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(int), generateBitCount(abs(value)), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, ulong value)
	{
		return writeUnsignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(ulong), generateBitCount(value), value);
	}
	public static bool writeBit(byte[] buffer, int bufferSize, ref int bitIndex, long value)
	{
		return writeSignedIntegerBit(buffer, bufferSize, ref bitIndex, sizeof(long), generateBitCount(abs(value)), value);
	}
	// 因为long是最大的带符号整型类型,所以可以使用long传递任何带符号整数类型的值
	public static bool writeSignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, byte bitCount, long value)
	{
		if ((value != 0 && bitCount == 0) || bitCount > typeSize << 3)
		{
			return false;
		}

		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, typeSize, bitCount))
		{
			return false;
		}
		if (bitCount == 0)
		{
			return true;
		}

		bool isNegative = value < 0;
		if (isNegative)
		{
			value = -value;
		}

		if (bitCountToByteCount(bitIndex + 1 + bitCount) > bufferSize)
		{
			return false;
		}
		// 写入符号位
		if (isNegative)
		{
			setBitOne(ref buffer[bitIndex >> 3], bitIndex & 7);
		}
		++bitIndex;
		// 再写入值的所有位
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(value, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += bitCount;
		return true;
	}
	// 因为long是最大的无符号整型类型,所以可以使用long传递任何无符号整数类型的值
	public static bool writeUnsignedIntegerBit(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, byte bitCount, ulong value)
	{
		if ((value != 0 && bitCount == 0) || bitCount > typeSize << 3)
		{
			return false;
		}
		if (!writeUnsignedBitLength(buffer, bufferSize, ref bitIndex, typeSize, bitCount))
		{
			return false;
		}

		if (bitCountToByteCount(bitIndex + bitCount) > bufferSize)
		{
			return false;
		}
		// 再写入值的所有位
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(value, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += bitCount;
		return true;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<byte> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMax(list));
		if (!writeUnsignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(byte), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<sbyte> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMaxAbs(list));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(sbyte), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<short> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMaxAbs(list));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(short), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ushort> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMax(list));
		if (!writeUnsignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(ushort), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<int> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMaxAbs(list));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(int), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<uint> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMax(list));
		if (!writeUnsignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(uint), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<long> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMaxAbs(list));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(long), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<ulong> list)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int bitCount = generateBitCount(findMax(list));
		if (!writeUnsignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(ulong), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeUnsignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, list[i], bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<float> list, int precision = 3)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		int powValue = pow10(precision);
		int bitCount = generateBitCount(round(findMaxAbs(list) * powValue));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(int), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, round(list[i] * powValue), bitCount);
		}
		return result;
	}
	public static bool writeListBit(byte[] buffer, int bufferSize, ref int bitIndex, List<double> list, int precision = 4)
	{
		// 写入长度
		int count = list.Count;
		if (!writeBit(buffer, bufferSize, ref bitIndex, count))
		{
			return false;
		}
		if (count == 0)
		{
			return true;
		}

		// 找出其中最大的值,计算出最大值的位数量,所有的值都使用这个位数量来存储
		long powValue = pow10Long(precision);
		int bitCount = generateBitCount(round(findMaxAbs(list) * powValue));
		if (!writeSignedBitLength(buffer, bufferSize, ref bitIndex, sizeof(long), bitCount))
		{
			return false;
		}
		// 写入列表中所有的值
		bool result = true;
		for (int i = 0; i < count; ++i)
		{
			result = result && writeSignedIntegerBitNoLengthBit(buffer, bufferSize, ref bitIndex, round(list[i] * powValue), bitCount);
		}
		return result;
	}
	public static bool wrtiteBufferBit(byte[] buffer, int bufferSize, ref int bitIndex, byte[] srcBuffer, int writeCount)
	{
		if (writeCount == 0 || srcBuffer == null)
		{
			return true;
		}
		// 将前面空出来的位填充为0
		fillZeroToByteEnd(buffer, ref bitIndex);
		int curByte = bitIndex >> 3;
		bool result = writeBytes(buffer, ref curByte, srcBuffer, -1, -1, writeCount);
		bitIndex = curByte << 3;
		return result;
	}
	public static void fillZeroToByteEnd(byte[] buffer, ref int bitIndex)
	{
		// 将前面空出来的位填充为0
		int curByte = bitCountToByteCount(bitIndex);
		int targetBitIndex = curByte << 3;
		for (int i = bitIndex; i < targetBitIndex; ++i)
		{
			setBitZero(ref buffer[i >> 3], i & 7);
		}
		bitIndex = curByte << 3;
	}
	public static bool writeBool(byte[] buffer, int bufferSize, ref int index, bool value)
	{
		if (bufferSize < index + sizeof(bool))
		{
			return false;
		}
		buffer[index++] = (byte)(value ? 1 : 0);
		return true;
	}
	public static bool writeByte(byte[] buffer, int bufferSize, ref int index, byte value)
	{
		if (bufferSize < index + sizeof(byte))
		{
			return false;
		}
		buffer[index++] = value;
		return true;
	}
	public static bool writeShort(byte[] buffer, int bufferSize, ref int index, short value)
	{
		if (bufferSize < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeShortBigEndian(byte[] buffer, int bufferSize, ref int index, short value)
	{
		if (bufferSize < index + sizeof(short))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeUShort(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		return true;
	}
	public static bool writeUShortBigEndian(byte[] buffer, int bufferSize, ref int index, ushort value)
	{
		if (bufferSize < index + sizeof(ushort))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeInt(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		return true;
	}
	public static bool writeIntBigEndian(byte[] buffer, int bufferSize, ref int index, int value)
	{
		if (bufferSize < index + sizeof(int))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeUInt(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		return true;
	}
	public static bool writeUIntBigEndian(byte[] buffer, int bufferSize, ref int index, uint value)
	{
		if (bufferSize < index + sizeof(uint))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeLong(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)(((0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)(((0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)(((0xFF << (8 * 7)) & value) >> (8 * 7));
		return true;
	}
	public static bool writeLongBigEndian(byte[] buffer, int bufferSize, ref int index, long value)
	{
		if (bufferSize < index + sizeof(long))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)(((0xFF << (8 * 7)) & value) >> (8 * 7));
		buffer[index++] = (byte)(((0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)(((0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)(((0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeULong(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 0)) & value) >> (8 * 0));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 7)) & value) >> (8 * 7));
		return true;
	}
	public static bool writeULongBigEndian(byte[] buffer, int bufferSize, ref int index, ulong value)
	{
		if (bufferSize < index + sizeof(ulong))
		{
			return false;
		}
		// 为了获得最快速度,不使用for循环
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 7)) & value) >> (8 * 7));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 6)) & value) >> (8 * 6));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 5)) & value) >> (8 * 5));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 4)) & value) >> (8 * 4));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 3)) & value) >> (8 * 3));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 2)) & value) >> (8 * 2));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 1)) & value) >> (8 * 1));
		buffer[index++] = (byte)((((ulong)0xFF << (8 * 0)) & value) >> (8 * 0));
		return true;
	}
	public static bool writeFloat(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[0];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[3];
		return true;
	}
	public static bool writeFloatBigEndian(byte[] buffer, int bufferSize, ref int index, float value)
	{
		if (bufferSize < index + sizeof(float))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[0];
		return true;
	}
	public static bool writeDouble(byte[] buffer, int bufferSize, ref int index, double value)
	{
		if (bufferSize < index + sizeof(double))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[0];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[4];
		buffer[index++] = valueByte[5];
		buffer[index++] = valueByte[6];
		buffer[index++] = valueByte[7];
		return true;
	}
	public static bool writeDoubleBigEndian(byte[] buffer, int bufferSize, ref int index, double value)
	{
		if (bufferSize < index + sizeof(double))
		{
			return false;
		}
		byte[] valueByte = toBytes(value);
		// 为了获得最快速度,不使用for循环
		buffer[index++] = valueByte[7];
		buffer[index++] = valueByte[6];
		buffer[index++] = valueByte[5];
		buffer[index++] = valueByte[4];
		buffer[index++] = valueByte[3];
		buffer[index++] = valueByte[2];
		buffer[index++] = valueByte[1];
		buffer[index++] = valueByte[0];
		return true;
	}
	public static bool writeVector2(byte[] buffer, int bufferSize, ref int index, Vector2 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2UShort(byte[] buffer, int bufferSize, ref int index, Vector2UShort value)
	{
		bool result = writeUShort(buffer, bufferSize, ref index, value.x);
		result = result && writeUShort(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector2Int(byte[] buffer, int bufferSize, ref int index, Vector2Int value)
	{
		bool result = writeInt(buffer, bufferSize, ref index, value.x);
		result = result && writeInt(buffer, bufferSize, ref index, value.y);
		return result;
	}
	public static bool writeVector3(byte[] buffer, int bufferSize, ref int index, Vector3 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		result = result && writeFloat(buffer, bufferSize, ref index, value.z);
		return result;
	}
	public static bool writeVector4(byte[] buffer, int bufferSize, ref int index, Vector4 value)
	{
		bool result = writeFloat(buffer, bufferSize, ref index, value.x);
		result = result && writeFloat(buffer, bufferSize, ref index, value.y);
		result = result && writeFloat(buffer, bufferSize, ref index, value.z);
		result = result && writeFloat(buffer, bufferSize, ref index, value.w);
		return result;
	}
	// writeCount表示要写入的bool个数,小于0表示将整个数组全部写入
	public static bool writeBools(byte[] buffer, int bufferSize, ref int index, bool[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int boolCount = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < boolCount; ++i)
		{
			ret = writeBool(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static bool writeBytes(byte[] buffer, ref int index, byte[] sourceBuffer, int bufferSize = -1, int sourceBufferSize = -1, int writeSize = -1)
	{
		if (bufferSize == 0 || buffer == null)
		{
			return true;
		}
		if (bufferSize == -1)
		{
			bufferSize = buffer.Length;
		}
		if (sourceBufferSize == -1)
		{
			sourceBufferSize = sourceBuffer.Length;
		}
		if (writeSize == -1)
		{
			writeSize = sourceBuffer.Length;
		}
		if (writeSize > sourceBufferSize || writeSize + index > bufferSize)
		{
			return false;
		}
		memcpy(buffer, sourceBuffer, index, 0, writeSize);
		index += writeSize;
		return true;
	}
	// writeCount表示要写入的short个数,小于0表示写入整个数组
	public static bool writeShorts(byte[] buffer, int bufferSize, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShort(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的short个数,小于0表示写入整个数组
	public static bool writeShortsBigEndian(byte[] buffer, int bufferSize, ref int index, short[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeShortBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShorts(byte[] buffer, int bufferSize, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShort(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ushort个数,小于0表示写入整个数组
	public static bool writeUShortsBigEndian(byte[] buffer, int bufferSize, ref int index, ushort[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUShortBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeInts(byte[] buffer, int bufferSize, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeInt(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的int个数,小于0表示写入整个数组
	public static bool writeIntsBigEndian(byte[] buffer, int bufferSize, ref int index, int[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeIntBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUInts(byte[] buffer, int bufferSize, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUInt(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的uint个数,小于0表示写入整个数组
	public static bool writeUIntsBigEndian(byte[] buffer, int bufferSize, ref int index, uint[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeUIntBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongs(byte[] buffer, int bufferSize, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLong(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的long的个数,小于0表示写入整个数组
	public static bool writeLongsBigEndian(byte[] buffer, int bufferSize, ref int index, long[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeLongBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongs(byte[] buffer, int bufferSize, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULong(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的ulong的个数,小于0表示写入整个数组
	public static bool writeULongsBigEndian(byte[] buffer, int bufferSize, ref int index, ulong[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeULongBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloats(byte[] buffer, int bufferSize, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloat(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	// writeCount表示要写入的浮点数的数量,小于0表示写入整个数组
	public static bool writeFloatsBigEndian(byte[] buffer, int bufferSize, ref int index, float[] sourceBuffer, int writeCount = -1)
	{
		bool ret = true;
		int count = writeCount < 0 ? sourceBuffer.Length : writeCount;
		for (int i = 0; i < count; ++i)
		{
			ret = writeFloatBigEndian(buffer, bufferSize, ref index, sourceBuffer[i]) && ret;
		}
		return ret;
	}
	public static void memcpyObject<T>(T[] dest, T[] src, int destOffset, int srcOffset, int count)
	{
		for (int i = 0; i < count; ++i)
		{
			dest[destOffset + i] = src[srcOffset + i];
		}
	}
	public static void memcpy(Array dest, Array src, int destByteOffset, int srcByteOffset, int byteCount)
	{
		Buffer.BlockCopy(src, srcByteOffset, dest, destByteOffset, byteCount);
	}
	public static void memmove<T>(ref T[] data, int dest, int src, int count)
	{
		if (count <= 0)
		{
			return;
		}
		// 如果两个内存区有相交的部分,并且源地址在前面,则从后面往前拷贝字节
		if (src < dest && src + count > dest)
		{
			for (int i = 0; i < count; ++i)
			{
				data[count - i - 1 + dest] = data[count - i - 1 + src];
			}
		}
		else
		{
			for (int i = 0; i < count; ++i)
			{
				data[i + dest] = data[i + src];
			}
		}
	}
	// 将数组每个元素值设置为指定值
	public static void memset<T>(T[] p, T value, int length = -1)
	{
		if (length == -1)
		{
			length = p.Length;
		}
		for (int i = 0; i < length; ++i)
		{
			p[i] = value;
		}
	}
	public static void memset<T>(T[] p, T value, int startIndex, int length)
	{
		if (length == -1 || length > p.Length - startIndex)
		{
			length = p.Length - startIndex;
		}
		for (int i = 0; i < length; ++i)
		{
			p[startIndex + i] = value;
		}
	}
	public static void ushortToBytes(ushort value, byte[] bytes)
	{
		if (bytes.Length != sizeof(ushort))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void ushortToBytes(ushort value, out byte byte0, out byte byte1)
	{
		// 为了获得最快速度,不使用for循环
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void shortToBytes(short value, byte[] bytes)
	{
		if (bytes.Length != sizeof(short))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void shortToBytes(short value, out byte byte0, out byte byte1)
	{
		// 为了获得最快速度,不使用for循环
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
	}
	public static void shortToBytesBigEndian(short value, byte[] bytes)
	{
		if (bytes.Length != sizeof(short))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[1] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static void intToBytes(int value, byte[] bytes)
	{
		if (bytes.Length != sizeof(int))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
	}
	public static void intToBytes(int value, out byte byte0, out byte byte1, out byte byte2, out byte byte3)
	{
		byte0 = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		byte1 = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		byte2 = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		byte3 = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
	}
	public static void intToBytesBigEndian(int value, byte[] bytes)
	{
		if (bytes.Length != sizeof(int))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		bytes[1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static void uintToBytes(uint value, byte[] bytes)
	{
		if (bytes.Length != sizeof(uint))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
		bytes[1] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[2] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[3] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
	}
	public static void uintToBytesBigEndian(uint value, byte[] bytes)
	{
		if (bytes.Length != sizeof(uint))
		{
			return;
		}
		// 为了获得最快速度,不使用for循环
		bytes[0] = (byte)(((0xFF << (8 * 3)) & value) >> (8 * 3));
		bytes[1] = (byte)(((0xFF << (8 * 2)) & value) >> (8 * 2));
		bytes[2] = (byte)(((0xFF << (8 * 1)) & value) >> (8 * 1));
		bytes[3] = (byte)(((0xFF << (8 * 0)) & value) >> (8 * 0));
	}
	public static byte[] toBytes(float value)
	{
		return BitConverter.GetBytes(value);
	}
	public static byte[] toBytes(double value)
	{
		return BitConverter.GetBytes(value);
	}
	public static byte bytesToByte(byte[] array)
	{
		return array[0];
	}
	public static short bytesToShort(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToShort(array[0], array[1]);
	}
	public static short bytesToShortBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToShortBigEndian(array[0], array[1]);
	}
	public static short bytesToShort(byte byte0, byte byte1) { return (short)((byte1 << (8 * 1)) | (byte0 << (8 * 0))); }
	public static short bytesToShortBigEndian(byte byte0, byte byte1) { return (short)((byte1 << (8 * 0)) | (byte0 << (8 * 1))); }
	public static ushort bytesToUShort(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUShort(array[0], array[1]);
	}
	public static ushort bytesToUShortBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUShortBigEndian(array[0], array[1]);
	}
	public static ushort bytesToUShort(byte b0, byte b1) { return (ushort)((b1 << (8 * 1)) | (b0 << (8 * 0))); }
	public static ushort bytesToUShortBigEndian(byte b0, byte b1) { return (ushort)((b1 << (8 * 0)) | (b0 << (8 * 1))); }
	public static int bytesToInt(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToInt(array[0], array[1], array[2], array[3]);
	}
	public static int bytesToIntBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToIntBigEndian(array[0], array[1], array[2], array[3]);
	}
	public static int bytesToInt(byte b0, byte b1, byte b2, byte b3) { return (b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)); }
	public static int bytesToIntBigEndian(byte b0, byte b1, byte b2, byte b3) { return (b3 << (8 * 0)) | (b2 << (8 * 1)) | (b1 << (8 * 2)) | (b0 << (8 * 3)); }
	public static uint bytesToUInt(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUInt(array[0], array[1], array[2], array[3]);
	}
	public static uint bytesToUIntBigEndian(byte[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return bytesToUIntBigEndian(array[0], array[1], array[2], array[3]);
	}
	public static uint bytesToUInt(byte b0, byte b1, byte b2, byte b3)
	{
		return (uint)((b3 << (8 * 3)) | (b2 << (8 * 2)) | (b1 << (8 * 1)) | (b0 << (8 * 0)));
	}
	public static uint bytesToUIntBigEndian(byte b0, byte b1, byte b2, byte b3)
	{
		return (uint)((b3 << (8 * 0)) | (b2 << (8 * 1)) | (b1 << (8 * 2)) | (b0 << (8 * 3)));
	}
	public static long bytesToLong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 7) | (long)b6 << (8 * 6) | (long)b5 << (8 * 5) | (long)b4 << (8 * 4) |
				(long)b3 << (8 * 3) | (long)b2 << (8 * 2) | (long)b1 << (8 * 1) | (long)b0 << (8 * 0);
	}
	public static long bytesToLongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (long)b7 << (8 * 0) | (long)b6 << (8 * 1) | (long)b5 << (8 * 2) | (long)b4 << (8 * 3) |
				(long)b3 << (8 * 4) | (long)b2 << (8 * 5) | (long)b1 << (8 * 6) | (long)b0 << (8 * 7);
	}
	public static ulong bytesToULong(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 7) | (ulong)b6 << (8 * 6) | (ulong)b5 << (8 * 5) | (ulong)b4 << (8 * 4) |
				(ulong)b3 << (8 * 3) | (ulong)b2 << (8 * 2) | (ulong)b1 << (8 * 1) | (ulong)b0 << (8 * 0);
	}
	public static ulong bytesToULongBigEndian(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
	{
		return (ulong)b7 << (8 * 0) | (ulong)b6 << (8 * 1) | (ulong)b5 << (8 * 2) | (ulong)b4 << (8 * 3) |
				(ulong)b3 << (8 * 4) | (ulong)b2 << (8 * 5) | (ulong)b1 << (8 * 6) | (ulong)b0 << (8 * 7);
	}
	public static float bytesToFloat(byte[] array)
	{
		if (array == null)
		{
			return 0.0f;
		}
		return BitConverter.ToSingle(array, 0);
	}
	public static double bytesToDouble(byte[] array)
	{
		if (array == null)
		{
			return 0.0;
		}
		return BitConverter.ToDouble(array, 0);
	}
	public static byte[] stringToBytes(string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return encoding.GetBytes(str);
	}
	public static string bytesToString(byte[] bytes, Encoding encoding = null)
	{
		if (bytes == null)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return EMPTY;
		}
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return removeLastZero(encoding.GetString(bytes));
	}
	public static string bytesToString(byte[] bytes, int startIndex, int count, Encoding encoding = null)
	{
		if (bytes == null || count < 0)
		{
			return null;
		}
		if (bytes.Length == 0 || count == 0 || startIndex + count > bytes.Length)
		{
			return EMPTY;
		}
		// 默认为UTF8
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return removeLastZero(encoding.GetString(bytes, startIndex, count));
	}
	public static string convertStringFormat(string str, Encoding source, Encoding target)
	{
		return bytesToString(stringToBytes(str, source), target);
	}
	public static string UTF8ToUnicode(string str)
	{
		return convertStringFormat(str, Encoding.UTF8, Encoding.Unicode);
	}
	public static string UTF8ToGB2312(string str)
	{
		return convertStringFormat(str, Encoding.UTF8, getGB2312());
	}
	public static string UnicodeToUTF8(string str)
	{
		return convertStringFormat(str, Encoding.Unicode, Encoding.UTF8);
	}
	public static string UnicodeToGB2312(string str)
	{
		return convertStringFormat(str, Encoding.Unicode, getGB2312());
	}
	public static string GB2312ToUTF8(string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.UTF8);
	}
	public static string GB2312ToUnicode(string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.Unicode);
	}
	// 字节数组转换为字符串时,末尾可能会带有数字0,此时在字符串比较时会出现错误,所以需要移除字符串末尾的0
	public static string removeLastZero(string str)
	{
		int strLen = str.Length;
		int newLen = strLen;
		for (int i = 0; i < strLen; ++i)
		{
			if (str[i] == 0)
			{
				newLen = i;
				break;
			}
		}
		str = str.Substring(0, newLen);
		return str;
	}
	public static bool isMemoryEqual(byte[] buffer0, byte[] buffer1, int length, int offset0 = 0, int offset1 = 0)
	{
		// 如果长度不足,则返回失败
		if (offset0 + length > buffer0.Length || offset1 + length > buffer1.Length)
		{
			return false;
		}
		for (int i = 0; i < length; ++i)
		{
			if (buffer0[i + offset0] != buffer1[i + offset1])
			{
				return false;
			}
		}
		return true;
	}
	public static bool hasBit(byte value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(sbyte value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(short value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(ushort value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(int value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(uint value, int pos) { return (value & (1 << pos)) != 0; }
	public static bool hasBit(long value, int pos) { return (value & ((long)1 << pos)) != 0; }
	public static bool hasBit(ulong value, int pos) { return (value & ((ulong)1 << pos)) != 0; }
	public static int getLowestBit(byte value) { return value & 1; }
	public static int getLowestBit(short value) { return value & 1; }
	public static int getLowestBit(int value) { return value & 1; }
	public static int getHighestBit(byte value) { return (value & ~0x7F) >> (8 * sizeof(byte) - 1) & 1; }
	public static int getHighestBit(short value) { return (value & ~0x7FFF) >> (8 * sizeof(short) - 1) & 1; }
	public static int getHighestBit(int value) { return (value & ~0x7FFFFFFF) >> (8 * sizeof(int) - 1) & 1; }
	public static void setBitOne(ref byte value, int pos) { value |= (byte)(1 << pos); }
	public static void setBitOne(ref sbyte value, int pos) { value |= (sbyte)(1 << pos); }
	public static void setBitOne(ref short value, int pos) { value |= (short)(1 << pos); }
	public static void setBitOne(ref ushort value, int pos) { value |= (ushort)(1 << pos); }
	public static void setBitOne(ref int value, int pos) { value |= 1 << pos; }
	public static void setBitOne(ref uint value, int pos) { value |= (uint)(1 << pos); }
	public static void setBitOne(ref long value, int pos) { value |= (long)1 << pos; }
	public static void setBitOne(ref ulong value, int pos) { value |= (ulong)1 << pos; }
	public static void setBitZero(ref byte value, int pos) { value &= (byte)~(1 << pos); }
	public static void setBitZero(ref sbyte value, int pos) { value &= (sbyte)~(1 << pos); }
	public static void setBitZero(ref short value, int pos) { value &= (short)~(1 << pos); }
	public static void setBitZero(ref ushort value, int pos) { value &= (ushort)~(1 << pos); }
	public static void setBitZero(ref int value, int pos) { value &= ~(1 << pos); }
	public static void setBitZero(ref uint value, int pos) { value &= (uint)~(1 << pos); }
	public static void setBitZero(ref long value, int pos) { value &= ~((long)1 << pos); }
	public static void setBitZero(ref ulong value, int pos) { value &= ~((ulong)1 << pos); }
	public static void setLowestBit(ref byte value, int bit)
	{
		if (bit == 0)
		{
			value = (byte)(value & 0xFE);
		}
		else
		{
			value = (byte)(value | 1);
		}
	}
	public static void setLowestBit(ref short value, int bit)
	{
		if (bit == 0)
		{
			value = (short)(value & 0xFFFE);
		}
		else
		{
			value = (short)(value | 1);
		}
	}
	public static void setLowestBit(ref int value, int bit)
	{
		if (bit == 0)
		{
			value >>= 1;
			value <<= 1;
		}
		else
		{
			value |= 1;
		}
	}
	public static void setHighestBit(ref byte value, int bit)
	{
		if (bit == 0)
		{
			value = (byte)(value & 0x7F);
		}
		else
		{
			value = (byte)(value | 0x80);
		}
	}
	public static void setHighestBit(ref short value, int bit)
	{
		if (bit == 0)
		{
			value = (short)(value & 0x7FFF);
		}
		else
		{
			value = (short)(value | 0x8000);
		}
	}
	public static void setHighestBit(ref int value, int bit)
	{
		if (bit == 0)
		{
			value &= 0x7FFFFFFF;
		}
		else
		{
			value = (int)(value | 0x80000000);
		}
	}
	public static int bitCountToByteCount(int bitCount) { return (bitCount & 7) != 0 ? (bitCount >> 3) + 1 : bitCount >> 3; }
	//------------------------------------------------------------------------------------------------------------------------------
	public static bool readSignedBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out int bitCount)
	{
		bitCount = 0;
		// 读取长度位
		int TYPE_LENGTH_MAX_BIT = SIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		bitCount = 0;
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref bitCount, i);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		return true;
	}
	public static bool readUnsignedBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out int bitCount)
	{
		bitCount = 0;
		// 读取长度位
		int TYPE_LENGTH_MAX_BIT = UNSIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		bitCount = 0;
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref bitCount, i);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		return true;
	}
	public static bool readSignedListBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out int count, out int bitCount)
	{
		bitCount = 0;
		// 先读列表元素数量
		readBit(buffer, bufferSize, ref bitIndex, out count);
		if (count == 0)
		{
			return true;
		}
		// 读取长度位
		int TYPE_LENGTH_MAX_BIT = SIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref bitCount, i);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		// 每个元素会多一个符号位,所以+1,但是如果bitCount是0,则连符号位都不会有
		return bitCountToByteCount(bitIndex + (bitCount + 1 * sign(bitCount)) * count) <= bufferSize;
	}
	public static bool readUnsignedListBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, out int count, out int bitCount)
	{
		bitCount = 0;
		// 先读列表元素数量
		readBit(buffer, bufferSize, ref bitIndex, out count);
		if (count == 0)
		{
			return true;
		}
		// 读取长度位
		int TYPE_LENGTH_MAX_BIT = UNSIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7))
			{
				setBitOne(ref bitCount, i);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		if (count > 10000)
		{
			UnityUtility.logError("列表元素不能超过10000");
			return false;
		}
		return bitCountToByteCount(bitIndex + bitCount * count) <= bufferSize;
	}
	// 可以按位写入带符号的整数,并且不写入长度位,因为long是最大的带符号整数,所以可以表示所有的带符号整数
	protected static bool writeSignedIntegerBitNoLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, long value, int bitCount)
	{
		if (bitCountToByteCount(bitIndex + 1 * sign(bitCount) + bitCount) > bufferSize)
		{
			return false;
		}

		if (bitCount == 0)
		{
			return true;
		}

		bool isNegative = value < 0;
		if (isNegative)
		{
			value = -value;
		}
		// 写入符号位
		if (isNegative)
		{
			setBitOne(ref buffer[bitIndex >> 3], bitIndex & 7);
		}
		++bitIndex;
		// 再写入值的所有位
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(value, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += bitCount;
		return true;
	}
	// 可以按位写入无符号的整数,并且不写入长度位,因为ulong是最大的无符号整数,所以可以表示所有的无符号整数
	protected static bool writeUnsignedIntegerBitNoLengthBit(byte[] buffer, int bufferSize, ref int bitIndex, ulong value, int bitCount)
	{
		if (bitCountToByteCount(bitIndex + bitCount) > bufferSize)
		{
			return false;
		}
		// 写入值的所有位
		for (int i = 0; i < bitCount; ++i)
		{
			if (hasBit(value, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += bitCount;
		return true;
	}
	protected static bool writeSignedBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, int bitCount)
	{
		int TYPE_LENGTH_MAX_BIT = SIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}

		// 先写入TYPE_LENGTH_MAX_BIT个bit
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(bitCount, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		return true;
	}
	protected static bool writeUnsignedBitLength(byte[] buffer, int bufferSize, ref int bitIndex, int typeSize, int bitCount)
	{
		int TYPE_LENGTH_MAX_BIT = UNSIGNED_LENGTH_MAX_BIT[typeSize];
		if (bitCountToByteCount(bitIndex + TYPE_LENGTH_MAX_BIT) > bufferSize)
		{
			return false;
		}

		// 先写入TYPE_LENGTH_MAX_BIT个bit
		for (int i = 0; i < TYPE_LENGTH_MAX_BIT; ++i)
		{
			if (hasBit(bitCount, i))
			{
				setBitOne(ref buffer[(bitIndex + i) >> 3], (bitIndex + i) & 7);
			}
		}
		bitIndex += TYPE_LENGTH_MAX_BIT;
		return true;
	}
	protected static byte generateBitCount(char value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			return 0;
		}
		return mBitCountTable[value];
	}
	protected static byte generateBitCount(byte value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		return mBitCountTable[value];
	}
	protected static byte generateBitCount(short value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			return 0;
		}
		return mBitCountTable[value];
	}
	protected static byte generateBitCount(ushort value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		return mBitCountTable[value];
	}
	protected static byte generateBitCount(int value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			return 0;
		}
		ushort part1 = (ushort)((value & 0xFFFF0000) >> 16);
		if (part1 > 0)
		{
			return (byte)(mBitCountTable[part1] + 16);
		}
		else
		{
			return mBitCountTable[value & 0x0000FFFF];
		}
	}
	protected static byte generateBitCount(uint value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		ushort part1 = (ushort)((value & 0xFFFF0000) >> 16);
		if (part1 > 0)
		{
			return (byte)(mBitCountTable[part1] + 16);
		}
		else
		{
			return mBitCountTable[value & 0x0000FFFF];
		}
	}

	protected static byte generateBitCount(long value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if (value < 0)
		{
			return 0;
		}
		ulong ulongValue = (ulong)value;
		if ((ulongValue & 0xFFFFFFFF00000000) > 0)
		{
			ushort part3 = (ushort)((ulongValue & 0xFFFF000000000000) >> 48);
			if (part3 > 0)
			{
				return (byte)(mBitCountTable[part3] + 16 * 3);
			}
			ushort part2 = (ushort)((ulongValue & 0x0000FFFF00000000) >> 32);
			return (byte)(mBitCountTable[part2] + 16 * 2);
		}
		else
		{
			ushort part1 = (ushort)((ulongValue & 0x00000000FFFF0000) >> 16);
			if (part1 > 0)
			{
				return (byte)(mBitCountTable[part1] + 16 * 1);
			}
			return mBitCountTable[ulongValue & 0x000000000000FFFF];
		}
	}

	protected static byte generateBitCount(ulong value)
	{
		if (mBitCountTable == null)
		{
			initBitCountTable();
		}
		if ((value & 0xFFFFFFFF00000000) > 0)
		{
			ushort part3 = (ushort)((value & 0xFFFF000000000000) >> 48);
			if (part3 > 0)
			{
				return (byte)(mBitCountTable[part3] + 16 * 3);
			}
			ushort part2 = (ushort)((value & 0x0000FFFF00000000) >> 32);
			return (byte)(mBitCountTable[part2] + 16 * 2);
		}
		else
		{
			ushort part1 = (ushort)((value & 0x00000000FFFF0000) >> 16);
			if (part1 > 0)
			{
				return (byte)(mBitCountTable[part1] + 16 * 1);
			}
			return mBitCountTable[value & 0x000000000000FFFF];
		}
	}
	protected static void initBitCountTable()
	{
		using (new ThreadLockScope(mBitCountTableLock))
		{
			if (mBitCountTable != null)
			{
				return;
			}
			mBitCountTable = new byte[65536];
			for (int i = 0; i < 65536; ++i)
			{
				mBitCountTable[i] = internalGenerateBitCount((ushort)i);
			}
		}
	}
	protected static byte internalGenerateBitCount(ushort value)
	{
		if (value == 0)
		{
			return 0;
		}
		// 从高到低遍历每一位,找到最高位1的下标
		for (int i = 0; i < 16; ++i)
		{
			if (hasBit(value, 15 - i))
			{
				return (byte)(16 - i);
			}
		}
		return 0;
	}
	protected static void initCRC16()
	{
		using (new ThreadLockScope(mCRC16TableLock))
		{
			mCRC16Table = new ushort[256]
			{
				0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
				0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
				0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
				0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
				0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
				0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
				0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
				0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
				0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
				0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
				0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
				0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
				0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
				0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
				0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
				0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
				0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
				0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
				0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
				0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
				0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
				0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
				0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
				0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
				0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
				0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
				0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
				0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
				0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
				0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
				0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
				0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
			};
		}
	}
}