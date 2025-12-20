using UnityEngine;

namespace Game
{
    public class LoseCondition : MonoBehaviour
    {
        public static LoseCondition Instance => _instance;
        public static LoseCondition _instance;
        public GameObject JoystickToDisable;
        public GameObject UI;
        public BrickMb[] Bricks;
        private int _bricksLeft;

        public void Awake()
        {
            _instance = this;
        }

        public void Start()
        {
            Bricks = FindObjectsByType<BrickMb>(FindObjectsSortMode.None);
            foreach (var brick in Bricks)
            {
                if (brick.TryGetComponent<HealthCompponent>(out var healthComp))
                {
                    //TODO: Нет очистки от событий 
                    healthComp.OnDeath +=  UpdateBricksLeft; 
                }
            }
            _bricksLeft = Bricks.Length;
        }

        private void UpdateBricksLeft()
        {
            _bricksLeft--;
            if (_bricksLeft < 1)
            {
                JoystickToDisable.SetActive(false);
                UI.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
}