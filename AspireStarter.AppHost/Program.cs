var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");

// RAF_GRAFANA
var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithVolumeMount("../grafana/config", "/etc/grafana")
                     .WithVolumeMount("../grafana/dashboards", "/var/lib/grafana/dashboards")
                     .WithEndpoint(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

var apiservice = builder.AddProject<Projects.AspireStarter_ApiService>("apiservice")
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"));// RAF_GRAFANA

builder.AddProject<Projects.AspireStarter_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(apiservice);

// RAF_GRAFANA
builder.AddContainer("prometheus", "prom/prometheus")
       .WithVolumeMount("../prometheus", "/etc/prometheus")
       .WithEndpoint(9090, hostPort: 9090);


builder.Build().Run();
