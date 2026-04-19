namespace SaasStarter.Application.Common.Exceptions;

public class EmailNotConfirmedException(string message) : Exception(message);

/// <summary>
/// Lançar quando o usuário ultrapassar os limites do plano.
/// Mapeada para 403 Forbidden no GlobalExceptionHandlerMiddleware.
/// </summary>
public class PlanLimitExceededException(string message) : Exception(message);
