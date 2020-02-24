using Xunit;
using Platform.Reflection;

namespace Platform.Scopes.Tests
{
    public class ScopeTests
    {
        public interface IInterface
        {
        }

        public class Class : IInterface
        { 
        }

        [Fact]
        public static void SingleDependencyTest()
        {
            using var scope = new Scope();
            scope.IncludeAssemblyOf<IInterface>();
            var instance = scope.Use<IInterface>();
            Assert.IsType<Class>(instance);
        }

        [Fact]
        public static void TypeParametersTest()
        {
            using var scope = new Scope<Types<Class>>();
            var instance = scope.Use<IInterface>();
            Assert.IsType<Class>(instance);
        }
    }
}
