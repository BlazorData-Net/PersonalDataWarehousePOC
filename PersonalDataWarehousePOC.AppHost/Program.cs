var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PersonalDataWarehousePOCWeb>("personaldatawarehousepocweb");

builder.Build().Run();
