/*--------------------------------------------------------------------------------*
  Copyright (C)Nintendo All rights reserved.

  These coded instructions, statements, and computer programs contain proprietary
  information of Nintendo and/or its licensed developers and are protected by
  national and international copyright laws. They may not be disclosed to third
  parties or copied or duplicated in any form, in whole or in part, without the
  prior written consent of Nintendo.

  The content herein is highly confidential and should be handled accordingly.
 *--------------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

/*!
  @brief PiaUnity C# スクリプトのバージョン情報を管理している構造体です。
*/

public struct PiaPluginVersion
{
    public const string  Date         = "20221012";   //!< 日付です。
    public const UInt32  VersionMajor = 0;                               //!< メジャーバージョンの値です。
    public const UInt32  VersionMinor = 38;                               //!< マイナーバージョンの値です。
    public const UInt32  VersionMicro =  0;                              //!< マイクロバージョンの値です。
    public const UInt32  Revision     = 65781;                            //!< リビジョンの値です。
    public const string  RootName     = "NintendoSDK-PiaUnity0_38_0_Pia5_44_0_sdk14_3_0-en";                            //!< ルートの名前です。
}
