using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using DG.Tweening;

namespace BP
{
    public class TutorialController : BaseBehaviour
    {
        #region Constants
        public readonly string[] TUTORIAL_FILE_NAME_LIST = { "Board_Tutorial_1", "Board_Tutorial_2", "Board_Tutorial_3" };
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private GameObject m_TutorialLayout;
        [SerializeField]
        private RectTransform m_HighlightRectTrans;
        [SerializeField]
        private RectTransform m_HandRectTrans;
        #endregion

        #region Properties
        private Queue<TutorialData> m_TutorialQueue;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure()
        {
            m_TutorialLayout.SetActive(false);
            m_HandRectTrans.gameObject.SetActive(false);

            m_TutorialQueue = new Queue<TutorialData>();
            bool shownTutorial = PrefsUtils.GetBool(Consts.PREFS_SHOWN_TUTORIAL, false);
            if (!shownTutorial)
            {
                for (int i = 0; i < TUTORIAL_FILE_NAME_LIST.Length; i++)
                {
                    TextAsset textAsset = (TextAsset) Resources.Load(TUTORIAL_FILE_NAME_LIST[i], typeof(TextAsset));

                    if (textAsset != null)
                    {
                        var rootJsonData = JSON.Parse(textAsset.text);
                        var boardJson = rootJsonData["board"];
                        var blockJson = rootJsonData["blocks"];
                        JSONArray targetPosJson = rootJsonData["targetPositions"].AsArray;
                        Logger.d("Target: ", targetPosJson.ToString());

                        TutorialData tutorialData = new TutorialData();
                        tutorialData.boardData = boardJson;
                        tutorialData.blockData = blockJson;

                        tutorialData.targetPositions = new List<GridCoordinate>();
                        for (int j = 0; j < targetPosJson.Count; j++)
                        {
                            GridCoordinate coord = new GridCoordinate(targetPosJson[j]["coordX"].AsInt, targetPosJson[j]["coordY"].AsInt);
                            tutorialData.targetPositions.Add(coord);
                        }

                        m_TutorialQueue.Enqueue(tutorialData);
                    }
                }
            }
        }

        public TutorialData GetTutorial()
        {
            if (m_TutorialQueue.Count == 0)
                return null;

            return m_TutorialQueue.Dequeue();
        }

        public void Show(Vector2 targetPos, float width, float height)
        {
            m_TutorialLayout.SetActive(true);

            m_HighlightRectTrans.position = new Vector3(targetPos.x, targetPos.y, 0f);
            m_HighlightRectTrans.sizeDelta = new Vector2(width, height);
        }

        public void Hide()
        {
            HideHand();
            m_TutorialLayout.SetActive(false);
        }

        public void AnimateMovingHand(Vector2 blockPosition)
        {
            DOTween.Kill("hand_animation_seq");
            m_HandRectTrans.gameObject.SetActive(true);
            Vector2 startPos = blockPosition + new Vector2(1, -1);
            m_HandRectTrans.position = startPos;
            m_HandRectTrans.localScale = Vector3.one * 1.2f;

            Sequence handMovingSeq = DOTween.Sequence();
            handMovingSeq.Append(m_HandRectTrans.DOMove(blockPosition, 1f));
            handMovingSeq.Append(m_HandRectTrans.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.OutBack));
            handMovingSeq.Append(m_HandRectTrans.DOMove(m_HighlightRectTrans.position, 1.5f));
            handMovingSeq.Append(m_HandRectTrans.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutBack));
            handMovingSeq.Append(m_HandRectTrans.DOMove(startPos, 2f));
            handMovingSeq.SetLoops(-1);
            handMovingSeq.SetId("hand_animation_seq");
        }

        public void AnimatePressingHand(Vector2 targetPosition)
        {
            DOTween.Kill("hand_animation_seq");
            m_HandRectTrans.gameObject.SetActive(true);
            Vector2 startPos = targetPosition + new Vector2(1, -1);
            m_HandRectTrans.position = startPos;
            m_HandRectTrans.localScale = Vector3.one * 1.2f;

            Sequence handMovingSeq = DOTween.Sequence();
            handMovingSeq.Append(m_HandRectTrans.DOMove(targetPosition, 1f));
            handMovingSeq.Append(m_HandRectTrans.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.OutBack));
            handMovingSeq.Append(m_HandRectTrans.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutBack));
            handMovingSeq.Append(m_HandRectTrans.DOMove(startPos, 1f));
            handMovingSeq.SetLoops(-1);
            handMovingSeq.SetId("hand_animation_seq");
        }

        public void HideHand()
        {
            DOTween.Kill("hand_animation_seq");
            m_HandRectTrans.gameObject.SetActive(false);
        }
        #endregion
    }

    public class TutorialData
    {
        public JSONNode boardData;
        public JSONNode blockData;
        public List<GridCoordinate> targetPositions;
    }
}