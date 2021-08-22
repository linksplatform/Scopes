using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Scopes
{
    /// <summary>
    /// <para>
    /// Represents the scope.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="Scope"/>
    public class Scope<TInclude> : Scope
    {
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="Scope"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope() : this(false, false) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="Scope"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="autoInclude">
        /// <para>A auto include.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude) : this(autoInclude, false) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="Scope"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="autoInclude">
        /// <para>A auto include.</para>
        /// <para></para>
        /// </param>
        /// <param name="autoExplore">
        /// <para>A auto explore.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude, bool autoExplore) : base(autoInclude, autoExplore) => Include<TInclude>();
    }
}
