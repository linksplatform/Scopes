using Xunit;
using Platform.Reflection;

namespace Platform.Scopes.Tests
{
    /// <summary>
    /// <para>
    /// Represents the scope tests.
    /// </para>
    /// <para></para>
    /// </summary>
    public class ScopeTests
    {
        /// <summary>
        /// <para>
        /// Defines the interface.
        /// </para>
        /// <para></para>
        /// </summary>
        public interface IInterface
        {
        }

        /// <summary>
        /// <para>
        /// Represents the .
        /// </para>
        /// <para></para>
        /// </summary>
        /// <seealso cref="IInterface"/>
        public class Class : IInterface
        { 
        }

        /// <summary>
        /// <para>
        /// Tests that single dependency test.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void SingleDependencyTest()
        {
            using var scope = new Scope();
            scope.IncludeAssemblyOf<IInterface>();
            var instance = scope.Use<IInterface>();
            Assert.IsType<Class>(instance);
        }

        /// <summary>
        /// <para>
        /// Tests that type parameters test.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void TypeParametersTest()
        {
            using var scope = new Scope<Types<Class>>();
            var instance = scope.Use<IInterface>();
            Assert.IsType<Class>(instance);
        }
    }
}
