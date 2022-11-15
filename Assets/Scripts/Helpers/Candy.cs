using UnityEngine;

public struct Candy
{
    public Candy(Sprite sprite, int id)
    {
        SpriteCandy = sprite;
        ID = id;
    } 

    public Sprite SpriteCandy { get; }

    public int ID { get; }
}