namespace SaasStarter.Application.Common.Exceptions;

/// <summary>
/// Use DomainError.PlanLimitExceeded() para retornar via Result em vez de lançar esta exceção.
/// Mantida para compatibilidade com code fora do fluxo de controllers (ex: middlewares futuros).
/// </summary>
public class PlanLimitExceededException(string message) : Exception(message);
