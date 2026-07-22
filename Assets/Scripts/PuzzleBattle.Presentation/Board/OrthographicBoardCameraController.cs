using UnityEngine;

namespace PuzzleBattle.Presentation.Board
{
    public sealed class OrthographicBoardCameraController : MonoBehaviour
    {
        [SerializeField] private Camera boardCamera;
        [SerializeField, Min(0.01f)] private float orthographicSize = 5f;

        private void Awake()
        {
            ConfigureProjection();
        }

        public void SetCamera(Camera value)
        {
            boardCamera = value;
        }

        public bool ConfigureProjection()
        {
            if (boardCamera == null)
                return false;
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = Mathf.Max(0.01f, orthographicSize);
            return true;
        }
    }
}
