# Credential Provider Demo on C#

## Testing

For testing, you need:
- Checking the path to the OTP.Provider.Helper.dll library
- Compile the project
- Register your Credential Provider in the system using the credential_provider_key.reg file
- Register your DLL use command 
```
regSvr32 .\OTP.Provider\bin\Debug\net6.0-windows\OTP.Provider.comhost.dll
```

## ATTENTION 
When testing, you can access your workstation, after testing in your machine
Run tests in a virtual machine

## Remove
If you need to remove the provider from the system: 
- delete the HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{D26F523C-A346-4FC8-B9B4-2B57EAEDA723}] branch from the registry
- Unregister your DLL use command 
```
regSvr32 /U .\OTP.Provider\bin\Debug\net6.0-windows\OTP.Provider.comhost.dll
```
