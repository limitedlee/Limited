using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Limited.Gateway
{
    public class ServiceMonitorTask : BackgroundService
    {
        private ILogger<ServiceMonitorTask> logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await new TaskFactory().StartNew(() =>
                {
                    var services = ConsulHelper.Client.Agent.Services();

                    var consulServices = new List<AgentService>();
                    foreach (var s in services.Result.Response)
                    {
                        s.Value.Service = s.Value.Service.ToLower();
                        consulServices.Add(s.Value);
                    }

                    foreach (var agentService in consulServices)
                    {

                        var serviceName = agentService.Service.ToLower();
                        if (!ServiceTable.Services.ContainsKey(serviceName))
                        {
                            var bag = new ConcurrentBag<AgentService>();
                            bag.Add(agentService);

                            ServiceTable.Services.TryAdd(serviceName, bag);
                        }
                        else
                        {
                            if (!ServiceTable.Services[serviceName].Any(x => x.Address == agentService.Address && x.Port == agentService.Port))
                            {
                                ServiceTable.Services[serviceName].Add(agentService);
                            }
                        }
                    }


                    var cacheServices = ServiceTable.Services.ToArray();
                    foreach (var kvp in cacheServices)
                    {
                        if (!consulServices.Any(x => x.Service == kvp.Key.ToLower()))
                        {
                            ServiceTable.Services.Remove(kvp.Key.ToLower(), out ConcurrentBag<AgentService> removeData);
                            break;
                        }

                        foreach (var service in kvp.Value)
                        {
                            if (!consulServices.Any(x => x.Address == service.Address && x.Port == service.Port))
                            {
                                ServiceTable.Services.Remove(kvp.Key.ToLower(), out ConcurrentBag<AgentService> removeData);
                            }
                        }
                    }

                    Thread.Sleep(1 * 1000);
                });
            }
        }
    }
}
