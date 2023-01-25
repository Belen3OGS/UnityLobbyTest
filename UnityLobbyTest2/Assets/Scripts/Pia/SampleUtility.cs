/*--------------------------------------------------------------------------------*
  Copyright Nintendo.  All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

// -----------------------------------------------------------------------
// Macros for platform-dependent code.
// These macros are used to enable code to run only on specific platforms or builds.
// -----------------------------------------------------------------------
#if (UNITY_EDITOR || UNITY_STANDALONE)
// Only when running in Unity Editor or standalone builds.
// The following macro is enabled when running in Unity Editor, regardless of the Switch Platform setting.
#define UNITY_EDITOR_OR_STANDALONE
#endif

#if (UNITY_SWITCH && !UNITY_EDITOR_OR_STANDALONE)
// Only on Switch builds.
#define UNITY_ONLY_SWITCH
#endif


using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// -----------------------------------------------------------------------
//! @brief  Utility class.
// -----------------------------------------------------------------------
public static class SampleUtility
{
    const int JoyStickNum = 1;      //!<  Specifies game pads to use other than the debug pad (1 when using Joy-Con).

    // -----------------------------------------------------------------------
    // The class for managing the data used for sending and receiving.
    // -----------------------------------------------------------------------
    public class PinnedValue<T> where T : new()
    {
        public T m_Data;
        private GCHandle m_Handle;
        private int m_DataSize;

        public PinnedValue()
        {
            m_Data = new T();
            m_DataSize = Marshal.SizeOf(m_Data);
            PiaPluginUtil.UnityLog("PinnedValue called. " + typeof(T) + "sizeof " + m_DataSize);
            m_Handle = GCHandle.Alloc(m_Data, GCHandleType.Pinned);
        }

        ~PinnedValue()
        {
            PiaPluginUtil.UnityLog("~PinnedValue called.");
            m_Handle.Free();
        }

        public void WriteToBuffer(byte[] buf)
        {
            Marshal.Copy(m_Handle.AddrOfPinnedObject(), buf, 0, m_DataSize);
        }

        public void ReadFromBuffer(byte[] buf)
        {
            Marshal.Copy(buf, 0, m_Handle.AddrOfPinnedObject(), m_DataSize);
        }

        public IntPtr GetIntPtr()
        {
            return m_Handle.AddrOfPinnedObject();
        }
        public int GetSize()
        {
            return m_DataSize;
        }
    }

    // -----------------------------------------------------------------------
    // The class for managing signature keys and encryption keys.
    // -----------------------------------------------------------------------
    public class PinnedKey
    {
        GCHandle m_Handle;
        uint m_KeyDataSize;
        byte[] m_KeyData;

        public PinnedKey(byte[] keyData)
        {
            PiaPluginUtil.UnityLog("PinnedKey called.");
            m_KeyData = (byte[])keyData.Clone();
            m_Handle = GCHandle.Alloc(m_KeyData, GCHandleType.Pinned);
            m_KeyDataSize = (uint)keyData.Length;
        }

        ~PinnedKey()
        {
            PiaPluginUtil.UnityLog("~PinnedKey called.");
            m_Handle.Free();
        }

        public IntPtr GetKeyDataPtr()
        {
            PiaPluginUtil.UnityLog("KeyDataPtr is " + m_Handle.AddrOfPinnedObject());
            return m_Handle.AddrOfPinnedObject();
        }

        public uint GetKeyDataSize()
        {
            return (uint)m_KeyDataSize;
        }
    }

    // -----------------------------------------------------------------------
    // The class for managing screen display.
    // -----------------------------------------------------------------------
    public class DisplayMessage
    {
        private GUIStyle m_DefaultGuiStyle;
        private Vector2 m_DisplayPos;   // Render position.
        private float marginRate = 1.1f;
        private List<string> m_MessageList = new List<string>();
        private List<GUIStyle> m_GuiStyleList = new List<GUIStyle>();
        public DisplayMessage(GUIStyle guiStyle, Vector2 pos)
        {
            m_DefaultGuiStyle = guiStyle;
            m_DisplayPos = pos;
        }
        public void Add(string message)
        {
            m_MessageList.Add(message);
            m_GuiStyleList.Add(m_DefaultGuiStyle);
        }
        public void Add(string message, GUIStyle guiStyle)
        {
            m_MessageList.Add(message);
            m_GuiStyleList.Add(guiStyle);
        }

        public void Clear()
        {
            m_MessageList.Clear();
            m_GuiStyleList.Clear();
        }

        public void Draw()
        {
            for (int i = 0; i < m_MessageList.Count; ++i)
            {
                GUI.Label(new Rect(m_DisplayPos.x, m_DisplayPos.y + m_GuiStyleList[i].fontSize * marginRate * i, 300, 300), m_MessageList[i], m_GuiStyleList[i]);
            }
        }
    }
}
