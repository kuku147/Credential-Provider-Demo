using OTP.Provider.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static OTP.Provider.Constants;

namespace OTP.Provider
{
    public class CredentialProviderCredential : ICredentialProviderCredential2
    {
        private readonly CredentialView view;
        private string sid;

        public CredentialProviderCredential(CredentialView view, string sid)
        {
            Logger.Write();

            this.view = view;
            this.sid = sid;
        }

        public virtual int Advise(ICredentialProviderCredentialEvents pcpce)
        {
            Logger.Write();

            if (pcpce is ICredentialProviderCredentialEvents2 ev2)
            {
                Logger.Write("pcpce is ICredentialProviderCredentialEvents2");
            }

            return HRESULT.S_OK;
        }

        public virtual int UnAdvise()
        {
            Logger.Write();

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetSelected(out int pbAutoLogon)
        {
            Logger.Write();

            //Set this to 1 if you would like GetSerialization called immediately on selection
            pbAutoLogon = 0;

            return HRESULT.S_OK;
        }

        public virtual int SetDeselected()
        {
            Logger.Write();

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetFieldState(
            uint dwFieldID,
            out _CREDENTIAL_PROVIDER_FIELD_STATE pcpfs,
            out _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE pcpfis
        )
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            view.GetFieldState((int)dwFieldID, out pcpfs, out pcpfis);

            return HRESULT.S_OK;
        }

        public virtual int GetStringValue(uint dwFieldID, out string ppsz)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            ppsz = view.GetValue((int)dwFieldID);

            return HRESULT.S_OK;
        }

        private Bitmap tileIcon;

        public virtual int GetBitmapValue(uint dwFieldID, out IntPtr phbmp)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            try
            {
                TryLoadUserIcon();
            }
            catch (Exception ex)
            {
                Logger.Write("Error: " + ex);
            }

            phbmp = tileIcon?.GetHbitmap() ?? IntPtr.Zero;

            return HRESULT.S_OK;
        }

        private void TryLoadUserIcon()
        {
            if (tileIcon == null)
            {
                var fileName = "OTP.Provider.tile.bmp";
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(fileName);

                tileIcon = (Bitmap)Image.FromStream(stream);
            }
        }

        public virtual int GetCheckboxValue(uint dwFieldID, out int pbChecked, out string ppszLabel)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pbChecked = 0;
            ppszLabel = "";

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetSubmitButtonValue(uint dwFieldID, out uint pdwAdjacentTo)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pdwAdjacentTo = 0;

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetComboBoxValueCount(uint dwFieldID, out uint pcItems, out uint pdwSelectedItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pcItems = 0;
            pdwSelectedItem = 0;

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetComboBoxValueAt(uint dwFieldID, uint dwItem, out string ppszItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; dwItem: {dwItem}");

            ppszItem = "";

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetStringValue(uint dwFieldID, string psz)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; psz: {psz}");

            view.SetValue((int)dwFieldID, psz);

            return HRESULT.S_OK;
        }

        public virtual int SetCheckboxValue(uint dwFieldID, int bChecked)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; bChecked: {bChecked}");

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetComboBoxSelectedValue(uint dwFieldID, uint dwSelectedItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; dwSelectedItem: {dwSelectedItem}");

            return HRESULT.E_NOTIMPL;
        }

        public virtual int CommandLinkClicked(uint dwFieldID)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetSerialization(
            out _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE pcpgsr,
            out _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs,
            out string ppszOptionalStatusText,
            out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
        )
        {
            Logger.Write();

            var usage = this.view.Provider.GetUsage();

            pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_NO_CREDENTIAL_NOT_FINISHED;
            pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();
            ppszOptionalStatusText = "";
            pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;

            //Serialization can be called before the user has entered any values. Only applies to logon usage scenarios
            if (usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_LOGON || usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_UNLOCK_WORKSTATION)
            {
                //Determine the authentication package
                Common.RetrieveNegotiateAuthPackage(out var authPackage);

                //Only credential packing for msv1_0 is supported using this code
                Logger.Write($"Got authentication package: {authPackage}. Only local authenticsation package 0 (msv1_0) is supported.");

                //Get username and password
                var username = Common.GetNameFromSid(this.sid);
                GetStringValue(2, out var password);

                Logger.Write($"Preparing to serialise credential with password...");

                int positionOfNewLine = username.IndexOf("\\");
                var domain = username.Substring(0, positionOfNewLine);
                var usernameLength = username.Length;
                var shortUsername = username.Substring(positionOfNewLine + 1, usernameLength - positionOfNewLine - 1);

                string protectedPassword = "";

                try
                {
                    Logger.Write("Start ProtectIfNecessaryAndCopyPassword");
                    PInvoke.ProtectIfNecessaryAndCopyPassword(password, usage, ref protectedPassword);
                    Logger.Write(protectedPassword);
                    Logger.Write("Finish ProtectIfNecessaryAndCopyPassword");
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message);
                }

                var inCredSize = 0;
                var inCredBuffer = Marshal.AllocCoTaskMem(0);
                try
                {
                    Logger.Write("Start KerbInteractiveUnlockLogonInit");

                    Marshal.FreeCoTaskMem(inCredBuffer);
                    inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);

                    Logger.Write("Input Params");
                    Logger.Write("Domain:" + domain);
                    Logger.Write("Username:" + shortUsername);
                    Logger.Write("protectedPassword:" + protectedPassword);

                    PInvoke.KerbInteractiveUnlockLogonInit(domain, shortUsername, protectedPassword, usage, ref inCredBuffer, ref inCredSize);
                    //PInvoke.KerbInteractiveUnlockLogonInit(domain, username, password, usage, ref inCredBuffer, ref inCredSize);
                    Logger.Write("KerbInteractiveUnlockLogonInit Finish");

                    pcpcs.clsidCredentialProvider = Guid.Parse("D26F523C-A346-4FC8-B9B4-2B57EAEDA723");
                    pcpcs.rgbSerialization = inCredBuffer;
                    pcpcs.cbSerialization = (uint)inCredSize;
                    pcpcs.ulAuthenticationPackage = authPackage;
                    pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_RETURN_CREDENTIAL_FINISHED;


                    Logger.Write(inCredSize.ToString());

                    return HRESULT.S_OK;

                }
                catch (Exception ex)
                {
                    Logger.Write("Exception KerbInteractiveUnlockLogonInit");
                    Logger.Write(ex.Message);
                }

                pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_RETURN_CREDENTIAL_FINISHED;
                pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();

            }
            //Implement code to change password here. This is not handled natively.
            else if (usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CHANGE_PASSWORD)
            {
                pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_NO_CREDENTIAL_FINISHED;
                pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();
                ppszOptionalStatusText = "Password changed success message.";
                pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_SUCCESS;
            }

            Logger.Write("Returning S_OK");
            return HRESULT.S_OK;
        }

        public virtual int ReportResult(
            int ntsStatus,
            int ntsSubstatus,
            out string ppszOptionalStatusText,
            out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
        )
        {
            Logger.Write($"ntsStatus: {ntsStatus}; ntsSubstatus: {ntsSubstatus}");

            ppszOptionalStatusText = "";
            pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;

            return HRESULT.S_OK;
        }

        public virtual int GetUserSid(out string sid)
        {
            Logger.Write();

            sid = this.sid;

            Console.WriteLine($"Returning sid: {sid}");
            return HRESULT.S_OK;
        }
    }
}
