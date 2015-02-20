namespace Beacon
{
    internal interface IBuildLight
    {
        void Success();
        void Warning();
        void Fail();
        void Off();
    }
}