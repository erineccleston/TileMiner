﻿using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    private TileDatum Data;
    private float Damage = 0;
    private int Depth;

    private bool Animating = false;
    private Color NewColor;

    private Vector3 LastPosition;

    private SpriteRenderer SR;
    private BoxCollider2D BC;

    public static int NumAnimating = 0;
    public const float AnimTime = 1.5f;

    private void Start()
	{
        LastPosition = transform.position;
        SR = gameObject.AddComponent<SpriteRenderer>();
        BC= gameObject.AddComponent<BoxCollider2D>();
        BC.size = new Vector2(1, 1);
        ResetTile(false);
    }

    public void ResetTile(bool animate)
    {
        // needs to change based on direction
        Vector3 position = new Vector3(transform.position.x, transform.position.y, PlayerController.Position.z);
        Data = WorldManager.TileTable[WorldManager.GetTileType(position, out Depth)];

        //if (depth != 0)
        //{
        //    Sprite temp = Data.Sprite;
        //    Data = WorldManager.TileTable[TileType.Air];
        //    Data.Sprite = temp;
        //}

        ChangeSprite(Depth, animate);
        name = Data.Type.ToString();
        ResetDamage();
    }

    private void ChangeSprite(int depth, bool animate)
    {
        int shade = (16 - depth * 2) * (16 - depth * 2) - 1;
        byte value = depth < 16 / 2 ? (byte)shade : (byte)0;
        NewColor = new Color32(value, value, value, 255);

        if (animate)
        {
            Animating = true;
            NumAnimating++;
        }
        else
        {
            SR.sprite = Data.Sprite;
            SR.color = NewColor;
        }
    }

    public static void ResetAll()
    {
        TileBehaviour[] allTiles = FindObjectsOfType<TileBehaviour>();
        float[] delays = new float[allTiles.Length];

        float left = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - 1;
        float top = Camera.main.transform.position.y + Camera.main.orthographicSize + 1;

        float max = 0;
        for (int i = 0; i < allTiles.Length; i++)
        {
            delays[i] = Mathf.Abs(left - allTiles[i].transform.position.x) / Camera.main.aspect
                      + Mathf.Abs(top - allTiles[i].transform.position.y);
            if (delays[i] > max)
                max = delays[i];
        }

        for (int i = 0; i < allTiles.Length; i++)
            allTiles[i].Invoke("ResetTile", delays[i] / max * AnimTime);
    }

    private void ResetTile()
    {
        ResetTile(true);
    }

    private void Update()
    {
        if (Animating)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 360);

            if (transform.eulerAngles.y > 90)
            {
                SR.sprite = Data.Sprite;
                SR.color = NewColor;
                SR.flipX = true;
            }

            if (transform.eulerAngles.y > 180)
            {
                Animating = false;
                NumAnimating--;
                SR.flipX = false;
                transform.eulerAngles = new Vector3(0, 0, 0);
                Destroy(BC);
                BC = gameObject.AddComponent<BoxCollider2D>();
                BC.size = new Vector2(1, 1);
            }
        }

        if (LastPosition != transform.position)
        {
            ResetTile(false);
            LastPosition = transform.position;
        }
    }

    public void Mine(float amount)
    {
        if (Data.Hardness == -1 || Animating || Depth > 1)
            return;

        Damage += amount;
        if (Damage >= Data.Hardness)
            Break();
    }

    public void ResetDamage()
    {
        Damage = 0;
    }

    private void Break()
    {
        Vector3 position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, PlayerController.Position.z);
        WorldManager.SetTileType(position + new Vector3(0, 0, Depth), TileType.Air);
        ResetTile(true);
    }
}
