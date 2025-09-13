using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Xunit;

namespace Platform.Scopes.Tests
{
    /// <summary>
    /// Tests to verify that XML documentation include tags are working correctly.
    /// </summary>
    public class XmlDocumentationIncludeTests
    {
        private const string ExpectedScopeClassSummary = "Represents a dependency injection scope that manages object dependencies and their lifecycles.";
        private const string ExpectedUseMethodSummary = "Resolves and returns an instance of the specified type T, automatically managing its dependencies.";
        private const string ExpectedConstructorSummary = "Initializes a new instance of the";
        
        [Fact]
        public void XmlDocumentationFile_ShouldExist()
        {
            // Arrange
            var assemblyLocation = Assembly.GetAssembly(typeof(Scope))?.Location;
            Assert.NotNull(assemblyLocation);
            
            var xmlDocPath = Path.ChangeExtension(assemblyLocation, ".xml");
            
            // Act & Assert
            Assert.True(File.Exists(xmlDocPath), $"XML documentation file should exist at: {xmlDocPath}");
        }

        [Fact]
        public void ScopeClass_ShouldHaveIncludedDocumentation()
        {
            // Arrange
            var xmlDoc = LoadXmlDocumentation();
            
            // Act
            var scopeClassNode = xmlDoc.SelectSingleNode("//member[@name='T:Platform.Scopes.Scope']/summary");
            
            // Assert
            Assert.NotNull(scopeClassNode);
            var summaryText = scopeClassNode.InnerText.Trim();
            Assert.Contains(ExpectedScopeClassSummary, summaryText);
            
            // Verify that the included documentation contains the expected detailed description
            var remarksNode = xmlDoc.SelectSingleNode("//member[@name='T:Platform.Scopes.Scope']/remarks");
            Assert.NotNull(remarksNode);
            var remarksText = remarksNode.InnerText.Trim();
            Assert.Contains("The Scope class is the core component of the Platform.Scopes library", remarksText);
            Assert.Contains("Automatic dependency resolution", remarksText);
        }

        [Fact]
        public void ScopeConstructor_ShouldHaveIncludedDocumentation()
        {
            // Arrange
            var xmlDoc = LoadXmlDocumentation();
            
            // Act
            var constructorNode = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.#ctor(System.Boolean,System.Boolean)']/summary");
            
            // Assert
            Assert.NotNull(constructorNode);
            var summaryText = constructorNode.InnerText.Trim();
            Assert.Contains(ExpectedConstructorSummary, summaryText);
            Assert.Contains("auto-include and auto-explore settings", summaryText);
            
            // Verify parameter documentation is included
            var autoIncludeParam = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.#ctor(System.Boolean,System.Boolean)']/param[@name='autoInclude']");
            Assert.NotNull(autoIncludeParam);
            Assert.Contains("automatically include types in the scope", autoIncludeParam.InnerText);
            
            var autoExploreParam = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.#ctor(System.Boolean,System.Boolean)']/param[@name='autoExplore']");
            Assert.NotNull(autoExploreParam);
            Assert.Contains("automatically explore and include assemblies", autoExploreParam.InnerText);
        }

        [Fact]
        public void UseMethod_ShouldHaveIncludedDocumentation()
        {
            // Arrange
            var xmlDoc = LoadXmlDocumentation();
            
            // Act
            var useMethodNode = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.Use``1']/summary");
            
            // Assert
            Assert.NotNull(useMethodNode);
            var summaryText = useMethodNode.InnerText.Trim();
            Assert.Contains(ExpectedUseMethodSummary, summaryText);
            
            // Verify return documentation
            var returnsNode = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.Use``1']/returns");
            Assert.NotNull(returnsNode);
            Assert.Contains("An instance of type T with all dependencies resolved", returnsNode.InnerText);
            
            // Verify exception documentation
            var exceptionNode = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.Use``1']/exception[@cref='T:System.InvalidOperationException']");
            Assert.NotNull(exceptionNode);
            Assert.Contains("Thrown when the type T is excluded", exceptionNode.InnerText);
            
            // Verify detailed remarks are included
            var remarksNode = xmlDoc.SelectSingleNode("//member[@name='M:Platform.Scopes.Scope.Use``1']/remarks");
            Assert.NotNull(remarksNode);
            var remarksText = remarksNode.InnerText.Trim();
            Assert.Contains("This method is the primary entry point for dependency resolution", remarksText);
            Assert.Contains("Check if the type is excluded", remarksText);
        }

        [Fact]
        public void IncludeTag_ShouldReferenceCorrectExternalFile()
        {
            // Arrange
            var sourceFilePath = GetScopeSourceFilePath();
            var sourceContent = File.ReadAllText(sourceFilePath);
            
            // Act & Assert
            Assert.Contains("include file='Documentation.xml'", sourceContent);
            Assert.Contains("path='docs/members[@name=\"Scope\"]/Scope/*'", sourceContent);
            Assert.Contains("path='docs/members[@name=\"Scope\"]/constructor/*'", sourceContent);
            Assert.Contains("path='docs/members[@name=\"Scope\"]/Use/*'", sourceContent);
        }

        [Fact]
        public void ExternalDocumentationFile_ShouldExist()
        {
            // Arrange
            var sourceDirectory = Path.GetDirectoryName(GetScopeSourceFilePath());
            var documentationPath = Path.Combine(sourceDirectory!, "Documentation.xml");
            
            // Act & Assert
            Assert.True(File.Exists(documentationPath), $"External documentation file should exist at: {documentationPath}");
            
            // Verify the external file has valid XML structure
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(documentationPath);
            
            var scopeNode = xmlDoc.SelectSingleNode("//members[@name='Scope']/Scope");
            Assert.NotNull(scopeNode);
            
            var constructorNode = xmlDoc.SelectSingleNode("//members[@name='Scope']/constructor");
            Assert.NotNull(constructorNode);
            
            var useNode = xmlDoc.SelectSingleNode("//members[@name='Scope']/Use");
            Assert.NotNull(useNode);
        }

        private static XmlDocument LoadXmlDocumentation()
        {
            var assemblyLocation = Assembly.GetAssembly(typeof(Scope))?.Location;
            var xmlDocPath = Path.ChangeExtension(assemblyLocation, ".xml");
            
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlDocPath);
            return xmlDoc;
        }

        private static string GetScopeSourceFilePath()
        {
            // Navigate to the source file from test assembly location
            var testAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            var testDirectory = Path.GetDirectoryName(testAssemblyLocation);
            var sourceDirectory = Path.GetFullPath(Path.Combine(testDirectory!, "..", "..", "..", "..", "Platform.Scopes"));
            return Path.Combine(sourceDirectory, "Scope.cs");
        }
    }
}