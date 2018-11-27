﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logger
{
    public interface ILogger
    {
        void Log(string msg);
    }
    public interface IPathLogger
    {
        void Log(string msg, string path);
    }
}
