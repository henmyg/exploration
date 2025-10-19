namespace Maui.Core.Features.Debug;

public interface IPreviewView<TState> where TState : Enum
{
    public TState ViewState { get; set; }
    public IReadOnlyList<ViewStateOption<TState>> States {  get; }
}

public record ViewStateOption<TEnum>(string Title, TEnum Value) where TEnum : Enum;
