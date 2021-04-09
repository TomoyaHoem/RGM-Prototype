using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundManager : MonoBehaviour
{

    GameObject backGround;
    SpriteRenderer sR;

    int curRes = 1;

    public Camera mainCamera;

    public Texture2D grid1X;
    Sprite gridSprite1X;

    public Texture2D grid5X;
    Sprite gridSprite5X;

    public Texture2D grid10X;
    Sprite gridSprite10X;

    public Texture2D grid20X;
    Sprite gridSprite20X;

    public Texture2D grid40X;
    Sprite gridSprite40X;

    public Texture2D grid80X;
    Sprite gridSprite80X;

    public Texture2D grid160X;
    Sprite gridSprite160X;


    void Awake()
    {
        //background gameobject
        backGround = new GameObject("GridBackground", typeof(SpriteRenderer));
        backGround.transform.position = new Vector3(0, 0, 10);

        //initialize all sprites and switch between
        gridSprite1X = Sprite.Create(grid1X, new Rect(0.0f, 0.0f, grid1X.width, grid1X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite5X = Sprite.Create(grid5X, new Rect(0.0f, 0.0f, grid5X.width, grid5X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite10X = Sprite.Create(grid10X, new Rect(0.0f, 0.0f, grid10X.width, grid10X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite20X = Sprite.Create(grid20X, new Rect(0.0f, 0.0f, grid20X.width, grid20X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite40X = Sprite.Create(grid40X, new Rect(0.0f, 0.0f, grid40X.width, grid40X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite80X = Sprite.Create(grid80X, new Rect(0.0f, 0.0f, grid80X.width, grid80X.height), new Vector2(0.5f, 0.5f), 1000, 0);
        gridSprite160X = Sprite.Create(grid160X, new Rect(0.0f, 0.0f, grid160X.width, grid160X.height), new Vector2(0.5f, 0.5f), 1000, 0);

        sR = backGround.GetComponent<SpriteRenderer>();
        sR.drawMode = SpriteDrawMode.Tiled;

        SwitchTo(gridSprite1X, 1);
    }

    void Update()
    {
        float size = mainCamera.orthographicSize;

        Vector2 curPos = new Vector2(Mathf.Round(((int)mainCamera.transform.position.x / curRes) * curRes), Mathf.Round(((int)mainCamera.transform.position.y / curRes) * curRes));

        if((Vector2)mainCamera.transform.position != curPos)
        {
            backGround.transform.position = new Vector3(curPos.x, curPos.y, 10);
        }

        if(size < 3)
        {
            SwitchTo(gridSprite1X, 1);
        } 
        else if (size < 15)
        {
            SwitchTo(gridSprite5X, 5);
        }
        else if (size < 30)
        {
            SwitchTo(gridSprite10X, 10);
        }
        else if (size < 60)
        {
            SwitchTo(gridSprite20X, 20);
        }
        else if (size < 120)
        {
            SwitchTo(gridSprite40X, 40);
        }
        else if (size < 240)
        {
            SwitchTo(gridSprite80X, 80);
        } else
        {
            SwitchTo(gridSprite160X, 160);
        }

    }

    void SwitchTo(Sprite grid, int res)
    {
        if (backGround.transform.localScale.x == res) return;

        curRes = res * 5;

        sR.sprite = grid;
        sR.size = new Vector2(29, 21);
        sR.transform.localScale = new Vector3(res, res, 1);
    }
}
