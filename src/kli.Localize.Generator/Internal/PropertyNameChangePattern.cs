namespace kli.Localize.Generator.Internal
{
    internal static class PropertyNameChangePattern
    {
        /// <summary>
        /// Is use to create an valid property name for a translation key.
        /// Can later extracted to an configurable interface method or what ever to allow user their own name behaviors.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string Change(string propertyName)
        {
            // Currently we only changing spaces to _
            return propertyName.Replace(" ", "_")
                .Replace("[","").Replace("]","")
                .Replace("{","").Replace("}","");
        }
    }
}