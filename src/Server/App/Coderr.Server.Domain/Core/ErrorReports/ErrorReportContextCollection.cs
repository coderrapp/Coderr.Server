﻿using System;
using System.Collections.Generic;

namespace Coderr.Server.Domain.Core.ErrorReports
{
    /// <summary>
    ///     Context used when analysing the report
    /// </summary>
    public class ErrorReportContextCollection
    {
        private IDictionary<string, string> _properties;

        /// <summary>
        ///     Creates a new instance of <see cref="ErrorReportContextCollection" />.
        /// </summary>
        /// <param name="name">context collection name</param>
        /// <param name="properties">properties for the collection</param>
        /// <exception cref="ArgumentNullException">name; properties</exception>
        public ErrorReportContextCollection(string name, IDictionary<string, string> properties)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            Name = name;
            Properties = properties;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ErrorReportContextCollection" />.
        /// </summary>
        protected ErrorReportContextCollection()
        {
        }

        /// <summary>
        ///     Context collection name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Context collection properties
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return _properties; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _properties = value;
            }
        }
    }
}