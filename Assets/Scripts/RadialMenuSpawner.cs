using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuSpawner : MonoBehaviour
{

    public static RadialMenuSpawner ins;
    public RadialMenu menuPrefab;

    void Awake()
    {
        ins = this;
    }

    public void SpawnMenu(Interactable obj)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        this.transform.SetParent(obj.transform, false);
        newMenu.transform.SetParent(transform, false);
        newMenu.transform.localPosition = new Vector3(-442, 111, 136); ;//Input.mousePosition;//obj.transform.position;//////
        //Debug.Log(newMenu.transform.position.x+"+"+ newMenu.transform.position.y+"+"+ newMenu.transform.position.z);
        //Debug.Log(obj.transform.position.x + "+" + obj.transform.position.y + "+" + obj.transform.position.z);
        newMenu.label.text = obj.Name.ToUpper ();
        newMenu.SpawnButtons(obj);
    }
}