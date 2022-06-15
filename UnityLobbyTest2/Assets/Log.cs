using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Log
{
    Text _text;

    public Log (Text text)
    {
        _text = text;
    }
    public void Write(string st)
    {
        _text.text = st;
    }

}
