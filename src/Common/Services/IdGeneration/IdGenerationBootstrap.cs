namespace Passwordless.Common.Services.IdGeneration;

public static class IdGenerationBootstrap
{
    public static void AddIdGeneration(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IIdGeneratorFactory, IdGeneratorFactory>();
    }
}