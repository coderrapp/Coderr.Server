﻿using System.Collections.Generic;
using Coderr.Server.Abstractions.Config;
using Coderr.Server.Infrastructure.Configuration;

namespace Coderr.Server.App.Tests.Configuration.TestEntitites
{
    internal class SoCultural : IConfigurationSection
    {
        public float Number { get; set; }

        public string SectionName
        {
            get { return "SoCultural"; }
        }

        public IDictionary<string, string> ToDictionary()
        {
            return this.ToConfigDictionary();
        }

        public void Load(IDictionary<string, string> settings)
        {
        }
    }
}