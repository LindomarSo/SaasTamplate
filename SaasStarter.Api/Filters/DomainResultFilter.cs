using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SaasStarter.Application.Common;
using System.Text.Json;

namespace SaasStarter.Api.Filters;

/// <summary>
/// Intercepta ObjectResult cujo Value seja Result ou Result&lt;T&gt; e converte para a resposta HTTP adequada.
/// - Result&lt;T&gt; com sucesso → 200 OK com o valor desembrulhado
/// - Result (void) com sucesso → 204 No Content
/// - Qualquer Result com falha → status code do DomainError com corpo de erro padronizado
/// </summary>
public sealed class DomainResultFilter : IAlwaysRunResultFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult { Value: Result result })
            return;

        if (!result.IsSuccess)
        {
            var error = result.Error!;
            context.Result = new ObjectResult(new
            {
                status = error.HttpStatus,
                message = error.Message,
                errorCode = error.Code,
                errors = (object?)null
            })
            { StatusCode = error.HttpStatus };
            return;
        }

        var resultType = result.GetType();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var value = resultType.GetProperty(nameof(Result<object>.Value))!.GetValue(result);
            context.Result = new OkObjectResult(value);
        }
        else
        {
            context.Result = new NoContentResult();
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
