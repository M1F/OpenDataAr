using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public Text label;
    public RadialButton buttonPrefab;
    public RadialButton selected;

    public  delegate void OnClickContainer();
    public event OnClickContainer OnMenuClickEvent;

    void Start()
    {
        //string InvokingComponentName = this.transform.parent.parent.GetComponent<Interactable>().InvokingComponentName;
    }

    public void SpawnButtons(Interactable obj)
    {
        StartCoroutine(AnimateButtons(obj));        
    }

    IEnumerator AnimateButtons (Interactable obj)
    {
        for (int i = 0; i < obj.options.Length; i++)
        {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            newButton.transform.SetParent(transform, false);
            float theta = (2 * Mathf.PI / obj.options.Length) * i;
            Debug.Log(obj.options.Length);
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            newButton.transform.localPosition = new Vector3(xPos, yPos, 0f) * 100f;
            newButton.circle.color = obj.options[i].color;
            newButton.icon.sprite = obj.options[i].sprite;
            newButton.Title = obj.options[i].Title;
            newButton.IntTitle = obj.options[i].IntTitle;
            newButton.myMenu = this;
            newButton.Anim();
            yield return new WaitForSeconds(0.06f);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) //OnChooseOption
        {
            if (selected)
            {

                this.transform.parent.FindChild("Button").GetComponent<Interactable>().ChosenOptionInt = selected.IntTitle;
                this.transform.parent.FindChild("Button").GetComponent<Interactable>().ChosenOption = selected.Title;
                //WakeUpComponentByName(this.transform.parent.FindChild("Button").GetComponent<Interactable>().ScriptToCall);
                GameObject.Find("Main Camera").GetComponent<MapsAPI>().MenuInvoker();
            }
            Destroy(gameObject);
        }
    }

    void WakeUpComponentByName(string ComponentName)
    {
        Component[] allComponents = this.transform.parent.FindChild("Button").GetComponents(typeof(MonoBehaviour));
        foreach (MonoBehaviour script in allComponents)
        {
            if (script.GetType().ToString() == ComponentName)
            {
                Debug.Log("SDFSDFSDF");
                if (script.GetType().GetMethod("MenuInvoker") != null) script.Invoke("MenuInvoker", 0);
            }
        }
    }

}