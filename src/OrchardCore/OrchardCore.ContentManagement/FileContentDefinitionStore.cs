using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.ContentManagement
{
    public class FileContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;

        public FileContentDefinitionStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var scopedCache = ShellScope.Services.GetRequiredService<FileContentDefinitionScopedCache>();

            if (scopedCache.ContentDefinitionRecord != null)
            {
                return scopedCache.ContentDefinitionRecord;
            }

            (_, var result) = await GetContentDefinitionAsync();

            return scopedCache.ContentDefinitionRecord = result;
        }

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        public Task<(bool, ContentDefinitionRecord)> GetContentDefinitionAsync()
        {
            var scopedCache = ShellScope.Services.GetRequiredService<FileContentDefinitionScopedCache>();

            if (scopedCache.ContentDefinitionRecord != null)
            {
                // Return the already loaded document but indicating that it should not be cached.
                return Task.FromResult((false, scopedCache.ContentDefinitionRecord));
            }

            ContentDefinitionRecord result;

            if (!File.Exists(Filename))
            {
                result = new ContentDefinitionRecord();
            }
            else
            {
                lock (this)
                {
                    using (var file = File.OpenText(Filename))
                    {
                        var serializer = new JsonSerializer();
                        result = (ContentDefinitionRecord)serializer.Deserialize(file, typeof(ContentDefinitionRecord));
                    }
                }
            }

            return Task.FromResult((true, result));
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
            lock (this)
            {
                var directoryPath = Path.GetDirectoryName(Filename);
                if (!String.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var file = File.CreateText(Filename))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, contentDefinitionRecord);
                }
            }

            return Task.CompletedTask;
        }

        private string Filename => PathExtensions.Combine(
            _shellOptions.Value.ShellsApplicationDataPath,
            _shellOptions.Value.ShellsContainerName,
            _shellSettings.Name, "ContentDefinition.json");
    }

    internal class FileContentDefinitionScopedCache
    {
        public ContentDefinitionRecord ContentDefinitionRecord { get; internal set; }
    }
}
