#pragma once
#include <windows.h>
#include <strsafe.h>
#include <credentialprovider.h>
#include <ntsecapi.h>
#define SECURITY_WIN32
#include <security.h>
#include <intsafe.h>

#define MAX_ULONG  ((ULONG)(-1))

#pragma warning(push)
#pragma warning(disable : 4995)
#include <shlwapi.h>
#pragma warning(pop)

//encrypt a password (if necessary) and copy it; if not, just copy it
extern "C" __declspec(dllexport) HRESULT ProtectIfNecessaryAndCopyPassword(
    LPWSTR pwzPassword,
    CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
    LPWSTR * ppwzProtectedPassword
);

//creates a UNICODE_STRING from a NULL-terminated string
HRESULT UnicodeStringInitWithString(
    LPWSTR pwz,
    UNICODE_STRING* pus
);

//initializes a KERB_INTERACTIVE_UNLOCK_LOGON with weak references to the provided credentials
extern "C" __declspec(dllexport) HRESULT KerbInteractiveUnlockLogonInit(
    LPWSTR pwzDomain,
    LPWSTR pwzUsername,
    LPWSTR pwzPassword,
    CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
    BYTE * *prgb,
    DWORD * pcb
    //KERB_INTERACTIVE_UNLOCK_LOGON* pkiul
);