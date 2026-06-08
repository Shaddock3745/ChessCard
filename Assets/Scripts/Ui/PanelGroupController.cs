using System.Collections.Generic;
using UnityEngine;

public class PanelGroupController : MonoBehaviour
{
    [SerializeField] List<GameObject> panels;

    private int index = 0;

    private void Start()
    {
        updatePanelView();
    }

    public bool isStart() => index == 0;

    public bool isEnd() => index == panels.Count - 1;

    public void next()
    {
        if (++index >= panels.Count)
        {
            index = panels.Count - 1;
            return;
        }

        updatePanelView();
    }

    public void previous()
    {
        if (--index < 0)
        {
            index = 0;
            return;
        }

        updatePanelView();
    }

    private void updatePanelView()
    {
        for (int i = 0; i < panels.Count; i++)
            panels[i].SetActive(i == index);
    }
}
