#include "Config.h"
#include "ImageUtility.h"

void main()
{
	// �������ߺ����ĳ�ʼ����Ҫ���ȵ���
	MathUtility::initMathUtility();
	StringUtility::initStringUtility();
	Config::parse("./AtlasPackerConfig.txt");

	cout << "��ʼ���ȫ��ͼ��,����ɹ��󽫻��ɢͼɾ��..." << endl;
	string texturePath = "./";
	if (!ImageUtility::texturePackerAll(texturePath, Config::mAtlasPath))
	{
		system("pause");
	}
	else
	{
		// ������ɹ���ͼ��ɢͼɾ��
		myVector<string> folderList;
		FileUtility::findFolders(texturePath, folderList, false);
		FOR_VECTOR(folderList)
		{
			FileUtility::deleteFolder(folderList[i]);
		}
	}
	return;
}