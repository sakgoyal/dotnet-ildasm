using System;

namespace DotNet.Ildasm.Configuration
{
    public class ItemFilter
    {
        public ItemFilter(string itemFilter)
        {
            if (!string.IsNullOrEmpty(itemFilter))
            {
                int index = itemFilter.IndexOf("::", StringComparison.CurrentCultureIgnoreCase);
                if (index > -1)
                {
                    if (index > 0)
                    {
                        Class = itemFilter[..^index];
                    }

                    Method = itemFilter[(index + 2)..];
                }
                else
                {
                    Class = itemFilter;
                }
            }
        }

        public bool HasFilter => !string.IsNullOrEmpty(Class) || !string.IsNullOrEmpty(Method);
        public string Method { get; }
        public string Class { get; }
    }
}
