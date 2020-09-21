using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    TextMeshProUGUI textMesh;
    public static long score;

    // Start is called before the first frame update
    void Start()
    {
        ResetScore();
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.text = score.ToString();
    }

    public static void ResetScore()
    {
        score = 0;
    }
}
