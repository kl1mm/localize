using System.Collections.Generic;

namespace kli.Localize.Generator.Internal.Helper
{
    internal class TranslationData : Dictionary<string, object>
    {
        public IDictionary<string, string> Flatten() 
            => this.FlattenCore(this, string.Empty);

        private IDictionary<string, string> FlattenCore(TranslationData translationData, string parentKey)
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in translationData)
            {
                var key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}::{kvp.Key}";
                if (kvp.Value is TranslationData nested)
                {
                    foreach (var nestedKvp in this.FlattenCore(nested, key)) 
                        result[nestedKvp.Key] = nestedKvp.Value;
                }
                else
                    result[key] = kvp.Value.ToString();
            }

            return result;
        }        
    }
}