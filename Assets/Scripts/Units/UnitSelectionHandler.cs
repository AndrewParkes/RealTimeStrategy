using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{

    [SerializeField]
    private RectTransform unitSelectionArea = null;

    [SerializeField]
    private LayerMask layerMask = new LayerMask();

    private Vector2 startPostion;

    private RTSPlayer player;

    private Camera mainCamera;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void Update()
    {

        if(player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed) 
        {
            foreach(Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }
        
        unitSelectionArea.gameObject.SetActive(true);

        startPostion = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPostion.x;
        float areaHeight = mousePosition.y - startPostion.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPostion + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);
        
        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return;
            }

            if(!hit.collider.TryGetComponent<Unit>(out Unit unit))
            {
                return;
            }

            if(!unit.hasAuthority)
            {
                return;
            }

            SelectedUnits.Add(unit);

            foreach(Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }
        
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta/2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta/2);

        foreach(Unit unit in player.GetMyUnits())
        {

            if(SelectedUnits.Contains(unit))
            {
                continue;
            }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }
}
