﻿namespace Coderr.Server.App.Modules.Whitelists
{
    /// <summary>
    ///     Typ of stored IP record.
    /// </summary>
    public enum IpType
    {
        /// <summary>
        ///     Added when doing a lookup for the domain
        /// </summary>
        Lookup = 0,

        /// <summary>
        ///     Manually specified by the user
        /// </summary>
        Manual = 1,

        /// <summary>
        ///     We got a request from this IP and a lookup didn't match it.
        /// </summary>
        Denied = 2
    }
}