using UnityEngine;


namespace FGJ26
{
    public class LevelTile : MonoBehaviour
    {

        [SerializeField] public TileWall tileWallNorth;
        [SerializeField] public TileWall tileWallEast;
        [SerializeField] public TileWall tileWallSouth;
        [SerializeField] public TileWall tileWallWest;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmos()
        {
            float offset = 0.15f;
            float length = 1.9f;
            float width = 0.05f;
            Gizmos.color = GetWallColor(tileWallNorth.type);
            Gizmos.DrawCube(transform.position + new Vector3(0, 0.25f, 1 - offset), new Vector3(length, 0.5f, width));
            Gizmos.color = GetWallColor(tileWallEast.type);
            Gizmos.DrawCube(transform.position + new Vector3(1 - offset, 0.25f, 0), new Vector3(width, 0.5f, length));
            Gizmos.color = GetWallColor(tileWallSouth.type);
            Gizmos.DrawCube(transform.position + new Vector3(0, 0.25f, -1 + offset), new Vector3(length, 0.5f, width));
            Gizmos.color = GetWallColor(tileWallWest.type);
            Gizmos.DrawCube(transform.position + new Vector3(-1 + offset, 0.25f, 0), new Vector3(width, 0.5f, length));
        }

        private static Color GetWallColor(WallType wall)
        {
            switch (wall)
            {
                case WallType.open:
                    return new Color(0f, 1f, 0f, 0.1f);
                case WallType.wall:
                    return new Color(1f, 0f, 0f, 0.5f);
                case WallType.door:
                    return new Color(0f, 0f, 1f, 0.5f);
                default:
                    return new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }
}
