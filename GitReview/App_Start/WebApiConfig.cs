// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GitReview
{
    using System.Web.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Contains API route registrations.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the application's API routes against the specified <see cref="HttpConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration"/> to update.</param>
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.Converters.Add(new StringEnumConverter());
        }
    }
}
