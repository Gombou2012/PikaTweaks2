namespace PikaTweaks.Models;
public enum TweakRisk { Low, Medium, High }
public sealed class Tweak : System.ComponentModel.INotifyPropertyChanged
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Description { get; init; } = "";
    public TweakRisk Risk { get; init; } = TweakRisk.Low;
    public bool Recommended { get; init; } = true;
    bool _isSelected = true;
    public bool IsSelected { get => _isSelected; set { if (_isSelected == value) return; _isSelected = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected))); } }
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    public string Script { get; init; } = "";
    public override string ToString() => Name;
}
