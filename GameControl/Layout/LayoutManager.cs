using System;
using UnityEngine;

using Ham.GameControl;

namespace Ham.GameControl.Layout
{
    public class LayoutManager : Manager
    {
        public event Action<Vector2Int> OnScreenSizeChanged;

        private Vector2Int lastScreenSize;

        protected override void Awake()
        {
            base.Awake();
            this.lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            Debug.Log("ScalingManager: Awake(), initial screen size: " + this.lastScreenSize);
        }

        void Update()
        {
            Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize != this.lastScreenSize)
            {
                Debug.Log("ScalingManager: Screen size changed: " + currentSize);
                this.lastScreenSize = currentSize;
                this.OnScreenSizeChanged?.Invoke(currentSize);
            }
        }
    }
}