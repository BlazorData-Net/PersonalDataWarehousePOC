var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PersonalDataWarehousePOC>("personaldatawarehousepoc");

builder.Build().Run();
