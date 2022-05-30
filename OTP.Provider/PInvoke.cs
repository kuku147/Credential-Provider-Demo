using OTP.Provider.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OTP.Provider
{
    static class PInvoke
    {
        //http://www.pinvoke.net/default.aspx/secur32/LsaLogonUser.html
        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public /*PCHAR*/ IntPtr Buffer;
        }

        public class LsaStringWrapper : IDisposable
        {
            public LSA_STRING _string;

            public LsaStringWrapper(string value)
            {
                _string = new LSA_STRING();
                _string.Length = (ushort)value.Length;
                _string.MaximumLength = (ushort)value.Length;
                _string.Buffer = Marshal.StringToHGlobalAnsi(value);
            }

            ~LsaStringWrapper()
            {
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (_string.Buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_string.Buffer);
                    _string.Buffer = IntPtr.Zero;
                }
                if (disposing)
                    GC.SuppressFinalize(this);
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaConnectUntrusted([Out] out IntPtr lsaHandle);

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaLookupAuthenticationPackage([In] IntPtr lsaHandle, [In] ref LSA_STRING packageName, [Out] out UInt32 authenticationPackage);

        [DllImport("secur32.dll", SetLastError = false)]
        public static extern uint LsaDeregisterLogonProcess([In] IntPtr lsaHandle);

        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredPackAuthenticationBuffer(
            int dwFlags,
            string pszUserName,
            string pszPassword,
            IntPtr pPackedCredentials,
            ref int pcbPackedCredentials
        );

        [DllImport("./OTP.Provider.Helper.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint ProtectIfNecessaryAndCopyPassword(
            string pwzPassword,
            _CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
            ref string ppwzProtectedPassword
        );

        [DllImport("./OTP.Provider.Helper.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint KerbInteractiveUnlockLogonInit(
            string pwzDomain,
            string pwzUsername,
            string pwzPassword,
            _CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
            ref IntPtr prgb,
            ref int pcb
        );
    }
}
