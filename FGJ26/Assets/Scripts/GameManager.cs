using UnityEngine;
namespace FGJ26
{
    public class GameManager : MonoBehaviour
    {

        public static GameManager instance;

        private PlayerController playerController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            playerController = FindFirstObjectByType<PlayerController>();
            Debug.Log("PlayerController found: " + playerController);
            StartGame();
        }


        public void StartGame()
        {
            Debug.Log("Game started");
            // playerController.mainCamera.enabled = true;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
