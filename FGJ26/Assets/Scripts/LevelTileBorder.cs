
[System.Serializable]
public class TileWall
{
    public WallType type = WallType.open;
}

public enum WallType
{
    open,
    wall,
    door
}