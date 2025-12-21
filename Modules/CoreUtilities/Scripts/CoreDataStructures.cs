// A common place for adding very simple and broad interfaces that may be used across the entire applicaition
namespace HitTrax.CoreUtilities
{
    public interface IProgressPercent
    {
        float ProgressPercent { get; }
    }

    public enum EDisplays : byte
    {
        Kiosk = 0,
        Projector = 1
    }

}