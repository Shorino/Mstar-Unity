using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectricBehaviour : MonoBehaviour
{
    [HideInInspector] public GameObject bubble, bubble2;
    Bubble bubbleComponent, bubble2Component;
    RectTransform rectTrans;

    void Start()
    {
        bubbleComponent = bubble.GetComponent<Bubble>();
        bubble2Component = bubble2.GetComponent<Bubble>();
        rectTrans = GetComponent<RectTransform>();
    }

    void Update()
    {
        #region Destroy self
        if (bubble == null || bubble2 == null)
        {
            Destroy(gameObject);
            return;
        }
        #endregion

        #region Update visual pos and rot
        if (SceneManager.GetActiveScene().name == "NoteEditor")
        {
            if (bubbleComponent.index == EditingPanel.singleton.currentlySelectedBubbleIndex ||
            bubble2Component.index == EditingPanel.singleton.currentlySelectedBubbleIndex)
            {
                float x1 = bubble.transform.position.x;
                float y1 = bubble.transform.position.y;

                float x2 = bubble2.transform.position.x;
                float y2 = bubble2.transform.position.y;

                float distanceX = x2 - x1;
                float distanceY = y2 - y1;
                float distance = Mathf.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
                float angle = -1;

                if (distanceX == 0)
                { // vertically line up
                    if (distanceY > 0)
                    { // above
                        angle = 90;
                    }
                    else
                    { // below
                        angle = 270;
                    }
                }
                else if (distanceY == 0)
                { // horizontally line up
                    if (distanceX > 0)
                    { // right
                        angle = 0;
                    }
                    else
                    { // left
                        angle = 180;
                    }
                }

                if (angle == -1)
                {
                    angle = Mathf.Abs((Mathf.Atan(distanceY / distanceX)) * Mathf.Rad2Deg);
                    if (distanceX < 0 && distanceY > 0)
                    { // upper left quad
                        angle = 180 - angle;
                    }
                    else if (distanceX < 0 && distanceY < 0)
                    { // lower left quad
                        angle += 180;
                    }
                    else if (distanceX > 0 && distanceY < 0)
                    { // lower right quad
                        angle = -angle;
                    }
                }

                rectTrans.position = bubble.transform.position;
                rectTrans.localRotation = Quaternion.Euler(0, 0, angle);
                rectTrans.localScale = new Vector3(distance + 0.5f, 1, 1);
            }
        }
        #endregion
    }
}