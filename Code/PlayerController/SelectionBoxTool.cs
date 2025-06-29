using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SelectionBoxTool : Component
{
    [Property] public List<GameObject> Selected { get; private set; } = new List<GameObject>();
    public CameraComponent Camera { get; private set; }
    
    private const float DRAG_THRESHOLD = 5f; // Pixels needed to start drag
    private Vector3 _selectionBoxStart;
    private Vector3 _selectionBoxEnd;
    private bool _isDragging;
    private bool _multiSelectMode;
    private Vector2 _mousePressPosition;
    private GameObject _initialHitObject;

	protected override void OnAwake()
	{
		Camera = Scene.Camera ?? Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		Sandbox.Mouse.Visibility = MouseVisibility.Visible;
    }

    protected override void OnUpdate()
    {
        HandleMouseInput();
        DrawDebugVisuals();
    }

    private void HandleMouseInput()
    {
        // Update multi-select mode based on Shift key
        _multiSelectMode = Input.Down("Shift");
        
        // Handle mouse press
        if (Input.Pressed("Mouse1"))
        {
            StartPress();
        }
        
        // Check for drag start
        if (!_isDragging && Input.Down("Mouse1") && 
            Vector2.DistanceBetween(_mousePressPosition, Mouse.Position) > DRAG_THRESHOLD)
        {
            StartDrag();
        }
        
        // Handle active drag
        if (_isDragging && Input.Down("Mouse1"))
        {
            UpdateDrag();
        }
        
        // Handle mouse release
        if (Input.Released("Mouse1"))
        {
            if (_isDragging)
            {
                FinalizeDrag();
            }
            else
            {
                HandleClick();
            }
        }
    }

    private void StartPress()
    {
        // Record initial mouse position and hit object
        _mousePressPosition = Mouse.Position;
        var ray = Camera.ScreenPixelToRay(_mousePressPosition);
        var tr = Scene.Trace.Ray(ray, 5000f).WithTag("selectable").Run();
        _initialHitObject = tr.Hit ? tr.GameObject : null;
    }

    private void StartDrag()
    {
        _isDragging = true;
        
        // Convert initial mouse position to world position
        var ray = Camera.ScreenPixelToRay(_mousePressPosition);
        var tr = Scene.Trace.Ray(ray, 5000f).WithTag("world").Run();
        _selectionBoxStart = tr.Hit ? tr.HitPosition : Vector3.Zero;
        _selectionBoxEnd = _selectionBoxStart;
    }

    private void UpdateDrag()
    {
        // Update selection box end position
        var ray = Camera.ScreenPixelToRay(Mouse.Position);
        var tr = Scene.Trace.Ray(ray, 5000f).WithTag("world").Run();
        if (tr.Hit)
        {
            _selectionBoxEnd = tr.HitPosition;
        }
    }

    private void FinalizeDrag()
    {
        // Handle box selection
        SelectObjectsInBox();
        
        // Reset drag state
        _isDragging = false;
        _selectionBoxStart = Vector3.Zero;
        _selectionBoxEnd = Vector3.Zero;
    }

    private void HandleClick()
    {
        if (_initialHitObject != null)
        {
            if (_multiSelectMode)
            {
                ToggleSelection(_initialHitObject);
            }
            else
            {
                ClearSelection();
                AddToSelection(_initialHitObject);
            }
        }
        else if (!_multiSelectMode)
        {
            ClearSelection();
        }
    }

    private void SelectObjectsInBox()
    {
        // Create selection bounds
        var min = new Vector3(
            Math.Min(_selectionBoxStart.x, _selectionBoxEnd.x),
            Math.Min(_selectionBoxStart.y, _selectionBoxEnd.y),
            Math.Min(_selectionBoxStart.z, _selectionBoxEnd.z) - 50f
        );
        
        var max = new Vector3(
            Math.Max(_selectionBoxStart.x, _selectionBoxEnd.x),
            Math.Max(_selectionBoxStart.y, _selectionBoxEnd.y),
            Math.Max(_selectionBoxStart.z, _selectionBoxEnd.z) + 50f
        );
        
        var bounds = new BBox(min, max);
        
        // Find selectable objects in bounds
        var newSelection = Scene.GetAllObjects(true)
            .Where(go => go.Tags.Has("selectable") && 
                         bounds.Contains(go.WorldPosition))
            .ToList();

        // Update selection based on mode
        if (!_multiSelectMode)
        {
            ClearSelection();
        }

        foreach (var obj in newSelection)
        {
            if (!Selected.Contains(obj))
            {
                AddToSelection(obj);
            }
        }
    }

    private void DrawDebugVisuals()
    {
        // Draw hover effect for selectables
        var ray = Camera.ScreenPixelToRay(Mouse.Position);
        var tr = Scene.Trace.Ray(ray, 5000f).WithTag("selectable").Run();
        if (tr.Hit)
        {
            DebugOverlay.Sphere(
                new Sphere(tr.GameObject.WorldPosition + Vector3.Up * 30, 50f),
                Color.Yellow.WithAlpha(0.1f),
                0.01f
            );
        }

        // Draw selection box when dragging
        if (_isDragging)
        {
            var min = new Vector3(
                Math.Min(_selectionBoxStart.x, _selectionBoxEnd.x),
                Math.Min(_selectionBoxStart.y, _selectionBoxEnd.y),
                Math.Min(_selectionBoxStart.z, _selectionBoxEnd.z) - 50f
            );
            
            var max = new Vector3(
                Math.Max(_selectionBoxStart.x, _selectionBoxEnd.x),
                Math.Max(_selectionBoxStart.y, _selectionBoxEnd.y),
                Math.Max(_selectionBoxStart.z, _selectionBoxEnd.z) + 50f
            );
            
            DebugOverlay.Box(
                new BBox(min, max),
                Color.Cyan.WithAlpha(0.2f),
                0.1f
            );
        }
    }

    private void AddToSelection(GameObject obj)
    {
        if (obj == null || Selected.Contains(obj)) return;
        
        var selectable = obj.Components.Get<Selectable>();
        if (selectable != null)
        {
            selectable._isSelected = true;
            Selected.Add(obj);
        }
    }

    private void RemoveFromSelection(GameObject obj)
    {
        if (obj == null || !Selected.Contains(obj)) return;
        
        var selectable = obj.Components.Get<Selectable>();
        if (selectable != null)
        {
            selectable._isSelected = false;
            Selected.Remove(obj);
        }
    }

    private void ToggleSelection(GameObject obj)
    {
        if (Selected.Contains(obj))
        {
            RemoveFromSelection(obj);
        }
        else
        {
            AddToSelection(obj);
        }
    }

    private void ClearSelection()
    {
        foreach (var obj in Selected.ToList())
        {
            RemoveFromSelection(obj);
        }
    }
}