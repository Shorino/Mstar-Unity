using UnityEngine;

public class EditorCanvas : MonoBehaviour
{
    public static GameObject canvasEditor;

    // Start is called before the first frame update
    void Start()
    {
        canvasEditor = gameObject;
    }
}
