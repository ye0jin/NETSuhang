using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private ColorSO wallSO;

    [Header("Prefab")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject outLinePrefab;
    [Header("Left")]
    [SerializeField] private Transform leftWalls;
    [SerializeField] private Transform leftWallsInclude;
    [Header("Right")]
    [SerializeField] private Transform rightWalls;
    [SerializeField] private Transform rightWallsInclude;

    private int currentWallsCnt = 0;
    public int CurrentWallsCnt => currentWallsCnt;


    private float wallScaleY = 10f;

    private void Awake()
    {
        if (Instance != null)
        {
            print("MapManager Error");
        }
        Instance = this;
    }
    private void Start()
    {
        currentWallsCnt = GameManager.Instance.StageUpdate();

        SpawnLeftWalls();
        SpawnRightWalls();
        SpawnOutLines();
    }

    public void SpawnLeftWalls()
    {
        for (int i = 0; i < currentWallsCnt; i++)
        {
            Instantiate(wallPrefab, leftWallsInclude);
        }

        for (int i = 0; i < currentWallsCnt; i++)
        {
            Vector3 wallScale = new Vector3(0.5f, 1, 1);
            wallScale.y = wallScaleY / currentWallsCnt;

            Vector3 wallPos = Vector3.zero; // Initialize
            wallPos.y = wallScale.y * (currentWallsCnt / 2 - i) - (currentWallsCnt % 2 == 0 ? wallScale.y / 2 : 0);
            SetTransform(leftWallsInclude.GetChild(i), wallPos, wallScale);
        }

        ChangeColorLeft();
    }
    public void DestroyLeftWalls()
    {
        Transform[] childList = leftWallsInclude.gameObject.GetComponentsInChildren<Transform>();
        if (childList != null) // null이 아닐 때
        {
            for (int i = 1; i < childList.Length; i++) // 0부터 하면 부모도 삭제
            {
                if (childList[i] != transform) Destroy(childList[i].gameObject);
            }
        }

        leftWallsInclude.DetachChildren();
        SpawnLeftWalls();
    }
    public void ChangeColorLeft()
    {
        leftWalls.transform.DOMoveX(-3.0f, 0.1f).OnComplete(() => leftWalls.transform.DOMoveX(-2.8f, 0.2f));

        int idx = currentWallsCnt;

        List<int> randomList = new List<int>();

        for (int i = 0; i < idx; i++)
        {
            randomList.Add(i);
        }

        for (int i = 0; i < currentWallsCnt; i++)
        {
            int randIdx = Random.Range(0, randomList.Count);

            GameObject wallObject = leftWallsInclude.GetChild(i).gameObject;
            wallObject.GetComponent<Renderer>().material.color = wallSO.RandomMapList[randomList[randIdx]].color;
            wallObject.layer = wallSO.RandomMapList[randomList[randIdx]].layerMask;

            randomList.RemoveAt(randIdx);
        }
    }

    public void SpawnRightWalls()
    {
        for (int i = 0; i < currentWallsCnt; i++)
        {
            Instantiate(wallPrefab, rightWallsInclude);
        }
        //print(rightWallsInclude.childCount);

        for (int i = 0; i < currentWallsCnt; i++)
        {
            Vector3 wallScale = new Vector3(0.5f, 1, 1);
            wallScale.y = wallScaleY / currentWallsCnt;

            Vector3 wallPos = Vector3.zero; // Initialize
            wallPos.y = wallScale.y * (currentWallsCnt / 2 - i) - (currentWallsCnt % 2 == 0 ? wallScale.y / 2 : 0);
            SetTransform(rightWallsInclude.GetChild(i), wallPos, wallScale);
        }

        ChangeColorRight();
    }
    public void DestroyRightWalls()
    {
        Transform[] childList = rightWallsInclude.gameObject.GetComponentsInChildren<Transform>();
        if (childList != null) // null이 아닐 때
        {
            for (int i = 1; i < childList.Length; i++) // 0부터 하면 부모도 삭제
            {
                if (childList[i] != transform) Destroy(childList[i].gameObject);
            }
        }

        rightWallsInclude.DetachChildren();
        //print(rightWallsInclude.childCount);
        SpawnRightWalls();
    }
    public void ChangeColorRight()
    {
        rightWalls.transform.DOMoveX(3.0f, 0.1f).OnComplete(() => rightWalls.transform.DOMoveX(2.8f, 0.2f));
        int idx = currentWallsCnt;

        List<int> randomList = new List<int>();

        for (int i = 0; i < idx; i++)
        {
            randomList.Add(i);
        }

        for (int i = 0; i < currentWallsCnt; i++)
        {
            int randIdx = Random.Range(0, randomList.Count);

            GameObject wallObject = rightWallsInclude.GetChild(i).gameObject;
            wallObject.GetComponent<Renderer>().material.color = wallSO.RandomMapList[randomList[randIdx]].color;
            wallObject.layer = wallSO.RandomMapList[randomList[randIdx]].layerMask;

            randomList.RemoveAt(randIdx);
        }
    }

    private void SpawnOutLines()
    {
        GameObject outlineLeft = Instantiate(outLinePrefab, leftWalls);
        outlineLeft.transform.position = new Vector3(-3f, 0, 0);
        GameObject outlineRight = Instantiate(outLinePrefab, rightWalls);
        outlineRight.transform.position = new Vector3(3, 0, 0);
    }
    private void SetTransform(Transform transform, Vector3 wallPos, Vector3 wallScale)
    {
        transform.localPosition = wallPos;
        transform.localScale = wallScale;
    }

    public void WallCntUpdate()
    {
        currentWallsCnt = GameManager.Instance.StageUpdate();
    }

    public void ResetWalls()
    {
        WallCntUpdate();
        DestroyLeftWalls();
        DestroyRightWalls();
        print(currentWallsCnt);
    }
}
