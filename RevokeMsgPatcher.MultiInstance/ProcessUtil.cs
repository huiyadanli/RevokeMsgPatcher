using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.MultiInstance
{
    /// <summary>
    /// 微信多开原理：
    /// https://mp.weixin.qq.com/s/bb7XMxop7e8rd7YqQ88nyA
    /// 参考：
    /// https://stackoverflow.com/questions/54872228/c-sharp-how-to-find-all-handles-associated-with-current-process
    /// https://hintdesk.com/2010/05/22/c-get-all-handles-of-a-given-process-in-64-bits/
    /// </summary>
    public class ProcessUtil
    {
        /// <summary>
        /// https://www.geoffchappell.com/studies/windows/km/ntoskrnl/api/ex/sysinfo/handle_table_entry.htm?ts=0,242
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_HANDLE_INFORMATION
        { // Information Class 16
            public ushort ProcessID;
            public ushort CreatorBackTrackIndex;
            public byte ObjectType;
            public byte HandleAttribute;
            public ushort Handle;
            public IntPtr Object_Pointer;
            public IntPtr AccessMask;
        }

        private enum OBJECT_INFORMATION_CLASS : int
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct OBJECT_NAME_INFORMATION
        { // Information Class 1
            public UNICODE_STRING Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [Flags]
        private enum PROCESS_ACCESS_FLAGS : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("ntdll.dll")]
        private static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(PROCESS_ACCESS_FLAGS dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("ntdll.dll")]
        private static extern int NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool GetHandleInformation(IntPtr hObject, out uint lpdwFlags);

        private const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
        private const int DUPLICATE_CLOSE_SOURCE = 0x1;
        private const int DUPLICATE_SAME_ACCESS = 0x2;

        private const int CNST_SYSTEM_HANDLE_INFORMATION = 0x10;
        private const int OBJECT_TYPE_MUTANT = 17;

        public static List<SYSTEM_HANDLE_INFORMATION> GetHandles(Process process)
        {
            List<SYSTEM_HANDLE_INFORMATION> aHandles = new List<SYSTEM_HANDLE_INFORMATION>();
            int handle_info_size = Marshal.SizeOf(new SYSTEM_HANDLE_INFORMATION()) * 20000;
            IntPtr ptrHandleData = IntPtr.Zero;
            try
            {
                ptrHandleData = Marshal.AllocHGlobal(handle_info_size);
                int nLength = 0;

                while (NtQuerySystemInformation(CNST_SYSTEM_HANDLE_INFORMATION, ptrHandleData, handle_info_size, ref nLength) == STATUS_INFO_LENGTH_MISMATCH)
                {
                    handle_info_size = nLength;
                    Marshal.FreeHGlobal(ptrHandleData);
                    ptrHandleData = Marshal.AllocHGlobal(nLength);
                }

                long handle_count = Marshal.ReadIntPtr(ptrHandleData).ToInt64();
                IntPtr ptrHandleItem = ptrHandleData + Marshal.SizeOf(ptrHandleData);

                for (long lIndex = 0; lIndex < handle_count; lIndex++)
                {
                    SYSTEM_HANDLE_INFORMATION oSystemHandleInfo = new SYSTEM_HANDLE_INFORMATION();
                    oSystemHandleInfo = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ptrHandleItem, oSystemHandleInfo.GetType());
                    ptrHandleItem += Marshal.SizeOf(new SYSTEM_HANDLE_INFORMATION());
                    if (oSystemHandleInfo.ProcessID != process.Id) { continue; }
                    aHandles.Add(oSystemHandleInfo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrHandleData);
            }
            return aHandles;
        }

        public static bool FindAndCloseWeChatMutexHandle(SYSTEM_HANDLE_INFORMATION systemHandleInformation, Process process)
        {
            IntPtr ipHandle = IntPtr.Zero;
            IntPtr openProcessHandle = IntPtr.Zero;
            IntPtr hObjectName = IntPtr.Zero;
            try
            {
                PROCESS_ACCESS_FLAGS flags = PROCESS_ACCESS_FLAGS.DupHandle | PROCESS_ACCESS_FLAGS.VMRead;
                openProcessHandle = OpenProcess(flags, false, process.Id);
                // 通过 DuplicateHandle 访问句柄
                if (!DuplicateHandle(openProcessHandle, new IntPtr(systemHandleInformation.Handle), GetCurrentProcess(), out ipHandle, 0, false, DUPLICATE_SAME_ACCESS))
                {
                    return false;
                }

                int nLength = 0;
                hObjectName = Marshal.AllocHGlobal(256 * 1024);

                // 查询句柄名称
                while ((uint)(NtQueryObject(ipHandle, (int)OBJECT_INFORMATION_CLASS.ObjectNameInformation, hObjectName, nLength, ref nLength)) == STATUS_INFO_LENGTH_MISMATCH)
                {
                    Marshal.FreeHGlobal(hObjectName);
                    if (nLength == 0)
                    {
                        Console.WriteLine("Length returned at zero!");
                        return false;
                    }
                    hObjectName = Marshal.AllocHGlobal(nLength);
                }
                OBJECT_NAME_INFORMATION objObjectName = new OBJECT_NAME_INFORMATION();
                objObjectName = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(hObjectName, objObjectName.GetType());

                if (objObjectName.Name.Buffer != IntPtr.Zero)
                {
                    string strObjectName = Marshal.PtrToStringUni(objObjectName.Name.Buffer);
                    Console.WriteLine(strObjectName);
                    //  \Sessions\1\BaseNamedObjects\_WeChat_App_Instance_Identity_Mutex_Name
                    if (strObjectName.Contains("_Instance_Identity_Mutex_Name"))
                    {
                        // 通过 DuplicateHandle DUPLICATE_CLOSE_SOURCE 关闭句柄
                        IntPtr mHandle = IntPtr.Zero;
                        if (DuplicateHandle(openProcessHandle, new IntPtr(systemHandleInformation.Handle), GetCurrentProcess(), out mHandle, 0, false, DUPLICATE_CLOSE_SOURCE))
                        {
                            CloseHandle(mHandle);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(hObjectName);
                CloseHandle(ipHandle);
                CloseHandle(openProcessHandle);
            }
            return false;
        }

        /// <summary>
        /// 关闭微信的互斥句柄
        /// </summary>
        /// <param name="processes">微信的进程</param>
        /// <returns></returns>
        public static void CloseMutexHandle(Process[] processes)
        {
            foreach (Process process in processes)
            {
                CloseMutexHandle(process);
            }
        }

        public static bool CloseMutexHandle(Process process)
        {
            bool existMutexHandle = false;
            List<SYSTEM_HANDLE_INFORMATION> aHandles = GetHandles(process);
            foreach (SYSTEM_HANDLE_INFORMATION handle in aHandles)
            {
                // Mutant 类型的句柄
                // if (handle.ObjectType == OBJECT_TYPE_MUTANT)
                // {
                    if (FindAndCloseWeChatMutexHandle(handle, process))
                    {
                        existMutexHandle = true;
                    }
                // }
            }
            return existMutexHandle;
        }
    }
}