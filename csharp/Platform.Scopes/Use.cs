using System.Runtime.CompilerServices;
using Platform.Disposables;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Scopes
{
    /// <summary>
    /// <para>
    /// Represents the use.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class Use<T>
    {
        /// <summary>
        /// <para>
        /// Gets the single value.
        /// </para>
        /// <para></para>
        /// </summary>
        public static T Single
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Scope.Global.Use<T>();
        }

        /// <summary>
        /// <para>
        /// Gets the new value.
        /// </para>
        /// <para></para>
        /// </summary>
        public static Disposable<T> New
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var scope = new Scope(autoInclude: true, autoExplore: true);
                return new Disposable<T, Scope>(scope.Use<T>(), scope);
            }
        }
    }
}
