﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class UILogInitializer : MonoBehaviour
    {
        [SerializeField] Text debugText;

        void Awake()
        {
            DontDestroyOnLoad(debugText.transform.parent.gameObject);
            UILogManager.log = new Log(debugText);
        }
    }
}