using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stereologue.SourceGenerator;

[Generator]
public class LogGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributedTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Stereologue.GenerateLogAttribute",
                predicate: static (s, _) => s is TypeDeclarationSyntax,
                transform: static (ctx, token) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.TargetNode) is not INamedTypeSymbol classSymbol)
                    {
                        return null;
                    }
                    return classSymbol.GetLoggableType(token);
                })
            .Where(static m => m is not null);

        context.RegisterSourceOutput(attributedTypes,
            static (spc, source) => source.ExecuteSourceGeneration(spc));
    }
}

// Notes on what analyzer needs to block
// * Type marked [GenerateLog] is not partial
// * Type marked [GenerateLog] is interface (Might be fixed)
// * Array Like types of Nullable<T> for [Log] marked members
// * Pointer types for [Log] marked members
// * Only Allow Span, ROS and [] for array types
// * If array, only allow Long for integers
// * Types must be either primitives, ILogged, Array of ILogged, IStructSerializable, Array of IStructSerializable, or IProtobufSerializable

// What analyzer needs to warn
// * Class contains [Log] annotations, but is not marked [GenerateLog]
