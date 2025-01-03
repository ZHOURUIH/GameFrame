﻿using UnityEngine;
using System.Collections.Generic;
using System.Net;
#if USE_AVPRO_VIDEO
using RenderHeads.Media.AVProVideo;
#endif
using UObject = UnityEngine.Object;

// 游戏委托定义
public delegate bool BoolFunction();
public delegate void StringCallback(string info);
public delegate void IntCallback(int value);
public delegate void FloatStringParam(float floatParam, string stringParam);
public delegate void StringListCallback(List<string> info);
public delegate void StringArrayCallback(string[] lines);
public delegate void String2Callback(string str0, string str1);
public delegate void String3Callback(string str0, string str1, string str2);
public delegate void ULongFloatCallback(ulong value0, float value1);
public delegate void Vector3Callback(Vector3 vec3);
public delegate void Vector3IntCallback(Vector3 vec3, int value);
public delegate void Vector3BoolCallback(Vector3 vec3, bool value);
public delegate void BytesCallback(byte[] bytes);
public delegate void StringBytesCallback(string str, byte[] bytes);
public delegate void BytesIntCallback(byte[] bytes, int value);
public delegate void BytesStringCallback(byte[] bytes, string value);
public delegate void FloatCallback(float progress);
public delegate void Float2Callback(float value0, float value1);
public delegate void Float3Callback(float value0, float value1, float value2);
public delegate void BoolCallback(bool value);
public delegate void BoolIntCallback(bool value0, int value1);
public delegate void StringIntCallback(string str, int value);
public delegate void StringBoolCallback(string str, bool value);
public delegate void FloatBoolCallback(float progress, bool done);
public delegate void UObjectCallback(UObject obj);
public delegate void OnLog(string time, string info, LOG_LEVEL level, bool isError);
public delegate void RecordCallback(short[] data, int dataCount);
public delegate void ClassObjectCallback(ClassObject owner);
public delegate void TextureAnimCallback(IUIAnimation window, bool isBreak);
public delegate void KeyFrameCallback(ComponentKeyFrame com, bool isBreak);
public delegate void CommandCallback(Command cmd);
public delegate void AssetBundleCallback(AssetBundleInfo assetBundle);
public delegate void AssetBundleBytesCallback(AssetBundleInfo assetBundle, byte[] bytes);
public delegate void AssetLoadDoneCallback(UObject asset, UObject[] assets, byte[] bytes, string loadPath);
public delegate void DownloadCallback(ulong downloadedBytes, int downloadDelta, double deltaTimeMillis, float percent);
public delegate void GameLayoutCallback(GameLayout layout);
public delegate void VideoCallback(string videoName, bool isBreak);
public delegate void GameDownloadCallback(float progress, PROGRESS_TYPE type, string info, int bytesPerSecond, int downloadRemainSeconds);
public delegate void GameDownloadTipCallback(DOWNLOAD_TIP type);
#if USE_AVPRO_VIDEO
public delegate void VideoErrorCallback(ErrorCode errorCode);
#endif
public delegate void OnReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent);
public delegate void OnDragHover(IMouseEventCollect dragObj, Vector3 mousePos, bool hover);
public delegate void OnMouseEnter(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void OnMouseLeave(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void OnMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID);
public delegate void OnScreenMouseUp(IMouseEventCollect obj, Vector3 mousePos, int touchID);
public delegate void HeadDownloadCallback(Texture head, string openID);
public delegate void OnDragViewStartCallback(ref bool allowDrag);
public delegate void MyThreadCallback(ref bool run);    // run表示是否继续运行该线程,可在运行时修改
public delegate void OnPlayingCallback(AnimControl control, int frame, bool isPlaying); // isPlaying表示是否是在播放过程中触发的该回调
public delegate void OnPlayEndCallback(AnimControl control, bool callback, bool isBreak);
public delegate void DragCallback(ComponentOwner dragObj, Vector3 pos);
public delegate void DragEndCallback(ComponentOwner dragObj, Vector3 pos, bool cancel);
public delegate void DragStartCallback(ComponentOwner dragObj, TouchPoint touchPoint, ref bool allowDrag);
public delegate void StartDownloadCallback(string fileName, long totalSize);
public delegate void ClickCallback(IMouseEventCollect obj, Vector3 mousePos);
public delegate void HoverCallback(IMouseEventCollect obj, Vector3 mousePos, bool hover);
public delegate void PressCallback(IMouseEventCollect obj, Vector3 mousePos, bool press);
public delegate void DownloadingCallback(string fileName, long fileSize, long downloadedSize);
public delegate void LayoutScriptCallback(LayoutScript script);
public delegate void GameObjectCallback(GameObject go);
public delegate void CreateObjectGroupCallback(Dictionary<string, GameObject> go);
public delegate void UGUIAtlasPtrCallback(UGUIAtlasPtr atlas);
public delegate myUGUIImage CreateImage();
public delegate myUGUIImageAnim CreateImageAnim();
public delegate void UGUIImageCallback(myUGUIImage image);
public delegate void UGUIImageAnimCallback(myUGUIImageAnim imageAnim);
public delegate void UIObjectCallback(myUIObject window);
public delegate void EncryptPacket(byte[] data, int offset, int length, byte param);
public delegate void DecryptPacket(byte[] data, int offset, int length, byte param);
public delegate void OnHotKeyCallback(KeyCode key);
public delegate void NetStateCallback(NET_STATE state, NET_STATE lastState);
public delegate void OnLocalization(myUGUIText textObj, string localizedText, List<string> localizedParams);
public delegate void OnReloadLanguage(string languageType, Dictionary<string, string> zhKeyList, Dictionary<int, string> idKeyList);
public delegate void CheckAndDownloadFileListCallback(byte[] localFileListBytes, BytesCallback callback);
public delegate void HttpCallback(string result, WebExceptionStatus status, HttpStatusCode code);
public delegate void AudioInfoCallback(AudioInfo info);