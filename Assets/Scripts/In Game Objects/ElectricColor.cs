using UnityEngine;
using UnityEngine.UI;

public class ElectricColor : MonoBehaviour
{
    public void ChangeColor(Color color)
    {
        transform.GetChild(0).GetComponent<Image>().color = color;
        transform.GetChild(1).GetComponent<Image>().color = color;
    }
}
