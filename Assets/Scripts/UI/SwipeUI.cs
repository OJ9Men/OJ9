using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour
{
    [SerializeField]
    private Scrollbar scrollBar;
    [SerializeField]
    private float swipeTime = 0.2f;
    [SerializeField]
    private float swipeDistance = 50.0f;
    [SerializeField]
    private GameObject indicatorCirclePrefab;
    [SerializeField]
    private Transform indicatorTransform;

    private Transform[] indicators;

    private float[] scrollPageValues;
    private float valueDistance = 0.0f;
    private int currentPage = 0;
    private int maxPage = 0;
    private float startTouchX;
    private float endTouchX;
    private bool isSwipeMode = false;
    private float indicatorCircleScale = 1.6f;

    private void Awake()
    {
        scrollPageValues = new float[transform.childCount];

        valueDistance = 1f / (scrollPageValues.Length - 1f);

        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            scrollPageValues[i] = valueDistance * i;
        }

        maxPage = transform.childCount;
    }


    // Start is called before the first frame update
    void Start()
    {
        SetScrollBarValue(0);

        indicators = new Transform[(int)transform.childCount];
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject circle = Instantiate(indicatorCirclePrefab, indicatorTransform, false);
            indicators[i] = circle.transform;
        }
    }

    public void SetScrollBarValue(int index)
    {
        currentPage = index;
        scrollBar.value = scrollPageValues[index];
    }

    private void UpdateInput()
    {
        if (isSwipeMode == true)
        {
            return;
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchX = Input.mousePosition.x;

            UpdateSwipe();
        }
#endif

        // TODO
        // In mobile
        //if (Input.touchCount == 1)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        startTouchX = touch.position.x;
        //    }
        //    else if (touch.phase == TouchPhase.Ended)
        //    {
        //        endTouchX = touch.position.x;
        //
        //        UpdateSwipe();
        //    }
        //}
    }

    private void UpdateSwipe()
    {
        if (Mathf.Abs(startTouchX - endTouchX) < swipeDistance)
        {
            StartCoroutine(OnSwipeOnStep(currentPage));
            return;
        }

        bool isLeft = startTouchX < endTouchX ? true : false;

        if (isLeft)
        {
            if (currentPage == 0)
            {
                return;
            }

            --currentPage;
        }
        else
        {
            if (currentPage == maxPage - 1)
            {
                return;
            }

            ++currentPage;
        }

        StartCoroutine(OnSwipeOnStep(currentPage));
    }

    private IEnumerator OnSwipeOnStep(int index)
    {
        float start = scrollBar.value;
        float current = 0f;
        float percent = 0f;

        isSwipeMode = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;

            scrollBar.value = Mathf.Lerp(start, scrollPageValues[index], percent);

            yield return null;
        }

        isSwipeMode = false;
    }

    void Update()
    {
        UpdateInput();
        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            indicators[i].localScale = Vector2.one;
            indicators[i].GetComponent<Image>().color = Color.white;

            if (scrollBar.value < scrollPageValues[i] + (valueDistance / 2) &&
                scrollBar.value > scrollPageValues[i] - (valueDistance / 2))
            {
                indicators[i].localScale = Vector2.one * indicatorCircleScale;
                indicators[i].GetComponent<Image>().color = Color.black;
            }
        }
    }
}
