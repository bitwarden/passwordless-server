# During dev
dotnet ef migrations add CHANGE_NAME --startup-project ../src/Api/ --context DbTenantContext
dotnet ef database update --context DbTenantContext --startup-project ../src/Api


# Script generation
dotnet ef migrations script --idempotent --context DbTenantContext --startup-project ../src/Api > migrate.sql
