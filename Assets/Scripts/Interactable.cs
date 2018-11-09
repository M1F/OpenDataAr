using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string ScriptToCall;
    public string ChosenOption;
    public int ChosenOptionInt;
    public string Name;
    public static string Filter;

    [System.Serializable]
    public class Action
    {
        public Color color;
        public Sprite sprite;
        public string Title;
        public int IntTitle;
    }

    public Action[] options;

    void Start()
    {
        Name = gameObject.name??null;
    }

    public void OnMouseDown()
    {
        RadialMenuSpawner.ins.SpawnMenu(this);
    }
}

