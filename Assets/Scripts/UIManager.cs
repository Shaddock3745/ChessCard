using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    private static readonly float tileSize = 0.96f;
    private static readonly float startOffeset = -3.36f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        
    }

}
