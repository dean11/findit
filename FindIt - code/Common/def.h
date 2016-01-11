#ifndef COMMON_DEF_H
#define COMMON_DEF_H

#if DLL_COMPILE
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllimport)
#endif

#endif // !COMMON_DEF
