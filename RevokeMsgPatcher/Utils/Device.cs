using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RevokeMsgPatcher.Utils
{
    public class Device
    {
        private static string macID = null;
        private static string osVersion = null;

        private static string fingerPrint = null;

        #region PROP, get it only once

        public static string MacID
        {
            get
            {
                if (macID == null)
                {
                    macID = ObtainMacID();
                }
                return macID;
            }
        }

        public static string OSVersion
        {
            get
            {
                if (osVersion == null)
                {
                    var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                                select x.GetPropertyValue("Caption")).FirstOrDefault();
                    osVersion = name != null ? name.ToString() : "Unknown";
                }
                return osVersion;
            }
        }
        #endregion

        /// <summary>
        /// Calculate GUID
        /// </summary>
        /// <returns>GUID</returns>
        public static string Value()
        {
            try
            {
                if (fingerPrint == null)
                {
                    fingerPrint = GetHash(
                        "MAC >> " + MacID
                        );
                }
                return fingerPrint;
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }

        }

        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }

        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }


        #region Original Device ID Getting Code

        public static string ObtainMacID()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }

        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            try
            {
                ManagementClass mc = new ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo[wmiMustBeTrue].ToString() == "True")
                    {
                        //Only get the first one
                        if (result == "")
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            try
            {
                ManagementClass mc = new ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    //Only get the first one
                    if (result == "")
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                }
            }
            catch
            {
            }
            return result;
        }
        #endregion
    }
}
