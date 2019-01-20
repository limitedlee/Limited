using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using Consul;

namespace Limited.Gateway.Core.ServiceDiscovery
{
    public class ServiceTask : BackgroundService
    {
        private ILogger<ServiceTask> logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await new TaskFactory().StartNew(() =>
                {
                    try
                    {
                        var services = ConsulHelper.Client.Agent.Services().Result.Response;



                        foreach (var agentService in services)
                        {
                            var serviceName = agentService.Value.Service.ToLower();
                            var microService = new MicroService
                            {
                                Id = agentService.Key,
                                Name = agentService.Value.Service,
                                Address = agentService.Value.Address,
                                Port = agentService.Value.Port
                            };

                            if (!ServiceCache.Services.ContainsKey(serviceName))
                            {
                                var bag = new ConcurrentBag<MicroService>();
                                bag.Add(microService);
                                ServiceCache.Services.TryAdd(serviceName, bag);
                            }
                            else
                            {
                                if (!ServiceCache.Services[serviceName].Any(x => x.Id == agentService.Key))
                                {
                                    ServiceCache.Services[serviceName].Add(microService);
                                }
                            }
                        }

                        var consulServices = new List<AgentService>();
                        foreach (var s in services)
                        {
                            s.Value.Service = s.Value.Service.ToLower();
                            consulServices.Add(s.Value);
                        }

                        var cacheServices = ServiceCache.Services.ToList();
                        foreach (var kvp in cacheServices)
                        {
                            if (!consulServices.Any(x => x.Service == kvp.Key.ToLower()))
                            {
                                ServiceCache.Services.Remove(kvp.Key.ToLower(), out ConcurrentBag<MicroService> removeData);
                                break;
                            }

                            foreach (var service in kvp.Value)
                            {
                                if (!consulServices.Any(x => x.Address == service.Address && x.Port == service.Port))
                                {
                                    ServiceCache.Services.Remove(kvp.Key.ToLower(), out ConcurrentBag<MicroService> removeData);
                                }
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        logger.LogError(exp.ToString());
                    }

                    Thread.Sleep(1 * 1000);
                });
            }
        }
    }
}
