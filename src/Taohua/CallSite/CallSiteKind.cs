namespace Taohua.CallSite
{
    internal enum CallSiteKind
    {
        Factory,

        Constructor,

        Constant,

        IEnumerable,

        ServiceProvider,

        Scope,

        Transient,

        CreateInstance,

        ServiceScopeFactory,

        Singleton
    }
}