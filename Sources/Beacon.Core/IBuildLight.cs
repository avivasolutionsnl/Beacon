namespace Beacon.Core
{
    public interface IBuildLight
    {
        void Initialize();
        void Dispose();
        void Success();
        void Investigate();
        void Fail();
        void Fixed();
        void NoStatus();
    }
}