using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Scopes
{
    public class Scope<TInclude> : Scope
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope() : this(false, false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude) : this(autoInclude, false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude, bool autoExplore) : base(autoInclude, autoExplore) => Include<TInclude>();
    }
}
