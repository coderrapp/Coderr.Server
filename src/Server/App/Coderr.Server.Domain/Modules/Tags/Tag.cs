﻿using System;

namespace Coderr.Server.Domain.Modules.Tags
{
    /// <summary>
    ///     Stack overflow tag
    /// </summary>
    public class Tag
    {
        /// <summary>
        ///     Creates a new instance of <see cref="Tag" />.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="orderNumber">order. 1 = first.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        public Tag(string name, int orderNumber)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            OrderNumber = orderNumber;
        }

        /// <summary>
        ///     Serialization constructor
        /// </summary>
        protected Tag()
        {
        }

        /// <summary>
        ///     Identity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Order
        /// </summary>
        public int OrderNumber { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}