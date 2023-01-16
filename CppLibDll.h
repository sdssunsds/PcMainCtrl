#define CPPLIBDLL_EXPORTS
#ifdef CPPLIBDLL_EXPORTS
#define CPPLIBDLL_API __declspec(dllexport)
#else
#define CPPLIBDLL_API __declspec(dllimport)
#endif
#pragma  once


#include <string>

// �����Ľӿ���
class IExport
{
public:
	// ����ֵ���ɹ�:0��ʧ��:��0��ʧ����Ϣ������D:/Log.txt
	virtual int OnInit() = 0;

	// ����ֵ���ɹ�:0��ʧ��:��0��ʧ����Ϣ������D:/Log.txt
	virtual double getlocation(std::string input_train_type, std::string Input_Map_ID, std::string point_id) = 0;

	virtual ~IExport() {}
};

// ��������ԭ����DLL��¶�Ľӿں���
// ���ַ��ؽӿ�������ָ��ĵ�������������C++��˵û��ʲô���⣬���Ƕ���C#û�취ֱ���ö���ָ����ýӿڷ���
extern "C" CPPLIBDLL_API IExport* __stdcall ExportObjectFactory();
extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport* obj);



// ͨ������һ���ӿڲ㣬��C#��ɼ�ӵ��ýӿڷ���
// ��2���������Ե�������һ����Ӳ�dll(�˴�ֻ��Ϊ�˷��㣬һ�����Ҳֻ���Լ�����дһ��dll����Ϊ�㲻���޸ı��˵�dllԴ��)
// ����strSaveFilePath�������Ͳ�Ҫ��string��C#�е�string���ͺ�C++��string��ƥ��

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport* obj);
extern "C" CPPLIBDLL_API double __stdcall CallOnLoc(IExport* obj, const char* input_train_type, const char* Input_Map_ID, const char* point_id);
