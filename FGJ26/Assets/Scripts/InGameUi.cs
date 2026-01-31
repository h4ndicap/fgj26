using UnityEngine;
using TMPro;

namespace FGJ26
{

    public class InGameUi : MonoBehaviour
    {

        [SerializeField] private PlayerController playerController;

        [SerializeField] private TextMeshProUGUI actionPointsText;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            playerController.OnActionPointsChanged += OnActionPointsChanged;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnActionPointsChanged()
        {
            actionPointsText.text = playerController.CurrentActionPoints.ToString();
        }
    }
}