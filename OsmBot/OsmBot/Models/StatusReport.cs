using System;
using Newtonsoft.Json;

// ReSharper disable NotAccessedField.Global

// ReSharper disable MemberCanBePrivate.Global

namespace OsmBot.Models
{
    /// <summary>
    /// The status report gives some insight in the server.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StatusReport
    {
        /// <summary>
        /// Indicates if the server is online
        /// </summary>
        public bool Online => true;


        /// <summary>
        /// A small string so that the programmer knows a little what version is running.
        /// Should be taken with a grain of salt
        /// </summary>
        public string Version { get; } = "OsmBot 0.1";



        public StatusReport()
        {

        }
    }



}