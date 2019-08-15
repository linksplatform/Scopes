#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Scopes
{
    public class Scope<TInclude> : Scope
    {
        public Scope() : this(false, false) { }
        public Scope(bool autoInclude) : this(autoInclude, false) { }
        public Scope(bool autoInclude, bool autoExplore) : base(autoInclude, autoExplore) => Include<TInclude>();
    }
}
