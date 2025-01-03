﻿#if USE_SQLITE
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static CSharpUtility;
using static BinaryUtility;
using static FrameBase;
using static FrameDefine;
using static FrameDefineBase;
using static FrameEditorUtility;

// 表示一个SQLite表格
public class SQLiteTable : ClassObject
{
	protected Dictionary<int, SQLiteData> mDataMap = new();     // 以数据ID为索引的数据缓存列表
	protected SqliteConnection mConnection;						// SQLite所需的Connection
	protected SqliteCommand mCommand;							// SQLite所需的Command
	protected string mDecryptFileName;							// 解密以后的文件名
	protected string mTableName;								// 表格名称
	protected Type mDataClassType;								// 数据类型
	public override void resetProperty()
	{
		base.resetProperty();
		mDataMap.Clear();
		mConnection = null;
		mCommand = null;
		mDecryptFileName = null;
		mTableName = null;
		mDataClassType = null;
	}
	public byte[] generateEncryptKey()
	{
		string key = generateFileMD5(stringToBytes("ASLD" + mTableName)).ToUpper() + "23y35y9832635872349862365274732047chsudhgkshgwshfoweh238c42384fync9388v45982nc3484";
		return stringToBytes(key);
	}
	// 返回值是解析以后生成的文件名
	public void load()
	{
		try
		{
			clearCommand();
			clearConnection();
			clearAll();
			// 解密文件
			byte[] encryptKey = generateEncryptKey();
			TextAsset textAsset = null;
			if (mResourceManager != null)
			{
				textAsset = mResourceManager.loadGameResource<TextAsset>(R_SQLITE_PATH + mTableName + ".bytes");
			}
			else
			{
				if (!isEditor())
				{
					return;
				}
				textAsset = loadAssetAtPath<TextAsset>(P_SQLITE_PATH + mTableName + ".bytes");
			}
			byte[] fileBuffer = textAsset.bytes;
			int fileSize = fileBuffer.Length;
			int index = 0;
			for (int i = 0; i < fileSize; ++i)
			{
				fileBuffer[i] ^= encryptKey[index];
				if (++index >= encryptKey.Length)
				{
					index = 0;
				}
			}
			// 将解密后的数据写入新的目录,需要写入临时目录,编辑器中写入固定路径即可
			mDecryptFileName = generateFileMD5(fileBuffer).ToUpper();
			string newPath = getDecryptFileFullPath();
			if (!isFileExist(newPath))
			{
				writeFile(newPath, fileBuffer, fileSize);
			}
			if (mResourceManager != null)
			{
				mResourceManager.unload(ref textAsset);
			}
			else
			{
				if (isEditor())
				{
					Resources.UnloadAsset(textAsset);
				}
			}
			connect(newPath);
		}
		catch (Exception e)
		{
			destroy();
			logException(e, "打开数据库失败");
		}
	}
	public string getDecryptFileName() { return mDecryptFileName; }
	public string getDecryptFileFullPath()
	{
		if (isEditor())
		{
			return getDecryptFilePath() + mTableName + "/" + mTableName;
		}
		else
		{
			return getDecryptFilePath() + mDecryptFileName;
		}
	}
	public static string getDecryptFilePath()
	{
		if (isEditor())
		{
			return F_TEMPORARY_CACHE_PATH + "../../";
		}
		if (isWindows())
		{
			return F_TEMPORARY_CACHE_PATH + "../../";
		}
		return FrameUtility.availableWritePath("temp/");
	}
	public override void destroy()
	{
		base.destroy();
		clearCommand();
		clearConnection();
		clearAll();
	}
	public void connect(string fullFileName)
	{
		if (isFileExist(fullFileName))
		{
			// 创建SQLite对象的同时，创建SqliteConnection对象  
			mConnection = new("URI=file:" + fullFileName);
			// 打开数据库链接
			mConnection.Open();
		}
		if (mConnection != null)
		{
			mCommand = mConnection.CreateCommand();
		}
	}
	public SqliteDataReader queryReader(string queryString)
	{
		if (mCommand == null)
		{
			return null;
		}
		mCommand.CommandText = queryString;
		SqliteDataReader reader = null;
		try
		{
			reader = mCommand.ExecuteReader();
		}
		catch (Exception) { }
		return reader;
	}
	public void queryNonReader(string queryString)
	{
		if (mCommand == null)
		{
			return;
		}
		mCommand.CommandText = queryString;
		try
		{
			mCommand.ExecuteNonQuery();
		}
		catch (Exception) { }
	}
	public void setDataType(Type dataClassType) { mDataClassType = dataClassType; }
	public void setTableName(string name) { mTableName = name; }
	public string getTableName() { return mTableName; }
	public SqliteDataReader doQuery(string condition = null)
	{
		if (!condition.isEmpty())
		{
			return queryReader("SELECT * FROM " + mTableName + " WHERE " + condition);
		}
		else
		{
			return queryReader("SELECT * FROM " + mTableName);
		}
	}
	public void doUpdate(string updateString, string conditionString)
	{
		queryNonReader(strcat("UPDATE ", mTableName, " SET ", updateString, " WHERE ", conditionString));
	}
	public void doInsert(string valueString)
	{
		queryNonReader(strcat("INSERT INTO ", mTableName, " VALUES (", valueString, ")"));
	}
	public SQLiteData query(int id, bool errorIfNull = true)
	{
		if (mDataMap.TryGetValue(id, out SQLiteData data))
		{
			return data;
		}
		using var a = new MyStringBuilderScope(out var condition);
		appendConditionInt(condition, SQLiteData.ID, id, EMPTY);
		parseReader(doQuery(condition.ToString()), out data);
		mDataMap.Add(id, data);
		if (data == null && errorIfNull)
		{
			logError("表格中找不到指定数据: ID:" + id + ", Type:" + mDataClassType);
		}
		return data;
	}
	public void insert<T>(T data) where T : SQLiteData
	{
		if (mDataMap.ContainsKey(data.mID))
		{
			return;
		}
		if (data.GetType() != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + data.GetType());
			return;
		}
		using var a = new MyStringBuilderScope(out var valueString);
		data.insert(valueString);
		removeLastComma(valueString);
		doInsert(valueString.ToString());
		mDataMap.Add(data.mID, data);
	}
	public void queryDataList<T>(string condition, Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(condition), dataList);
		foreach (T data in dataList)
		{
			mDataMap.TryAdd(data.mID, data);
		}
	}
	public void queryDataList<T>(Type type, List<T> dataList) where T : SQLiteData
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		parseReader(type, doQuery(), dataList);
		foreach (T data in dataList)
		{
			mDataMap.TryAdd(data.mID, data);
		}
	}
	public void checkData(int checkID, int dataID, string refTableName)
	{
		if (checkID > 0 && query(checkID, false) == null)
		{
			logError("can not find item id:" + checkID + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTableName);
		}
	}
	public void checkData(List<int> checkIDList, int dataID, string refTableName)
	{
		foreach (int id in checkIDList)
		{
			if (query(id, false) == null)
			{
				logError("can not find item id:" + id + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTableName);
			}
		}
	}
	public void checkData(List<ushort> checkIDList, int dataID, string refTableName)
	{
		foreach (int id in checkIDList)
		{
			if (query(id, false) == null)
			{
				logError("can not find item id:" + id + " in " + mTableName + ", ref ID:" + dataID + ", ref Table:" + refTableName);
			}
		}
	}
	public void checkListPair(IList list0, IList list1, int dataID)
	{
		if (list0.Count != list1.Count)
		{
			logError("list pair size not match, table:" + mTableName + ", ref ID:" + dataID);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void clearAll()
	{
		mDataMap.Clear();
	}
	protected void parseReader(Type type, SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader == null)
		{
			return;
		}
		if (reader.Read())
		{
			data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(SqliteDataReader reader, out SQLiteData data)
	{
		data = null;
		if (reader != null && reader.Read())
		{
			data = createInstance<SQLiteData>(mDataClassType);
			data.mTable = this;
			data.parse(reader);
		}
		reader?.Close();
	}
	protected void parseReader(Type type, SqliteDataReader reader, IList dataList)
	{
		if (type != mDataClassType)
		{
			logError("sqlite table type error, this type:" + mDataClassType + ", param type:" + type);
			return;
		}
		if (reader == null)
		{
			return;
		}
		using var a = new ProfilerScope("sqlite parseReader " + mTableName);
		while (reader.Read())
		{
			var data = createInstance<SQLiteData>(type);
			data.mTable = this;
			data.parse(reader);
			dataList.Add(data);
		}
		reader.Close();
	}
	protected void clearConnection()
	{
		if (mConnection == null)
		{
			return;
		}
		mConnection.Close();
		mConnection.Dispose();
		mConnection = null;
	}
	protected void clearCommand()
	{
		if (mCommand == null)
		{
			return;
		}
		mCommand.Cancel();
		mCommand.Dispose();
		mCommand = null;
	}
}
#endif