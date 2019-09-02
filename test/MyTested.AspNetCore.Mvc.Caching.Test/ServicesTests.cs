﻿namespace MyTested.AspNetCore.Mvc.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Internal;
    using Internal.Services;
    using Internal.Caching;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Caching.Memory;
    using Setups.Common;
    using Setups.Controllers;
    using Setups.Startups;
    using Xunit;
    using Setups.ViewComponents;
    using Setups;

    public class ServicesTests
    {
        [Fact]
        public void MockMemoryCacheShouldBeRegistedWithAddedCaching()
        {
            MyApplication
                .StartsFrom<DefaultStartup>()
                .WithServices(services => services.AddMemoryCache());

            Assert.IsAssignableFrom<MemoryCacheMock>(TestServiceProvider.GetService<IMemoryCache>());

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void MockMemoryCacheShouldBeDifferentForEveryCallSynchronously()
        {
            MyApplication
                .StartsFrom<DefaultStartup>()
                .WithServices(services => services.AddMemoryCache());

            // second call should not have cache entries
            MyController<MvcController>
                .Instance()
                .WithMemoryCache(cache => cache.WithEntry("test", "value"))
                .Calling(c => c.MemoryCacheAction())
                .ShouldReturn()
                .Ok();

            MyController<MvcController>
                .Instance()
                .Calling(c => c.MemoryCacheAction())
                .ShouldReturn()
                .BadRequest();

            MyApplication.StartsFrom<DefaultStartup>();
        }
        
        [Fact]
        public void MockMemoryCacheShouldBeDifferentForEveryViewComponentCallSynchronously()
        {
            MyApplication
                .StartsFrom<DefaultStartup>()
                .WithServices(services => services.AddMemoryCache());

            // second call should not have cache entries
            MyViewComponent<MemoryCacheComponent>
                .Instance()
                .WithMemoryCache(cache => cache.WithEntry("test", "value"))
                .InvokedWith(c => c.Invoke())
                .ShouldReturn()
                .View();

            MyViewComponent<MemoryCacheComponent>
                .InvokedWith(c => c.Invoke())
                .ShouldReturn()
                .Content("No cache");

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void MockMemoryCacheShouldBeDifferentForEveryCallSynchronouslyWithCachedControllerBuilder()
        {
            MyApplication
                .StartsFrom<DefaultStartup>()
                .WithServices(services => services.AddMemoryCache());

            var controller = new MyController<MvcController>();

            // second call should not have cache entries
            controller
                .WithMemoryCache(cache => cache.WithEntry("test", "value"))
                .Calling(c => c.MemoryCacheAction())
                .ShouldReturn()
                .Ok();

            controller
                .WithMemoryCache(cache => cache.WithEntry(string.Empty, string.Empty))
                .Calling(c => c.MemoryCacheAction())
                .ShouldReturn()
                .BadRequest();

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void DefaultConfigurationShouldSetMockMemoryCache()
        {
            MyApplication
                .StartsFrom<DefaultStartup>()
                .WithServices(services => services.AddMemoryCache());

            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();

            Assert.NotNull(memoryCache);
            Assert.IsAssignableFrom<MemoryCacheMock>(memoryCache);

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void CustomMemoryCacheShouldOverrideTheMockOne()
        {
            MyApplication.StartsFrom<CachingDataStartup>();

            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();

            Assert.NotNull(memoryCache);
            Assert.IsAssignableFrom<CustomMemoryCache>(memoryCache);

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void ExplicitMockMemoryCacheShouldOverrideIt()
        {
            MyApplication
                .StartsFrom<DataStartup>()
                .WithServices(services =>
                {
                    services.ReplaceMemoryCache();
                });

            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();

            Assert.NotNull(memoryCache);
            Assert.IsAssignableFrom<MemoryCacheMock>(memoryCache);

            MyApplication.StartsFrom<DefaultStartup>();
        }

        [Fact]
        public void MockMemoryCacheShouldBeDifferentForEveryCallAsynchronously()
        {
            Task
                .Run(async () =>
                {
                    MyApplication
                        .StartsFrom<DefaultStartup>()
                        .WithServices(services => services.AddMemoryCache());

                    TestHelper.GlobalTestCleanup += () => TestServiceProvider.GetService<IMemoryCache>()?.Dispose();
                    TestHelper.ExecuteTestCleanup();

                    string firstValue = null;
                    string secondValue = null;
                    string thirdValue = null;
                    string fourthValue = null;
                    string fifthValue = null;

                    var tasks = new List<Task>
                    {
                        Task.Run(() =>
                        {
                            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();
                            memoryCache.Set("test", "first");
                            firstValue = TestServiceProvider.GetService<IMemoryCache>().Get<string>("test");
                            TestHelper.ExecuteTestCleanup();
                        }),
                        Task.Run(() =>
                        {
                            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();
                            memoryCache.Set("test", "second");
                            secondValue = TestServiceProvider.GetService<IMemoryCache>().Get<string>("test");
                            TestHelper.ExecuteTestCleanup();
                        }),
                        Task.Run(() =>
                        {
                            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();
                            memoryCache.Set("test", "third");
                            thirdValue = TestServiceProvider.GetService<IMemoryCache>().Get<string>("test");
                            TestHelper.ExecuteTestCleanup();
                        }),
                        Task.Run(() =>
                        {
                            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();
                            memoryCache.Set("test", "fourth");
                            fourthValue = TestServiceProvider.GetService<IMemoryCache>().Get<string>("test");
                            TestHelper.ExecuteTestCleanup();
                        }),
                        Task.Run(() =>
                        {
                            var memoryCache = TestServiceProvider.GetService<IMemoryCache>();
                            memoryCache.Set("test", "fifth");
                            fifthValue = TestServiceProvider.GetService<IMemoryCache>().Get<string>("test");
                            TestHelper.ExecuteTestCleanup();
                        })
                    };

                    await Task.WhenAll(tasks);

                    Assert.Equal("first", firstValue);
                    Assert.Equal("second", secondValue);
                    Assert.Equal("third", thirdValue);
                    Assert.Equal("fourth", fourthValue);
                    Assert.Equal("fifth", fifthValue);

                    MyApplication.StartsFrom<DefaultStartup>();
                })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
