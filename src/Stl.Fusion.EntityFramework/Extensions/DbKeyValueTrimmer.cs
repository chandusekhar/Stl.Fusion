using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stl.Fusion.Extensions;

namespace Stl.Fusion.EntityFramework.Extensions
{
    public class DbKeyValueTrimmer<TDbContext, TDbKeyValue> : DbWakeSleepProcessBase<TDbContext>
        where TDbContext : DbContext
        where TDbKeyValue : DbKeyValue, new()
    {
        public class Options
        {
            public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(5);
            public int BatchSize { get; set; } = 100;
            public LogLevel LogLevel { get; set; } = LogLevel.Information;
        }

        protected TimeSpan CheckInterval { get; }
        protected int BatchSize { get; }
        protected int LastTrimCount { get; set; }
        protected Random Random { get; }
        protected LogLevel LogLevel { get; }
        protected IKeyValueStore<TDbContext> KeyValueStore { get; }

        public DbKeyValueTrimmer(Options? options, IServiceProvider services)
            : base(services)
        {
            options ??= new();
            LogLevel = options.LogLevel;

            CheckInterval = options.CheckInterval;
            BatchSize = options.BatchSize;
            Random = new Random();

            KeyValueStore = services.GetRequiredService<IKeyValueStore<TDbContext>>();
        }

        protected override async Task WakeUpAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = CreateDbContext(true);
            dbContext.DisableChangeTracking();
            LastTrimCount = 0;

            var minExpiresAt = Clock.Now.ToDateTime();
            var keys = await dbContext.Set<TDbKeyValue>().AsQueryable()
                .Where(o => o.ExpiresAt < minExpiresAt)
                .OrderBy(o => o.ExpiresAt)
                .Select(o => o.Key)
                .Take(BatchSize)
                .ToArrayAsync(cancellationToken).ConfigureAwait(false);
            if (keys.Length == 0)
                return;

            // This must be done via IKeyValueStore & operations,
            // otherwise invalidation won't happen for removed entries
            await KeyValueStore.RemoveManyAsync(keys, cancellationToken).ConfigureAwait(false);
            LastTrimCount = keys.Length;

            var logEnabled = LogLevel != LogLevel.None && Log.IsEnabled(LogLevel);
            if (LastTrimCount > 0 && logEnabled)
                Log.Log(LogLevel, "Trimmed {Count} entries", LastTrimCount);
        }

        protected override Task SleepAsync(Exception? error, CancellationToken cancellationToken)
        {
            var delay = default(TimeSpan);
            if (error != null)
                delay = TimeSpan.FromMilliseconds(1000 * Random.NextDouble());
            else if (LastTrimCount < BatchSize)
                delay = CheckInterval + TimeSpan.FromMilliseconds(10 * Random.NextDouble());
            return Clock.DelayAsync(delay, cancellationToken);
        }
    }
}
