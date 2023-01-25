/*--------------------------------------------------------------------------------*
  Copyright Nintendo.  All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/


using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// ------------------------------------------------------------------------------------------------------
//! @brief  Class for managing unmanaged memory. Used when unmanaged memory has been allocated and the pointer has passed to C++, and then data passing is being executed.
// ------------------------------------------------------------------------------------------------------
public class UnmanagedMemoryManager
{
    public static IntPtr Alloc(int size)
    {
        IntPtr p = Marshal.AllocHGlobal(size);
        if (p == IntPtr.Zero)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.Alloc : Marshal.AllocHGlobal(" + size + ") failed.");
        }

        try
        {
            s_NeedFreeList.Add(new AllocInfo(p, size));
        }
        catch (OutOfMemoryException e)
        {
            PiaPluginUtil.UnityLog("s_NeedFreeList.Add is OutOfMemoryException.[" + e + "]");
            return IntPtr.Zero;
        }

        return p;
    }

    public static IntPtr Alloc<T>()
    {
        int size = Marshal.SizeOf(typeof(T));
        return Alloc(size);
    }

    public static bool Free(IntPtr p)
    {
        if (p == IntPtr.Zero)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.Free : p == IntPtr.Zero");
            return false;
        }

        List<AllocInfo> allocInfoList = s_NeedFreeList.FindAll(delegate(AllocInfo info)
        {
            if (info == null)
            {
                PiaPluginUtil.UnityLog("List.FindAll is failed.");
                return false;
            }
            return info.ptr == p;
        });
        if (allocInfoList.Count != 1)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.Free : allocInfoList.Count:" + allocInfoList.Count);
            for (int i = 0; i < allocInfoList.Count; ++i)
            {
                Debug.LogErrorFormat("[{0}] 0x{1:X} size:{2}", i, allocInfoList[i].ptr, allocInfoList[i].size);
            }
        }
        else
        {
            s_NeedFreeList.Remove(allocInfoList[0]);
        }

        Marshal.FreeHGlobal(p);
        return true;
    }

    public static void DestroyStructure<T>(IntPtr p)
    {
        Marshal.DestroyStructure(p, typeof(T));
    }

    public static IntPtr WriteObject<T>(T obj, ref int bufferSize, int allocSize = 0)
    {
        if (obj == null)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.WriteObject : obj == null");
            bufferSize = 0;
            return IntPtr.Zero;
        }

        if (allocSize == 0)
        {
            bufferSize = Marshal.SizeOf(typeof(T));
        }
        else
        {
            // The size to allocate has been specified.
            if (allocSize < Marshal.SizeOf(typeof(T)))
            {
                // Smaller than the required size.
                PiaPluginUtil.UnityLog(String.Format("UnmanagedMemoryManager.WriteObject : allocSize({0}) < {1}", allocSize, Marshal.SizeOf(typeof(T))));
                bufferSize = 0;
                return IntPtr.Zero;
            }
            bufferSize = allocSize;
        }
        IntPtr p = Alloc(bufferSize);
        if (p == IntPtr.Zero)
        {
            bufferSize = 0;
            return p;
        }
        Marshal.StructureToPtr(obj, p, false);
        return p;
    }

    public static bool ReadObject<T>(IntPtr p, ref T obj)
    {
        try
        {
            if (p == IntPtr.Zero)
            {
                PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadObject : p == IntPtr.Zero");
                return false;
            }

            obj = (T)Marshal.PtrToStructure(p, typeof(T));
        }
        catch(Exception e)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadObject : exception " + e);
        }
        return true;
    }

    public static IntPtr WriteArray<T>(T[] array, ref int bufferSize)
    {
        if (array.Length == 0)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.WriteArray : array.Length == 0");
            bufferSize = 0;
            return IntPtr.Zero;
        }

        int elemSize = Marshal.SizeOf(typeof(T));
        bufferSize = elemSize * array.Length;
        IntPtr retPtr = Alloc(bufferSize);
        if (retPtr == IntPtr.Zero)
        {
            bufferSize = 0;
            return retPtr;
        }

        IntPtr writePtr = retPtr;
        for (int i = 0; i < array.Length; ++i)
        {
            Marshal.StructureToPtr(array[i], writePtr, false);
            writePtr = new IntPtr(writePtr.ToInt64() + elemSize);
        }

        return retPtr;
    }

    public static bool ReadArray<T>(IntPtr p, int arrayLength, ref T[] array)
    {
        try
        {
            if (p == IntPtr.Zero)
            {
                PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadArray : p == IntPtr.Zero");
                return false;
            }

            int elemSize = Marshal.SizeOf(typeof(T));
            IntPtr readPtr = p;

            for (int i = 0; i < arrayLength; ++i)
            {
                array[i] = (T)Marshal.PtrToStructure(readPtr, typeof(T));
                readPtr = new IntPtr(readPtr.ToInt64() + elemSize);
            }
        }
        catch (Exception e)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadArray : exception " + e);
        }
        return true;
    }

    public static IntPtr WriteList<T>(List<T> list, ref int bufferSize)
    {
        if (list.Count == 0)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.WriteList : list.Count == 0");
            bufferSize = 0;
            return IntPtr.Zero;
        }

        int elemSize = Marshal.SizeOf(typeof(T));
        bufferSize = elemSize * list.Count;
        IntPtr retPtr = Alloc(bufferSize);
        if (retPtr == IntPtr.Zero)
        {
            bufferSize = 0;
            return retPtr;
        }

        IntPtr writePtr = retPtr;
        foreach (var elem in list)
        {
            Marshal.StructureToPtr(elem, writePtr, false);
            writePtr = new IntPtr(writePtr.ToInt64() + elemSize);
        }

        return retPtr;
    }

    public static bool ReadList<T>(IntPtr p, int listCount, ref List<T> list)
    {
        try
        {
            if (p == IntPtr.Zero)
            {
                PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadList : p == IntPtr.Zero");
                return false;
            }

            int elemSize = Marshal.SizeOf(typeof(T));
            IntPtr readPtr = p;

            list.Clear();
            for (int i = 0; i < listCount; ++i)
            {
                list.Add((T)Marshal.PtrToStructure(readPtr, typeof(T)));
                readPtr = new IntPtr(readPtr.ToInt64() + elemSize);
            }
        }
        catch (Exception e)
        {
            PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ReadList : exception " + e);
        }
        return true;
    }

    public static IntPtr WriteUtf8(String str, ref int bufferSize)
    {
        byte[] utf8Str = System.Text.Encoding.UTF8.GetBytes(str);
        int elemSize = Marshal.SizeOf(typeof(byte));
        bufferSize = elemSize * (utf8Str.Length + 1); // For null text.

        IntPtr retPtr = Alloc(bufferSize);
        IntPtr writePtr = retPtr;
        for (int i = 0; i < utf8Str.Length; ++i)
        {
            Marshal.WriteByte(writePtr, utf8Str[i]);
            writePtr = new IntPtr(writePtr.ToInt64() + elemSize);
        }
        Marshal.WriteByte(writePtr, 0); // Add null text.
        return retPtr;
    }

    public static String ReadUtf8(IntPtr pStr, int stringSize)
    {
        byte[] utf8Str = new byte[stringSize];
        int elemSize = Marshal.SizeOf(typeof(byte));
        IntPtr readPtr = pStr;
        for (int i = 0; i < stringSize; ++i)
        {
            utf8Str[i] = Marshal.ReadByte(readPtr);
            readPtr = new IntPtr(readPtr.ToInt64() + elemSize);
        }
        String str = System.Text.Encoding.UTF8.GetString(utf8Str);
        return str;
    }

    public static IntPtr WriteUtf16(String str, ref int bufferSize)
    {
        byte[] utf16Str = System.Text.Encoding.Unicode.GetBytes(str);
        int elemSize = Marshal.SizeOf(typeof(byte));
        bufferSize = elemSize * (utf16Str.Length + 2); // For null text.

        IntPtr retPtr = Alloc(bufferSize);
        IntPtr writePtr = retPtr;
        for (int i = 0; i < utf16Str.Length; ++i)
        {
            Marshal.WriteByte(writePtr, utf16Str[i]);
            writePtr = new IntPtr(writePtr.ToInt64() + elemSize);
        }
        Marshal.WriteByte(writePtr, 0); // Add null text.
        writePtr = new IntPtr(writePtr.ToInt64() + elemSize);
        Marshal.WriteByte(writePtr, 0); // The UTF16 terminating character is '\0\0'.

        return retPtr;
    }

    public static String ReadUtf16(IntPtr pStr, int stringSize)
    {
        byte[] utf16Str = new byte[stringSize];
        int elemSize = Marshal.SizeOf(typeof(byte));
        IntPtr readPtr = pStr;
        for (int i = 0; i < stringSize; ++i)
        {
            utf16Str[i] = Marshal.ReadByte(readPtr);
            readPtr = new IntPtr(readPtr.ToInt64() + elemSize);
        }
        String str = System.Text.Encoding.Unicode.GetString(utf16Str);
        return str;
    }

    public class AllocInfo
    {
        public IntPtr ptr { get; private set; }
        public int size { get; private set; }

        public AllocInfo(IntPtr _ptr, int _size)
        {
            ptr = _ptr;
            size = _size;
        }
    }

    public static void ValidateAllocInfo()
    {
        PiaPluginUtil.UnityLog("UnmanagedMemoryManager.ValidateAllocInfo s_NeedFreeList count:" + s_NeedFreeList.Count);
        for (int i = 0; i < s_NeedFreeList.Count; ++i)
        {
            Debug.LogFormat("[{0}] 0x{1:X} size:{2}", i, s_NeedFreeList[i].ptr.ToInt64(), s_NeedFreeList[i].size);
        }
    }

    public static List<AllocInfo> s_NeedFreeList = new List<AllocInfo>();
}
