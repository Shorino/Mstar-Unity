using UnityEngine;
using UnityEngine.EventSystems;

public class DisableSelectedES : MonoBehaviour
{
    EventSystem es;
    bool select;

    // Start is called before the first frame update
    void Start()
    {
        es = GetComponent<EventSystem>();
        select = true;
    }

    void Update()
    {
        if(select == false)
        {
            if(es.currentSelectedGameObject != null)
            {
                es.SetSelectedGameObject(null);
                select = false;
            }
            else
            {
                select = true;
            }
        }
    }

    public void DeselectedUI()
    {
        select = false;
    }
}
