#ifndef KEYLOGGINGMANAGER_H
#define KEYLOGGINGMANAGER_H

#include "..\..\Common\def.h"

class DLLEXPORT KeyloggingManager
{
public:
	KeyloggingManager& GetManager();

	KeyloggingManager();
	~KeyloggingManager();

	void StartLogging();
	void StopLogging();
};


#endif // !KEYLOGGINGMANAGER
