using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogText : Text
{
    protected override void Awake()
    {
        UILogManager.log = new Log(this);
    }
}
