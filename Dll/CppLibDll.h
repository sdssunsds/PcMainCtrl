#define CPPLIBDLL_EXPORTS
#ifdef CPPLIBDLL_EXPORTS
#define CPPLIBDLL_API __declspec(dllexport)
#else
#define CPPLIBDLL_API __declspec(dllimport)
#endif
#pragma  once


#include <string>

// 导出的接口类
class IExport
{
public:
	// 返回值：成功:0，失败:非0，失败信息保存在D:/Log.txt
	virtual int OnInit() = 0;

	// 返回值：成功:0，失败:非0，失败信息保存在D:/Log.txt
	virtual double getlocation(std::string input_train_type, std::string Input_Map_ID, std::string point_id) = 0;

	virtual ~IExport() {}
};

// 假设这是原来的DLL暴露的接口函数
// 这种返回接口类对象的指针的导出函数，对于C++来说没有什么问题，但是对于C#没办法直接用对象指针调用接口方法
extern "C" CPPLIBDLL_API IExport* __stdcall ExportObjectFactory();
extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport* obj);



// 通过建立一个接口层，帮C#完成间接调用接口方法
// 这2个方法可以单独做成一个间接层dll(此处只是为了方便，一般情况也只能自己另外写一个dll，因为你不能修改别人的dll源码)
// 下面strSaveFilePath变量类型不要用string，C#中的string类型和C++的string不匹配

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport* obj);
extern "C" CPPLIBDLL_API double __stdcall CallOnLoc(IExport* obj, const char* input_train_type, const char* Input_Map_ID, const char* point_id);
