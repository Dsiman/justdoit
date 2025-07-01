using Sandbox.UI;

public class InfoPanel : Storeable
{
	public enum InfoPanelMode
	{
		Error,
		Warning,
		Success,
		Info,
		Dialog
	}
	private Sandbox.WorldPanel _worldPanel;
	private InfoPanelRazor _panelComponent;
	private readonly List<InfoPanelMessage> _messageQueue = new List<InfoPanelMessage>();
	private InfoPanelMessage _currentMessage;
	private float _showTimer;

	protected override void OnAwake()
	{
		base.OnAwake();

		// Create world panel
		var worldPanelObject = new GameObject(GameObject, true, "InfoPanelWorld");
		worldPanelObject.WorldPosition = GameObject.WorldPosition + Vector3.Up * 50;

		_worldPanel = worldPanelObject.Components.Create<Sandbox.WorldPanel>();
		_worldPanel.PanelSize = new Vector2(1024, 512);
		_worldPanel.LookAtCamera = true;
		_worldPanel.RenderScale = 1f;
		_worldPanel.RenderOptions.Overlay = true;

		_panelComponent = worldPanelObject.AddComponent<InfoPanelRazor>();

		// Set initial visibility
		SetPanelVisible(false);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Process message queue
		ProcessMessages();

		// Handle timer for current message
		HandleMessageTimer();
	}

	private void ProcessMessages()
	{
		// Don't process if we're already showing a message or queue is empty
		if (_currentMessage != null || !_messageQueue.Any()) return;

		// Find highest priority message (Error > Warning > Success > Info > Dialog)
		var nextMessage = _messageQueue
			.OrderBy(m => m.Mode)
			.FirstOrDefault();

		if (nextMessage == null) return;

		// Remove from queue and display
		_messageQueue.Remove(nextMessage);
		ShowMessage(nextMessage);
	}

	private void HandleMessageTimer()
	{
		if (_currentMessage == null || !_currentMessage.IsAutoClose) return;

		// Only decrement timer when not selected
		if (!_isSelected)
		{
			_showTimer -= Time.Delta;

			if (_showTimer <= 0)
			{
				// Message expired, clear current
				ClearCurrentMessage();
			}
		}
	}

	private void ShowMessage(InfoPanelMessage message)
	{
		_currentMessage = message;

		_currentMessage.ShowTime = message.ShowTime;

		// Update Razor component
		if (_panelComponent != null && _panelComponent.IsValid())
		{
			_panelComponent.InfoText = message.Text;
			_panelComponent.TextColor = message.TextColor;
			_panelComponent.BackgroundColor = message.BackgroundColor;
			_panelComponent.ModeBorderColor = message.ModeBorderColor;
			_panelComponent.Icon = message.Icon;
		}

		// Start timer
		_showTimer = message.ShowTime;
		SetPanelVisible(true);
	}


	private void ClearCurrentMessage()
	{
		_currentMessage = null;
		SetPanelVisible(false);
	}

	private void SetPanelVisible(bool visible)
	{
		if (_worldPanel != null && _worldPanel.IsValid())
		{
			_panelComponent.IsVisible = visible;
		}
	}
	public void AddMessage(InfoPanelMessage message)
	{
		_messageQueue.Add(message);
	}

	public void Error(string text, float showTime = 10.0f)
	{
		AddMessage(new InfoPanelMessage
		{
			Text = text,
			Mode = InfoPanelMode.Error,
			ShowTime = showTime,
			IsAutoClose = true,
			BackgroundColor = Color.Black,
			TextColor = Color.White,
			Icon = "❌"
		});
	}

	public void Warning(string text, float showTime = 5.0f)
	{
		AddMessage(new InfoPanelMessage
		{
			Text = text,
			Mode = InfoPanelMode.Warning,
			ShowTime = showTime,
			IsAutoClose = true,
			BackgroundColor = new Color(0.9f, 0.7f, 0.1f),
			TextColor = Color.Black,
			Icon = "⚠️"
		});
	}

	public void Success(string text, float showTime = 5.0f)
	{
		AddMessage(new InfoPanelMessage
		{
			Text = text,
			Mode = InfoPanelMode.Success,
			ShowTime = showTime,
			IsAutoClose = true,
			BackgroundColor = new Color(0.1f, 0.7f, 0.2f),
			TextColor = Color.White,
			Icon = "✅"
		});
	}

	public void Info(string text, float showTime = 5.0f)
	{
		AddMessage(new InfoPanelMessage
		{
			Text = text,
			Mode = InfoPanelMode.Info,
			ShowTime = showTime,
			IsAutoClose = true,
			BackgroundColor = new Color(0.1f, 0.3f, 0.8f),
			TextColor = Color.White,
			Icon = "ℹ️"
		});
	}

	public void Dialog(string text, float showTime = 10.0f)
	{
		AddMessage(new InfoPanelMessage
		{
			Text = text,
			Mode = InfoPanelMode.Dialog,
			ShowTime = showTime,
			IsAutoClose = true,
			BackgroundColor = new Color(0.2f, 0.2f, 0.2f),
			TextColor = Color.White,
			Icon = "💬"
		});
	}

	[Button("Display Test Info Message")]
	public void DisplayTestMessage()
	{
		Info("This is a test message from the editor");
	}

	[Button("Display Test Error Message")]
	public void DisplayTestErrorMessage()
	{
		Error("This is a test error message from the editor");
	}
}

public class InfoPanelMessage
{
    public string Text { get; set; }
    public InfoPanel.InfoPanelMode Mode { get; set; } = InfoPanel.InfoPanelMode.Info;
    public float ShowTime { get; set; } = 5.0f;
	public bool IsAutoClose { get; set; } = true;
    public Color TextColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = Color.White;
	public Color ModeBorderColor => Mode switch
	{
		InfoPanel.InfoPanelMode.Error => Color.Red,
		InfoPanel.InfoPanelMode.Warning => new Color(0.9f, 0.7f, 0.1f),
		InfoPanel.InfoPanelMode.Success => new Color(0.1f, 0.7f, 0.2f),
		InfoPanel.InfoPanelMode.Info => new Color(0.1f, 0.3f, 0.8f),
		InfoPanel.InfoPanelMode.Dialog => new Color(0.5f, 0.5f, 0.5f),
		_ => Color.White
	};
    public string Icon { get; set; } = "ℹ️";
}