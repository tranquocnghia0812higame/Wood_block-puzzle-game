using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BP
{
    public enum FingerTouchState
    {
        PRESSED,
        HOLD,
        RELEASED
    }

    public class PlayerInput : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Camera m_MainCamera;
        [SerializeField]
        private UnityEngine.EventSystems.EventSystem m_EventSystem;
        #endregion

        #region Properties
        private bool m_CanTouch = false;
        private bool m_HasSelected = false;
        private int m_SelectedIndex = -1;

        private int m_CurrentFingerId;
        private FingerTouchState m_CurrentTouchState = FingerTouchState.RELEASED;
        #endregion

        #region Unity Events
        private void Update()
        {
            if (!m_CanTouch)
                return;

#if UNITY_EDITOR
            Vector2 mousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                HandlePressed(mousePos);
            }

            if (Input.GetMouseButton(0))
            {
                HandleMove(mousePos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                HandleRelease(mousePos);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.pBoosterController.CollectStars(new List<Vector3>() {Vector3.zero});
            }
#elif UNITY_ANDROID || UNITY_IOS
            bool matchedFinger = false;
            if (Input.touchCount == 1)
            {
                if (m_CurrentTouchState == FingerTouchState.RELEASED)
                    m_CurrentFingerId = Input.GetTouch(0).fingerId;

                var touch = Input.GetTouch(0);
                if (m_CurrentFingerId == touch.fingerId)
                {
                    matchedFinger = true;
                    Vector2 touchPos = touch.position;
                    if (touch.phase == TouchPhase.Began)
                    {
                        m_CurrentTouchState = FingerTouchState.PRESSED;
                        HandlePressed(touchPos);
                    }

                    if (touch.phase == TouchPhase.Moved)
                    {
                        m_CurrentTouchState = FingerTouchState.HOLD;
                        HandleMove(touchPos);
                    }

                    if (touch.phase == TouchPhase.Ended)
                    {
                        m_CurrentTouchState = FingerTouchState.RELEASED;
                        HandleRelease(touchPos);
                    }
                }
            }

            if ((!matchedFinger || Input.touchCount == 0 || Input.touchCount > 1) && m_CurrentTouchState != FingerTouchState.RELEASED)
            {
                m_CurrentTouchState = FingerTouchState.RELEASED;
                m_HasSelected = false;
                m_EventSystem.enabled = true;

                if (m_SelectedIndex >= 0)
                    GameManager.Instance.UnselectBlock(m_SelectedIndex, Vector3.zero, true);
            }
#endif
        }
        #endregion

        #region Methods
        public void Configure()
        {
            Input.multiTouchEnabled = false;
            EnableTouch(true);
        }

        public void EnableEventSystem(bool enable)
        {
            m_EventSystem.enabled = enable;
        }

        public void EnableTouch(bool enable)
        {
            m_CanTouch = enable;
        }

        private void HandlePressed(Vector2 touchPos)
        {
            Vector2 worldPos = m_MainCamera.ScreenToWorldPoint(touchPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Selection Area 1"))
                {
                    m_HasSelected = true;
                    m_SelectedIndex = 0;
                    GameManager.Instance.SelectBlock(m_SelectedIndex, worldPos);
                }
                else if (hit.collider.CompareTag("Selection Area 2"))
                {
                    m_HasSelected = true;
                    m_SelectedIndex = 1;
                    GameManager.Instance.SelectBlock(m_SelectedIndex, worldPos);
                }
                else if (hit.collider.CompareTag("Selection Area 3"))
                {
                    m_HasSelected = true;
                    m_SelectedIndex = 2;
                    GameManager.Instance.SelectBlock(m_SelectedIndex, worldPos);
                }
                else if (hit.collider.CompareTag("Use Rotate"))
                {
                    GameManager.Instance.SelectBoosterRotate();
                }
                else if (hit.collider.CompareTag("Buy Rotate"))
                {
                    GameManager.Instance.ShowRotateRewardedVideoPopup();
                }
                else if (hit.collider.CompareTag("Buy Switch"))
                {
                    GameManager.Instance.ShowSwitchRewardedVideoPopup();
                }
                else if (hit.collider.CompareTag("Buy Bomb"))
                {
                    GameManager.Instance.ShowBombRewardedVideoPopup();
                }
                else if (hit.collider.CompareTag("Use Switch"))
                {
                    GameManager.Instance.SelectBoosterSwitch();
                }
                else if (hit.collider.CompareTag("Use Bomb"))
                {
                    GameManager.Instance.SelectBoosterBomb();
                }
                else if (hit.collider.CompareTag("Chest"))
                {
                    GameManager.Instance.OnChestPressed();
                }

                m_EventSystem.enabled = false;
            }
        }

        private void HandleMove(Vector2 touchPos)
        {
            if (m_HasSelected)
            {
                Vector2 worldPos = m_MainCamera.ScreenToWorldPoint(touchPos);
                GameManager.Instance.MoveBlock(m_SelectedIndex, worldPos);
            }
        }

        private void HandleRelease(Vector2 touchPos)
        {
            if (m_HasSelected)
            {
                Vector2 worldPos = m_MainCamera.ScreenToWorldPoint(touchPos);
                GameManager.Instance.UnselectBlock(m_SelectedIndex, worldPos, false);

                m_HasSelected = false;
            }

            m_EventSystem.enabled = true;
        }
        #endregion
    }
}