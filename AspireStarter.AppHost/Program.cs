var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");

// RAF_GRAFANA_3
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithVolumeMount("../grafana/config", "/etc/grafana")
                     .WithVolumeMount("../grafana/dashboards", "/var/lib/grafana/dashboards")
                     .WithServiceBinding(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

var apiservice = builder.AddProject<Projects.AspireStarter_ApiService>("apiservice")
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"));// RAF_GRAFANA_2

builder.AddProject<Projects.AspireStarter_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(apiservice);

builder.AddContainer("prometheus", "prom/prometheus")
       .WithVolumeMount("../prometheus", "/etc/prometheus")
       .WithServiceBinding(9090, hostPort: 9090);


builder.Build().Run();
