using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using TDTK;
using UnityEngine.EventSystems;

namespace TDTK
{
    public class UI : MonoBehaviour
    {
        public enum _BuildMode { PointNBuild, DragNDrop };
        public _BuildMode buildMode = _BuildMode.PointNBuild;
        public static bool UseDragNDrop() { return instance.buildMode == _BuildMode.PointNBuild ? false : true; }

        private UnitTower selectedTower;

        public float fastForwardTimeScale = 4;
        public static float GetFFTime() { return instance.fastForwardTimeScale; }

        public bool disableTextOverlay = false;
        public static bool DisableTextOverlay() { return instance.disableTextOverlay; }

        public bool pauseGameInPerkMenu = true;
        public static bool PauseGameInPerkMenu() { return instance.pauseGameInPerkMenu; }

        public static UI instance;
        void Awake()
        {
            instance = this;
        }

        void Start() { }

        void OnEnable()
        {
            GameControl.onGameOverE += OnGameOver;
            Unit.onDestroyedE += OnUnitDestroyed;
            UnitTower.onUpgradedE += SelectTower;
            BuildManager.onAddNewTowerE += OnNewTower;
        }
        void OnDisable()
        {
            GameControl.onGameOverE -= OnGameOver;
            Unit.onDestroyedE -= OnUnitDestroyed;
            UnitTower.onUpgradedE -= SelectTower;
            BuildManager.onAddNewTowerE -= OnNewTower;
        }

        void OnGameOver(bool playerWon) { StartCoroutine(_OnGameOver(playerWon)); }
        IEnumerator _OnGameOver(bool playerWon)
        {
            UIBuildButton.Hide();
            yield return new WaitForSeconds(1.0f);
            UIGameOverMenu.Show(playerWon);
        }

        void OnUnitDestroyed(Unit unit)
        {
            if (!unit.IsTower()) return;
            if (selectedTower == unit.GetUnitTower()) ClearSelectedTower();
        }

        void Update()
        {
            if (GameControl.GetGameState() == _GameState.Over) return;
            if (UIUtilities.IsCursorOnUI()) return;

            if (!UseDragNDrop() && !UIBuildButton.isOn)
                BuildManager.SetIndicator(Input.mousePosition);

            ProcessInput();
        }

        private void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleInput(Input.mousePosition);
            }
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleInput(touch.position);
                }
            }
        }

        private void HandleInput(Vector3 inputPosition)
        {
            if (IsPointerOverUIObject(inputPosition))
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                UnitTower tower = hit.collider.GetComponent<UnitTower>();

                if (tower != null)
                {
                    SelectTower(tower);
                    UIBuildButton.Hide();
                }
                else
                {
                    if (selectedTower != null)
                    {
                        ClearSelectedTower();
                        return;
                    }

                    if (!UseDragNDrop())
                    {
                        if (BuildManager.CheckBuildPoint(inputPosition) == _TileStatus.Available)
                        {
                            UIBuildButton.Show();
                        }
                        else
                        {
                            UIBuildButton.Hide();
                        }
                    }
                }
            }
            else
            {
                UIBuildButton.Hide();
                ClearSelectedTower();
            }
        }

        private bool IsPointerOverUIObject(Vector3 inputPosition)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = inputPosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        void SelectTower(UnitTower tower)
        {
            selectedTower = tower;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(selectedTower.thisT.position);
            UITowerInfo.SetScreenPos(screenPos);
            UITowerInfo.Show(selectedTower, true);
        }

        public static void ClearSelectedTower()
        {
            if (instance.selectedTower == null) return;
            instance.selectedTower = null;
            UITowerInfo.Hide();
            GameControl.ClearSelectedTower();
        }

        public static UnitTower GetSelectedTower() { return instance.selectedTower; }

        void OnNewTower(UnitTower newTower)
        {
            UIBuildButton.AddNewTower(newTower);
        }
    }
}
