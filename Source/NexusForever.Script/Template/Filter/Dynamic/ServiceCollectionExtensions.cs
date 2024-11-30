using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NexusForever.Script.Template.Filter.Dynamic
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScriptTemplateFilterDynamic(this IServiceCollection sc)
        {
            sc.AddTransient<IScriptFilterDynamicEntitySpline, ScriptFilterDynamicEntitySpline>();
        }
    }
}
