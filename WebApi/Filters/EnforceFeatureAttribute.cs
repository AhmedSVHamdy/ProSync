using Microsoft.AspNetCore.Mvc;

namespace WebApi.Filters
{
    public class EnforceFeatureAttribute : TypeFilterAttribute
    {
        public EnforceFeatureAttribute(string featureName) : base(typeof(EnforceFeatureFilter))
        {
            Arguments = new object[] { featureName };
        }
    }
}
