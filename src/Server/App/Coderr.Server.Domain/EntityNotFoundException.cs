﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Coderr.Server.Domain
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message)
        {

        }
    }
}
