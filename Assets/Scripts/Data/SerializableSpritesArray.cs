using UnityEngine;


[System.Serializable]
public class SerializableSpritesArray
{
    public Sprite this[int x] => sprites[x];
    public Sprite[] sprites;
    public int Length => sprites.Length;
}


[System.Serializable]
public class SerializableSpritesMatrix
{
    public SerializableSpritesArray this[int x] => spritesArrays[x];
    public Sprite this[int x, int y] => spritesArrays[x][y];
    public SerializableSpritesArray[] spritesArrays;
    public int Length => spritesArrays.Length;
    public int GetLength(int x) => spritesArrays[x].Length;
}