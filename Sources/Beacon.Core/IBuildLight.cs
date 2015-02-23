namespace Beacon.Core
{
    public interface IBuildLight
    {
        void Success();
        void Investigate();
        void Fail();
        void NoStatus();
    }
}